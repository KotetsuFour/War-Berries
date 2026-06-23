using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BoxCollider))]
public class Tile : MonoBehaviour
{
    [SerializeField] private Material moveHighlight;
    [SerializeField] private Material attackHighlight;
    [SerializeField] private Material interactHighlight;

    [SerializeField] private GameObject castle;
    [SerializeField] private GameObject village;
    [SerializeField] private GameObject farm;
    [SerializeField] private GameObject storehouse;
    [SerializeField] private GameObject miningFacility;
    [SerializeField] private GameObject trainingFacility;

    public int x, y;
    public WorldMapTile tile;

    public static float HALF_LENGTH = 0.5f;
    public static float TILE_HEIGHT_MULTIPLIER = 0.25f;
    public static float MAX_DECORATION_HEIGHT = 100;

    private Vector3 cursorPosition;

    public void draw(int x, int y, WorldMapTile tile, float seaLevel)
    {
        this.x = x;
        this.y = y;
        this.tile = tile;

        float meshHeight = (/*tile.getType().getHeight()*/Mathf.Max(0, ((float)tile.getHeight() - seaLevel) * 10f)) * TILE_HEIGHT_MULTIPLIER;
        setUtilityPositions(meshHeight);

        Mesh mesh = new Mesh
        {
            name = "TileMesh"
        };

        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(new Vector3(-HALF_LENGTH, meshHeight, -HALF_LENGTH));
        vertices.Add(new Vector3(-HALF_LENGTH, meshHeight, HALF_LENGTH));
        vertices.Add(new Vector3(HALF_LENGTH, meshHeight, HALF_LENGTH));
        vertices.Add(new Vector3(HALF_LENGTH, meshHeight, -HALF_LENGTH));

        List<int> triangles = new List<int>(new int[] { 0, 1, 2, 0, 2, 3 });

        List<Vector3> normals = new List<Vector3>();
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);
        normals.Add(Vector3.up);

