using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class VisualMapGenerator : MonoBehaviour
{
	[SerializeField] private Tile tilePrefab;
	[SerializeField] private int SQRT_OF_MAP_SIZE;
	[SerializeField] private int randomSeed;
	[SerializeField] private int randomCivilizationSeed;
	[SerializeField] private float frequency;
	[SerializeField, Range(0f, 1f)] private float persistence;
	[SerializeField] private int oddNumberedLacunarity;
	[SerializeField] private float octaveCount;
	[SerializeField] private string mapName;
	private WorldMapTile[][] map;
	private int[][][] civMap;
	private int xVal;
	private int yVal;
	private bool working;
	private bool civMaking;
	private bool valuesSet;
	private Perlin heatMap;
	private Perlin moistureMap;
	private Perlin heightMap;
	private Perlin magicTypeMap;
	private Perlin magicStrengthMap;
	private Perlin nationalismMap;
	private Perlin altruismMap;
	private Perlin familismMap;
	private Perlin militarismMap;
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

	[SerializeField] private int testExpandPosX1;
	[SerializeField] private int testExpandPosY1;
	[SerializeField] private int testExpandPosX2;
	[SerializeField] private int testExpandPosY2;
	[SerializeField] private BattlefieldGenerator bfGeneratorPrefab;

	private static float staticSeaLevel = 0;

	void Awake()
    {
		map = new WorldMapTile[SQRT_OF_MAP_SIZE][];
		for (int q = 0; q < map.Length; q++)
		{
			map[q] = new WorldMapTile[SQRT_OF_MAP_SIZE];
		}
		civMap = new int[SQRT_OF_MAP_SIZE][][];
		for (int q = 0; q < civMap.Length; q++)
		{
			civMap[q] = new int[SQRT_OF_MAP_SIZE][];
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
		staticSeaLevel = seaLevelThreshold;
	}

	public void clear()
    {
		if (working)
        {
			Debug.Log("The program is still working. Please wait.");
			return;
        }
		valuesSet = false;
		Transform ter = StaticData.findDeepChild(transform, "Terrain");
		int count = ter.childCount;
		for (int q = 0; q < count; q++)
        {
			Destroy(ter.GetChild(count - (q + 1)).gameObject);
        }
		Transform civ = StaticData.findDeepChild(transform, "Civ");
		count = civ.childCount;
		for (int q = 0; q < count; q++)
		{
			Destroy(civ.GetChild(count - (q + 1)).gameObject);
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
		StaticData.battlefieldPerlinSeed = randomSeed * 6;
		//set frequency, persistence (between 0 and 1), lacunarity (odd number), and octave count
		setPerlinSettings(new Perlin[] { heatMap, moistureMap, heightMap, magicTypeMap, magicStrengthMap },
			0.05, 0.5, 3, 2);
		Debug.Log("Generation Start!");
	}
	public void startCivilizationGeneration()
	{
		if (working)
		{
			Debug.Log("The program is still working. Please wait.");
			return;
		}
		civMaking = true;
		working = true;
		xVal = 0;
		yVal = 0;
		Debug.Log("Perlin Civilization Mapping Start!");
		nationalismMap = new Perlin();
		nationalismMap.Seed = randomCivilizationSeed;
		altruismMap = new Perlin();
		altruismMap.Seed = randomCivilizationSeed * 2;
		familismMap = new Perlin();
		familismMap.Seed = randomCivilizationSeed * 3;
		militarismMap = new Perlin();
		militarismMap.Seed = randomCivilizationSeed * 4;
		setPerlinSettings(new Perlin[] { nationalismMap, altruismMap, militarismMap, familismMap },
			0.05, 0.5, 3, 2);
		Debug.Log("Perlin Civilization Mapping Complete!");
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

		double heat = heatMap.GetValue(xVal, yVal, 0);
		double moisture = moistureMap.GetValue(xVal, yVal, 0);
		double height = heightMap.GetValue(xVal, yVal, 0);
		double magic = magicTypeMap.GetValue(xVal, yVal, 0);
		double magicStrength = magicStrengthMap.GetValue(xVal, yVal, 0);

		int magicType = magic > 0.67 ? 0 : magic > 0.33 ? 1 : 2;

		map[xVal][yVal] = new WorldMapTile(heat, moisture, height,
			Mathf.RoundToInt((float)magicStrength * 100), magicType,
			(byte)xVal, (byte)yVal);
		updateTile();
	}
	private void generateCivTile()
    {
		WorldMapTile tile = map[xVal][yVal];
		if (tile.getHeight() < seaLevelThreshold || tile.getMagicPotency() < 50)
        {
			return;
        }
		double nat = (nationalismMap.GetValue(xVal, yVal, 0) + 1) / 2;
		double alt = (altruismMap.GetValue(xVal, yVal, 0) + 1) / 2;
		double fam = (familismMap.GetValue(xVal, yVal, 0) + 1) / 2;
		double mil = (militarismMap.GetValue(xVal, yVal, 0) + 1) / 2;
		civMap[xVal][yVal] = new int[] { Mathf.RoundToInt((float)nat), Mathf.RoundToInt((float)alt),
			Mathf.RoundToInt((float)fam), Mathf.RoundToInt((float)mil)};
		Tile city = Instantiate(tilePrefab, StaticData.findDeepChild(transform, "Civ"));
		city.transform.position = new Vector3(xVal, 10, yVal);
		double maxVal = alt;
		Material mat = shallowWater;
		if (nat > maxVal)
        {
			maxVal = nat;
			mat = forest;
        }
		if (fam > maxVal)
		{
			maxVal = fam;
			mat = desert;
		}
		if (mil > maxVal)
		{
			mat = snowyPlain;
		}
		city.setMaterial(mat);
		city.drawGeneral(xVal, yVal, 10);
	}
	private void connectCityStates()
    {
		for (int q = 0; q < civMap.Length; q++)
        {
			for (int w = 0; w < civMap[q].Length; w++)
            {
				if (q > 0 && civMap[q][w] != null)
				{
					if (q > 0 && civMap[q - 1][w] != null)
					{
						map[q - 1][w].getAffiliation().addTile(map[q][w], civMap[q][w]);
					}
					else if (w > 0 && civMap[q][w - 1] != null)
                    {
						map[q][w - 1].getAffiliation().addTile(map[q][w], civMap[q][w]);
					}
					else
                    {
						Affiliation aff = new Affiliation();
						aff.addTile(map[q][w], civMap[q][w]);
					}
				}
			}
        }
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
		Tile tile = Instantiate(tilePrefab, StaticData.findDeepChild(transform, "Terrain"));
		tile.transform.position = new Vector3(xVal, 0, yVal);
		tile.setMaterial(mat);
		tile.draw(xVal, yVal, map[xVal][yVal], seaLevelThreshold);
		tile.name = $"{xVal},{yVal}";
    }
	public static void setPerlinSettings(Perlin[] perlins, double freq, double persist, double lacun,
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
		MapSaveData data = new MapSaveData(this);
		BinaryFormatter formatter = new BinaryFormatter();
		FileStream stream = new FileStream(savePath + mapName + ".map", FileMode.Create);
		formatter.Serialize(stream, data);
		stream.Close();
	}

	public int[] getDimensions()
    {
		return new int[] { SQRT_OF_MAP_SIZE, SQRT_OF_MAP_SIZE };
    }

	public int getSeed()
    {
		return randomSeed;
    }

	public void tileExpansionTest()
    {
		StaticData.affiliations = new List<Affiliation>();
		BattlefieldGenerator bfGen = Instantiate(bfGeneratorPrefab);
		Affiliation aff1 = new Affiliation();
		Affiliation aff2 = new Affiliation();
		BerryData data1 = new BerryData($"TestBerry1", aff1, 0, Color.blue,
			Color.blue, Color.blue, 0);
		BerryData data2 = new BerryData($"TestBerry2", aff2, 0, Color.blue,
			Color.blue, Color.blue, 0);
		CharacterTeam team1 = new CharacterTeam(data1);
		CharacterTeam team2 = new CharacterTeam(data2);
		team1.xCoord = testExpandPosX1;
		team1.yCoord = testExpandPosY1;
		team2.xCoord = testExpandPosX2;
		team2.yCoord = testExpandPosY2;
		at(team1.xCoord, team1.yCoord).placeTeam(team1);
		at(team2.xCoord, team2.yCoord).placeTeam(team2);
		StaticData.findDeepChild(transform, "Terrain").gameObject.SetActive(false);
		bfGen.setup(new CharacterTeam[] { team1, team2 });
    }

	public void loadMap()
    {
		string file = savePath + mapName + ".map";
		if (File.Exists(file))
        {
			BinaryFormatter formatter = new BinaryFormatter();
			FileStream stream = new FileStream(file, FileMode.Open);
			MapSaveData data = formatter.Deserialize(stream) as MapSaveData;
			stream.Close();

			clear();
			map = new WorldMapTile[data.xDimension][];
			for (int q = 0; q < map.Length; q++)
			{
				map[q] = new WorldMapTile[data.yDimension];
			}

			randomSeed = data.seed;

			xVal = 0;
			yVal = 0;
			for (int q = 0; q < data.heat.Length; q++)
            {
				map[xVal][yVal] = new WorldMapTile(data.heat[q], data.moisture[q],
					data.height[q], data.magicPotency[q], data.magicType[q],
					(byte)xVal, (byte)yVal);
				updateTile();
				yVal++;
				if (yVal == data.xDimension)
                {
					yVal = 0;
					xVal++;
                }
            }
			StaticData.worldMap = new WorldMap(map);
		}
		else
        {
			Debug.LogError($"Could not find file at " + file);
        }
    }

	public static float getSeaLevel()
    {
		return staticSeaLevel;
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
			else if (civMaking)
            {
				generateCivTile();
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
				if (civMaking)
                {
					connectCityStates();
					civMaking = false;
				}
				working = false;
				valuesSet = true;
				StaticData.worldMap = new WorldMap(map);
            }
        }
    }
}
