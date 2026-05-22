using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneralManager : MonoBehaviour
{
    public WorldMapDisplay mapDisplay;

    void Awake()
    {
        StaticData.WMPaused = true;
        StaticData.BFPaused = true;
    }
    public void displayMap()
    {
        mapDisplay.generateMap();
        mapDisplay.gameObject.SetActive(true);
        mapDisplay.GetComponent<CustomNavMesh>().bake(0, StaticData.worldMap.getMap().Length,
            -100, 100,
            0, StaticData.worldMap.getMap().GetLength(1));
    }
    public void hideMap()
    {
        mapDisplay.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        StaticData.updateTime();
    }
}
