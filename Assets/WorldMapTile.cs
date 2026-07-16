using UnityEngine;
public class WorldMapTile : TileData
{
	private BuildSite buildSite;
	private WorldMapTileType type;
	private Affiliation affiliation;
	private int magicPotency;
	private int magicType;
	private double heat;
	private double moisture;

	public Tile tileModel;

	private CharacterTeam occupyingTeam;

	private string buildingName;
	private string buildingType;
	private uint buildingIntegrity;

	private byte x;
	private byte y;

	private double resourceTimeStamp; // The time stamp of the last time resources were taken
									//from this tile. Used to calculate how much can be taken
									//this tile. If not all resources are taken, this value is
									//incremented, but not fully set to the current time stamp

	public WorldMapTile(double heat, double moisture,
		double height, int magicPotency, int magicType,
		byte x, byte y) : base(height)
	{
		this.heat = heat;
		this.moisture = moisture;
		this.magicPotency = magicPotency;
		this.magicType = magicType;
		this.x = x;
		this.y = y;
	}
	public void setTileType(WorldMapTileType type)
    {
		this.type = type;
    }
	public void placeTeam(CharacterTeam occupyingTeam)
    {
		this.occupyingTeam = occupyingTeam;
    }
	public double getHeat()
    {
		return heat;
    }
	public double getMoisture()
	{
		return moisture;
	}
	public int getMagicType()
    {
		return magicType;
    }

	public int getX()
    {
		return x;
    }

	public int getY()
    {
		return y;
    }

	public int getMagicPotency()
    {
		return magicPotency;
    }

	public override string getTypeName()
    {
		return type.getName();
    }

	public int getMoveCost(CharacterTeam group)
	{
		if (group.canFly())
		{
			return 1;
		}
		return type.moveCostOnFoot();
	}
	public string getBuildingType()
	{
		return buildingType;
	}
	public void setBuilding(BuildSite buildSite, string buildingName, string buildingType)
	{
		this.buildSite = buildSite;
		this.buildingName = buildingName;
		this.buildingType = buildingType;
		buildingIntegrity = 0;
	}
	public bool canProduce()
    {
		return buildingType != null && buildSite == null && buildingIntegrity > 0;
    }

