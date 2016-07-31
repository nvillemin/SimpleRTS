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
	public int startEnergy, startEnergyLimit, startMetal, startMetalLimit;

	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private Dictionary<ResourceType, float> resources;
	private Dictionary<ResourceType, int> resourceLimits;

	// --------------------------------------------------------------------------------------------
	// Initialization
	void Awake() {
		this.resources = this.InitResourceValues();
		this.resourceLimits = this.InitResourceLimits();
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
	// Resource values initialization
	private Dictionary<ResourceType, float> InitResourceValues() {
		Dictionary<ResourceType, float> list = new Dictionary<ResourceType, float>();
		list.Add(ResourceType.Energy, 0.0f);
		list.Add(ResourceType.Metal, 0.0f);
		return list;
	}

	// --------------------------------------------------------------------------------------------
	// Resource limits initialization
	private Dictionary<ResourceType, int> InitResourceLimits() {
		Dictionary<ResourceType, int> list = new Dictionary<ResourceType, int>();
		list.Add(ResourceType.Energy, 0);
		list.Add(ResourceType.Metal, 0);
		return list;
	}

	// --------------------------------------------------------------------------------------------
	// Resource limits initialization
	private void AddStartResourceLimits() {
		this.IncrementResourceLimit(ResourceType.Energy, this.startEnergyLimit);
		this.IncrementResourceLimit(ResourceType.Metal, this.startMetalLimit);
	}

	// --------------------------------------------------------------------------------------------
	// Start resources initialization
	private void AddStartResources() {
		this.AddResource(ResourceType.Energy, this.startEnergy);
		this.AddResource(ResourceType.Metal, this.startMetal);
	}

	// --------------------------------------------------------------------------------------------
	// Add resource by type
	public void AddResource(ResourceType type, float amount) {
		this.resources[type] += amount;
		if(this.resources[type] > this.resourceLimits[type]) {
			this.resources[type] = this.resourceLimits[type];
		}
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
