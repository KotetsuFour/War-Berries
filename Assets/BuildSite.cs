using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BuildSite : MonoBehaviour
{
	protected List<int[]> materials;
	public WorldMapTile location;
	public float timeLeft;

	public const string COLISEUM = "Coliseum";
	public const string HOSPITAL = "Hospital";
	public const string PORT = "Port";
	public const string RESEARCH_CENTER = "Research Center";
	public const string SHIPYARD = "Shipyard";
	public const string VILLAGE = "Village";
	public const string WARP_PAD = "Warp Pad";
	public const string BARRACKS = "Barracks";
	public const string CASTLE = "Castle";
	public const string FORTRESS = "Fortress";
	public const string PRISON = "Prison";
	public const string TRAINING_FACILITY = "Training Facility";
	public const string FACTORY = "Factory";
	public const string FARM = "Farm";
	public const string MAGIC_PROCESSING_FACILITY = "Magic Processing Facility";
	public const string MINING_FACILITY = "Mining Facility";
	public const string RANCH = "Ranch";
	public const string STOREHOUSE = "Storehouse";
	public const string TRADE_CENTER = "Trade Center";

	public void setLocation(WorldMapTile location)
	{
		this.location = location;
	}

	public string getName()
	{
		return name;
	}

	public string getNameAndType()
	{
		return $"{name} ({GetType()})";
	}

	public bool takeDamage(bool isMagicAttack, int damage)
	{
		//TODO bitwise function
		return true;
	}

	public void rename(string newName)
	{
		this.name = newName;
	}

	public List<int[]> getMaterials()
	{
		return materials;
	}

	public WorldMapTile getLocation()
	{
		return location;
	}

	public float percentageHealth()
	{
		//TODO what percentage of the bits in structuralIngerity are true?
		return 0;
	}

	public string tostring()
	{
		return getNameAndType();
	}

	/**
	 * Gives a list of materials that the building requires from a storehouse
	 * in order to do its tasks
	 * 
	 * By default, this returns null to indicate that no materials are needed
	 * @return
	 */
	public List<int[]> getStorehouseNeeds()
	{
		return null;
	}

}