	public float[] getAllResources()
    {
		StaticData.BuildingData data = StaticData.getBuildingData(buildingName);

		float resourceAmount = (float)(StaticData.worldTime - resourceTimeStamp);
		float[] freq = type.getResourceFrequencies();

		int clay = (int)Item.ResouceType.CLAY;
		int fabr = (int)Item.ResouceType.FABRIC;
		int food = (int)Item.ResouceType.FOOD;
		int ores = (int)Item.ResouceType.ORE;
		int wood = (int)Item.ResouceType.WOOD;

		float[] ret = new float[(int)Item.ResouceType.BOUND];
		ret[clay] = resourceAmount * freq[clay] * (data.resourcesProduced[clay] ? 1 : 0);
		ret[fabr] = resourceAmount * freq[fabr] * (data.resourcesProduced[fabr] ? 1 : 0);
		ret[food] = resourceAmount * freq[food] * (data.resourcesProduced[food] ? 1 : 0);
		ret[ores] = resourceAmount * freq[ores] * (data.resourcesProduced[ores] ? 1 : 0);
		ret[wood] = resourceAmount * freq[wood] * (data.resourcesProduced[wood] ? 1 : 0);

		return ret;
    }
	public void setAffiliation(Affiliation aff)
    {
		affiliation = aff;
    }
	public Affiliation getAffiliation()
    {
		return affiliation;
    }
	public WorldMapTileType getType()
	{
		return type;
	}
	public CharacterTeam getCurrentOccupants()
    {
		return occupyingTeam;
    }
	public class WorldMapTileType
	{
		public static WorldMapTileType PLAIN = new WorldMapTileType("Plain", 1, 1,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.GRASS },
			new int[] { 100 },
			new float[] { 1, 0.5f, 0.1f, 0.3f, 1 });
		public static WorldMapTileType DESERT = new WorldMapTileType("Desert", 2, 0.2f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.SAND },
			new int[] { 100 },
			new float[] { 0, 0.2f, 0, 0.01f, 0 });
		public static WorldMapTileType FOREST = new WorldMapTileType("Forest", 2, 7,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.GRASS },
			new int[] { 100 },
			new float[] { 0.8f, 0, 0.9f, 0.2f, 0.1f });
		public static WorldMapTileType DENSE_FOREST = new WorldMapTileType("Dense Forest", 5, 0.5f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.GRASS },
			new int[] { 100 },
			new float[] { 0.7f, 0, 1, 0, 0 });
		public static WorldMapTileType MOUNTAIN = new WorldMapTileType("Mountain", 4, 5,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.STONE },
			new int[] { 100 },
			new float[] { 0.1f, 1, 0, 0.5f, 0.8f });
		public static WorldMapTileType SHALLOW_WATER = new WorldMapTileType("Shallow Water", 5, 0.3f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.SHALLOW_WATER },
			new int[] { 100 },
			new float[] { 0.7f, 0, 0, 1, 0 });
		public static WorldMapTileType DEEP_WATER = new WorldMapTileType("Deep Water", int.MaxValue, 0,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.DEEP_WATER },
			new int[] { 100 },
			new float[] { 0, 0, 0, 0, 0 });
		public static WorldMapTileType SNOWY_PLAIN = new WorldMapTileType("Snowy Plain", 1, 0.3f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.SNOW },
			new int[] { 100 },
			new float[] { 0.5f, 0.1f, 0.4f, 0f, 0.1f });
		public static WorldMapTileType SNOWY_MOUNTAIN = new WorldMapTileType("Snowy Mountain", 4, 0.3f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.SNOW },
			new int[] { 100 },
			new float[] { 0, 1, 0, 0, 0 });
		public static WorldMapTileType SWAMP = new WorldMapTileType("Swamp", 4, 0.5f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.MUD },
			new int[] { 100 },
			new float[] { 0.4f, 0, 0.8f, 1, 0.2f });
		public static WorldMapTileType WASTELAND = new WorldMapTileType("Wasteland", 1, 0,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.DIRT, BattlefieldTile.BattlefieldTileType.RUBBLE, BattlefieldTile.BattlefieldTileType.POISON },
			new int[] { 90, 9, 1 },
			new float[] { 0, 0.1f, 0, 0, 0 });
		public static WorldMapTileType GLACIER = new WorldMapTileType("Glacier", 2, 0.2f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.ICE },
			new int[] { 100 },
			new float[] { 0, 0, 0, 0, 0 });


		private string name;
		private int moveCost;
		private float height;
		private float[] resourceFrequency;
		private BattlefieldTile.BattlefieldTileType[] possibleBFTiles;
		private int[] bfTileFrequency;
		private WorldMapTileType(string name, int moveCost, float height,
				BattlefieldTile.BattlefieldTileType[] possibleBFTiles, int[] bfTileFrequency,
				float[] resourceFrequency)
		{
			this.name = name;
			this.moveCost = moveCost;
			//It is easier to grow certain animals and plants depending on the climate
			//0 means nothing can be grown. Otherwise...
			//Multiple of 2 means temperate, multiple of 3 is cold, 5 is hot, 7 is dry,
			//11 is wet, 13 is elevated, 17 is dark
			this.height = height;
			this.possibleBFTiles = possibleBFTiles;
			this.bfTileFrequency = bfTileFrequency;
			this.resourceFrequency = resourceFrequency;
		}

		public string getName()
		{
			return name;
		}

		public int moveCostOnFoot()
		{
			return moveCost;
		}

		public float getHeight()
        {
			return height;
        }

		public float[] getResourceFrequencies()
        {
			return resourceFrequency;
        }

		public BattlefieldTile.BattlefieldTileType getTileTypeWithRandomNumber0to99(int randomNumber0to99)
        {
			int freq = 0;
			for (int q = 0; q < bfTileFrequency.Length; q++)
            {
				freq += bfTileFrequency[q];
				if (randomNumber0to99 < freq)
                {
					return possibleBFTiles[q];
                }
            }
			return possibleBFTiles[0];
        }
	}

}
