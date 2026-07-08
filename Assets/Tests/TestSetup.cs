using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSetup : MonoBehaviour
{
    [SerializeField] private GameObject olderMale;
    [SerializeField] private GameObject olderFemale;
    [SerializeField] private GameObject youngerMale;
    [SerializeField] private GameObject youngerFemale;

    [SerializeField] private int testMaxPlayers;
    [SerializeField] private InputManager inputSpecs;
    [SerializeField] private GeneralManager manager;
    [SerializeField] private Transform[] playerPos;
    [SerializeField] private Transform[] enemyPos;
    [SerializeField] private WMTeam teamPrefab;

    // Start is called before the first frame update
    void Start()
    {
        StaticData.setWorldMap(null);
        manager.displayMap();
        inputSpecs.setMaxPlayers(testMaxPlayers);
        StaticData.affiliations = new List<Affiliation>();

        Affiliation plyrAff = new Affiliation();
        Affiliation enmyAff = new Affiliation();
        StaticData.affiliations.Add(plyrAff);
        StaticData.affiliations.Add(enmyAff);
        makeTeam(plyrAff, 20, playerPos[0].position);
        makeTeam(enmyAff, 20, enemyPos[0].position);
    }
    private void makeTeam(Affiliation aff, int num, Vector3 pos)
    {
        CharacterTeam team = null;
        for (int q = 0; q < num; q++)
        {
            BerryData data = new BerryData($"TestBerry{num}", aff, 0, Color.blue,
                Color.blue, Color.blue, 0);
            if (team == null)
            {
                team = new CharacterTeam(data);
            }
            else
            {
                team.add(data);
            }
        }
        WMTeam wm = Instantiate(teamPrefab, pos, Quaternion.identity);
        wm.setTeam(team);
    }
}
