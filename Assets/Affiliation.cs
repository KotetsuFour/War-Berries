using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Affiliation
{
    private int id;
    private Affiliation suzerain; //Note that an affiliation with a suzerain CANNOT gain
                                  //subordinates of its own (for simplicity)
    private List<CharacterData> members;
    private float[] values; //0-1 Nationalism, Altruism, Tribalism/Familism, Militarism
    private List<WorldMapTile> tiles;
    private int population;
    private int villageCount;
    private int storehouseCount;
    private float food;
    private float ore;
    private float fabric;
    private float clay;
    private float wood;
    private LinkedList<Task> taskQueue;

    public const float EXPECTED_POPULATION_PER_TILE = 10000;
    public const float RESOURCES_PER_STOREHOUSE = 10000; //A little over the number of hours per year

    public enum PersonalValue
    {
        NATIONALISM, ALTRUISM, TRIBALISM, MILITARISM
    }

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

    public void sufferFromOverpopulation()
    {
        //TODO
    }

    public void assessOptions()
    {
        //Figure out how many extra resources we have
        foreach (WorldMapTile tile in tiles)
        {
            if (tile.canProduce())
            {
                float[] resources = tile.getAllResources();
                food += resources[(int)Item.ResouceType.FOOD];
                ore += resources[(int)Item.ResouceType.ORE];
                wood += resources[(int)Item.ResouceType.WOOD];
                clay += resources[(int)Item.ResouceType.CLAY];
                fabric += resources[(int)Item.ResouceType.FABRIC];
            }
        }
        LinkedListNode<Task> current = taskQueue.First;
        
        while (current != null)
        {
            //Calculate heuristic (heuristics should be between 0 and 100)
            current.Value.calculateHeuristic(this);
            //Reorder
            LinkedListNode<Task> placeholder = current.Next;
            while (current.Previous != null && current.Value.heuristic > current.Previous.Value.heuristic)
            {
                LinkedListNode<Task> prev = current.Previous;
                taskQueue.Remove(current);
                taskQueue.AddBefore(prev, current);
            }
            current = placeholder;
        }

        //Theoretically, if a task's maximum cost is the same as the maximum heuristic, then you
        //can complete up to 4 max-priority tasks
        int energy = Mathf.RoundToInt(Mathf.Min(population, Task.MAX_HEURISTIC * 4));

        //Then, with all options weighed, go through the list in order, completing all tasks you have the
        //energy for and neglecting the ones you don't Some tasks can be deleted after completion. Delete
        //oneTimeTasks upon execution
        current = taskQueue.First;
        while (current != null)
        {
            int cost = current.Value.energyCost(this);
            if (cost <= energy && current.Value.hasEnoughResources(this))
            {
                current.Value.execute(this);
                energy -= cost;
                LinkedListNode<Task> store = current;
                current = current.Next;
                if (store.Value.oneTimeTask)
                {
                    taskQueue.Remove(store);
                }
            }
            else
            {
                current.Value.neglect(this);
                current = current.Next;
            }
        }

        food = Mathf.Min(food, RESOURCES_PER_STOREHOUSE * storehouseCount);
        ore = Mathf.Min(ore, RESOURCES_PER_STOREHOUSE * storehouseCount);
        wood = Mathf.Min(wood, RESOURCES_PER_STOREHOUSE * storehouseCount);
        clay = Mathf.Min(clay, RESOURCES_PER_STOREHOUSE * storehouseCount);
        fabric = Mathf.Min(fabric, RESOURCES_PER_STOREHOUSE * storehouseCount);
    }

    private abstract class Task
    {
        public float heuristic;
        public const int MAX_HEURISTIC = 100;
        public bool oneTimeTask;
 
        //This should set the heuristic parameter, rather than return it
        //The heuristic should be between 0 and 100
        public abstract void calculateHeuristic(Affiliation aff);
        public abstract int energyCost(Affiliation aff);
        public abstract void execute(Affiliation aff);
        public abstract void neglect(Affiliation aff);
        public abstract bool hasEnoughResources(Affiliation aff);
    }

    private class ProvideHousing : Task
    {
        public WorldMapTile tileToBuildOn;
        public override void calculateHeuristic(Affiliation aff)
        {
            float populationRatio = (float)aff.population / (aff.tiles.Count + aff.villageCount);
            heuristic = Mathf.Max(0,
                Mathf.Min(MAX_HEURISTIC,
                Mathf.Abs(populationRatio - EXPECTED_POPULATION_PER_TILE) * aff.values[(int)PersonalValue.ALTRUISM]));
        }
        public override int energyCost(Affiliation aff)
        {
            //Go through the tiles and find one that can be expanded or where a village can be built
            //Prioritize building villages, and prioritize tiles with better conditions
            //The actual cost is inversely proportional to the need
            //If there are no available tiles for the action, the cost is maxValue

            //TODO account for tiles owned by others
            int currentPriority = 0;
            foreach (WorldMapTile tile in aff.tiles)
            {
                int prior = 0;
                if (tile.getBuildingType() == null)
                {
                    prior += 100 + tile.getMagicPotency()
                        + tile.getHeight() > VisualMapGenerator.getSeaLevel() ? 0 : -50;
                }
                else
                {
                    WorldMap map = StaticData.worldMap;
                    int x = tile.getX();
                    int y = tile.getY();
                    if (x > 0)
                    {
                        WorldMapTile check = map.at(x - 1, y);
                        int checkVal = check.getMagicPotency()
                            + check.getHeight() > VisualMapGenerator.getSeaLevel() ? 0 : -50;
                        if (check.getBuildingType() == null && checkVal > currentPriority)
                        {
                            currentPriority = checkVal;
                            tileToBuildOn = check;
                        }
                    }
                    if (x < map.getMap().Length - 1)
                    {
                        WorldMapTile check = map.at(x + 1, y);
                        int checkVal = check.getMagicPotency()
                            + check.getHeight() > VisualMapGenerator.getSeaLevel() ? 0 : -50;
                        if (check.getBuildingType() == null && checkVal > currentPriority)
                        {
                            currentPriority = checkVal;
                            tileToBuildOn = check;
                        }
                    }
                    if (y > 0)
                    {
                        WorldMapTile check = map.at(x, y - 1);
                        int checkVal = check.getMagicPotency()
                            + check.getHeight() > VisualMapGenerator.getSeaLevel() ? 0 : -50;
                        if (check.getBuildingType() == null && checkVal > currentPriority)
                        {
                            currentPriority = checkVal;
                            tileToBuildOn = check;
                        }
                    }
                    if (y < map.getMap()[0].Length)
                    {
                        WorldMapTile check = map.at(x, y + 1);
                        int checkVal = check.getMagicPotency()
                            + check.getHeight() > VisualMapGenerator.getSeaLevel() ? 0 : -50;
                        if (check.getBuildingType() == null && checkVal > currentPriority)
                        {
                            currentPriority = checkVal;
                            tileToBuildOn = check;
                        }
                    }
                }

                if (prior > currentPriority)
                {
                    currentPriority = prior;
                    tileToBuildOn = tile;
                }
            }
            if (tileToBuildOn == null)
            {
                return int.MaxValue;
            }
            return Mathf.RoundToInt(MAX_HEURISTIC - heuristic);
        }
        public override void execute(Affiliation aff)
        {
            if (tileToBuildOn.getAffiliation() == null)
            {
                int[] vals = new int[aff.values.Length];
                for (int q = 0; q < aff.values.Length; q++)
                {
                    vals[q] = Mathf.RoundToInt(aff.values[q]);
                }
                aff.addTile(tileToBuildOn, vals);
            }
            else
            {
                BuildSite buildSite = AssetDictionary.newBuildSite();
                buildSite.setLocation(tileToBuildOn);
                tileToBuildOn.setBuilding(buildSite, $"{FantasyNames.getName()} Village", BuildSite.VILLAGE);
                aff.villageCount++;

                StaticData.BuildingData data = StaticData.getBuildingData(BuildSite.VILLAGE);
                aff.food -= data.recipe[(int)Item.ResouceType.FOOD];
                aff.ore -= data.recipe[(int)Item.ResouceType.ORE];
                aff.wood -= data.recipe[(int)Item.ResouceType.WOOD];
                aff.clay -= data.recipe[(int)Item.ResouceType.CLAY];
                aff.fabric -= data.recipe[(int)Item.ResouceType.FABRIC];
            }
        }
        public override void neglect(Affiliation aff)
        {
            if (heuristic == MAX_HEURISTIC)
            {
                aff.sufferFromOverpopulation();
            }
        }
        public override bool hasEnoughResources(Affiliation aff)
        {
            if (tileToBuildOn.getAffiliation() == null)
            {
                return true;
            }
            StaticData.BuildingData data = StaticData.getBuildingData(BuildSite.VILLAGE);
            return aff.food >= data.recipe[(int)Item.ResouceType.FOOD]
                && aff.ore >= data.recipe[(int)Item.ResouceType.ORE]
                && aff.wood >= data.recipe[(int)Item.ResouceType.WOOD]
                && aff.clay >= data.recipe[(int)Item.ResouceType.CLAY]
                && aff.fabric >= data.recipe[(int)Item.ResouceType.FABRIC];
        }
    }
    private class ProvideFoodForCitizens : Task
    {
        public override void calculateHeuristic(Affiliation aff)
        {
            float populationRatio = (float)aff.population / (aff.tiles.Count + aff.villageCount);
            heuristic = Mathf.Max(0,
                Mathf.Min(MAX_HEURISTIC,
                Mathf.Abs(EXPECTED_POPULATION_PER_TILE - populationRatio) * aff.values[(int)PersonalValue.ALTRUISM]));
        }
        public override int energyCost(Affiliation aff)
        {
            //TODO
            return 0;
        }
        public override void execute(Affiliation aff)
        {
            //TODO
        }
        public override void neglect(Affiliation aff)
        {
            //TODO
        }
        public override bool hasEnoughResources(Affiliation aff)
        {
            //TODO
            return true;
        }
    }
}
