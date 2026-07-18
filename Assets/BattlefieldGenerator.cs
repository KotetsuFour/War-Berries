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
    public const int ARBITRARY_HIGH_RAYCAST_START_HEIGHT = 100;
    public const float TINY_HEIGHT_ABOVE_MESH = 0.001f;
    public const float SAFETY_HEIGHT = 0.2f;
    public const float SLAB_THICKNESS = 1;
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
    [SerializeField] private LayerMask terrainLayer;
    private Transform[] positions;
    List<Vector3> colliderMeshVertices;
    List<int> colliderMeshTriangles;
    List<Vector3> colliderMeshNormals;
    List<Vector2> colliderMeshUVs;
    private Mesh colliderMesh;

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

        colliderMeshVertices = new List<Vector3>();
        colliderMeshTriangles = new List<int>();
        colliderMeshNormals = new List<Vector3>();
        colliderMeshUVs = new List<Vector2>();

        xWorkingVal = 0;
        yWorkingVal = 0;

        workingStage = 1;
    }

    private void generateTile()
    {
        WorldMap wm = StaticData.worldMap;
        Vector2 wmCoords = getWMTileAccordingToBFTile(xWorkingVal, yWorkingVal);
        int wmTileX = Mathf.RoundToInt(wmCoords.x);
        int wmTileY = Mathf.RoundToInt(wmCoords.y);
        int bfPosXWithRespectToWM = xWorkingVal + (coreMinX * BATTLEFIELD_TO_WORLDTILE_RATIO);
        int bfPosYWithRespectToWM = yWorkingVal + (coreMinY * BATTLEFIELD_TO_WORLDTILE_RATIO);

        double height = (heightMap.GetValue(bfPosXWithRespectToWM, bfPosYWithRespectToWM, 0) + 1) / 2;

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

        if (wm.at(wmTileX, wmTileY).getHeight() < VisualMapGenerator.getSeaLevel())
        {
            height = Mathf.Min(VisualMapGenerator.getSeaLevel(), (float)height);
        }

        BattlefieldTile tile = new BattlefieldTile(height);
        bfMap[xWorkingVal][yWorkingVal] = tile;

        xWorkingVal++;
        if (xWorkingVal == bfMap.Length)
        {
            xWorkingVal = 0;
            yWorkingVal++;
        }
        if (yWorkingVal == bfMap[xWorkingVal].Length)
        {
            workingStage = 2;
        }
    }
    private void renderTiles()
    {
        for (int q = 0; q < bfMap.Length; q++)
        {
            for (int w = 0; w < bfMap[q].Length; w++)
            {
                Vector2 wmCoords = getWMTileAccordingToBFTile(q, w);
                int wmTileX = Mathf.RoundToInt(wmCoords.x);
                int wmTileY = Mathf.RoundToInt(wmCoords.y);
                BattlefieldTile.BattlefieldTileType bfTileType = getMaterial(StaticData.worldMap.at(wmTileX, wmTileY).getType());
                makeTile(q, w, battlefieldMaterials[bfTileType]);
            }
        }

        workingStage = 3;
    }
    private void makeTile(int x, int y, Material mat)
    {
        Tile tile = Instantiate(tilePrefab, StaticData.findDeepChild(transform, "Terrain"));
        tile.transform.position = new Vector3(x, 0, y);
        tile.drawBFTile(bfMap, x, y, mat);

        Mesh mesh = tile.getMesh();
        Vector3[] vert = mesh.vertices;
        Vector3[] normals = mesh.normals;
        Vector3[] verticesToAdd = new Vector3[vert.Length * 2];
        Vector3[] normalsToAdd = new Vector3[vert.Length * 2];
        for (int q = 0; q < vert.Length; q++)
        {
            verticesToAdd[q] = new Vector3(vert[q].x + x, vert[q].y + TINY_HEIGHT_ABOVE_MESH,
                vert[q].z + y);
            verticesToAdd[q + vert.Length] = verticesToAdd[q] + new Vector3(0, -SLAB_THICKNESS, 0);

            normalsToAdd[q] = Vector3.up;
            normalsToAdd[q + normals.Length] = Vector3.down;
        }

        int offset = colliderMeshVertices.Count;

        int[] tri = mesh.triangles;
        int[] trianglesToAdd = new int[tri.Length * 2];
        for (int q = 0; q < tri.Length; q += 3)
        {
            trianglesToAdd[q] = tri[q] + offset;
            trianglesToAdd[q + 1] = tri[q + 1] + offset;
            trianglesToAdd[q + 2] = tri[q + 2] + offset;
            trianglesToAdd[tri.Length + q] = tri[q] + vert.Length + offset;
            trianglesToAdd[tri.Length + q + 1] = tri[q + 2] + vert.Length + offset;
            trianglesToAdd[tri.Length + q + 2] = tri[q + 1] + vert.Length + offset;
        }

        colliderMeshVertices.AddRange(verticesToAdd);
        colliderMeshTriangles.AddRange(trianglesToAdd);
        colliderMeshNormals.AddRange(normalsToAdd);
    }
    private BattlefieldTile.BattlefieldTileType getMaterial(WorldMapTile.WorldMapTileType tileType)
    {
        //TODO figure out the randomization process
        return tileType.getTileTypeWithRandomNumber0to99(0);
    }

    private void setCollider()
    {
        colliderMesh = new Mesh
        {
            name = "ColliderMesh",
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        colliderMesh.SetVertices(colliderMeshVertices.ToArray());
        colliderMesh.SetNormals(colliderMeshNormals.ToArray());
        colliderMesh.SetTriangles(colliderMeshTriangles.ToArray(), 0);
        //        colliderMesh.SetUVs(0, colliderMeshUVs);

        MeshCollider coll = GetComponent<MeshCollider>();
        coll.sharedMesh = null;
        coll.sharedMesh = colliderMesh;
        colliderMesh.RecalculateBounds();
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

        workingStage = 4;
    }
    private void setupFormations()
    {
        positions = new Transform[teams.Count];
        for (int q = 0; q < teams.Count; q++)
        {
            positions[q] = Instantiate(formations[teams[q].getFormation()]);
        }

        workingStage = 5;
    }
    private void placeUnits()
    {
        for (int q = 0; q < teams.Count; q++)
        {
            CharacterTeam team = teams[q];
            Vector2 teamBFCoords = getWMTileCenterAsBFCoords(team.xCoord, team.yCoord);
            Vector3 teamPos = new Vector3(teamBFCoords.x, 0, teamBFCoords.y);
            float xMid = bfMap.Length / 2;
            float yMid = bfMap[0].Length / 2;
            Transform pos = positions[q];
            pos.SetPositionAndRotation(teamPos,
                Quaternion.LookRotation(new Vector3(xMid, 0, yMid)
                - new Vector3(teamPos.x, 0, teamPos.z)));
            for (int w = 0; w < team.size(); w++)
            {
                Vector3 spawn = pos.GetChild(w).position;
                RaycastHit hit;
                Physics.Raycast(new Vector3(spawn.x, ARBITRARY_HIGH_RAYCAST_START_HEIGHT, spawn.z), Vector3.down,
                    out hit, float.MaxValue, terrainLayer);
                Vector3 exactSpawnPoint = hit.point + new Vector3(0, SAFETY_HEIGHT, 0);
                Warrior war = Instantiate(warriorPrefab, exactSpawnPoint, pos.GetChild(w).rotation);
                war.setData(team.getMember(w));
            }
            team.setTempPositionObject(pos);
        }

        workingStage = 0;
    }

    public Vector2 getWMTileCenterAsBFCoords(int wmXPos, int wmYPos)
    {
        Vector2 ret = new Vector2();
        ret.x = ((wmXPos - coreMinX) * BATTLEFIELD_TO_WORLDTILE_RATIO) + (BATTLEFIELD_TO_WORLDTILE_RATIO / 2);
        ret.y = ((wmYPos - coreMinY) * BATTLEFIELD_TO_WORLDTILE_RATIO) + (BATTLEFIELD_TO_WORLDTILE_RATIO / 2);
        return ret;
    }

    public Vector2 getWMTileAccordingToBFTile(int bfX, int bfY)
    {
        Vector2 ret = new Vector2();
        ret.x = (bfX / BATTLEFIELD_TO_WORLDTILE_RATIO) + coreMinX;
        ret.y = (bfY / BATTLEFIELD_TO_WORLDTILE_RATIO) + coreMinY;
        return ret;
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
                renderTiles();
            }
            else if (workingStage == 3)
            {
                setCollider();
                generateBuildingsAndNature();
            }
            else if (workingStage == 4)
            {
                setupFormations();
            }
            else if (workingStage == 5)
            {
                placeUnits();
            }
        }
    }
}
