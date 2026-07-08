using UnityEngine;
public class WorldMapTile : TileData
{
	private Building building;
	private WorldMapTileType type;
	private Affiliation affiliation;
	private int magicPotency;
	private int magicType;
	private double heat;
	private double moisture;
	private double height;

	public Tile tileModel;

	private CharacterTeam occupyingTeam;

	public WorldMapTile(double heat, double moisture,
		double height, int magicPotency, int magicType) : base(height)
	{
		this.heat = heat;
		this.moisture = moisture;
		this.magicPotency = magicPotency;
		this.magicType = magicType;
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
	public Building getBuilding()
	{
		return building;
	}
	public void setBuilding(Building b)
	{
		this.building = b;
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
		public static WorldMapTileType PLAIN = new WorldMapTileType("Plain", 1, 1, 6, new Color(0.5f, 1, 0), 0.5f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.GRASS },
			new int[] { 100 });
		public static WorldMapTileType DESERT = new WorldMapTileType("Desert", 2, 0.2f, 7, new Color(1, 1, 0), 0.5f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.SAND },
			new int[] { 100 });
		public static WorldMapTileType FOREST = new WorldMapTileType("Forest", 2, 7, 6, new Color(0.03f, 0.8f, 0), 0.5f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.GRASS },
			new int[] { 100 });
		public static WorldMapTileType DENSE_FOREST = new WorldMapTileType("Dense Forest", 5, 0.5f, 4, new Color(0.01f, 0.3f, 0), 0.5f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.GRASS },
			new int[] { 100 });
		public static WorldMapTileType MOUNTAIN = new WorldMapTileType("Mountain", 4, 5, 10, new Color(0.45f, 0.4f, 0.5f), 2,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.STONE },
			new int[] { 100 });
		public static WorldMapTileType SHALLOW_WATER = new WorldMapTileType("Shallow Water", 5, 0.3f, 1, new Color(0, 0.9f, 1), 0,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.SHALLOW_WATER },
			new int[] { 100 });
		public static WorldMapTileType DEEP_WATER = new WorldMapTileType("Deep Water", int.MaxValue, 0, 0, new Color(0, 0, 1), 0,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.DEEP_WATER },
			new int[] { 100 });
		public static WorldMapTileType SNOWY_PLAIN = new WorldMapTileType("Snowy Plain", 1, 0.3f, 5, new Color(1, 1, 1), 0.5f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.SNOW },
			new int[] { 100 });
		public static WorldMapTileType SNOWY_MOUNTAIN = new WorldMapTileType("Snowy Mountain", 4, 0.3f, 8, new Color(0.7f, 0.7f, 0.7f), 2,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.SNOW },
			new int[] { 100 });
		public static WorldMapTileType SWAMP = new WorldMapTileType("Swamp", 4, 0.5f, 4, new Color(0.3f, 0.4f, 0.3f), 0.5f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.MUD },
			new int[] { 100 });
		public static WorldMapTileType WASTELAND = new WorldMapTileType("Wasteland", 1, 0, 5, new Color(0.48f, 0.37f, 0), 0.5f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.DIRT, BattlefieldTile.BattlefieldTileType.RUBBLE, BattlefieldTile.BattlefieldTileType.POISON },
			new int[] { 90, 9, 1 });
		public static WorldMapTileType GLACIER = new WorldMapTileType("Glacier", 2, 0.2f, 2, new Color(0.7f, 1, 1), 0.25f,
			new BattlefieldTile.BattlefieldTileType[] { BattlefieldTile.BattlefieldTileType.ICE },
			new int[] { 100 });


		private string name;
		private int moveCost;
		private float proliferability;
		private int minability;
		private Color displayColor;
		private float height;
		private BattlefieldTile.BattlefieldTileType[] possibleBFTiles;
		private int[] bfTileFrequency;
		private WorldMapTileType(string name, int moveCost, float proliferability,
				int minability, Color displayColor, float height,
				BattlefieldTile.BattlefieldTileType[] possibleBFTiles, int[] bfTileFrequency)
		{
			this.name = name;
			this.moveCost = moveCost;
			//It is easier to grow certain animals and plants depending on the climate
			//0 means nothing can be grown. Otherwise...
			//Multiple of 2 means temperate, multiple of 3 is cold, 5 is hot, 7 is dry,
			//11 is wet, 13 is elevated, 17 is dark
			this.proliferability = proliferability;
			this.minability = minability;
			this.displayColor = displayColor;
			this.height = height;
			this.possibleBFTiles = possibleBFTiles;
			this.bfTileFrequency = bfTileFrequency;
		}

		public string getName()
		{
			return name;
		}

		public int moveCostOnFoot()
		{
			return moveCost;
		}

		public int getMinability()
		{
			return minability;
		}
		public float getProliferability()
		{
			return proliferability;
		}
		public Color getDisplayColor()
		{
			return displayColor;
		}
		public float getHeight()
        {
			return height;
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
