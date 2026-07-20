using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomPhysics))]
public class BFNavigator : MonoBehaviour
{
    [SerializeField] private LayerMask tileLayer;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float stoppingDistance;
    [SerializeField] private bool terrestrial;
    [SerializeField] private LayerMask traversableLayer;
    private int shortTimeMainMapDest;
    private int shortTimeSubMapDest;
    private BattlefieldTile destination;
    private BattlefieldTile[] tilePath;
    private BattlefieldTile[][] tileMap;
    private Vector3[] subMapPath; //Represents actual world space
    private bool[][] navigationSubMap;
    private float formationPositionX;
    private float formationPositionY;
    private Vector3 myPosition;
    private Vector3 globalSubMapOffset;
    private bool moving;

    private CustomPhysics physics;

    public const int SUBMAP_RADIUS = 7;
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
        this.formationPositionX = formationPositionX;
        this.formationPositionY = formationPositionY;
        navigationSubMap = new bool[Mathf.RoundToInt(SUBMAP_RADIUS * 2)][];
        globalSubMapOffset = new Vector3(tileMap[0][0].x, tileMap[0][0].y);
        for (int q = 0; q < navigationSubMap.Length; q++)
        {
            navigationSubMap[q] = new bool[Mathf.RoundToInt(SUBMAP_RADIUS * 2)];
        }
        physics = GetComponent<CustomPhysics>();
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

        myPosition = transform.position;

        tilePath = getPath(worldCoordinatesToBattlefieldTile(myPosition));

