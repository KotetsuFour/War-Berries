using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticData
{
    public static WorldMap worldMap;

    public const int TEST_SQRT = 20;
    public static List<Color> playerColors;
    public static bool canWMPause;
    public static bool canBFPause;
    public static bool WMPaused;
    public static WMCursor WMPausedBy;

    public static BattlefieldGenerator currentBattlefield;
    public static bool BFPaused;
    //    public static BFCursor BFPausedBy;

    public static float timeDilation = 1;
    public static double worldTime;
    public static double battleTime;
    public static float deltaTimeStore;
    public static List<Affiliation> affiliations;
    public const float GRAVITY = -1;

    public static int battlefieldPerlinSeed;
    public static double seaLevel;

    public static Dictionary<string, BuildingData> buildingDataDictionary;

    public static float HOUR_TIME = 1;
    public static float DAY_TIME = HOUR_TIME * 24;
    public static float MONTH_TIME = DAY_TIME * 30;
    public static float YEAR_TIME = DAY_TIME * 12;

    /**
     * The categories for items include:
     * [00] - support (healing, buffs, debuffs)
     * [01] - pistols
     * [02] - rifles
     * [03] - rocket/grenade launchers
     * [04] - snipers
     * [05] - machine guns
     * [06] - explosives
     * [07] - swords (including knives)
     * [08] - axes
     * [09] - spears
     * [10] - bows
     * [11] - clubs
     * [12] - firearm ammunition
     * [13] - blasters
     * [14] - stationary weapon
     * [15] - magic (each has its own subclass probably)
     * [16] - shields
     * [17] - field vehicle blueprints
     * [18] - ship blueprints
     * [19] - armor
     * [20] - arrows
     */
    public static List<Item>[] itemIdx;
    public static int SUPPORT_IDX = 0;
    public static int PISTOL_IDX = 1;
    public static int RIFLE_IDX = 2;
    public static int LAUNCHER_IDX = 3;
    public static int SNIPER_IDX = 4;
    public static int MACHINEGUN_IDX = 5;
    public static int EXPLOSIVE_IDX = 6;
    public static int SWORD_IDX = 7;
    public static int AXE_IDX = 8;
    public static int SPEAR_IDX = 9;
    public static int BOW_IDX = 10;
    public static int CLUB_IDX = 11;
    public static int AMMUNITION_IDX = 12;
    public static int BLASTER_IDX = 13;
    public static int STATION_IDX = 14;
    public static int MAGIC_IDX = 15;
    public static int SHIELD_IDX = 16;
    public static int VEHICLE_IDX = 17;
    public static int SHIP_IDX = 18;
    public static int ARMOR_IDX = 19;
    public static int ARROW_IDX = 19;

    public static void pauseWM(WMCursor requester)
    {
        if (requester == null || (canWMPause && !WMPaused))
        {
            WMPaused = true;
            WMPausedBy = requester;
            //TODO update all displays to show pause
        }
    }
    public static void unpauseWM(WMCursor requester)
    {
        if (WMPaused && WMPausedBy == requester)
        {
            WMPaused = false;
            WMPausedBy = null;
            //TODO update all displays to show unpause
        }
    }
    public static void setWorldMap(int[] settings)
    {
        if (settings == null)
        {
            worldMap = new WorldMap(TEST_SQRT);
            return;
        }
    }
    public static void updateTime()
    {
        deltaTimeStore = Time.deltaTime * timeDilation;
        if (!WMPaused)
        {
            worldTime += deltaTimeStore;
        }
        else if (!BFPaused)
        {
            battleTime += deltaTimeStore;
        }
        else
        {
            deltaTimeStore = 0;
        }
    }

    public static float deltaTime()
    {
        return Time.deltaTime * timeDilation;
    }

    public static Transform findDeepChild(Transform parent, string childName)
    {
        LinkedList<Transform> kids = new LinkedList<Transform>();
        for (int q = 0; q < parent.childCount; q++)
        {
            kids.AddLast(parent.GetChild(q));
        }
        while (kids.Count > 0)
        {
            Transform current = kids.First.Value;
            kids.RemoveFirst();
            if (current.name == childName || current.name + "(Clone)" == childName)
            {
                return current;
            }
            for (int q = 0; q < current.childCount; q++)
            {
                kids.AddLast(current.GetChild(q));
            }
        }
        return null;
    }
    public static Material getMaterialByName(Material[] materials, string matName)
    {
        foreach (Material m in materials)
        {
            if (m.name.Replace(" ", "").Replace("1", "").Replace("(Instance)", "")
                == matName.Replace(" ", "").Replace("1", "").Replace("(Instance)", ""))
            {
                return m;
            }
        }
        Debug.Log(matName);
        return null;
    }

    public static void paintHairSkinEye(Material[] materials, Color hair, Color skin, Color eye)
    {
        foreach (Material m in materials)
        {
            m.color = skin;
        }
        getMaterialByName(materials, "Hair").color = hair;
        getMaterialByName(materials, "RightEye").color = eye;
        getMaterialByName(materials, "LeftEye").color = eye;
    }

    public static void initializeBuildingIndex()
    {
        buildingDataDictionary = new Dictionary<string, BuildingData>();

        int[] villRecipe = new int[(int)Item.ResouceType.BOUND];
        villRecipe[(int)Item.ResouceType.CLAY] = 400;
        villRecipe[(int)Item.ResouceType.FABRIC] = 400;
        villRecipe[(int)Item.ResouceType.FOOD] = 400;
        villRecipe[(int)Item.ResouceType.ORE] = 0;
        villRecipe[(int)Item.ResouceType.WOOD] = 400;
        bool[] villResources = new bool[(int)Item.ResouceType.BOUND];
        villResources[(int)Item.ResouceType.CLAY] = false;
        villResources[(int)Item.ResouceType.FABRIC] = false;
        villResources[(int)Item.ResouceType.FOOD] = false;
        villResources[(int)Item.ResouceType.ORE] = false;
        villResources[(int)Item.ResouceType.WOOD] = false;
        buildingDataDictionary.Add(BuildSite.VILLAGE, new BuildingData()
        {
            recipe = villRecipe,
            resourcesProduced = villResources
        });
    }

    public static void initializeItemIndex()
    {
        itemIdx = new List<Item>[21];
        //TODO setup all types
        itemIdx[RIFLE_IDX] = setupRifles();
        itemIdx[RIFLE_IDX] = setupBows();
        itemIdx[AMMUNITION_IDX] = setupAmmunition();
        itemIdx[ARROW_IDX] = setupArrows();

    }
    private static List<Item> setupRifles()
    {
        List<Item> ret = new List<Item>();
        Firearm standardRifle = new Firearm();
        standardRifle.itemName = "Standard Rifle";
        standardRifle.maxRecommendedRange = 50;
        standardRifle.bluntAttackPower = 0;
        standardRifle.cooldown = 0.5f;
        standardRifle.launchSpeed = 30;
        standardRifle.pierceAttackPower = 0;
        standardRifle.size = 0.1f;
        standardRifle.sliceAttackPower = 0;
        standardRifle.weight = 0.1f;
        ret.Add(standardRifle);

        return ret;
    }
    private static List<Item> setupAmmunition()
    {
        List<Item> ret = new List<Item>();
        foreach (Firearm wep in itemIdx[PISTOL_IDX])
        {
            wep.ammunitionUsed = ret.Count;
            ret.Add(makeAmmunitionForFirearm(wep));
        }
        foreach (Firearm wep in itemIdx[RIFLE_IDX])
        {
            wep.ammunitionUsed = ret.Count;
            ret.Add(makeAmmunitionForFirearm(wep));
        }
        foreach (Firearm wep in itemIdx[LAUNCHER_IDX])
        {
            wep.ammunitionUsed = ret.Count;
            ret.Add(makeAmmunitionForFirearm(wep));
        }
        foreach (Firearm wep in itemIdx[SNIPER_IDX])
        {
            wep.ammunitionUsed = ret.Count;
            ret.Add(makeAmmunitionForFirearm(wep));
        }
        foreach (Firearm wep in itemIdx[MACHINEGUN_IDX])
        {
            wep.ammunitionUsed = ret.Count;
            ret.Add(makeAmmunitionForFirearm(wep));
        }

        return ret;
    }
    private static List<Item> setupBows()
    {
        List<Item> ret = new List<Item>();
        Bow wood = new Bow();
        wood.itemName = "Wooden Bow";
        wood.maxRecommendedRange = 30;
        wood.bluntAttackPower = 0;
        wood.cooldown = 1.5f;
        wood.launchSpeed = 10;
        wood.pierceAttackPower = 0;
        wood.size = 0.1f;
        wood.sliceAttackPower = 0;
        wood.weight = 0.01f;
        ret.Add(wood);

        return ret;
    }
    private static List<Item> setupArrows()
    {
        List<Item> ret = new List<Item>();


        return ret;
    }
    private static Item makeAmmunitionForFirearm(Firearm wep)
    {
        Item ret = new Item();
        ret.itemName = $"{wep.itemName} Ammunition";
        return ret;
    }
    public static Item getItemFromIndex(int[] idx)
    {
        return itemIdx[idx[0]][idx[1]];
    }

    public static BuildingData getBuildingData(string buildingType)
    {
        return buildingDataDictionary[buildingType];
    }

    public class BuildingData
    {
        public int[] recipe;
        public bool[] resourcesProduced;
    }
}
