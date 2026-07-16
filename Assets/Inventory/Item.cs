using UnityEngine;

public class Item
{
    public string itemName;
    public float weight;
    public float size;
    public int initialUses;

    public enum ResouceType
    {
        //DO NOT REORDER THESE!
        FOOD, ORE, WOOD, CLAY, FABRIC, BOUND
    }
}
