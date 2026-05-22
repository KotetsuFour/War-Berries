using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Battle
{
    public List<CharacterTeam> coreTeams;
    public List<Vector3> coreStartPositions;
    public List<Quaternion> coreStartRotations;
    public List<CharacterTeam> allyTeams;
    public List<Vector3> allyStartPositions;
    public List<Quaternion> allyStartRotations;
    public List<CharacterTeam> completeTeamsList;
    public Battle(int coreMinX, int coreMaxX, int coreMinY, int coreMaxY)
    {
        coreTeams = new List<CharacterTeam>();
        allyTeams = new List<CharacterTeam>();
        completeTeamsList = new List<CharacterTeam>();
    }
    public void setPeripheral(int periMinX, int periMaxX, int periMinY, int periMaxY)
    {

    }
    public void addCoreTeam(CharacterTeam team, Vector3 position, Quaternion rotation)
    {
        coreTeams.Add(team);
        coreStartPositions.Add(position);
        coreStartRotations.Add(rotation);
        completeTeamsList.Add(team);
    }
    public void addAllyTeam(CharacterTeam team, Vector3 position, Quaternion rotation)
    {
        allyTeams.Add(team);
        allyStartPositions.Add(position);
        allyStartRotations.Add(rotation);
        completeTeamsList.Add(team);
    }
}
