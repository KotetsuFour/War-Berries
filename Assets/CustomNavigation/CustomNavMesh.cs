using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomNavMesh : MonoBehaviour
{
    [SerializeField] private float maxX;
    [SerializeField] private float minX;
    [SerializeField] private float maxY;
    [SerializeField] private float minY;
    [SerializeField] private float maxZ;
    [SerializeField] private float minZ;
    [SerializeField] private float tileSize;
    [SerializeField] private LayerMask land;
    [SerializeField] private LayerMask water;
    [SerializeField] private LayerMask deco;
    [SerializeField] private LayerMask landAndWater;

    [SerializeField] private TileNode tile;

    // Start is called before the first frame update
    void Start()
    {
        //Bake on start only if the bounds were set in the editor
        if (minX != maxX || minY != maxY || minZ != maxZ)
        {
            bake(minX, maxX, minY, maxY, minZ, maxZ);
        }
    }

    public void bake(float startX, float endX, float startY, float endY, float startZ, float endZ)
    {
        for (float x = startX; x <= endX; x += tileSize)
        {
            for (float z = startZ; z <= endZ; z += tileSize)
            {
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(x, endY, z), Vector3.down, out hit, endY - startY, land))
                {
                    makeTile(hit.point, TileNode.LAND);
                }
                if (Physics.Raycast(new Vector3(x, endY, z), Vector3.down, out hit, endY - startY, water))
                {
                    makeTile(hit.point, TileNode.WATER);
                }
                float decoHeight = endY;
                while (Physics.Raycast(new Vector3(x, decoHeight, z), Vector3.down, out hit, decoHeight - startY, deco))
                {
                    decoHeight = makeDecoTile(hit.point, TileNode.DECO);
                }
            }
        }
        for (int q = 0; q < transform.childCount; q++)
        {
            TileNode tile = transform.GetChild(q).GetComponent<TileNode>();
            tile.connect(tileSize * 0.75f);
        }
    }
    private TileNode makeTile(Vector3 position, int tileType)
    {
        TileNode node = Instantiate(tile, position, Quaternion.identity);
        node.setType(tileType);
        node.setLength(tileSize);
        node.transform.SetParent(transform);
        node.gameObject.name = $"{position.x}, {position.z}: {tileType}";
//        Debug.Log(node.transform.position);
        return node;
    }
    private float makeDecoTile(Vector3 position, int tileType)
    {
        TileNode node = makeTile(position, tileType);
//        Debug.Log(node.transform.position);
        return node.transform.position.y - tileSize;
    }
}
