using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using System.IO;

public class VisualMapGenerator : MonoBehaviour
{
	[SerializeField] private Tile tilePrefab;
	[SerializeField] private int SQRT_OF_MAP_SIZE;
	[SerializeField] private int randomSeed;
	[SerializeField] private float frequency;
	[SerializeField, Range(0f, 1f)] private float persistence;
	[SerializeField] private int oddNumberedLacunarity;
	[SerializeField] private float octaveCount;
	[SerializeField] private string mapName;
	private WorldMapTile[][] map;
	private int xVal;
	private int yVal;
	private bool working;
	private bool valuesSet;
	private Perlin heatMap;
	private Perlin moistureMap;
	private Perlin heightMap;
	private Perlin magicTypeMap;
	private Perlin magicStrengthMap;
	[SerializeField] private Material deepWater;
	[SerializeField] private Material denseForest;
	[SerializeField] private Material desert;
	[SerializeField] private Material forest;
	[SerializeField] private Material glacier;
	[SerializeField] private Material mountain;
	[SerializeField] private Material plain;
	[SerializeField] private Material shallowWater;
	[SerializeField] private Material snowyMountain;
	[SerializeField] private Material snowyPlain;
	[SerializeField] private Material swamp;
	[SerializeField] private Material wasteland;
	[SerializeField, Range(-1, 1)] private float coldThreshold;
	[SerializeField, Range(-1, 1)] private float temperateThreshold;
	[SerializeField, Range(-1, 1)] private float wetThreshold;
	[SerializeField, Range(-1, 1)] private float dryThreshold;
	[SerializeField, Range(-1, 1)] private float mountainLevelThreshold;
	[SerializeField, Range(-1, 1)] private float normalLevelThreshold;
	[SerializeField, Range(-1, 1)] private float seaLevelThreshold;
	[SerializeField, Range(-1, 1)] private float deepSeaLevelThreshold;
	private Dictionary<WorldMapTile.WorldMapTileType, Material> materialDictionary;
	private string savePath = "./Assets/Tests/TestMaps/";

	void Awake()
    {
		map = new WorldMapTile[SQRT_OF_MAP_SIZE][];
		for (int q = 0; q < map.Length; q++)
		{
			map[q] = new WorldMapTile[SQRT_OF_MAP_SIZE];
		}
		materialDictionary = new Dictionary<WorldMapTile.WorldMapTileType, Material>();
		materialDictionary.Add(WorldMapTile.WorldMapTileType.DEEP_WATER, deepWater);
		materialDictionary.Add(WorldMapTile.WorldMapTileType.DENSE_FOREST, denseForest);
		materialDictionary.Add(WorldMapTile.WorldMapTileType.DESERT, desert);
		materialDictionary.Add(WorldMapTile.WorldMapTileType.FOREST, forest);
		materialDictionary.Add(WorldMapTile.WorldMapTileType.GLACIER, glacier);
		materialDictionary.Add(WorldMapTile.WorldMapTileType.MOUNTAIN, mountain);
		materialDictionary.Add(WorldMapTile.WorldMapTileType.PLAIN, plain);
		materialDictionary.Add(WorldMapTile.WorldMapTileType.SHALLOW_WATER, shallowWater);
		materialDictionary.Add(WorldMapTile.WorldMapTileType.SNOWY_MOUNTAIN, snowyMountain);
		materialDictionary.Add(WorldMapTile.WorldMapTileType.SNOWY_PLAIN, snowyPlain);
		materialDictionary.Add(WorldMapTile.WorldMapTileType.SWAMP, swamp);
		materialDictionary.Add(WorldMapTile.WorldMapTileType.WASTELAND, wasteland);
	}

