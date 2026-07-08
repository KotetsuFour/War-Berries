using UnityEngine;

public abstract class TileData
{
    private double height;
    public TileData(double height)
    {
        this.height = height;
    }
    public abstract string getTypeName();
    public double getHeight()
    {
        return height;
    }
    public void setHeight(double height)
    {
        this.height = height;
    }
}
