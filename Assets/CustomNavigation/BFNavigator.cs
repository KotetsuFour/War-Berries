using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomPhysics))]
public class BFNavigator : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float stoppingDistance;
    [SerializeField] private bool terrestrial;
    [SerializeField] private LayerMask traversableLayer;
    private int shorttermMainMapDestination;
    private int shorttermSubMapDestination;
    private BattlefieldTile destination;
    private BattlefieldTile[] tilePath;
    private BattlefieldTile[][] tileMap;
    private Vector3[] subMapPath; //Represents actual world space
    private bool[][] navigationSubMap;
    private Vector2Int formationPosition;
    private bool moving;
    private BattlefieldTile subMapAnchorTile;
    private CustomPhysics physics;
    private const float DiagonalCostMultiplier = 1.41421356f;

    public const int SUBMAP_RADIUS = 7;
    public const int ARBITRARY_HIGH_RAYCAST_START_HEIGHT = 100;

    //The last 8 bits are for door accessibility. BattlefieldTile will eliminate these if there's no door,
    //or if it's an aquatic unit that can't go through a door
    public const ushort toPosXDirection = 0b_1000_0000_1111_1111;
    public const ushort toNegXDirection = 0b_0100_0000_1111_1111;
    public const ushort toPosYDirection = 0b_0010_0000_1111_1111;
    public const ushort toNegYDirection = 0b_0001_0000_1111_1111;

    public const ushort fromPosXDirection = 0b_0000_1000_1111_1111;
    public const ushort fromNegXDirection = 0b_0000_0100_1111_1111;
    public const ushort fromPosYDirection = 0b_0000_0010_1111_1111;
    public const ushort fromNegYDirection = 0b_0000_0001_1111_1111;

    public void setBattlefieldData(BattlefieldTile[][] tileMap, float formationPositionX, float formationPositionY)
    {
        this.tileMap = tileMap;
//        this.formationPosition = formationPositionX;
        navigationSubMap = new bool[Mathf.RoundToInt(SUBMAP_RADIUS * 2)][];
        for (int q = 0; q < navigationSubMap.Length; q++)
        {
            navigationSubMap[q] = new bool[Mathf.RoundToInt(SUBMAP_RADIUS * 2)];
        }
        physics = GetComponent<CustomPhysics>();
    }
    public void setRandomTestDestination()
    {
//        setDestination(tileMap[Random.Range(0, tileMap.Length)][Random.Range(0, tileMap[0].Length)]);
    }
    public void setSpeed(float movementSpeed)
    {
        this.movementSpeed = movementSpeed;
    }
    public void setDestination(BattlefieldTile destination)
    {
        if (destination == null)
        {
            moving = false;
            return;
        }

        this.destination = destination;

        Vector3 myPosition = transform.position;

        tilePath = getPath(worldCoordinatesToBattlefieldTile(myPosition));

        refreshSubMap();
    }
    private BattlefieldTile worldCoordinatesToBattlefieldTile(Vector3 pos)
    {
        return tileMap[Mathf.FloorToInt(pos.x)][Mathf.FloorToInt(pos.z)];
    }
    public BattlefieldTile getDestination()
    {
        return destination;
    }
    public bool reachedDestination()
    {
        return tilePath == null || shorttermMainMapDestination == tilePath.Length - 1;
    }
    public bool reachedSubMapOfNextBFTile()
    {
        return reachedDestination()
            || worldCoordinatesToBattlefieldTile(subMapPath[shorttermSubMapDestination]) == tilePath[shorttermMainMapDestination];
    }
    public bool reachedFormationPosition()
    {
        return (subMapPath[subMapPath.Length - 1] - transform.position).magnitude <= stoppingDistance;
    }
    public void setMoving(bool moving)
    {
        if (tilePath == null)
        {
            this.moving = false;
            return;
        }
        this.moving = moving;
    }
    public bool isMoving()
    {
        return moving;
    }
    public void setStoppingDistance(float stoppingDistance)
    {
        this.stoppingDistance = stoppingDistance;
    }

    /// <summary>
    /// Drives movement along tilePath/subMapPath while moving is true; does
    /// nothing otherwise. If already precisely at the absolute destination
    /// (final tile AND final submap waypoint already reached), finalizes
    /// immediately WITHOUT calling move() again, avoiding a redundant nudge
    /// on an already-arrived frame. Otherwise calls move() every tick,
    /// advances shorttermSubMapDestination once within stoppingDistance
    /// (horizontally) of the current waypoint, and detects crossing into the
    /// next tile by checking whether the new waypoint's world coordinates
    /// now fall on a different tile than tilePath[shorttermMainMapDestination].
    /// Crossing into a tile that is NOT the final one triggers a submap
    /// refresh; crossing into the final tile needs no refresh, since the
    /// subMapPath computed one crossing earlier was already built to reach
    /// the true final destination.
    /// </summary>
    private void Update()
    {
        if (!moving)
        {
            return;
        }

        if (tilePath == null || subMapPath == null)
        {
            return; // Incomplete setup, or a previous refreshSubMap failed to find a path -- see refreshSubMap.
        }

        bool onFinalTile = shorttermMainMapDestination == tilePath.Length - 1;
        bool atFinalSubMapWaypoint = shorttermSubMapDestination == subMapPath.Length - 1;

        if (onFinalTile && atFinalSubMapWaypoint)
        {
            moving = false;
            tilePath = null;
            return;
        }

        move();

        Vector3 currentTarget = subMapPath[shorttermSubMapDestination];

        if (horizontalDistance(transform.position, currentTarget) > stoppingDistance)
        {
            return; // Not close enough yet.
        }

        if (shorttermSubMapDestination < subMapPath.Length - 1)
        {
            shorttermSubMapDestination++;
        }

        if (onFinalTile)
        {
            return; // No crossing possible -- there's no tile beyond the final one.
        }

        BattlefieldTile currentTile = tilePath[shorttermMainMapDestination];
        Vector3 newTarget = subMapPath[shorttermSubMapDestination];

        if (isOnTile(newTarget, currentTile))
        {
            return; // Still on the same tile -- no crossing yet.
        }

        // Crossed into the next tile.
        shorttermMainMapDestination++;
        bool isNowOnFinalTile = shorttermMainMapDestination == tilePath.Length - 1;

        if (!isNowOnFinalTile)
        {
            refreshSubMap();
        }
    }

    /// <summary>
    /// Nudges the GameObject toward subMapPath[shorttermSubMapDestination]
    /// via CustomPhysics.Move(), scaled by movementSpeed and
    /// StaticData.deltaTime(). Includes the y component of the direction
    /// (the battlefield isn't flat), so the nudge itself has a vertical
    /// component toward the target's actual ground height, on top of
    /// whatever CustomPhysics's own gravity/grounding does independently.
    /// </summary>
    private void move()
    {
        Vector3 target = subMapPath[shorttermSubMapDestination];
        Vector3 toTarget = target - transform.position;

        float distance = toTarget.magnitude;

        if (distance <= 0f)
        {
            return;
        }

        Vector3 direction = toTarget / distance;
        float step = Mathf.Min(movementSpeed * StaticData.deltaTime(), distance);

        physics.Move(direction * step);
    }

    private float horizontalDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    /// <summary>
    /// Whether worldPosition falls within tile's footprint, assuming each
    /// tile spans exactly 1 world unit in x/z (matching the scale used
    /// throughout the rest of this class).
    /// </summary>
    private bool isOnTile(Vector3 worldPosition, BattlefieldTile tile)
    {
        int tileX = Mathf.FloorToInt(worldPosition.x);
        int tileY = Mathf.FloorToInt(worldPosition.z);
        return tileX == tile.x && tileY == tile.y;
    }

    /// <summary>
    /// Calls setSubMap for the new tilePath[shorttermMainMapDestination]/[+1]
    /// pair, computes the new submap destination (the next tile's
    /// formationPosition slot, searching outward for the nearest open cell
    /// if that slot is blocked AND the next tile is the final one), and
    /// calls getSubMapPath. If getSubMapPath still returns null, subMapPath
    /// and shorttermSubMapDestination are left untouched rather than
    /// crashing -- a deliberately unhandled gap for anything beyond the
    /// final-tile case (see conversation): blocking there is assumed rare,
    /// since static obstacles only ever get destroyed (never added or
    /// moved) and allied destinations are commander-coordinated.
    /// </summary>
    private void refreshSubMap()
    {
        setSubMap();

        BattlefieldTile nextTile = tilePath[shorttermMainMapDestination + 1];
        bool nextTileIsFinal = shorttermMainMapDestination + 1 == tilePath.Length - 1;

        Vector3 newDestination = computeSubMapDestination(nextTile, nextTileIsFinal);
        Vector3[] newSubMapPath = getSubMapPath(transform.position, newDestination);

        if (newSubMapPath == null)
        {
            return;
        }

        subMapPath = newSubMapPath;
        shorttermSubMapDestination = 1; // Index 0 is just the object's own current position.
    }

    /// <summary>
    /// The real-world position of tile's formationPosition slot within its
    /// own local submap. If isFinalTileTarget and that slot is blocked,
    /// searches outward for the nearest open cell instead -- the one case
    /// where a blocked destination can't just be papered over by a later
    /// submap refresh, since there is no later refresh for the final tile.
    /// </summary>
    private Vector3 computeSubMapDestination(BattlefieldTile tile, bool isFinalTileTarget)
    {
        int clampedX = Mathf.Clamp(formationPosition.x, 0, SUBMAP_RADIUS - 1);
        int clampedY = Mathf.Clamp(formationPosition.y, 0, SUBMAP_RADIUS - 1);
        Vector2Int localOffset = new Vector2Int(clampedX, clampedY);

        if (isFinalTileTarget)
        {
            bool[][] localSubMap = tile.getSubMap();

            if (!isLocalCellPassable(localSubMap, localOffset.x, localOffset.y)
                && tryFindNearestOpenCell(localSubMap, localOffset.x, localOffset.y, out Vector2Int openCell))
            {
                localOffset = openCell;
            }
        }

        return tileLocalIndexToWorld(tile, localOffset);
    }

    /// <summary>
    /// The world position of the CENTER of localIndex within tile's own
    /// local submap (y left at 0; getSubMapPath Raycasts its own).
    /// </summary>
    private Vector3 tileLocalIndexToWorld(BattlefieldTile tile, Vector2Int localIndex)
    {
        float x = tile.x + (localIndex.x + 0.5f) / SUBMAP_RADIUS;
        float z = tile.y + (localIndex.y + 0.5f) / SUBMAP_RADIUS;
        return new Vector3(x, 0f, z);
    }

    private bool isLocalCellPassable(bool[][] localSubMap, int x, int y)
    {
        return x >= 0 && x < localSubMap.Length
            && y >= 0 && y < localSubMap[x].Length
            && localSubMap[x][y];
    }

    /// <summary>
    /// Searches outward from (startX, startY) in expanding rings (Chebyshev
    /// distance) for the nearest passable cell in localSubMap -- not
    /// necessarily the exact Euclidean-nearest cell when a ring has more
    /// than one open cell, but a cheap, simple approximation that's good
    /// enough for this. Returns false if nothing passable is found anywhere
    /// in localSubMap.
    /// </summary>
    private bool tryFindNearestOpenCell(bool[][] localSubMap, int startX, int startY, out Vector2Int result)
    {
        if (isLocalCellPassable(localSubMap, startX, startY))
        {
            result = new Vector2Int(startX, startY);
            return true;
        }

        int maxRadius = Mathf.Max(localSubMap.Length, localSubMap.Length > 0 ? localSubMap[0].Length : 0);

        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) != radius)
                    {
                        continue; // Only examine this ring's perimeter -- inner rings were already checked.
                    }

                    int x = startX + dx;
                    int y = startY + dy;

                    if (isLocalCellPassable(localSubMap, x, y))
                    {
                        result = new Vector2Int(x, y);
                        return true;
                    }
                }
            }
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Bookkeeping wrapper used only during an A* search itself -- never
    /// stored on BattlefieldTile or persisted anywhere. Shared by getPath
    /// (T = BattlefieldTile) and getSubMapPath (T = Vector2Int).
    /// </summary>
    private class AStarNode<T>
    {
        public T value;
        public AStarNode<T> parent;
        public float gScore;
        public float fScore;
        public bool inOpenSet;
    }

    /// <summary>
    /// Generic A* search shared by getPath and getSubMapPath. Returns the
    /// path from start to destination inclusive of both endpoints, or null
    /// if no path exists. getNeighbors returns each reachable neighbor of a
    /// node paired with the cost of moving there; heuristicToDestination
    /// estimates the remaining cost from a given node to destination and
    /// must stay admissible (never overestimate) for the result to be
    /// guaranteed optimal.
    /// </summary>
    private List<T> runAStar<T>(
        T start,
        T destination,
        Func<T, List<(T neighbor, float cost)>> getNeighbors,
        Func<T, float> heuristicToDestination)
    {
        if (EqualityComparer<T>.Default.Equals(start, destination))
        {
            return new List<T> { start };
        }

        Dictionary<T, AStarNode<T>> allNodes = new Dictionary<T, AStarNode<T>>();
        List<AStarNode<T>> openSet = new List<AStarNode<T>>();

        AStarNode<T> startNode = new AStarNode<T>
        {
            value = start,
            parent = null,
            gScore = 0f,
            fScore = heuristicToDestination(start),
            inOpenSet = true
        };

        openSet.Add(startNode);
        allNodes[start] = startNode;

        while (openSet.Count > 0)
        {
            // Lowest fScore in the open set. A linear scan is fine here --
            // the grid tops out at 36x36 tiles, and pathfinding doesn't run
            // every frame, so this isn't worth a dedicated priority queue.
            int bestIndex = 0;
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fScore < openSet[bestIndex].fScore)
                {
                    bestIndex = i;
                }
            }

            AStarNode<T> current = openSet[bestIndex];

            if (EqualityComparer<T>.Default.Equals(current.value, destination))
            {
                return reconstructAStarPath(current);
            }

            openSet.RemoveAt(bestIndex);
            current.inOpenSet = false;

            foreach ((T neighborValue, float moveCost) in getNeighbors(current.value))
            {
                if (!allNodes.TryGetValue(neighborValue, out AStarNode<T> neighborNode))
                {
                    neighborNode = new AStarNode<T> { value = neighborValue, gScore = float.PositiveInfinity };
                    allNodes[neighborValue] = neighborNode;
                }

                float tentativeGScore = current.gScore + moveCost;

                if (tentativeGScore >= neighborNode.gScore)
                {
                    continue;
                }

                neighborNode.parent = current;
                neighborNode.gScore = tentativeGScore;
                neighborNode.fScore = tentativeGScore + heuristicToDestination(neighborValue);

                if (!neighborNode.inOpenSet)
                {
                    neighborNode.inOpenSet = true;
                    openSet.Add(neighborNode);
                }
            }
        }

        return null; // Open set exhausted without reaching destination.
    }

    private List<T> reconstructAStarPath<T>(AStarNode<T> endNode)
    {
        List<T> path = new List<T>();
        AStarNode<T> current = endNode;

        while (current != null)
        {
            path.Add(current.value);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Finds the lowest-cost path from startTile to destination using A*,
    /// considering 8-directional movement (diagonal moves are only legal
    /// when both of their contributing orthogonal moves are also legal, per
    /// the no-corner-cutting rule -- there's no separate diagonal
    /// permission bit to check). Returns an array with startTile at index 0
    /// and destination at the final index, or null if no path exists.
    /// </summary>
    public BattlefieldTile[] getPath(BattlefieldTile startTile)
    {
        if (startTile == null || destination == null)
        {
            return null;
        }

        List<BattlefieldTile> path = runAStar(
            startTile,
            destination,
            getTraversableNeighbors,
            tile => heuristic(tile, destination));

        return path?.ToArray();
    }

    /// <summary>
    /// Returns every tile reachable from "from" in one step (up to 8
    /// directions), paired with the cost of making that move. Orthogonal
    /// legality comes directly from canExitFrom/canEnterFrom. Diagonal
    /// legality requires both full L-shaped paths around the shared corner
    /// to be open -- from -> horizontal neighbor -> diagonal tile, AND
    /// from -> vertical neighbor -> diagonal tile -- not just the first leg
    /// of each, or a wall on the second leg could be cut through diagonally.
    /// </summary>
    private List<(BattlefieldTile tile, float cost)> getTraversableNeighbors(BattlefieldTile from)
    {
        List<(BattlefieldTile, float)> neighbors = new List<(BattlefieldTile, float)>(8);

        int x = from.x;
        int y = from.y;

        bool canGoPosX = canMove(from, x + 1, y, toPosXDirection, fromNegXDirection);
        bool canGoNegX = canMove(from, x - 1, y, toNegXDirection, fromPosXDirection);
        bool canGoPosY = canMove(from, x, y + 1, toPosYDirection, fromNegYDirection);
        bool canGoNegY = canMove(from, x, y - 1, toNegYDirection, fromPosYDirection);

        if (canGoPosX)
        {
            addOrthogonalNeighbor(neighbors, x + 1, y);
        }

        if (canGoNegX)
        {
            addOrthogonalNeighbor(neighbors, x - 1, y);
        }

        if (canGoPosY)
        {
            addOrthogonalNeighbor(neighbors, x, y + 1);
        }

        if (canGoNegY)
        {
            addOrthogonalNeighbor(neighbors, x, y - 1);
        }

        // A diagonal move touches 4 tiles meeting at one shared corner, which
        // means 4 distinct wall edges surround that corner -- not just the 2
        // that canGoPosX/canGoNegX/canGoPosY/canGoNegY already check (the
        // edges directly touching "from"). The other 2 edges, between each
        // orthogonal neighbor and the diagonal tile itself, also need to be
        // open, or a wall sitting exactly on one of those could be cut
        // through diagonally. Both full L-shaped paths through the corner
        // (from -> horizontal -> diagonal, and from -> vertical -> diagonal)
        // must be entirely open for the diagonal shortcut to be legal.

        if (canGoPosX && canGoPosY)
        {
            BattlefieldTile posXNeighbor = tileMap[x + 1][y];
            BattlefieldTile posYNeighbor = tileMap[x][y + 1];

            bool cornerClear = canMove(posXNeighbor, x + 1, y + 1, toPosYDirection, fromNegYDirection)
                && canMove(posYNeighbor, x + 1, y + 1, toPosXDirection, fromNegXDirection);

            if (cornerClear)
            {
                addDiagonalNeighbor(neighbors, x + 1, y + 1);
            }
        }

        if (canGoPosX && canGoNegY)
        {
            BattlefieldTile posXNeighbor = tileMap[x + 1][y];
            BattlefieldTile negYNeighbor = tileMap[x][y - 1];

            bool cornerClear = canMove(posXNeighbor, x + 1, y - 1, toNegYDirection, fromPosYDirection)
                && canMove(negYNeighbor, x + 1, y - 1, toPosXDirection, fromNegXDirection);

            if (cornerClear)
            {
                addDiagonalNeighbor(neighbors, x + 1, y - 1);
            }
        }

        if (canGoNegX && canGoPosY)
        {
            BattlefieldTile negXNeighbor = tileMap[x - 1][y];
            BattlefieldTile posYNeighbor = tileMap[x][y + 1];

            bool cornerClear = canMove(negXNeighbor, x - 1, y + 1, toPosYDirection, fromNegYDirection)
                && canMove(posYNeighbor, x - 1, y + 1, toNegXDirection, fromPosXDirection);

            if (cornerClear)
            {
                addDiagonalNeighbor(neighbors, x - 1, y + 1);
            }
        }

        if (canGoNegX && canGoNegY)
        {
            BattlefieldTile negXNeighbor = tileMap[x - 1][y];
            BattlefieldTile negYNeighbor = tileMap[x][y - 1];

            bool cornerClear = canMove(negXNeighbor, x - 1, y - 1, toNegYDirection, fromPosYDirection)
                && canMove(negYNeighbor, x - 1, y - 1, toNegXDirection, fromPosXDirection);

            if (cornerClear)
            {
                addDiagonalNeighbor(neighbors, x - 1, y - 1);
            }
        }

        return neighbors;
    }

    private void addOrthogonalNeighbor(List<(BattlefieldTile, float)> neighbors, int x, int y)
    {
        BattlefieldTile tile = tileMap[x][y];
        neighbors.Add((tile, tile.getMovementCost(terrestrial)));
    }

    private void addDiagonalNeighbor(List<(BattlefieldTile, float)> neighbors, int x, int y)
    {
        // Guaranteed in-bounds already for a rectangular grid, since both
        // contributing orthogonal moves were already validated -- checked
        // anyway in case tileMap is ever irregular.
        if (!inBounds(x, y))
        {
            return;
        }

        BattlefieldTile tile = tileMap[x][y];
        neighbors.Add((tile, tile.getMovementCost(terrestrial) * DiagonalCostMultiplier));
    }

    private bool canMove(BattlefieldTile from, int toX, int toY, ushort exitDirection, ushort enterDirection)
    {
        if (!inBounds(toX, toY))
        {
            return false;
        }

        BattlefieldTile to = tileMap[toX][toY];
        return from.canExitFrom(terrestrial, exitDirection) && to.canEnterFrom(terrestrial, enterDirection);
    }

    private bool inBounds(int x, int y)
    {
        return x >= 0 && x < tileMap.Length && y >= 0 && y < tileMap[x].Length;
    }

    /// <summary>
    /// Octile distance: admissible for 8-directional grid movement where a
    /// diagonal step costs sqrt(2) times an orthogonal one, given the
    /// cheapest possible per-step cost is 1. Shared by getPath's and
    /// getSubMapPath's heuristics.
    /// </summary>
    private static float octileDistance(int dx, int dy)
    {
        int minDelta = Mathf.Min(dx, dy);
        int maxDelta = Mathf.Max(dx, dy);

        return maxDelta + (DiagonalCostMultiplier - 1f) * minDelta;
    }

    private float heuristic(BattlefieldTile from, BattlefieldTile to)
    {
        return octileDistance(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y));
    }

    /// <summary>
    /// Finds the shortest path across navigationSubMap from submapStart to
    /// submapDestination (real-world x/z coordinates), using A* with
    /// 8-directional movement over the boolean passability grid. Diagonal
    /// moves require both contributing orthogonal cells AND the diagonal
    /// cell itself to be passable (unlike getPath, there's no entry/exit
    /// permission data here to fold that consideration into, so it has to
    /// be checked directly).
    ///
    /// Relies on setSubMap having already been called -- subMapAnchorTile is
    /// null until then. Index 0 of the returned array is submapStart exactly
    /// as given (already has an accurate y). The final index uses
    /// submapDestination's given x/z with a freshly Raycast y (its given y
    /// is a placeholder). Every index in between is a submap cell's center
    /// position, with y found the same way. Returns null if no path exists,
    /// either endpoint falls outside navigationSubMap or on a blocked cell,
    /// or a required ground Raycast unexpectedly finds nothing.
    /// </summary>
    public Vector3[] getSubMapPath(Vector3 submapStart, Vector3 submapDestination)
    {
        if (subMapAnchorTile == null || navigationSubMap == null)
        {
            return null;
        }

        Vector2Int startIndex = worldToSubMapIndex(submapStart, subMapAnchorTile);
        Vector2Int destinationIndex = worldToSubMapIndex(submapDestination, subMapAnchorTile);

        if (!isSubMapCellPassable(startIndex.x, startIndex.y) || !isSubMapCellPassable(destinationIndex.x, destinationIndex.y))
        {
            return null;
        }

        List<Vector2Int> indexPath = runAStar(
            startIndex,
            destinationIndex,
            getTraversableSubMapNeighbors,
            index => octileDistance(Mathf.Abs(destinationIndex.x - index.x), Mathf.Abs(destinationIndex.y - index.y)));

        if (indexPath == null)
        {
            return null;
        }

        return buildSubMapWorldPath(indexPath, submapStart, submapDestination, subMapAnchorTile);
    }

    /// <summary>
    /// Rebuilds navigationSubMap from the submaps of tilePath[shorttermMainMapDestination]
    /// and tilePath[shorttermMainMapDestination + 1]. If that step is a
    /// diagonal move, also includes the two tiles that complete the 2x2
    /// square between them, for the full 4-tile case. Also updates
    /// subMapAnchorTile to whichever involved tile has the lowest x and/or y
    /// value, which is what navigationSubMap[0][0] is anchored to.
    ///
    /// Builds a 2x2 grid of which tile (if any) belongs in each quadrant
    /// slot, then copies all 4 slots in one uniform pass -- copyTileSubMap
    /// fills a slot with false itself when given a null tile, so the
    /// 2-tile (non-diagonal) case's unused half is handled in the same pass
    /// as everything else, with no separate clearing step.
    /// </summary>
    public void setSubMap()
    {
        if (tilePath == null || shorttermMainMapDestination < 0 || shorttermMainMapDestination + 1 >= tilePath.Length)
        {
            return;
        }

        BattlefieldTile tileA = tilePath[shorttermMainMapDestination];
        BattlefieldTile tileB = tilePath[shorttermMainMapDestination + 1];

        if (tileA == null || tileB == null)
        {
            return;
        }

        int anchorX = Mathf.Min(tileA.x, tileB.x);
        int anchorY = Mathf.Min(tileA.y, tileB.y);

        if (!inBounds(anchorX, anchorY))
        {
            return;
        }

        subMapAnchorTile = tileMap[anchorX][anchorY];

        bool isDiagonalStep = tileA.x != tileB.x && tileA.y != tileB.y;

        // Which tile (if any) occupies each of the 4 possible quadrant
        // slots, relative to the anchor. Slots left null represent quadrants
        // with no real tile backing them.
        BattlefieldTile[,] slotTiles = new BattlefieldTile[2, 2];
        slotTiles[tileA.x - anchorX, tileA.y - anchorY] = tileA;
        slotTiles[tileB.x - anchorX, tileB.y - anchorY] = tileB;

        if (isDiagonalStep)
        {
            slotTiles[tileA.x - anchorX, tileB.y - anchorY] = getTileSafe(tileA.x, tileB.y);
            slotTiles[tileB.x - anchorX, tileA.y - anchorY] = getTileSafe(tileB.x, tileA.y);
        }

        for (int offsetX = 0; offsetX <= 1; offsetX++)
        {
            for (int offsetY = 0; offsetY <= 1; offsetY++)
            {
                copyTileSubMap(slotTiles[offsetX, offsetY], offsetX, offsetY);
            }
        }
    }

    private BattlefieldTile getTileSafe(int x, int y)
    {
        return inBounds(x, y) ? tileMap[x][y] : null;
    }

    /// <summary>
    /// Copies tile's own SUBMAP_RADIUS x SUBMAP_RADIUS submap into the
    /// navigationSubMap quadrant at the given tile-offset (0 or 1 in each
    /// dimension, relative to the anchor). If tile is null (an absent
    /// quadrant, e.g. the unused half in the 2-tile non-diagonal case),
    /// fills that quadrant with false instead.
    /// </summary>
    private void copyTileSubMap(BattlefieldTile tile, int tileOffsetX, int tileOffsetY)
    {
        bool[][] tileSubMap = tile?.getSubMap();

        int offsetX = tileOffsetX * SUBMAP_RADIUS;
        int offsetY = tileOffsetY * SUBMAP_RADIUS;

        for (int i = 0; i < SUBMAP_RADIUS; i++)
        {
            for (int j = 0; j < SUBMAP_RADIUS; j++)
            {
                navigationSubMap[offsetX + i][offsetY + j] = tileSubMap != null && tileSubMap[i][j];
            }
        }
    }

    /// <summary>
    /// Converts a real-world x/z position into the navigationSubMap cell
    /// index that contains it, anchored so that navigationSubMap[0][0]
    /// corresponds to (anchorTile.x, anchorTile.y) in world space, with each
    /// cell spanning 1/SUBMAP_RADIUS world units.
    /// </summary>
    private Vector2Int worldToSubMapIndex(Vector3 worldPosition, BattlefieldTile anchorTile)
    {
        int i = Mathf.FloorToInt((worldPosition.x - anchorTile.x) * SUBMAP_RADIUS);
        int j = Mathf.FloorToInt((worldPosition.z - anchorTile.y) * SUBMAP_RADIUS);
        return new Vector2Int(i, j);
    }

    /// <summary>
    /// The inverse of worldToSubMapIndex -- the world position of the CENTER
    /// of the given cell (y left at 0; callers fill it in via Raycast).
    /// </summary>
    private Vector3 subMapIndexToWorld(Vector2Int index, BattlefieldTile anchorTile)
    {
        float x = anchorTile.x + (index.x + 0.5f) / SUBMAP_RADIUS;
        float z = anchorTile.y + (index.y + 0.5f) / SUBMAP_RADIUS;
        return new Vector3(x, 0f, z);
    }

    private bool isSubMapCellPassable(int x, int y)
    {
        return x >= 0 && x < navigationSubMap.Length
            && y >= 0 && y < navigationSubMap[x].Length
            && navigationSubMap[x][y];
    }

    /// <summary>
    /// Up to 8 neighboring cells reachable from "from", each costing 1 for
    /// an orthogonal step or sqrt(2) for a diagonal one (there's no
    /// per-cell cost data here, just passable/blocked). A diagonal
    /// neighbor requires both orthogonal cells that make up its corner AND
    /// the diagonal cell itself to be passable.
    /// </summary>
    private List<(Vector2Int index, float cost)> getTraversableSubMapNeighbors(Vector2Int from)
    {
        List<(Vector2Int, float)> neighbors = new List<(Vector2Int, float)>(8);

        bool canGoPosX = isSubMapCellPassable(from.x + 1, from.y);
        bool canGoNegX = isSubMapCellPassable(from.x - 1, from.y);
        bool canGoPosY = isSubMapCellPassable(from.x, from.y + 1);
        bool canGoNegY = isSubMapCellPassable(from.x, from.y - 1);

        if (canGoPosX)
        {
            neighbors.Add((new Vector2Int(from.x + 1, from.y), 1f));
        }

        if (canGoNegX)
        {
            neighbors.Add((new Vector2Int(from.x - 1, from.y), 1f));
        }

        if (canGoPosY)
        {
            neighbors.Add((new Vector2Int(from.x, from.y + 1), 1f));
        }

        if (canGoNegY)
        {
            neighbors.Add((new Vector2Int(from.x, from.y - 1), 1f));
        }

        if (canGoPosX && canGoPosY && isSubMapCellPassable(from.x + 1, from.y + 1))
        {
            neighbors.Add((new Vector2Int(from.x + 1, from.y + 1), DiagonalCostMultiplier));
        }

        if (canGoPosX && canGoNegY && isSubMapCellPassable(from.x + 1, from.y - 1))
        {
            neighbors.Add((new Vector2Int(from.x + 1, from.y - 1), DiagonalCostMultiplier));
        }

        if (canGoNegX && canGoPosY && isSubMapCellPassable(from.x - 1, from.y + 1))
        {
            neighbors.Add((new Vector2Int(from.x - 1, from.y + 1), DiagonalCostMultiplier));
        }

        if (canGoNegX && canGoNegY && isSubMapCellPassable(from.x - 1, from.y - 1))
        {
            neighbors.Add((new Vector2Int(from.x - 1, from.y - 1), DiagonalCostMultiplier));
        }

        return neighbors;
    }

    /// <summary>
    /// Converts an index-path into world-space Vector3s: submapStart exactly
    /// at index 0, submapDestination's x/z (with a fresh Raycast y) at the
    /// final index, and Raycast-grounded cell-center positions for anything
    /// strictly in between. Returns null if any required Raycast fails.
    /// </summary>
    private Vector3[] buildSubMapWorldPath(List<Vector2Int> indexPath, Vector3 submapStart, Vector3 submapDestination, BattlefieldTile anchorTile)
    {
        int intermediateCount = Mathf.Max(indexPath.Count - 2, 0);
        Vector3[] worldPath = new Vector3[intermediateCount + 2];

        worldPath[0] = submapStart;

        for (int i = 0; i < intermediateCount; i++)
        {
            Vector2Int cellIndex = indexPath[i + 1]; // Skip indexPath[0], which corresponds to the start cell.
            Vector3 cellCenter = subMapIndexToWorld(cellIndex, anchorTile);

            if (!tryRaycastGroundHeight(cellCenter.x, cellCenter.z, out float groundY))
            {
                return null;
            }

            cellCenter.y = groundY;
            worldPath[i + 1] = cellCenter;
        }

        if (!tryRaycastGroundHeight(submapDestination.x, submapDestination.z, out float destinationY))
        {
            return null;
        }

        worldPath[worldPath.Length - 1] = new Vector3(submapDestination.x, destinationY, submapDestination.z);

        return worldPath;
    }

    private bool tryRaycastGroundHeight(float worldX, float worldZ, out float groundY)
    {
        Vector3 origin = new Vector3(worldX, ARBITRARY_HIGH_RAYCAST_START_HEIGHT, worldZ);

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, Mathf.Infinity, traversableLayer))
        {
            groundY = hit.point.y;
            return true;
        }

        groundY = 0f;
        return false;
    }
}
