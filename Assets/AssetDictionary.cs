using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssetDictionary : MonoBehaviour
{

    private static Dictionary<string, Projectile> projectileDictionary;
    public static Projectile getProjectilePrefab(string projectileName)
    {
        return projectileDictionary[projectileName];
    }
}