        List<Vector2> uvs = new List<Vector2>();
        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));

        if (meshHeight > 0)
        {
            //Front
            vertices.Add(new Vector3(-HALF_LENGTH, meshHeight, -HALF_LENGTH)); //4
            vertices.Add(new Vector3(HALF_LENGTH, meshHeight, -HALF_LENGTH)); //5
            vertices.Add(new Vector3(-HALF_LENGTH, 0, -HALF_LENGTH)); //6
            vertices.Add(new Vector3(HALF_LENGTH, 0, -HALF_LENGTH)); //7

            triangles.AddRange(new int[] { 4, 7, 6, 4, 5, 7 });

            normals.Add(new Vector3(0, 0, -1));
            normals.Add(new Vector3(0, 0, -1));
            normals.Add(new Vector3(0, 0, -1));
            normals.Add(new Vector3(0, 0, -1));

            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));

            //Left
            vertices.Add(new Vector3(-HALF_LENGTH, meshHeight, -HALF_LENGTH)); //8
            vertices.Add(new Vector3(-HALF_LENGTH, meshHeight, HALF_LENGTH)); //9
            vertices.Add(new Vector3(-HALF_LENGTH, 0, -HALF_LENGTH)); //10
            vertices.Add(new Vector3(-HALF_LENGTH, 0, HALF_LENGTH)); //11

            triangles.AddRange(new int[] { 8, 10, 9, 9, 10, 11 });

            normals.Add(new Vector3(-1, 0, 0));
            normals.Add(new Vector3(-1, 0, 0));
            normals.Add(new Vector3(-1, 0, 0));
            normals.Add(new Vector3(-1, 0, 0));

            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));

            //Back
            vertices.Add(new Vector3(-HALF_LENGTH, meshHeight, HALF_LENGTH)); //12
            vertices.Add(new Vector3(HALF_LENGTH, meshHeight, HALF_LENGTH)); //13
            vertices.Add(new Vector3(-HALF_LENGTH, 0, HALF_LENGTH)); //14
            vertices.Add(new Vector3(HALF_LENGTH, 0, HALF_LENGTH)); //15

            triangles.AddRange(new int[] { 12, 14, 13, 13, 14, 15 });

            normals.Add(new Vector3(0, 0, 1));
            normals.Add(new Vector3(0, 0, 1));
            normals.Add(new Vector3(0, 0, 1));
            normals.Add(new Vector3(0, 0, 1));

            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));

            //Right
            vertices.Add(new Vector3(HALF_LENGTH, meshHeight, HALF_LENGTH)); //16
            vertices.Add(new Vector3(HALF_LENGTH, meshHeight, -HALF_LENGTH)); //17
            vertices.Add(new Vector3(HALF_LENGTH, 0, HALF_LENGTH)); //18
            vertices.Add(new Vector3(HALF_LENGTH, 0, -HALF_LENGTH)); //19

            triangles.AddRange(new int[] { 16, 18, 17, 17, 18, 19 });

            normals.Add(new Vector3(1, 0, 0));
            normals.Add(new Vector3(1, 0, 0));
            normals.Add(new Vector3(1, 0, 0));
            normals.Add(new Vector3(1, 0, 0));

            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(0, 0));
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.SetUVs(0, uvs);

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<BoxCollider>().center = new Vector3(0, meshHeight / 2, 0);
        GetComponent<BoxCollider>().size = new Vector3(HALF_LENGTH * 2, meshHeight + 0.1f, HALF_LENGTH * 2);
    }
    public void setMaterial(Material mat)
    {
        GetComponent<MeshRenderer>().material = mat;
    }
    public WorldMapTile getTile()
    {
        return tile;
    }
    /*
    public void setOccupant(UnitModel occupant)
    {
        this.occupant = occupant;
        if (occupant != null)
        {
            occupant.setTile(this);
        }
    }
    public UnitModel getOccupant()
    {
        return occupant;
    }
    public bool isVacant()
    {
        return occupant == null;
    }
    */
    public string getName()
    {
        return tile.getType().getName();
    }
    public void decorate(GameObject deco)
    {
        deco.transform.position = getDecoPosition().position;
        deco.transform.SetParent(transform);
    }
    public Transform getDecoPosition()
    {
        return StaticData.findDeepChild(transform, "DecorationPosition");
    }
    public Transform getStage()
    {
        return StaticData.findDeepChild(transform, "UnitPosition");
    }
    public Transform getHighlight()
    {
        return StaticData.findDeepChild(transform, "Highlight");
    }
    public Vector3 getCursorPosition()
    {
        return cursorPosition;
    }
    private void setUtilityPositions(float stageHeight)
    {
        getStage().Translate(0, stageHeight - getStage().position.y, 0);
        getDecoPosition().Translate(0, stageHeight - getDecoPosition().position.y, 0);
        getHighlight().Translate(0, stageHeight + 0.05f - getHighlight().position.y, 0); ;
        cursorPosition = getHighlight().position + new Vector3(0, 0.01f, 0);
    }
    public void highlightMove()
    {
        getHighlight().gameObject.SetActive(true);
        getHighlight().GetComponent<MeshRenderer>().material = moveHighlight;
    }
    public void highlightAttack()
    {
        getHighlight().gameObject.SetActive(true);
        getHighlight().GetComponent<MeshRenderer>().material = attackHighlight;
    }
    public void highlightInteract()
    {
        getHighlight().gameObject.SetActive(true);
        getHighlight().GetComponent<MeshRenderer>().material = interactHighlight;
    }
    public void unhighlight()
    {
        getHighlight().gameObject.SetActive(false);
    }

    /*
    public void updateDeco()
    {
        Building b = tile.getBuilding();
        if (b == null)
        {
            return;
        }
        else if (b is Castle)
        {
            decorate(Instantiate(castle));
        }
        else if (b is Village)
        {
            decorate(Instantiate(village));
        }
        else if (b is Farm)
        {
            decorate(Instantiate(farm));
        }
        else if (b is Storehouse)
        {
            decorate(Instantiate(storehouse));
        }
        else if (b is MiningFacility)
        {
            decorate(Instantiate(miningFacility));
        }
        else if (b is TrainingFacility)
        {
            decorate(Instantiate(trainingFacility));
        }
    }
    */
}
