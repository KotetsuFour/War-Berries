using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class Building
{
	protected string name;
	protected ulong structuralIntegrity;
	protected int durability;
	protected int resistance;
	protected CharacterData owner;
	protected List<int[]> materials;
	protected WorldMapTile location;

	public static string COLISEUM = "Coliseum";
	public static string HOSPITAL = "Hospital";
	public static string PORT = "Port";
	public static string RESEARCH_CENTER = "Research Center";
	public static string SHIPYARD = "Shipyard";
	public static string VILLAGE = "Village";
	public static string WARP_PAD = "Warp Pad";
	public static string BARRACKS = "Barracks";
	public static string CASTLE = "Castle";
	public static string FORTRESS = "Fortress";
	public static string PRISON = "Prison";
	public static string TRAINING_FACILITY = "Training Facility";
	public static string FACTORY = "Factory";
	public static string FARM = "Farm";
	public static string MAGIC_PROCESSING_FACILITY = "Magic Processing Facility";
	public static string MINING_FACILITY = "Mining Facility";
	public static string RANCH = "Ranch";
	public static string STOREHOUSE = "Storehouse";
	public static string TRADE_CENTER = "Trade Center";

	public Building(string name, int durability, int resistance,
			CharacterData owner, WorldMapTile location)
	{
		this.name = name;
		structuralIntegrity = ulong.MaxValue;
		this.durability = durability;
		this.resistance = resistance;
		this.owner = owner;
		this.materials = new List<int[]>();
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

	public abstract string getType();

	public CharacterData getOwner()
	{
		return owner;
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

	public abstract void completeDailyAction();

	public abstract void completeMonthlyAction();

	public ulong getStructuralIntegrityRaw()
	{
		return structuralIntegrity;
	}

	public abstract void destroy();

	public abstract bool canReceiveGoods(int[] goods);

	public List<int[]> getMaterials()
	{
		return materials;
	}

	public int getDurability()
	{
		return durability;
	}
	public int getResistance()
	{
		return resistance;
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
