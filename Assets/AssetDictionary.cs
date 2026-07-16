using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AssetDictionary : MonoBehaviour
{
    [SerializeField] private List<Projectile> projectiles;
    [SerializeField] private List<string> projectileNames;
    [SerializeField] private List<GameObject> weapons;
    [SerializeField] private BuildSite buildSite;

    private static Dictionary<string, Projectile> projectileDictionary;
    private static BuildSite buildSitePrefab;
    void Awake()
    {
        buildSitePrefab = buildSite;
        projectileDictionary = new Dictionary<string, Projectile>();
        for (int q = 0; q < projectiles.Count; q++)
        {
            projectileDictionary.Add(projectileNames[q], projectiles[q]);
        }
    }
    public static Projectile getProjectilePrefab(string projectileName)
    {
        return projectileDictionary[projectileName];
    }

    public static BuildSite newBuildSite()
    {
        return Instantiate(buildSitePrefab);
    }
}
