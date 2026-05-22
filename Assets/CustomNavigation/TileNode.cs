using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileNode : MonoBehaviour
{
    [SerializeField] private LayerMask tileNodeLayer;
    [SerializeField] private LayerMask obstacleLayer;
    private int type;
    public static int AIR = 0;
    public static int LAND = 1;
    public static int WATER = 2;
    public static int DECO = 3;
    public static int IMPASS = 4;

    private List<TileNode> connections;

    public void setLength(float length)
    {
        GetComponent<BoxCollider>().size = new Vector3(length, length, length);
    }
    public void setType(int type)
    {
        this.type = type;
    }

    public int getType()
    {
        return type;
    }

    public void connectOneWay(TileNode node)
    {
        if (connections == null)
        {
            connections = new List<TileNode>();
        }
        connections.Add(node);
    }

    public List<TileNode> getConnections()
    {
        return connections;
    }

    public void connect(float distance)
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, distance, Vector3.down, distance, tileNodeLayer);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider != null
                && (type == DECO || !Physics.Raycast(transform.position,
                hit.collider.transform.position - transform.position,
                (hit.collider.transform.position - transform.position).magnitude,
                obstacleLayer)))
            {
                TileNode node = hit.collider.GetComponent<TileNode>();
                if (node == this)
                {
                    continue;
                }
                if (type == LAND && (node.transform.position - transform.position).normalized == Vector3.down)
                {
                    node.setType(IMPASS);
                }
                connectOneWay(node);
                node.connectOneWay(this);
            }
        }

//        Debug.Log($"{connections.Count} connections");
    }
}
