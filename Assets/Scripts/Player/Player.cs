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
	public Material notAllowedMaterial, allowedMaterial;
	public string username;
    public bool isHuman;
	public int startEnergy, startEnergyLimit, startMetal, startMetalLimit;

	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private Dictionary<ResourceType, float> resources;
	private Dictionary<ResourceType, int> resourceLimits;
	private Building tempBuilding;
	private Unit tempCreator;
	private bool isFindingPlacement = false;

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
		if(this.isFindingPlacement) {
			this.tempBuilding.CalculateBounds();
			if(this.CanPlaceBuilding()) {
				this.tempBuilding.SetTransparentMaterial(allowedMaterial, false);
			} else {
				this.tempBuilding.SetTransparentMaterial(notAllowedMaterial, false);
			}
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
	public void AddUnit(string unitName, Vector3 spawnPoint, Vector3 rallyPoint, 
		Quaternion rotation, Building creator) {
		Units units = GetComponentInChildren<Units>();
		GameObject newUnit = (GameObject)Instantiate(ResourceManager.GetUnit(unitName), spawnPoint, rotation);
		newUnit.transform.parent = units.transform;
		Unit unitObject = newUnit.GetComponent<Unit>();
		if(unitObject) {
			unitObject.SetBuilding(creator);
			if(spawnPoint != rallyPoint) {
				unitObject.StartMove(rallyPoint);
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Create a new building
	public void CreateBuilding(string buildingName, Vector3 buildPoint, Unit creator, Rect playingArea) {
		GameObject newBuilding = (GameObject)Instantiate(ResourceManager.GetBuilding(buildingName), 
			buildPoint, new Quaternion());
		this.tempBuilding = newBuilding.GetComponent<Building>();
		if(this.tempBuilding) {
			this.tempCreator = creator;
			this.isFindingPlacement = true;
			this.tempBuilding.SetTransparentMaterial(this.notAllowedMaterial, true);
			this.tempBuilding.SetColliders(false);
			this.tempBuilding.SetPlayingArea(playingArea);
		} else {
			Destroy(newBuilding);
		}
	}

	// --------------------------------------------------------------------------------------------
	// Returns if the player is currently finding a building location
	public bool IsFindingBuildingLocation() {
		return this.isFindingPlacement;
	}

	// --------------------------------------------------------------------------------------------
	// Find a new building location
	public void FindBuildingLocation() {
		Vector3 newLocation = WorkManager.FindHitPoint(Input.mousePosition);
		newLocation.y = 0;
		this.tempBuilding.transform.position = newLocation;
	}

	// --------------------------------------------------------------------------------------------
	// Returns if the player can place the building
	public bool CanPlaceBuilding() {
		bool canPlace = true;

		Bounds placeBounds = this.tempBuilding.GetSelectionBounds();
		// Shorthand for the coordinates of the center of the selection bounds
		float cx = placeBounds.center.x;
		float cy = placeBounds.center.y;
		float cz = placeBounds.center.z;
		// Shorthand for the coordinates of the extents of the selection box
		float ex = placeBounds.extents.x;
		float ey = placeBounds.extents.y;
		float ez = placeBounds.extents.z;

		// Determine the screen coordinates for the corners of the selection bounds
		List<Vector3> corners = new List<Vector3>();
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz + ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy + ey, cz - ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz + ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz + ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx + ex, cy - ey, cz - ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz + ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy + ey, cz - ez)));
		corners.Add(Camera.main.WorldToScreenPoint(new Vector3(cx - ex, cy - ey, cz - ez)));

		foreach(Vector3 corner in corners) {
			GameObject hitObject = WorkManager.FindHitObject(corner);
			if(hitObject && hitObject.name != "Ground") {
				WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
				if(worldObject && placeBounds.Intersects(worldObject.GetSelectionBounds())) {
					canPlace = false;
				}
			}
		}
		return canPlace;
	}

	// --------------------------------------------------------------------------------------------
	// Starts the construction of a new building
	public void StartConstruction() {
		this.isFindingPlacement = false;
		Buildings buildings = GetComponentInChildren<Buildings>();
		if(buildings) {
			this.tempBuilding.transform.parent = buildings.transform;
		}
		this.tempBuilding.SetPlayer();
		this.tempBuilding.SetColliders(true);
		this.tempCreator.SetBuilding(tempBuilding);
		this.tempBuilding.StartConstruction();
	}

	// --------------------------------------------------------------------------------------------
	// Cancels the current building placement
	public void CancelBuildingPlacement() {
		this.isFindingPlacement = false;
		Destroy(tempBuilding.gameObject);
		this.tempBuilding = null;
		this.tempCreator = null;
	}
}
