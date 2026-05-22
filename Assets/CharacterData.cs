using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterData
{
    public enum BodyPart
    {
        HEAD, TORSO, RIGHT_ARM, LEFT_ARM, RIGHT_LEG, LEFT_LEG, RIGHT_EYE, LEFT_EYE, MOUTH,
        END //The "END" value is just there as an easy way to quantify how many parts there
            //are in case it changes, by initializing body part arrays with size of (int)BodyPart.END
    }

    //TODO unitClass
    public string id; //Each type of soldier has their own id naming convention
    public string unitName;
    public Affiliation affiliation;
    public CharacterTeam team;
    public double timeBorn;

    public float[] bodyPartsMaxHP;
    public float[] bodyPartsCurrentHP;
    public float[] bodyPartsHPGrowthRates;
    public bool injured;

    public float strength; //Determines the amount of damage for traditional weapons
    public float dexterity; //Determines the range of error when aiming
    public float luck; //Determines how likely you are to be injured instead of dying from a fatal hit
    public float leadership; //Determines the amount of support given to underlings
    public float morale; //Determines behaviours associated with courage and loyalty

    public static float MAX_STRENGTH = 100;
    public static float MAX_ACCURACY = 100;
    public static float MAX_LUCK = 100;
    public static float MAX_LEADERSHIP = 100;
    public static float MAX_MORALE = 100;

    public float strengthGrowth;
    public float accuracyGrowth;
    public float luckGrowth;
    public float leadershipGrowth;

    public Dictionary<string, int[]> battleLog; //Takes a string name of the battle and gives an array
                                                //[numKills, numInjuries, numBaseCaptures, numVehiclesDestroyed, numStructuresDestroyed]

    public Color hair;
    public Color skin;
    public Color eye;
    public bool gender;
    public Demeanor demeanor;
    public bool leftHanded;
    public Interest[] interests;
    public Interest[] disinterests;
    public CombatTrait valuedTrait;
    public int[] appearance;
    public string supportPartnerId;
    public float attackPriorityPercentage;
    public float supportPriorityPercentage;
    public float demolitionPriorityPercentage;
    public bool taskToggle; //Used with the team's taskToggle variable to determine whether
                            //this character's task is up to date

    public List<int[]> inventory;
    public float weightCarried;
    public float[] armor;

    public float[] skillLevels; //One index for each CombatTrait

    public CharacterData(string id, double timeBorn, Affiliation affiliation)
    {
        this.id = id;
        this.timeBorn = timeBorn;
        this.affiliation = affiliation;

        generateStats();
    }
    private void generateStats()
    {
        unitName = FantasyNames.getName();
        //TODO remember to create the team somewhere

        bodyPartsMaxHP = new float[(int)BodyPart.END];
        bodyPartsCurrentHP = new float[(int)BodyPart.END];
        bodyPartsHPGrowthRates = new float[(int)BodyPart.END];
        bodyPartsMaxHP[(int)BodyPart.HEAD] = Random.Range(10, 15);
        bodyPartsMaxHP[(int)BodyPart.TORSO] = Random.Range(10, 25);
        int armHP = Random.Range(10, 15);
        int legHP = Random.Range(10, 15);
        bodyPartsMaxHP[(int)BodyPart.RIGHT_ARM] = armHP;
        bodyPartsMaxHP[(int)BodyPart.LEFT_ARM] = armHP;
        bodyPartsMaxHP[(int)BodyPart.RIGHT_LEG] = legHP;
        bodyPartsMaxHP[(int)BodyPart.LEFT_LEG] = legHP;
        for (int q = 0; q < bodyPartsMaxHP.Length; q++)
        {
            bodyPartsCurrentHP[q] = bodyPartsMaxHP[q];
        }
        bodyPartsHPGrowthRates[(int)BodyPart.HEAD] = Random.Range(1, 6);
        bodyPartsHPGrowthRates[(int)BodyPart.TORSO] = Random.Range(1, 6);
        int armGrowth = Random.Range(1, 6);
        int legGrowth = Random.Range(1, 6);
        bodyPartsHPGrowthRates[(int)BodyPart.RIGHT_ARM] = armGrowth;
        bodyPartsHPGrowthRates[(int)BodyPart.LEFT_ARM] = armGrowth;
        bodyPartsHPGrowthRates[(int)BodyPart.RIGHT_LEG] = legGrowth;
        bodyPartsHPGrowthRates[(int)BodyPart.LEFT_LEG] = legGrowth;

        strength = Random.Range(1, 26);
        dexterity = Random.Range(1, 26);
        luck = Random.Range(0, 10);
        leadership = Random.Range(0, 11);

        strengthGrowth = Random.Range(1, 11);
        accuracyGrowth = Random.Range(1, 11);
        luckGrowth += Random.Range(1, 11);

        battleLog = new Dictionary<string, int[]>();

        gender = Random.Range(0, 2) == 0;

        morale = 50;

        inventory = new List<int[]>();

        attackPriorityPercentage = Random.Range(0, 1.0f);
        supportPriorityPercentage = Random.Range(0, attackPriorityPercentage);
        demolitionPriorityPercentage = CharacterTeam.MAX_MEMBERS_ALLOWED - (attackPriorityPercentage + supportPriorityPercentage);
    }
    public bool isLeader()
    {
        return team == null || team.getLeader() == this;
    }
    public void setDemeanor(Demeanor[] allDemeanors)
    {
        demeanor = allDemeanors[Random.Range(0, allDemeanors.Length)];
    }
    public void setSkills(CombatTrait[] allTraits)
    {
        valuedTrait = allTraits[Random.Range(0, allTraits.Length)];
        skillLevels = new float[allTraits.Length];
    }
    public virtual float getMoveSpeed()
    {
        return 1;
    }
    public virtual float getRotationSpeed()
    {
        return 90;
    }

    public virtual Item getItem(int idx)
    {
        return StaticData.getItemFromIndex(inventory[idx]);
    }

    /**
     * Remove the amount of item from the inventory and return whether there's any left
     */
    public virtual bool removeItemAtIdx(int idx, int quant)
    {
        if (inventory[idx][2] == -1)
        {
            return true;
        }
        inventory[idx][2] -= quant;
        weightCarried -= StaticData.getItemFromIndex(inventory[idx]).weight * quant;
        if (inventory[idx][2] <= 0)
        {
            inventory.RemoveAt(idx);
            return false;
        }
        return true;
    }
    /**
     * Remove the entire item from the inventory and return it
     */
    public virtual Item removeItemAtIdxFully(int idx)
    {
        Item ret = StaticData.getItemFromIndex(inventory[idx]);
        inventory.RemoveAt(idx);
        return ret;
    }
    public virtual Weapon getEquippedWeapon()
    {
        //TODO figure out the equipping system
        Item item = StaticData.getItemFromIndex(inventory[0]);
        if (item is Weapon)
        {
            return (Weapon)item;
        }
        return null;
    }

    public void setInterestsAndDisinterests(Interest[] allInterests)
    {
        interests = new Interest[3];
        for (int q = 0; q < interests.Length; q++)
        {
            bool duplicate;
            do
            {
                duplicate = false;
                for (int w = 0; w < q; w++)
                {
                    if (interests[q] == interests[w])
                    {
                        duplicate = true;
                        interests[q] = (Interest)((int)(interests[q] + 1) % allInterests.Length);
                        break;
                    }
                }
            } while (duplicate);
        }

        disinterests = new Interest[3];
        for (int q = 0; q < disinterests.Length; q++)
        {
            bool duplicate;
            do
            {
                duplicate = false;
                for (int w = 0; w < q; w++)
                {
                    if (disinterests[q] == disinterests[w])
                    {
                        duplicate = true;
                        disinterests[q] = (Interest)((int)(disinterests[q] + 1) % allInterests.Length);
                        break;
                    }
                }
                if (!duplicate)
                {
                    for (int w = 0; w < interests.Length; w++)
                    {
                        if (disinterests[q] == interests[w])
                        {
                            duplicate = true;
                            disinterests[q] = (Interest)((int)(disinterests[q] + 1) % allInterests.Length);
                            break;
                        }
                    }
                }
            } while (duplicate);
        }

    }
    public void setColorsDirect(Color hair, Color skin, Color eye)
    {
        this.hair = hair;
        this.skin = skin;
        this.eye = eye;
    }
    public void setColorsWithVariance(Color baseHair, Color baseSkin, Color baseEye, int variance)
    {
        /*
        Debug.Log($"{baseHair.r}, {baseHair.g}, {baseHair.b}");
        Debug.Log($"{baseSkin.r}, {baseSkin.g}, {baseSkin.b}");
        Debug.Log($"{baseEye.r}, {baseEye.g}, {baseEye.b}");
        */
        float hairVaryR = (baseHair.r * 255) + (Random.Range(0, variance) * (Random.Range(0, 2) == 0 ? -1 : 1));
        float hairVaryG = (baseHair.g * 255) + (Random.Range(0, variance) * (Random.Range(0, 2) == 0 ? -1 : 1));
        float hairVaryB = (baseHair.b * 255) + (Random.Range(0, variance) * (Random.Range(0, 2) == 0 ? -1 : 1));

        float skinVaryR = (baseSkin.r * 255) + (Random.Range(0, variance) * (Random.Range(0, 2) == 0 ? -1 : 1));
        float skinVaryG = (baseSkin.g * 255) + (Random.Range(0, variance) * (Random.Range(0, 2) == 0 ? -1 : 1));
        float skinVaryB = (baseSkin.b * 255) + (Random.Range(0, variance) * (Random.Range(0, 2) == 0 ? -1 : 1));

        float eyeVaryR = (baseEye.r * 255) + (Random.Range(0, variance) * (Random.Range(0, 2) == 0 ? -1 : 1));
        float eyeVaryG = (baseEye.g * 255) + (Random.Range(0, variance) * (Random.Range(0, 2) == 0 ? -1 : 1));
        float eyeVaryB = (baseEye.b * 255) + (Random.Range(0, variance) * (Random.Range(0, 2) == 0 ? -1 : 1));

        hair = new Color(hairVaryR / 255f, hairVaryG / 255f, hairVaryB / 255f);
//        Debug.Log($"{hair.r}, {hair.g}, {hair.b}");
        skin = new Color(skinVaryR / 255f, skinVaryG / 255f, skinVaryB / 255f);
//        Debug.Log($"{skin.r}, {skin.g}, {skin.b}");
        eye = new Color(eyeVaryR / 255f, eyeVaryG / 255f, eyeVaryB / 255f);
//        Debug.Log($"{eye.r}, {eye.g}, {eye.b}");
    }
    public CharacterData(string id, string unitName, Affiliation affiliation, CharacterTeam team,
        double timeBorn, float strength, float accuracy, float luck, float leadership, float strengthGrowth,
        float accuracyGrowth, float luckGrowth, float leadershipGrowth, string[] battles, int[][] battleStats,
        float[] bodyPartsMaxHP, float[] bodyPartsCurrentHP, float[] bodyPartsHPGrowthRates)
    {
        this.id = id;
        this.unitName = unitName;
        this.affiliation = affiliation;
        this.team = team;
        this.timeBorn = timeBorn;
        this.strength = strength;
        this.dexterity = accuracy;
        this.luck = luck;
        this.leadership = leadership;
        this.strengthGrowth = strengthGrowth;
        this.accuracyGrowth = accuracyGrowth;
        this.luck = luckGrowth;
        this.leadershipGrowth = leadershipGrowth;
        battleLog = new Dictionary<string, int[]>();
        for (int q = 0; q < battles.Length; q++)
        {
            battleLog.Add(battles[q], battleStats[q]);
        }
        this.bodyPartsMaxHP = bodyPartsMaxHP;
        this.bodyPartsCurrentHP = bodyPartsCurrentHP;
        this.bodyPartsHPGrowthRates = bodyPartsHPGrowthRates;
    }
    public int[] getCareer()
    {
        if (battleLog.Count == 0)
        {
            return null;
        }
        int fields = 0;
        foreach (int[] val in battleLog.Values)
        {
            //Probably not the best way to do this, but whatever;
            fields = val.Length;
            break;
        }
        int[] ret = new int[fields];
        foreach (int[] entry in battleLog.Values)
        {
            for (int q = 0; q < entry.Length; q++)
            {
                ret[q] += entry[q];
            }
        }
        return ret;
    }
    public Affiliation getAffiliation()
    {
        return affiliation;
    }
    /*
    public void addExperience(int exp)
    {
        experience += exp;
        while (experience >= expToNextLevel())
        {
            experience -= expToNextLevel();
            levelUp();
        }
    }
    public int expToNextLevel()
    {
        return (level * 3) + 1;
    }
    private void levelUp()
    {
        level++;
        for (int q = 0; q < bodyPartsMaxHP.Length; q++)
        {
            bodyPartsMaxHP[q] += bodyPartsHPGrowthRates[q];
        }
        strength += strengthGrowth;
        accuracy = Mathf.Min(99, accuracy + accuracyGrowth);
        luck = Mathf.Min(99, luck + luckGrowth);
    }
    */
    public bool isAlive()
    {
        return (bodyPartsCurrentHP[(int)BodyPart.HEAD] > 0 && bodyPartsCurrentHP[(int)BodyPart.TORSO] > 0)
            || injured;
    }
    public enum Demeanor
    {
        SERIOUS, RELAXED, DETERMINED, ENTHUSIASTIC, NERVOUS, FRIENDLY, POLITE, CURIOUS, DISMISSIVE,
        CHARISMATIC, ASSERTIVE, REFLECTIVE, ABSENT, CREEPY, SNOBBISH, INTIMIDATING
    }
    public enum CombatTrait
    {
        RIFLE, SNIPER, MACHINE, PISTOL, SLICE, BLUNT, PIERCE, FISTS, FLYING, ARTILLERY, DRIVING, SAILING
    }
    public enum Interest
    {
        GARDENING, HORSEBACK_RIDING, STUDYING, COOKING, HUNTING, COLLECTING, PAINTING, WRITING,
		READING, PLAYING_MUSIC, WRITING_MUSIC, SCIENTIFIC_DISCOVERY, ANIMALS, STRATEGY_GAMES,
        ADVENTURE, CARPENTRY, PRACTICAL_JOKES, KNITTING, SWIMMING, PERFORMING, SCULPTING,
		SPORTS, HIKING, TRAVELING
    }
}
