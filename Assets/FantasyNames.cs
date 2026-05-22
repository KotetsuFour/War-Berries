using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Syllabore;

public static class FantasyNames
{
    public static NameGenerator twoSylls = new NameGenerator();
    public static NameGenerator thirdSyll = new NameGenerator().UsingTransform(x => x
        .ReplaceSyllable(0, ""));
    public static string getName()
    {
        string first = twoSylls.Next();
        if (Random.Range(0, 10) < 3)
        {
            first += thirdSyll.Next().ToLower();
        }
        return first;
    }
}
