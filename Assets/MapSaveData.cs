using UnityEngine;

[System.Serializable]
public class MapSaveData
{
    public int xDimension;
    public int yDimension;
    public int seed;

    public double[] heat;
    public double[] moisture;
    public double[] height;
    public int[] magicPotency;
    public int[] magicType;

    public MapSaveData(VisualMapGenerator gen)
    {
        WorldMapTile[][] map = gen.getMap();
        xDimension = map.Length;
        yDimension = map[0].Length;

        seed = gen.getSeed();

        int tileCount = xDimension * yDimension;
        heat = new double[tileCount];
        moisture = new double[tileCount];
        height = new double[tileCount];
        magicType = new int[tileCount];
        magicPotency = new int[tileCount];
        int counter = 0;
        for (int q = 0; q < xDimension; q++)
        {
            for (int w = 0; w < yDimension; w++)
            {
                heat[counter] = map[q][w].getHeat();
                moisture[counter] = map[q][w].getMoisture();
                height[counter] = map[q][w].getHeight();
                magicType[counter] = map[q][w].getMagicType();
                magicPotency[counter] = map[q][w].getMagicPotency();

                counter++;
            }
        }
    }
}
