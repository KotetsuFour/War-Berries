using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BerryData : CharacterData
{
    public BerryData(string id, string unitName, Affiliation affiliation, CharacterTeam team,
        double timeBorn, float strength, float accuracy, float luck, float leadership, float strengthGrowth,
        float accuracyGrowth, float luckGrowth, float leadershipGrowth, string[] battles, int[][] battleStats,
        float[] bodyPartsMaxHP, float[] bodyPartsCurrentHP, float[] bodyPartsHPGrowthRates)
        : base(id, unitName, affiliation, team,
        timeBorn, strength, accuracy, luck, leadership, strengthGrowth,
        accuracyGrowth, luckGrowth, leadershipGrowth, battles, battleStats,
        bodyPartsMaxHP, bodyPartsCurrentHP, bodyPartsHPGrowthRates)
    {

    }

    public BerryData(string id, Affiliation affiliation, double timeBorn,
        Color baseHair, Color baseSkin, Color baseEye, int variance) : base(id, timeBorn, affiliation)
    {
        Demeanor[] allDems = (Demeanor[])Enum.GetValues(typeof(Demeanor));
        CombatTrait[] allComs = (CombatTrait[])Enum.GetValues(typeof(CombatTrait));
        Interest[] allInts = (Interest[])Enum.GetValues(typeof(Interest));
        setDemeanor(allDems);
        setSkills(allComs);
        setInterestsAndDisinterests(allInts);

        setColorsWithVariance(baseHair, baseSkin, baseEye, variance);
        affiliation.join(this);
    }
}
