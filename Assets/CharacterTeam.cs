using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTeam
{
    public static int MAX_MEMBERS_ALLOWED = 10;

    private List<CharacterData> members;
    private WMOrders orders;
    private BFOrders tempOrders;
    private Battle currentBattle;

    private LinkedList<Warrior> attackerPriority;
    private LinkedList<Warrior> healerPriority;
    private LinkedList<Warrior> demolitionPriority;

    private bool taskToggle; //Used to keep tasks up to date

    public int xCoord;
    public int yCoord;

    private int formation;

    public CharacterTeam(CharacterData data)
    {
        members = new List<CharacterData>(MAX_MEMBERS_ALLOWED);
        members.Add(data);
    }
    public void battleInitializer()
    {
        attackerPriority = new LinkedList<Warrior>();
        healerPriority = new LinkedList<Warrior>();
        demolitionPriority = new LinkedList<Warrior>();
        foreach (CharacterData data in members)
        {
            data.taskToggle = !taskToggle;
        }
        //TODO add characters to queues and order them
    }
    public bool canFly()
    {
        //TODO
        return false;
    }
    public bool add(CharacterData data)
    {
        if (members.Count < MAX_MEMBERS_ALLOWED)
        {
            data.team = this;
            members.Add(data);
            return true;
        }
        return false;
    }
    public CharacterData getMember(int idx)
    {
        return members[idx];
    }
    public int size()
    {
        return members.Count;
    }
    public CharacterData getLeader()
    {
        if (members.Count == 0)
        {
            Debug.LogError("Tried to access a team with no members");
            return null;
        }
        return members[0];
    }
    public Affiliation getAffiliation()
    {
        return getLeader().getAffiliation();
    }
    public int getFormation()
    {
        return formation;
    }
    public void assignWarriorRoles()
    {
        //Go through each member, and give them a task based on the team's BFOrders
        //and the individual's stats. On that note, we need a way to update prior knowledge for
        //who's best suited for what kind of role, so that frequently calling this method won't
        //cause performance spikes. Instead, we simply go through, and whoever's within a certain
        //percentile of being good at something (which we can see in a previously stored value),
        //will be assigned a task accordingly
        taskToggle = !taskToggle;
        CharacterData lead = getLeader();
        int numAttackers = Mathf.CeilToInt(members.Count * lead.attackPriorityPercentage);
        int numSupporters = Mathf.Min(members.Count - numAttackers, Mathf.CeilToInt(members.Count * lead.supportPriorityPercentage));
        int numDemolitioners = members.Count - (numAttackers + numSupporters);
        LinkedListNode<Warrior> att = attackerPriority.First;
        LinkedListNode<Warrior> sup = attackerPriority.First;
        LinkedListNode<Warrior> dem = attackerPriority.First;
        for (int q = 0; q < numAttackers; q++)
        {
            Warrior warr = att.Value;
            att = att.Next;
            warr.setTask(Warrior.Task.OFFENSE);
            warr.getData().taskToggle = taskToggle;
        }
        for (int q = 0; q < numSupporters; q++)
        {
            Warrior warr = sup.Value;
            sup = sup.Next;
            if (warr.getData().taskToggle == taskToggle)
            {
                continue;
            }
            warr.setTask(Warrior.Task.SUPPORT);
            warr.getData().taskToggle = taskToggle;
        }
        for (int q = 0; q < numDemolitioners; q++)
        {
            Warrior warr = dem.Value;
            dem = dem.Next;
            if (warr.getData().taskToggle == taskToggle)
            {
                continue;
            }
            warr.setTask(Warrior.Task.DEMOLITION);
            warr.getData().taskToggle = taskToggle;
        }
    }

    public class WMOrders
    {
        public object target;
        public bool distractible;
        public enum OrderType
        {
            TALK, VISIT, FOLLOW, ATTACK, WEAPON, ESCAPE, ITEM, TRADE, CHEST, SEIZE, COLLECT, SKILL, WAIT
        }
    }

    public class BFOrders
    {
        public object target;
        public bool distractible;
        public enum OrderType
        {
            FOLLOW_ALLIES, DESTROY_STRUCTURE, SEIZE_STRUCTURE, DEFEND_STRUCTURE, PURSUE_GIVEN_ENEMY_TEAM,
            DEFEND_POSITION, PURSUE_ANY_ENEMY_TEAM, ESCAPE, RESUPPLY_ALLIES, REST, COLLECT, GO_TO_POSITION
        }
    }
}
