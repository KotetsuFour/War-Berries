using UnityEngine;

public class BattlefieldTile : TileData
{
    private BattlefieldTileType type;
    public BattlefieldTile(double height) : base(height)
    {
    }
    public void setType(BattlefieldTileType type)
    {
        this.type = type;
    }
    public BattlefieldTileType getType()
    {
        return type;
    }
    public override string getTypeName()
    {
        return type.getName();
    }
    public class BattlefieldTileType
    {
        public static BattlefieldTileType GRASS = new BattlefieldTileType("Grass", 1);
        public static BattlefieldTileType SAND = new BattlefieldTileType("Sand", 1);
        public static BattlefieldTileType STONE = new BattlefieldTileType("Stone", 1);
        public static BattlefieldTileType DEEP_WATER = new BattlefieldTileType("Deep Water", 100);
        public static BattlefieldTileType SHALLOW_WATER = new BattlefieldTileType("ShallowWater", 1);
        public static BattlefieldTileType SNOW = new BattlefieldTileType("Snow", 1);
        public static BattlefieldTileType DIRT = new BattlefieldTileType("Dirt", 1);
        public static BattlefieldTileType ICE = new BattlefieldTileType("Ice", 1);
        public static BattlefieldTileType MUD = new BattlefieldTileType("Mud", 1);
        public static BattlefieldTileType RUBBLE = new BattlefieldTileType("Rubble", 1);
        public static BattlefieldTileType FLOOR = new BattlefieldTileType("Floor", 1);
        public static BattlefieldTileType WALL = new BattlefieldTileType("Wall", 1);
        public static BattlefieldTileType PILLAR = new BattlefieldTileType("Pillar", 1);
        public static BattlefieldTileType TREE = new BattlefieldTileType("Tree", 1);
        public static BattlefieldTileType PATH = new BattlefieldTileType("Path", 1);
        public static BattlefieldTileType LAVA = new BattlefieldTileType("Lava", 1);
        public static BattlefieldTileType POISON = new BattlefieldTileType("Poison", 1);

        private string name;
        private int moveCost;

        public BattlefieldTileType(string name, int moveCost)
        {
            this.name = name;
            this.moveCost = moveCost;
        }

        public string getName()
        {
            return name;
        }
        public int getMoveCost()
        {
            return moveCost;
        }
    }
}
