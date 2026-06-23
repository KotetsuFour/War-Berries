using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapDisplay : MonoBehaviour
{
    private Tile[,] tiles;
    [SerializeField] private Tile tile;
    [SerializeField] private CustomNavMesh navMesh;

    [SerializeField] private Material plain;
    [SerializeField] private Material desert;
    [SerializeField] private Material forest;
    [SerializeField] private Material dense_forest;
    [SerializeField] private Material mountain;
    [SerializeField] private Material shallow_water;
    [SerializeField] private Material deep_water;
    [SerializeField] private Material snowy_plain;
    [SerializeField] private Material snowy_mountain;
    [SerializeField] private Material swamp;
    [SerializeField] private Material wasteland;
    [SerializeField] private Material glacier;

    [SerializeField] private Material movableHighlight;
    [SerializeField] private Material attackableHighlight;
    [SerializeField] private Material interactableHighlight;

    private Dictionary<WorldMapTile.WorldMapTileType, Material> materials;

    // Start is called before the first frame update
    void Awake()
    {
        materials = new Dictionary<WorldMapTile.WorldMapTileType, Material>();
        materials.Add(WorldMapTile.WorldMapTileType.PLAIN, plain);
        materials.Add(WorldMapTile.WorldMapTileType.DESERT, desert);
        materials.Add(WorldMapTile.WorldMapTileType.FOREST, forest);
        materials.Add(WorldMapTile.WorldMapTileType.DENSE_FOREST, dense_forest);
        materials.Add(WorldMapTile.WorldMapTileType.MOUNTAIN, mountain);
        materials.Add(WorldMapTile.WorldMapTileType.SHALLOW_WATER, shallow_water);
        materials.Add(WorldMapTile.WorldMapTileType.DEEP_WATER, deep_water);
        materials.Add(WorldMapTile.WorldMapTileType.SNOWY_PLAIN, snowy_plain);
        materials.Add(WorldMapTile.WorldMapTileType.SNOWY_MOUNTAIN, snowy_mountain);
        materials.Add(WorldMapTile.WorldMapTileType.SWAMP, swamp);
        materials.Add(WorldMapTile.WorldMapTileType.WASTELAND, wasteland);
        materials.Add(WorldMapTile.WorldMapTileType.GLACIER, glacier);
    }
    public void generateMap()
    {
        WorldMapTile[][] map = StaticData.worldMap.getMap();
        if (map != null && tiles == null)
        {
            tiles = new Tile[map.Length, map[0].Length];
            for (int q = 0; q < map.Length; q++)
            {
                for (int w = 0; w < map[0].Length; w++)
                {
                    Tile toPlace = Instantiate(tile, new Vector3(q, 0, w), Quaternion.identity, transform);
//                    toPlace.draw(q, w, map[q][w]);
                    toPlace.setMaterial(materials[map[q][w].getType()]);
                    //                    toPlace.updateDeco();
                    map[q][w].tileModel = toPlace;
                    tiles[q, w] = toPlace;
                }
            }
            //            GameObject.Find("PlayerInput").GetComponent<PlayerInput>().initialize();
        }
    }
}
