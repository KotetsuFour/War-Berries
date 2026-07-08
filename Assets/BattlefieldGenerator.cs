using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;

public class BattlefieldGenerator : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;
    public const int BATTLEFIELD_MAX_PERIPHERAL_RADIUS = 4;
    public const int BATTLEFIELD_MIN_CORE_RADIUS = 2;
    public const int BATTLEFIELD_TO_WORLDTILE_RATIO = 6;
    private List<CharacterTeam> teams;
    private List<CharacterTeam> reinforcements;
    private List<float> reinforcementAvailabilityTimers; //How long until each reinforcement can be deployed
    private List<Affiliation> affiliationsInvolved;

    private BattlefieldTile[][] bfMap;

    private int workingStage;

    private int coreMinX;
    private int coreMaxX;
    private int coreMinY;
    private int coreMaxY;

    private int periMinX;
    private int periMaxX;
    private int periMinY;
    private int periMaxY;

    private int xWorkingVal;
    private int yWorkingVal;
    private Perlin heightMap;

    [SerializeField] private Material sand;
    [SerializeField] private Material stone;
    [SerializeField] private Material grass;
    [SerializeField] private Material deepWater;
    [SerializeField] private Material shallowWater;
    [SerializeField] private Material snow;
    [SerializeField] private Material dirt;
    [SerializeField] private Material ice;
    [SerializeField] private Material mud;
    [SerializeField] private Material rubble;
    [SerializeField] private Material floor;
    [SerializeField] private Material wall;
    [SerializeField] private Material pillar;
    [SerializeField] private Material tree;
    [SerializeField] private Material path;
    [SerializeField] private Material lava;
    [SerializeField] private Material poison;

    [SerializeField] private Warrior warriorPrefab;
    [SerializeField] private List<Transform> formations;
    private Transform[] positions;

    private Dictionary<BattlefieldTile.BattlefieldTileType, Material> battlefieldMaterials;

    public void setup(CharacterTeam[] initialEngagement)
    {
        StaticData.pauseWM(null);
        coreMinX = int.MaxValue;
        coreMaxX = int.MinValue;
        coreMinY = int.MaxValue;
        coreMaxY = int.MinValue;
        int[] center = new int[2];
        affiliationsInvolved = new List<Affiliation>();
        foreach (CharacterTeam team in initialEngagement)
        {
            affiliationsInvolved.Add(team.getAffiliation());

            int xCoord = team.xCoord;
            int yCoord = team.yCoord;

            coreMinX = Mathf.Min(coreMinX, xCoord);
            coreMaxX = Mathf.Max(coreMaxX, xCoord);
            coreMinY = Mathf.Min(coreMinY, yCoord);
            coreMaxY = Mathf.Max(coreMaxY, yCoord);

            center[0] += xCoord;
            center[1] += yCoord;
        }
        center[0] /= initialEngagement.Length;
        center[1] /= initialEngagement.Length;

        //TODO ACCOUNT FOR WORLDMAP SIZE LIMITS (don't try to access negative or overly positive coords)
        coreMinX = Mathf.Min(coreMinX, center[0] - BATTLEFIELD_MIN_CORE_RADIUS);
        coreMaxX = Mathf.Max(coreMaxX, center[0] + BATTLEFIELD_MIN_CORE_RADIUS);
        coreMinY = Mathf.Min(coreMinY, center[1] - BATTLEFIELD_MIN_CORE_RADIUS);
        coreMaxY = Mathf.Max(coreMaxY, center[1] + BATTLEFIELD_MIN_CORE_RADIUS);

        teams = new List<CharacterTeam>();

        for (int q = coreMinX; q <= coreMaxX; q++)
        {
            for (int w = coreMinY; w <= coreMaxY; w++)
            {
                WorldMapTile tile = StaticData.worldMap.at(q, w);
                if (tile.getCurrentOccupants() != null)
                {
                    teams.Add(tile.getCurrentOccupants());
                    if (!affiliationsInvolved.Contains(tile.getCurrentOccupants().getAffiliation()))
                    {
                        affiliationsInvolved.Add(tile.getCurrentOccupants().getAffiliation());
                    }
                }
            }
        }

        periMinX = center[0] - BATTLEFIELD_MAX_PERIPHERAL_RADIUS;
        periMaxX = center[0] + BATTLEFIELD_MAX_PERIPHERAL_RADIUS;
        periMinY = center[1] - BATTLEFIELD_MAX_PERIPHERAL_RADIUS;
        periMaxY = center[1] + BATTLEFIELD_MAX_PERIPHERAL_RADIUS;

        for (int q = periMinX; q < periMaxX; q++)
        {
            if (q == coreMinX)
            {
                q = coreMaxX;
                continue;
            }
            for (int w = periMinY; w < periMaxY; w++)
            {
                if (w == coreMinY)
                {
                    w = coreMaxY;
                    continue;
                }
                WorldMapTile tile = StaticData.worldMap.at(q, w);
                if (tile.getCurrentOccupants() != null
                    && affiliationsInvolved.Contains(tile.getCurrentOccupants().getAffiliation()))
                {
                    reinforcements.Add(tile.getCurrentOccupants());
                    //TODO calculate timer
                }
            }
        }

        bfMap = new BattlefieldTile[(coreMaxX + 1 - coreMinX) * BATTLEFIELD_TO_WORLDTILE_RATIO][];
        for (int q = 0; q < bfMap.Length; q++)
        {
            bfMap[q] = new BattlefieldTile[(coreMaxY + 1 - coreMinY) * BATTLEFIELD_TO_WORLDTILE_RATIO];
        }

        heightMap = new Perlin();
        heightMap.Seed = StaticData.battlefieldPerlinSeed;
        VisualMapGenerator.setPerlinSettings(new Perlin[] { heightMap },
            0.05, 0.5, 3, 2);

        battlefieldMaterials = new Dictionary<BattlefieldTile.BattlefieldTileType, Material>();
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.SAND, sand);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.GRASS, grass);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.STONE, stone);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.DEEP_WATER, deepWater);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.SHALLOW_WATER, shallowWater);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.SNOW, snow);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.DIRT, dirt);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.ICE, ice);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.MUD, mud);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.RUBBLE, rubble);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.FLOOR, floor);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.WALL, wall);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.PILLAR, pillar);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.TREE, tree);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.PATH, path);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.LAVA, lava);
        battlefieldMaterials.Add(BattlefieldTile.BattlefieldTileType.POISON, poison);

        xWorkingVal = coreMinX * BATTLEFIELD_TO_WORLDTILE_RATIO;
        yWorkingVal = coreMinY * BATTLEFIELD_TO_WORLDTILE_RATIO;

        workingStage = 1;
    }

    private void generateTile()
    {
        WorldMap wm = StaticData.worldMap;
        int wmTileX = xWorkingVal / BATTLEFIELD_TO_WORLDTILE_RATIO;
        int wmTileY = yWorkingVal / BATTLEFIELD_TO_WORLDTILE_RATIO;
        int relativePosX = xWorkingVal - (coreMinX * BATTLEFIELD_TO_WORLDTILE_RATIO);
        int relativePosY = yWorkingVal - (coreMinY * BATTLEFIELD_TO_WORLDTILE_RATIO);

        double height = (heightMap.GetValue(xWorkingVal, yWorkingVal, 0) + 1) / 2;

        height *= wm.at(wmTileX, wmTileY).getHeight() + 1;

        //TODO Bilerp, so the BFTile is also affected by the surrounding WMTiles

        int leftX = Mathf.Max(wmTileX, 0);
        int rightX = Mathf.Min(wmTileX, Mathf.Min(wmTileY, wm.getMap().Length - 1));
        int topY = Mathf.Max(wmTileY, 0);
        int bottomY = Mathf.Min(wmTileY, wm.getMap()[0].Length - 1);

        float topRight = (float)(wm.at(rightX, topY).getHeight() + 1);
        float bottomRight = (float)(wm.at(rightX, bottomY).getHeight() + 1);
        float topLeft = (float)(wm.at(leftX, topY).getHeight() + 1);
        float bottomLeft = (float)(wm.at(leftX, bottomY).getHeight() + 1);

        float relativeXFloat = (xWorkingVal % BATTLEFIELD_TO_WORLDTILE_RATIO) / BATTLEFIELD_TO_WORLDTILE_RATIO;
        float relativeYFloat = (yWorkingVal % BATTLEFIELD_TO_WORLDTILE_RATIO) / BATTLEFIELD_TO_WORLDTILE_RATIO;

        float topXLerp = Mathf.Lerp(topLeft, topRight, relativeXFloat);
        float bottomXLerp = Mathf.Lerp(bottomLeft, bottomRight, relativeXFloat);
        float fullLerp = Mathf.Lerp(topXLerp, bottomXLerp, relativeYFloat);

        height *= fullLerp;

        BattlefieldTile tile = new BattlefieldTile(height);
        bfMap[relativePosX][relativePosY] = tile;
        BattlefieldTile.BattlefieldTileType bfTileType = getMaterial(StaticData.worldMap.at(xWorkingVal / BATTLEFIELD_TO_WORLDTILE_RATIO, yWorkingVal / BATTLEFIELD_TO_WORLDTILE_RATIO).getType());
        if (bfTileType == BattlefieldTile.BattlefieldTileType.SHALLOW_WATER
            || bfTileType == BattlefieldTile.BattlefieldTileType.DEEP_WATER)
        {
            tile.setHeight(0);
        }
        makeTile(relativePosX, relativePosY, battlefieldMaterials[bfTileType]);

        xWorkingVal++;
        if (xWorkingVal == (coreMaxX * BATTLEFIELD_TO_WORLDTILE_RATIO) - 1)
        {
            xWorkingVal = coreMinX * BATTLEFIELD_TO_WORLDTILE_RATIO;
            yWorkingVal++;
        }
        if (yWorkingVal == (coreMaxY * BATTLEFIELD_TO_WORLDTILE_RATIO) - 1)
        {
            workingStage = 2;
        }
    }
    private void makeTile(int x, int y, Material mat)
    {
        Tile tile = Instantiate(tilePrefab, StaticData.findDeepChild(transform, "Terrain"));
        tile.transform.position = new Vector3(x, 0, y);
        tile.setMaterial(mat);
        tile.draw(x, y, bfMap[x][y], 0);
    }

    private BattlefieldTile.BattlefieldTileType getMaterial(WorldMapTile.WorldMapTileType tileType)
    {
        //TODO figure out the randomization process
        return tileType.getTileTypeWithRandomNumber0to99(0);
    }

    private void generateBuildingsAndNature()
    {
        //TODO Buildings and nature
        for (int q = coreMinX; q < coreMaxX; q++)
        {
            for (int w = coreMinY; w < coreMaxY; w++)
            {
                WorldMapTile tile = StaticData.worldMap.at(q, w);
                double natureArrangement = tile.getHeight() + tile.getHeat() + tile.getMoisture() + tile.getMagicPotency();

                //TODO Use the tile associated CityState's dominant cultural value to decide which
                //preset arrangement to use for the buildings. If the tile is unowned, ignore this step

                //TODO Use natureArrangement to decide which preset arrangement to use for nature elements,
                //ignoring BF tiles that already have buildings on them
            }
        }

        workingStage = 3;
    }
    private void setupFormations()
    {
        positions = new Transform[teams.Count];
        for (int q = 0; q < teams.Count; q++)
        {
            positions[q] = Instantiate(formations[teams[q].getFormation()]);
        }

        workingStage = 4;
    }
    private void placeUnits()
    {
        for (int q = 0; q < teams.Count; q++)
        {
            CharacterTeam team = teams[q];
            int xPos = team.xCoord - coreMinX;
            int yPos = team.yCoord - coreMinY;
            float xMid = (coreMinX + coreMaxX) / 2;
            float yMid = (coreMinY + coreMaxY) / 2;
            Transform pos = positions[q];
            pos.SetPositionAndRotation(new Vector3(xPos * BATTLEFIELD_TO_WORLDTILE_RATIO, (float)bfMap[xPos][yPos].getHeight(), yPos * BATTLEFIELD_TO_WORLDTILE_RATIO),
                Quaternion.LookRotation(new Vector3(xMid, 0, yMid)
                - new Vector3(xPos, 0, yPos)));
            for (int w = 0; w < team.size(); w++)
            {
                Warrior war = Instantiate(warriorPrefab, pos.position, pos.rotation);
                war.setData(team.getMember(w));
            }
            Destroy(pos.gameObject);
        }

        workingStage = 0;
    }

    public void startBattle()
    {
        //TODO
    }

    void Update()
    {
        if (workingStage != 0)
        {
            if (workingStage == 1)
            {
                generateTile();
            }
            else if (workingStage == 2)
            {
                generateBuildingsAndNature();
            }
            else if (workingStage == 3)
            {
                setupFormations();
            }
            else if (workingStage == 4)
            {
                placeUnits();
            }
        }
    }
}
