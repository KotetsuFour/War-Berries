using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Affiliation
{
    private int id;
    private Affiliation suzerain;
    private List<CharacterData> members;
    public Affiliation()
    {
        if (StaticData.affiliations.Count == 0)
        {
            id = 0;
        }
        else
        {
            id = StaticData.affiliations[StaticData.affiliations.Count - 1].id + 1;
        }
        StaticData.affiliations.Add(this);
        members = new List<CharacterData>();
    }
    public Affiliation(int id)
    {
        this.id = id;
        StaticData.affiliations.Add(this);
    }

    public bool answersTo(Affiliation aff)
    {
        Affiliation test = this;
        while (test != null)
        {
            if (test == aff)
            {
                return true;
            }
            test = test.suzerain;
        }
        return false;
    }
    public void join(BerryData data)
    {
        members.Add(data);
    }
}