	public void clear()
    {
		if (working)
        {
			Debug.Log("The program is still working. Please wait.");
			return;
        }
		valuesSet = false;
		int count = transform.childCount;
		for (int q = 0; q < count; q++)
        {
			Destroy(transform.GetChild(count - (q + 1)).gameObject);
        }
    }
	public void startGeneration()
    {
		if (working)
		{
			Debug.Log("The program is still working. Please wait.");
			return;
		}
		clear();
		working = true;
		xVal = 0;
		yVal = 0;
		Debug.Log("Perlin Mapmaking Start!");
		heatMap = new Perlin();
		heatMap.Seed = randomSeed;
		moistureMap = new Perlin();
		moistureMap.Seed = randomSeed * 2;
		heightMap = new Perlin();
		heightMap.Seed = randomSeed * 3;
		magicTypeMap = new Perlin();
		magicTypeMap.Seed = randomSeed * 4;
		magicStrengthMap = new Perlin();
		magicStrengthMap.Seed = randomSeed * 5;
		//set frequency, persistence (between 0 and 1), lacunarity (odd number), and octave count
		setPerlinSettings(new Perlin[] { heatMap, moistureMap, heightMap, magicTypeMap, magicStrengthMap },
			0.05, 0.5, 3, 2);
		Debug.Log("Generation Start!");
	}
	public void startUpdating()
    {
		if (working)
		{
			Debug.Log("The program is still working. Please wait.");
			return;
		}
		if (!valuesSet)
        {
			Debug.Log("You have no current map. Please generate one first.");
			return;
        }
		clear();
		working = true;
		xVal = 0;
		yVal = 0;
		Debug.Log("Updating Start!");
	}
	private void generateTile()
	{
		Tile tile = Instantiate(tilePrefab, transform);
		tile.transform.position = new Vector3(xVal, 0, yVal);

		double heat = 
		heatMap.GetValue(xVal, yVal, 0);
		//Or heat should be based on longitude (yVal)
		//1 - (Mathf.Abs(yVal - (SQRT_OF_MAP_SIZE / 2)) * 4 / (float)SQRT_OF_MAP_SIZE);
		double moisture = moistureMap.GetValue(xVal, yVal, 0);
		double height = heightMap.GetValue(xVal, yVal, 0);
		double magic = magicTypeMap.GetValue(xVal, yVal, 0);
		double magicStrength = magicStrengthMap.GetValue(xVal, yVal, 0);

		int magicType = magic > 0.67 ? 0 : magic > 0.33 ? 1 : 2;

		map[xVal][yVal] = new WorldMapTile(heat, moisture, height,
			Mathf.RoundToInt((float)magicStrength * 100), magicType);
		updateTile();
	}
	private void updateTile()
    {
		WorldMapTile wmTile = map[xVal][yVal];
		if (wmTile.getHeight() < deepSeaLevelThreshold)
        {
			wmTile.setTileType(WorldMapTile.WorldMapTileType.DEEP_WATER);
        }
		else if (wmTile.getHeight() < seaLevelThreshold)
        {
			if (wmTile.getHeat() < coldThreshold)
            {
				wmTile.setTileType(WorldMapTile.WorldMapTileType.GLACIER);
            }
			else
            {
				wmTile.setTileType(WorldMapTile.WorldMapTileType.SHALLOW_WATER);
            }
        }
		else if (wmTile.getHeight() < normalLevelThreshold)
		{
			if (wmTile.getHeat() < coldThreshold)
			{
				if (wmTile.getMoisture() < wetThreshold)
				{
					wmTile.setTileType(WorldMapTile.WorldMapTileType.SNOWY_PLAIN);
				}
				else if (wmTile.getMoisture() < dryThreshold)
				{
					wmTile.setTileType(WorldMapTile.WorldMapTileType.PLAIN);
				}
				else
				{
					wmTile.setTileType(WorldMapTile.WorldMapTileType.PLAIN);
				}
			}
			else if (wmTile.getHeat() < temperateThreshold)
            {
				if (wmTile.getMoisture() < wetThreshold)
				{
					wmTile.setTileType(WorldMapTile.WorldMapTileType.FOREST);
				}
				else if (wmTile.getMoisture() < dryThreshold)
				{
					wmTile.setTileType(WorldMapTile.WorldMapTileType.PLAIN);
				}
				else
				{
					wmTile.setTileType(WorldMapTile.WorldMapTileType.DESERT);
				}
			}
			else
			{
				if (wmTile.getMoisture() < wetThreshold)
				{
					wmTile.setTileType(WorldMapTile.WorldMapTileType.SWAMP);
				}
				else if (wmTile.getMoisture() < dryThreshold)
				{
					wmTile.setTileType(WorldMapTile.WorldMapTileType.DENSE_FOREST);
				}
				else
				{
					wmTile.setTileType(WorldMapTile.WorldMapTileType.DESERT);
				}
			}
		}
		else if (wmTile.getHeight() < mountainLevelThreshold)
		{
			if (wmTile.getHeat() < coldThreshold)
            {
				wmTile.setTileType(WorldMapTile.WorldMapTileType.SNOWY_MOUNTAIN);
			}
			else
            {
				wmTile.setTileType(WorldMapTile.WorldMapTileType.MOUNTAIN);
			}
		}
		else
		{
			wmTile.setTileType(WorldMapTile.WorldMapTileType.SNOWY_MOUNTAIN);
		}
		makeTileDisplay();
	}
	private void makeTileDisplay()
    {
		Material mat = materialDictionary[map[xVal][yVal].getType()];
		Tile tile = Instantiate(tilePrefab, transform);
		tile.transform.position = new Vector3(xVal, 0, yVal);
		tile.setMaterial(mat);
		tile.draw(xVal, yVal, map[xVal][yVal], seaLevelThreshold);
    }
	private void setPerlinSettings(Perlin[] perlins, double freq, double persist, double lacun,
		int octave)
	{
		foreach (Perlin p in perlins)
		{
			p.Frequency = freq;
			p.Persistence = persist;
			p.Lacunarity = lacun;
			p.OctaveCount = octave;
		}
	}
	public WorldMapTile[][] getMap()
	{
		return map;
	}
	public WorldMapTile at(int x, int y)
	{
		return map[x][y];
	}

	public List<WorldMapTile> getAllAdjacentTiles(int x, int y)
	{
		List<WorldMapTile> ret = new List<WorldMapTile>(4);
		if (x != 0)
		{
			ret.Add(map[x - 1][y]);
		}
		if (x < map.Length - 1)
		{
			ret.Add(map[x + 1][y]);
		}
		if (y != 0)
		{
			ret.Add(map[x][y - 1]);
		}
		if (y < map[x].Length - 1)
		{
			ret.Add(map[x][y + 1]);
		}
		return ret;
	}

	public void saveMap()
    {
		if (working)
		{
			Debug.Log("The program is still working. Please wait.");
			return;
		}
		//TODO Save function
	}

	// Update is called once per frame
	void Update()
    {
        if (working)
        {
			if (valuesSet)
            {
				updateTile();
            }
			else
            {
				generateTile();
			}
			if (xVal < SQRT_OF_MAP_SIZE - 1)
            {
				xVal++;
            }
			else if (yVal < SQRT_OF_MAP_SIZE - 1)
            {
				xVal = 0;
				yVal++;
            }
			else
            {
				working = false;
				valuesSet = true;
            }
        }
    }
}
