using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CustomPhysics))]
public class BFNavigator : MonoBehaviour
{
    public static float TILE_SIZE = 1;

    [SerializeField] private LayerMask tileLayer;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float acceptableDistance;
    [SerializeField] private bool terrestrial;
    [SerializeField] private LayerMask traversableLayer;
    private int shortTimeMainMapDest;
    private BattlefieldTile destination;
    private BattlefieldTile[] tilePath;
    private BattlefieldTile[][] tileMap;
    private Vector3[] subMapPath; //Represents actual world space
    private short currentSubMapPositionX = -1;
    private short currentSubMapPositionY = -1;
    private float formationPositionX;
    private float formationPositionY;
    private bool moving;

    public const float SUBMAP_RADIUS = 7;
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
    }

    public void setSpeed(float movementSpeed)
    {
        this.movementSpeed = movementSpeed;
    }

    public void setDestination(Collider agentCollider)
    {
        RaycastHit hit;
        Physics.Raycast(agentCollider.bounds.center, Vector3.down, out hit,
            agentCollider.bounds.extents.y, tileLayer);
        if (hit.collider == null)
        {
            Debug.Log($"This agent is not on the map. Coords: {transform.position}");
            return;
        }

        Tile here = hit.collider.GetComponent<Tile>();
        Vector3 coords = hit.point;

        currentSubMapPositionX = (short)Mathf.FloorToInt((coords.x - here.x) * SUBMAP_RADIUS);
        currentSubMapPositionY = (short)Mathf.FloorToInt((coords.z - here.y) * SUBMAP_RADIUS);

        shortTimeMainMapDest = 0;
        tilePath = getPath((BattlefieldTile)here.tile);
    }
    public BattlefieldTile getDestination()
    {
        return destination;
    }

    public bool reachedDestination()
    {
        return tilePath == null || shortTimeMainMapDest == tilePath.Length;
    }

    public void setActive(bool active)
    {
        moving = active;
    }
    public bool isActive()
    {
        return moving;
    }

    public void setStoppingDistance(float distance)
    {
        acceptableDistance = distance;
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
            if (check.y < tileMap.Length - 1)
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

    private Vector3[] setSubMapPath(bool[][] currentNavigationSubMap, Vector3 subMapGlobalOffset,
        Vector3 subMapStartIdx, Vector3 subMapDestinationIdx)
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
                negX = checkBooleanConnection(check, child, parent, open, closed, pos, g, f, currentNavigationSubMap);
            }
            if (check.x < currentNavigationSubMap.Length - 1)
            {
                Vector3 child = new Vector3(check.x + 1, 0, check.z);
                posX = checkBooleanConnection(check, child, parent, open, closed, pos, g, f, currentNavigationSubMap);
            }
            if (check.z > 0)
            {
                Vector3 child = new Vector3(check.x, 0, check.z - 1);
                negY = checkBooleanConnection(check, child, parent, open, closed, pos, g, f, currentNavigationSubMap);
            }
            if (check.z < currentNavigationSubMap.Length - 1)
            {
                Vector3 child = new Vector3(check.x, 0, check.z + 1);
                posY = checkBooleanConnection(check, child, parent, open, closed, pos, g, f, currentNavigationSubMap);
            }
            if (posX && posY)
            {
                Vector3 child = new Vector3(check.x + 1, 0, check.z + 1);
                checkBooleanConnection(check, child, parent, open, closed, pos, g, f, currentNavigationSubMap);
            }
            if (posX && negY)
            {
                Vector3 child = new Vector3(check.x + 1, 0, check.z - 1);
                checkBooleanConnection(check, child, parent, open, closed, pos, g, f, currentNavigationSubMap);
            }
            if (negX && posY)
            {
                Vector3 child = new Vector3(check.x - 1, 0, check.z + 1);
                checkBooleanConnection(check, child, parent, open, closed, pos, g, f, currentNavigationSubMap);
            }
            if (negX && negY)
            {
                Vector3 child = new Vector3(check.x - 1, 0, check.z - 1);
                checkBooleanConnection(check, child, parent, open, closed, pos, g, f, currentNavigationSubMap);
            }
            closed.Add(check);
        }
        return interpretSubMapPath(subMapDestinationIdx, subMapStartIdx, subMapGlobalOffset, parent);
    }

    private bool checkBooleanConnection(Vector3 check, Vector3 child, Dictionary<Vector3, Vector3> parent,
        List<Vector3> open, List<Vector3> closed,
        Dictionary<Vector3, float> pos, Dictionary<Vector3, float> g,
        Dictionary<Vector3, float> f, bool[][] map)
    {
        if (closed.Contains(child)
            || !map[Mathf.RoundToInt(child.x)][Mathf.RoundToInt(child.z)])
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

    private Vector3[] interpretSubMapPath(Vector3 destination, Vector3 startPos, Vector3 subMapGlobalOffset,
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
            ret[(backwards.Count - 1) - q] = translateSubMapCoordsToBattlefieldCoords(backwards[q], subMapGlobalOffset);
        }
        return ret;
    }

    private Vector3 translateSubMapCoordsToBattlefieldCoords(Vector3 coords, Vector3 offset)
    {
        Vector3 ret = new Vector3(
            ((coords.x + 0.5f) * SUBMAP_RADIUS) + offset.x,
            BattlefieldGenerator.ARBITRARY_HIGH_RAYCAST_START_HEIGHT,
            ((coords.z + 0.5f) * SUBMAP_RADIUS) + offset.z
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
    private Vector3 translateBattlefieldCoordsToSubMapCoords(Vector3 coords, Vector3 offset)
    {
        Vector3 ret = transform.position;
        ret.x = Mathf.FloorToInt((coords.x - offset.x) * SUBMAP_RADIUS);
        ret.y = 0;
        ret.x = Mathf.FloorToInt((coords.z - offset.z) * SUBMAP_RADIUS);
        return ret;
    }

    // Update is called once per frame
    void Update()
    {
        //TODO If you're on your destination battlefieldTile, move to your formation position
        //When you enter a battlefieldTile that is NOT your destination (including when you start a new tilePath)
        //Set your subMap path to move towards your formation position on your NEXT battlefieldTile (regardless of
        //whether you've arrived at the one on THIS tile. That no-longer matters), taking into consideration
        //the submaps of only this tile and the next if moving adjacently, or including this tile, the next,
        //and both relevant adjacent tiles if moving diagonally
        /*
        if (moving && !reachedDestination())
        {
            if ((transform.position - path[shortTimeDest].transform.position).magnitude <= acceptableDistance)
            {
                shortTimeDest++;
            }
            else
            {
                Vector3 direction = (path[shortTimeDest].transform.position - transform.position).normalized;
                Vector3 movement = direction * movementSpeed * StaticData.deltaTime();
                transform.Translate(movement);
                Quaternion look = Quaternion.LookRotation(-direction);
                look = Quaternion.Euler(0, look.eulerAngles.y, 0);
                Quaternion.Lerp(transform.rotation, look, rotationSpeed * StaticData.deltaTime());
            }
        }
        */
    }
}
