using UnityEngine;

public class Weapon : Item
{
    //THIS SECTION APPLIES TO BOTH MELEE AND RANGE
    public float cooldown;
    //Damage types
    public float bluntAttackPower;
    public float sliceAttackPower;
    public float pierceAttackPower;
    //Needed mostly for NPCs
    public float maxRecommendedRange;
}
