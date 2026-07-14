using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Affiliation
{
    private int id;
    private Affiliation suzerain; //Note that an affiliation with a suzerain CANNOT gain
                                  //subordinates of its own (for simplicity)
    private List<CharacterData> members;
    private float[] values; //Nationalism, Altruism, Familism, Militarism
    private List<WorldMapTile> tiles;
    private int population;
    private LinkedList<Task> taskQueue;
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
        values = new float[4];
        tiles = new List<WorldMapTile>();
        taskQueue = new LinkedList<Task>();
    }
    public Affiliation(int id)
    {
        this.id = id;
        StaticData.affiliations.Add(this);
    }

    public void alterPopulation(int amount)
    {
        population = Mathf.Max(0, population + amount);
    }

    public int getPopulation()
    {
        int ret = population;
        foreach (Affiliation aff in StaticData.affiliations)
        {
            if (aff.answersTo(this))
            {
                ret += aff.population;
            }
        }
        return ret;
    }

    public void addTile(WorldMapTile tile, int[] values)
    {
        int tileCount = tiles.Count;
        for (int q = 0; q < this.values.Length; q++)
        {
            this.values[q] *= tileCount;
            this.values[q] += values[q];
            this.values[q] /= tileCount + 1;
        }
        tiles.Add(tile);
    }

    public int getTileCount()
    {
        return tiles.Count;
    }
    
    public int getTotalTileCount()
    {
        int ret = tiles.Count;
        foreach (Affiliation aff in StaticData.affiliations)
        {
            if (aff != this && aff.answersTo(this))
            {
                ret += aff.getTotalTileCount();
            }
        }
        return ret;
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

    public void assessOptions()
    {
        LinkedListNode<Task> current = taskQueue.First;
        
        while (current != null)
        {
            //TODO Calculate heuristic
            LinkedListNode<Task> placeholder = current.Next;
            while (current.Previous != null && current.Value.heuristic > current.Previous.Value.heuristic)
            {
                LinkedListNode<Task> prev = current.Previous;
                taskQueue.Remove(current);
                taskQueue.AddBefore(prev, current);
            }
            current = placeholder;
        }

        //TODO Then, with all options weighed, go through the list, completing tasks until you've
        //run out of energy. Some tasks can be deleted after completion, but others should stay and
        //merely change heuristic value
    }

    private class Task
    {
        public TaskType taskType;
        public object[] target;
        public int heuristic;
        public enum TaskType
        {

        }
    }
}
