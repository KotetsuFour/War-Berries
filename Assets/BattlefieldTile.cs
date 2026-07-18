using UnityEngine;

public class BattlefieldTile : TileData
{
    private BattlefieldTileType type;
    private ushort terrestrialObstacleTraversability;
    private ushort aquaticObstacleTraversability;
    public int x, y;
    public BattlefieldTile(double height) : base(height)
    {
    }
    public void setType(BattlefieldTileType type)
    {
        this.type = type;
        terrestrialObstacleTraversability = type.isGroundImpass() ? ushort.MinValue : ushort.MaxValue;
        terrestrialObstacleTraversability = type.isWaterImpass() ? ushort.MinValue : ushort.MaxValue;
    }
    public BattlefieldTileType getType()
    {
        return type;
    }
    public override string getTypeName()
    {
        return type.getName();
    }
    public void setObstacleTraversability(ushort groundTraverse, ushort waterTraverse)
    {
        terrestrialObstacleTraversability = groundTraverse;
        aquaticObstacleTraversability = waterTraverse;
    }
    public bool canEnterFrom(bool isTerrestrial, ushort inboundDirection)
    {
        if (isTerrestrial)
        {
            return (inboundDirection & terrestrialObstacleTraversability) != ushort.MinValue;
        }
        return (inboundDirection & aquaticObstacleTraversability) != ushort.MinValue;
    }
    public class BattlefieldTileType
    {
        public static BattlefieldTileType GRASS = new BattlefieldTileType("Grass", 1, false, true);
        public static BattlefieldTileType SAND = new BattlefieldTileType("Sand", 1, false, true);
        public static BattlefieldTileType STONE = new BattlefieldTileType("Stone", 1, false, true);
        public static BattlefieldTileType DEEP_WATER = new BattlefieldTileType("Deep Water", 100, true, false);
        public static BattlefieldTileType SHALLOW_WATER = new BattlefieldTileType("ShallowWater", 1, false, false);
        public static BattlefieldTileType SNOW = new BattlefieldTileType("Snow", 1, false, true);
        public static BattlefieldTileType DIRT = new BattlefieldTileType("Dirt", 1, false, true);
        public static BattlefieldTileType ICE = new BattlefieldTileType("Ice", 1, false, false);
        public static BattlefieldTileType MUD = new BattlefieldTileType("Mud", 1, false, true);
        public static BattlefieldTileType RUBBLE = new BattlefieldTileType("Rubble", 1, false, true);
        public static BattlefieldTileType FLOOR = new BattlefieldTileType("Floor", 1, false, true);
        public static BattlefieldTileType WALL = new BattlefieldTileType("Wall", 1, false, true);
        public static BattlefieldTileType PILLAR = new BattlefieldTileType("Pillar", 1, false, true);
        public static BattlefieldTileType TREE = new BattlefieldTileType("Tree", 1, false, true);
        public static BattlefieldTileType PATH = new BattlefieldTileType("Path", 1, false, true);
        public static BattlefieldTileType LAVA = new BattlefieldTileType("Lava", 1, true, true);
        public static BattlefieldTileType POISON = new BattlefieldTileType("Poison", 1, true, false);

        private string name;
        private int moveCost;
        private bool groundImpass;
        private bool waterImpass;

        public BattlefieldTileType(string name, int moveCost, bool preventTraversalFromGround,
            bool preventTraversaFromWater)
        {
            this.name = name;
            this.moveCost = moveCost;
            groundImpass = preventTraversalFromGround;
            waterImpass = preventTraversaFromWater;
        }

        public string getName()
        {
            return name;
        }
        public int getMoveCost()
        {
            return moveCost;
        }

        public bool isGroundImpass()
        {
            return groundImpass;
        }
        public bool isWaterImpass()
        {
            return waterImpass;
        }
    }
}
