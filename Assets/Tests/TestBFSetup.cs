using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBFSetup : MonoBehaviour
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
    [SerializeField] private Warrior warriorPrefab;
    [SerializeField] private CustomNavMesh navMesh;

    private float aiUpdateTimer;

    private List<Warrior> warriors;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputSpecs.setMaxPlayers(testMaxPlayers);
        StaticData.affiliations = new List<Affiliation>();

        navMesh.bake(-50, 50, -100, 100, -50, 50);
        warriors = new List<Warrior>();
        Affiliation plyrAff = new Affiliation();
        Affiliation enmyAff = new Affiliation();
        StaticData.affiliations.Add(plyrAff);
        StaticData.affiliations.Add(enmyAff);
        makeTeam(plyrAff, 1, playerPos);
        makeTeam(enmyAff, 1, enemyPos);
    }
    private void makeTeam(Affiliation aff, int num, Transform[] pos)
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
            Warrior war = Instantiate(warriorPrefab, pos[q].position, Quaternion.identity);
            war.GetComponent<CustomNavAgent>().setActive(true);
            warriors.Add(war);
        }
    }

    void Update()
    {
        aiUpdateTimer += Time.deltaTime;
        if (aiUpdateTimer > 10)
        {
            aiUpdateTimer = 0;
            foreach (Warrior war in warriors)
            {
                war.updateAI(80);
            }
        }
    }
}
