using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RTS;
using System;

public class Player : MonoBehaviour {
	// --------------------------------------------------------------------------------------------
	// UNITY VARIABLES
	public HUD hud;
	public WorldObject SelectedObject { get; set; }
	public string username;
    public bool isHuman;
	public int startMoney, startMoneyLimit, startPower, startPowerLimit;

	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private Dictionary<ResourceType, int> resources, resourceLimits;

	// --------------------------------------------------------------------------------------------
	// Initialization
	void Awake() {
		this.resources = this.InitResourceList();
		this.resourceLimits = this.InitResourceList();
	}

	// --------------------------------------------------------------------------------------------
	// Initialization
	void Start() {
		this.hud = GetComponentInChildren<HUD>();
		this.AddStartResourceLimits();
		this.AddStartResources();
	}

	// --------------------------------------------------------------------------------------------
	// Called each frame
	void Update() {
		if(this.isHuman) {
			this.hud.SetResourceValues(this.resources, this.resourceLimits);
		}
	}

	// --------------------------------------------------------------------------------------------
	// Resources initialization
	private Dictionary<ResourceType, int> InitResourceList() {
		Dictionary<ResourceType, int> list = new Dictionary<ResourceType, int>();
		list.Add(ResourceType.Money, 0);
		list.Add(ResourceType.Power, 0);
		return list;
	}

	// --------------------------------------------------------------------------------------------
	// Resource limits initialization
	private void AddStartResourceLimits() {
		this.IncrementResourceLimit(ResourceType.Money, this.startMoneyLimit);
		this.IncrementResourceLimit(ResourceType.Power, this.startPowerLimit);
	}

	// --------------------------------------------------------------------------------------------
	// Start resources initialization
	private void AddStartResources() {
		this.AddResource(ResourceType.Money, this.startMoney);
		this.AddResource(ResourceType.Power, this.startPower);
	}

	// --------------------------------------------------------------------------------------------
	// Add resource by type
	public void AddResource(ResourceType type, int amount) {
		this.resources[type] += amount;
	}

	// --------------------------------------------------------------------------------------------
	// Increment resource limit by type
	public void IncrementResourceLimit(ResourceType type, int amount) {
		this.resourceLimits[type] += amount;
	}

	// --------------------------------------------------------------------------------------------
	// Create a new unit
	public void AddUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, Quaternion rotation) {
		Units units = GetComponentInChildren<Units>();
		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
		newUnit.transform.parent = units.transform;
		Unit unitObject = newUnit.GetComponent<Unit>();
		if(unitObject && spawnPoint != rallyPoint) {
			unitObject.StartMove(rallyPoint);
		}
	}
}
