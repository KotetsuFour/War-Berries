using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BFNavigator : MonoBehaviour
{
    public static float TILE_SIZE = 1;

    [SerializeField] private LayerMask tileLayer;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float acceptableDistance;
    [SerializeField] private bool terrestrial;
    private int shortTimeDest;
    private BattlefieldTile destination;
    private BattlefieldTile[] path;
    private BattlefieldTile[][] tileMap;
    private short subMapDestinationX;
    private short subMapDestinationY;
    private short currentSubMapPositionX = -1;
    private short currentSubMapPositionY = -1;
    private float formationPositionX;
    private float formationPositionY;
    private bool moving;

    public const float SUBMAP_RADIUS = 7;
    public const ushort posXDirection = 0b_1000_0000;
    public const ushort negXDirection = 0b_0100_0000;
    public const ushort posYDirection = 0b_0010_0000;
    public const ushort negYDirection = 0b_0001_0000;



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
        currentSubMapPositionY = (short)Mathf.FloorToInt((coords.y - here.y) * SUBMAP_RADIUS);

        shortTimeDest = 0;
        path = getPath((BattlefieldTile)here.tile);
    }
    public BattlefieldTile getDestination()
    {
        return destination;
    }

    public bool reachedDestination()
    {
        return path == null || shortTimeDest == path.Length;
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
            if (check.x > 0)
            {
                BattlefieldTile child = tileMap[check.x - 1][check.y];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                negX = checkConnection(check, child, parent, open, closed, pos, g, f,
                    negXDirection);
            }
            if (check.x < tileMap.Length - 1)
            {
                BattlefieldTile child = tileMap[check.x + 1][check.y];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                posX = checkConnection(check, child, parent, open, closed, pos, g, f,
                    posXDirection);
            }
            if (check.y > 0)
            {
                BattlefieldTile child = tileMap[check.x][check.y - 1];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                negY = checkConnection(check, child, parent, open, closed, pos, g, f,
                    negYDirection);
            }
            if (check.y < tileMap.Length - 1)
            {
                BattlefieldTile child = tileMap[check.x][check.y + 1];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                posY = checkConnection(check, child, parent, open, closed, pos, g, f,
                    posYDirection);
            }
            if (posX && posY)
            {
                BattlefieldTile child = tileMap[check.x + 1][check.y + 1];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                checkConnection(check, child, parent, open, closed, pos, g, f,
                    posXDirection & posYDirection);
            }
            if (posX && negY)
            {
                BattlefieldTile child = tileMap[check.x + 1][check.y - 1];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                checkConnection(check, child, parent, open, closed, pos, g, f,
                    posXDirection & negYDirection);
            }
            if (negX && posY)
            {
                BattlefieldTile child = tileMap[check.x - 1][check.y + 1];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                checkConnection(check, child, parent, open, closed, pos, g, f,
                    negXDirection & posYDirection);
            }
            if (negX && negY)
            {
                BattlefieldTile child = tileMap[check.x - 1][check.y - 1];
                if (child == destination)
                {
                    parent.Add(child, check);
                    return interpretPath(parent, startTile);
                }
                checkConnection(check, child, parent, open, closed, pos, g, f,
                    negXDirection & negYDirection);
            }
            closed.Add(check);
        }
        return interpretPath(parent, startTile);
    }
    private bool checkConnection(BattlefieldTile check, BattlefieldTile child,
        Dictionary<BattlefieldTile, BattlefieldTile> parent,
        List<BattlefieldTile> open, List<BattlefieldTile> closed,
        Dictionary<BattlefieldTile, float> pos, Dictionary<BattlefieldTile, float> g,
        Dictionary<BattlefieldTile, float> f, ushort movementDirection)
    {
        if (closed.Contains(child)
            || child.canEnterFrom(terrestrial, movementDirection))
        {
            return false;
        }
        addToDictionary(child, pos[check] + 1, pos);
        addToDictionary(child, g[check] + child.getType().getMoveCost(), g);
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

    // Update is called once per frame
    void Update()
    {
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