        setSubMapAndSubPath();
    }
    public BattlefieldTile getDestination()
    {
        return destination;
    }
    public bool reachedDestination()
    {
        return tilePath == null || shortTimeMainMapDest == tilePath.Length - 1;
    }
    public bool reachedSubMapOfNextBFTile()
    {
        return reachedDestination()
            || worldCoordinatesToBattlefieldTile(subMapPath[shortTimeSubMapDest]) == tilePath[shortTimeMainMapDest];
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
    private BattlefieldTile[] getPath(BattlefieldTile startTile)
    {
        List<BattlefieldTile> open = new List<BattlefieldTile>();
        List<BattlefieldTile> closed = new List<BattlefieldTile>();
        Dictionary<BattlefieldTile, float> f = new Dictionary<BattlefieldTile, float>();
        Dictionary<BattlefieldTile, float> g = new Dictionary<BattlefieldTile, float>();
        Dictionary<BattlefieldTile, float> pos = new Dictionary<BattlefieldTile, float>();
        Dictionary<BattlefieldTile, BattlefieldTile> parent = new Dictionary<BattlefieldTile, BattlefieldTile>();

        open.Add(startTile);
        f.Add(startTile, 0);
        g.Add(startTile, 0);
        pos.Add(startTile, 0);
        while (open.Count > 0)
        {
            BattlefieldTile check = open[0];
            for (int idx = 1; idx < open.Count; idx++)
            {
                if (f[check] > f[open[idx]])
                {
                    check = open[idx];
                }
            }
            open.Remove(check);
            bool posY = false;
            bool negY = false;
            bool posX = false;
            bool negX = false;
            BattlefieldTile posXTile = null;
            BattlefieldTile posYTile = null;
            BattlefieldTile negXTile = null;
            BattlefieldTile negYTile = null;
            if (check.x > 0)
            {
                negXTile = tileMap[check.x - 1][check.y];
                if (negXTile == destination)
                {
                    parent.Add(negXTile, check);
                    return interpretPath(parent, startTile);
                }
                negX = checkConnection(check, negXTile, parent, open, closed, pos, g, f,
                    toNegXDirection, fromPosXDirection);
            }
            if (check.x < tileMap.Length - 1)
            {
                posXTile = tileMap[check.x + 1][check.y];
                if (posXTile == destination)
                {
                    parent.Add(posXTile, check);
                    return interpretPath(parent, startTile);
                }
                posX = checkConnection(check, posXTile, parent, open, closed, pos, g, f,
                    toPosXDirection, fromNegXDirection);
            }
            if (check.y > 0)
            {
                negYTile = tileMap[check.x][check.y - 1];
                if (negYTile == destination)
                {
                    parent.Add(negYTile, check);
                    return interpretPath(parent, startTile);
                }
                negY = checkConnection(check, negYTile, parent, open, closed, pos, g, f,
                    toNegYDirection, fromPosYDirection);
            }
            if (check.y < tileMap[0].Length - 1)
            {
                posYTile = tileMap[check.x][check.y + 1];
                if (posYTile == destination)
                {
                    parent.Add(posYTile, check);
                    return interpretPath(parent, startTile);
                }
                posY = checkConnection(check, posYTile, parent, open, closed, pos, g, f,
                    toPosYDirection, fromNegYDirection);
            }
            if (posX && posY)
            {
                BattlefieldTile child = tileMap[check.x + 1][check.y + 1];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                checkDiagonalConnection(posXTile, posYTile, check, child, parent, open, closed, pos, g, f,
                    toPosXDirection, fromNegXDirection,
                    toPosYDirection, fromNegYDirection);
            }
            if (posX && negY)
            {
                BattlefieldTile child = tileMap[check.x + 1][check.y - 1];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                checkDiagonalConnection(posXTile, negYTile, check, child, parent, open, closed, pos, g, f,
                    toPosXDirection, fromNegXDirection,
                    toNegYDirection, fromPosYDirection);
            }
            if (negX && posY)
            {
                BattlefieldTile child = tileMap[check.x - 1][check.y + 1];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                checkDiagonalConnection(negXTile, posYTile, check, child, parent, open, closed, pos, g, f,
                    toNegXDirection, fromPosXDirection,
                    toPosYDirection, fromNegYDirection);
            }
            if (negX && negY)
            {
                BattlefieldTile child = tileMap[check.x - 1][check.y - 1];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                checkDiagonalConnection(negXTile, negYTile, check, child, parent, open, closed, pos, g, f,
                    toNegXDirection, fromPosXDirection,
                    toNegYDirection, fromPosYDirection);
            }
            closed.Add(check);
        }
        return interpretPath(parent, startTile);
    }
    private bool checkConnection(BattlefieldTile check, BattlefieldTile child,
        Dictionary<BattlefieldTile, BattlefieldTile> parent,
        List<BattlefieldTile> open, List<BattlefieldTile> closed,
        Dictionary<BattlefieldTile, float> pos, Dictionary<BattlefieldTile, float> g,
        Dictionary<BattlefieldTile, float> f, ushort movementToDirection, ushort movementFromDirection)
    {
        if (closed.Contains(child)
            || !check.canExitFrom(terrestrial, movementFromDirection)
            || !child.canEnterFrom(terrestrial, movementToDirection))
        {
            return false;
        }
        addToDictionary(child, pos[check] + 1, pos);
        addToDictionary(child, g[check] + child.getType().getMoveCost(terrestrial), g);
        float h = Mathf.Sqrt(Mathf.Pow(child.x - check.x, 2) + Mathf.Pow(child.y - check.y, 2));
        float calculateF = g[child] + h;
        if (!f.ContainsKey(child) || f[child] > calculateF)
        {
            open.Add(child);
            addToDictionary(child, check, parent);
            addToDictionary(child, calculateF, f);
        }
        return true;
    }
    private bool checkDiagonalConnection(BattlefieldTile checkX, BattlefieldTile checkY,
        BattlefieldTile checkXY, BattlefieldTile child,
        Dictionary<BattlefieldTile, BattlefieldTile> parent,
        List<BattlefieldTile> open, List<BattlefieldTile> closed,
        Dictionary<BattlefieldTile, float> pos, Dictionary<BattlefieldTile, float> g,
        Dictionary<BattlefieldTile, float> f, ushort movementToXDirection, ushort movementFromXDirection,
        ushort movementToYDirection, ushort movementFromYDirection)
    {
        if (closed.Contains(child)
            || !checkX.canExitFrom(terrestrial, movementFromXDirection)
            || !child.canEnterFrom(terrestrial, movementToXDirection)
            || !checkY.canExitFrom(terrestrial, movementFromYDirection)
            || !child.canEnterFrom(terrestrial, movementToYDirection))
        {
            return false;
        }
        addToDictionary(child, pos[checkXY] + 1, pos);
        addToDictionary(child, g[checkXY] + child.getType().getMoveCost(terrestrial), g);
        float h = Mathf.Sqrt(Mathf.Pow(child.x - checkXY.x, 2) + Mathf.Pow(child.y - checkXY.y, 2));
        float calculateF = g[child] + h;
        if (!f.ContainsKey(child) || f[child] > calculateF)
        {
            open.Add(child);
            addToDictionary(child, checkXY, parent);
            addToDictionary(child, calculateF, f);
        }
        return true;
    }
    private BattlefieldTile[] interpretPath(Dictionary<BattlefieldTile, BattlefieldTile> pathData, BattlefieldTile startTile)
    {
        BattlefieldTile current = destination;
        List<BattlefieldTile> backwards = new List<BattlefieldTile>();
        while (current != startTile)
        {
            backwards.Add(current);
            current = pathData[current];
        }
        backwards.Add(startTile);
        BattlefieldTile[] ret = new BattlefieldTile[backwards.Count];
        for (int q = backwards.Count - 1; q >= 0; q--)
        {
            ret[(backwards.Count - 1) - q] = backwards[q];
        }
        shortTimeMainMapDest = 0;
        return ret;
    }

    private void addToDictionary(BattlefieldTile key, BattlefieldTile value, Dictionary<BattlefieldTile, BattlefieldTile> dictionary)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = value;
        }
        else
        {
            dictionary.Add(key, value);
        }
    }
    private void addToDictionary(BattlefieldTile key, float value, Dictionary<BattlefieldTile, float> dictionary)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = value;
        }
        else
        {
            dictionary.Add(key, value);
        }
    }
    private void addToDictionary(Vector3 key, Vector3 value, Dictionary<Vector3, Vector3> dictionary)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = value;
        }
        else
        {
            dictionary.Add(key, value);
        }
    }
    private void addToDictionary(Vector3 key, float value, Dictionary<Vector3, float> dictionary)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] = value;
        }
        else
        {
            dictionary.Add(key, value);
        }
    }
    private Vector3[] setSubMapPath(Vector3 subMapStartIdx, Vector3 subMapDestinationIdx)
    {
        List<Vector3> open = new List<Vector3>();
        List<Vector3> closed = new List<Vector3>();
        Dictionary<Vector3, float> f = new Dictionary<Vector3, float>();
        Dictionary<Vector3, float> g = new Dictionary<Vector3, float>();
        Dictionary<Vector3, float> pos = new Dictionary<Vector3, float>();
        Dictionary<Vector3, Vector3> parent = new Dictionary<Vector3, Vector3>();

        open.Add(subMapStartIdx);
        f.Add(subMapStartIdx, 0);
        g.Add(subMapStartIdx, 0);
        pos.Add(subMapStartIdx, 0);
        while (open.Count > 0)
        {
            Vector3 check = open[0];
            for (int idx = 1; idx < open.Count; idx++)
            {
                if (f[check] > f[open[idx]])
                {
                    check = open[idx];
                }
            }
            open.Remove(check);
            bool posY = false;
            bool negY = false;
            bool posX = false;
            bool negX = false;

            if (check.x > 0)
            {
                Vector3 child = new Vector3(check.x - 1, 0, check.z);
                negX = checkBooleanConnection(check, child, parent, open, closed, pos, g, f);
            }
            if (check.x < navigationSubMap.Length - 1)
            {
                Vector3 child = new Vector3(check.x + 1, 0, check.z);
                posX = checkBooleanConnection(check, child, parent, open, closed, pos, g, f);
            }
            if (check.z > 0)
            {
                Vector3 child = new Vector3(check.x, 0, check.z - 1);
                negY = checkBooleanConnection(check, child, parent, open, closed, pos, g, f);
            }
            if (check.z < navigationSubMap[0].Length - 1)
            {
                Vector3 child = new Vector3(check.x, 0, check.z + 1);
                posY = checkBooleanConnection(check, child, parent, open, closed, pos, g, f);
            }
            if (posX && posY)
            {
                Vector3 child = new Vector3(check.x + 1, 0, check.z + 1);
                checkBooleanConnection(check, child, parent, open, closed, pos, g, f);
            }
            if (posX && negY)
            {
                Vector3 child = new Vector3(check.x + 1, 0, check.z - 1);
                checkBooleanConnection(check, child, parent, open, closed, pos, g, f);
            }
            if (negX && posY)
            {
                Vector3 child = new Vector3(check.x - 1, 0, check.z + 1);
                checkBooleanConnection(check, child, parent, open, closed, pos, g, f);
            }
            if (negX && negY)
            {
                Vector3 child = new Vector3(check.x - 1, 0, check.z - 1);
                checkBooleanConnection(check, child, parent, open, closed, pos, g, f);
            }
            closed.Add(check);
        }
        return interpretSubMapPath(subMapDestinationIdx, subMapStartIdx, parent);
    }
    private bool checkBooleanConnection(Vector3 check, Vector3 child, Dictionary<Vector3, Vector3> parent,
        List<Vector3> open, List<Vector3> closed,
        Dictionary<Vector3, float> pos, Dictionary<Vector3, float> g,
        Dictionary<Vector3, float> f)
    {
        if (closed.Contains(child)
            || !navigationSubMap[Mathf.RoundToInt(child.x)][Mathf.RoundToInt(child.z)])
        {
            return false;
        }
        addToDictionary(child, pos[check] + 1, pos);
        addToDictionary(child, g[check] + 1, g);
        float h = Mathf.Sqrt(Mathf.Pow(child.x - check.x, 2) + Mathf.Pow(child.z - check.z, 2));
        float calculateF = g[child] + h;
        if (!f.ContainsKey(child) || f[child] > calculateF)
        {
            open.Add(child);
            addToDictionary(child, check, parent);
            addToDictionary(child, calculateF, f);
        }
        return true;
    }
    private Vector3[] interpretSubMapPath(Vector3 destination, Vector3 startPos,
        Dictionary<Vector3, Vector3> pathData)
    {
        Vector3 current = destination;
        List<Vector3> backwards = new List<Vector3>();
        while (current != startPos)
        {
            backwards.Add(current);
            current = pathData[current];
        }
        backwards.Add(startPos);
        Vector3[] ret = new Vector3[backwards.Count];
        for (int q = backwards.Count - 1; q >= 0; q--)
        {
            ret[(backwards.Count - 1) - q] = translateSubMapCoordsToBattlefieldCoords(backwards[q]);
        }
        shortTimeSubMapDest = 0;
        return ret;
    }
    private Vector3 translateSubMapCoordsToBattlefieldCoords(Vector3 coords)
    {
        Vector3 ret = new Vector3(
            ((coords.x + 0.5f) * SUBMAP_RADIUS) + globalSubMapOffset.x,
            BattlefieldGenerator.ARBITRARY_HIGH_RAYCAST_START_HEIGHT,
            ((coords.z + 0.5f) * SUBMAP_RADIUS) + globalSubMapOffset.z
            );
        RaycastHit hit;
        if (Physics.Raycast(ret, Vector3.down,
            out hit, float.MaxValue, traversableLayer))
        {
            ret.y = hit.point.y;
        }
        Debug.LogError("Hit a point that was not on the map");
        return ret;
    }
    private Vector3 translateBattlefieldCoordsToSubMapCoords(Vector3 coords)
    {
        Vector3 ret = transform.position;
        ret.x = Mathf.FloorToInt((coords.x - globalSubMapOffset.x) / SUBMAP_RADIUS);
        ret.y = 0;
        ret.x = Mathf.FloorToInt((coords.z - globalSubMapOffset.z) / SUBMAP_RADIUS);
        return ret;
    }
    private BattlefieldTile worldCoordinatesToBattlefieldTile(Vector3 coords)
    {
        return tileMap[Mathf.FloorToInt(coords.x)][Mathf.FloorToInt(coords.z)];
    }
    private void setSubMapAndSubPath()
    {
        if (shortTimeMainMapDest == tilePath.Length - 1)
        {
            setSubMapQuadrant(destination.getSubMap(), 0, 0);
            setSubMapQuadrant(null, 0, SUBMAP_RADIUS);
            setSubMapQuadrant(null, SUBMAP_RADIUS, 0);
            setSubMapQuadrant(null, SUBMAP_RADIUS, SUBMAP_RADIUS);

            Vector3 subMapDestinationIdx = translateBattlefieldCoordsToSubMapCoords(
            new Vector3(destination.x, 0, destination.y)
            + new Vector3(formationPositionX, 0, formationPositionY));

            setSubMapPath(translateBattlefieldCoordsToSubMapCoords(transform.position), subMapDestinationIdx);
        }
        else
        {
            BattlefieldTile here = tilePath[shortTimeMainMapDest];
            BattlefieldTile next = tilePath[shortTimeMainMapDest + 1];
            BattlefieldTile posXposY = null;
            BattlefieldTile posXnegY = null;
            BattlefieldTile negXposY = null;
            BattlefieldTile negXnegY = null;
            if (next.x == here.x)
            {
                if (next.y > here.y)
                {
                    negXposY = next;
                    negXnegY = here;
                }
                else
                {
                    negXposY = here;
                    negXnegY = next;
                }
            }
            else if (next.y == here.y)
            {
                if (next.x > here.x)
                {
                    posXnegY = here;
                    negXnegY = next;
                }
                else
                {
                    posXnegY = next;
                    negXnegY = here;
                }
            }
            else
            {
                negXnegY = tileMap[Mathf.Min(here.x, next.x)][Mathf.Min(here.y, next.y)];
                posXnegY = tileMap[Mathf.Max(here.x, next.x)][Mathf.Min(here.y, next.y)];
                negXposY = tileMap[Mathf.Min(here.x, next.x)][Mathf.Max(here.y, next.y)];
                posXposY = tileMap[Mathf.Max(here.x, next.x)][Mathf.Max(here.y, next.y)];
            }
            setSubMapQuadrant(negXnegY.getSubMap(), 0, 0);
            setSubMapQuadrant(negXposY.getSubMap(), 0, SUBMAP_RADIUS);
            setSubMapQuadrant(posXnegY.getSubMap(), SUBMAP_RADIUS, 0);
            setSubMapQuadrant(posXposY.getSubMap(), SUBMAP_RADIUS, SUBMAP_RADIUS);

            Vector3 subMapDestinationIdx = translateBattlefieldCoordsToSubMapCoords(
                new Vector3(next.x, 0, next.y)
                + new Vector3(formationPositionX, 0, formationPositionY));
            setSubMapPath(
                translateBattlefieldCoordsToSubMapCoords(transform.position),
                subMapDestinationIdx);
        }
    }
    private void setSubMapQuadrant(bool[][] tileSubMap, int xOffset, int yOffset)
    {
        if (tileSubMap == null)
        {
            for (int q = xOffset; q < SUBMAP_RADIUS + xOffset; q++)
            {
                for (int w = yOffset; w < SUBMAP_RADIUS + yOffset; w++)
                {
                    navigationSubMap[q + xOffset][w + yOffset] = false;
                }
            }
            return;
        }
        for (int q = 0; q < tileSubMap.Length; q++)
        {
            for (int w = 0; w < tileSubMap[q].Length; w++)
            {
                navigationSubMap[q + xOffset][w + yOffset] = tileSubMap[q][w];
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        //If you're on your destination battlefieldTile, move to your formation position
        //When you enter a battlefieldTile that is NOT your destination (including when you start a new tilePath)
        //Set your subMap path to move towards your formation position on your NEXT battlefieldTile (regardless of
        //whether you've arrived at the one on THIS tile. That no-longer matters), taking into consideration
        //the submaps of only this tile and the next if moving adjacently, or including this tile, the next,
        //and both relevant adjacent tiles if moving diagonally
        if (moving)
        {
            if (reachedDestination())
            {
                if (reachedFormationPosition())
                {
                    tilePath = null;
                    moving = false;
                }
                else
                {
                    move();
                }
            }
            else if (reachedSubMapOfNextBFTile())
            {
                shortTimeMainMapDest++;

                setSubMapAndSubPath();
            }
            else
            {
                move();
            }
        }
    }
    private void move()
    {
        Vector3 target = subMapPath[shortTimeSubMapDest];
        Vector3 forward = target - transform.position;
        physics.Move(forward.normalized * movementSpeed);
        transform.rotation = Quaternion.LookRotation(forward);
        if ((transform.position - subMapPath[shortTimeSubMapDest]).magnitude <= stoppingDistance)
        {
            shortTimeSubMapDest++;
        }
    }
}
