using UnityEngine;
using System.Collections;

public class Worker : Unit {
	// --------------------------------------------------------------------------------------------
	// UNITY VARIABLES
	public int buildSpeed;

	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private Building currentProject;
	private bool isBuilding = false;
	private float amountBuilt = 0.0f;

	// --------------------------------------------------------------------------------------------
	// Initialization
	protected override void Start() {
		base.Start();
		this.actions = new string[] { "WarFactory" };
	}

	// --------------------------------------------------------------------------------------------
	// Called each frame
	protected override void Update() {
		base.Update();
		if(!this.moving && !this.rotating) {
			if(this.isBuilding && this.currentProject && this.currentProject.UnderConstruction()) {
				this.amountBuilt += this.buildSpeed * Time.deltaTime;
				int amount = Mathf.FloorToInt(this.amountBuilt);
				if(amount > 0) {
					this.amountBuilt -= amount;
					this.currentProject.Construct(amount);
					if(!this.currentProject.UnderConstruction()) {
						this.isBuilding = false;
					}
				}
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Starts building 
	public override void SetBuilding(Building project) {
		base.SetBuilding(project);
		this.currentProject = project;
		this.StartMove(this.currentProject.transform.position);
		this.isBuilding = true;
	}

	// --------------------------------------------------------------------------------------------
	// Starts performing a worker action
	public override void PerformAction(string actionToPerform) {
		base.PerformAction(actionToPerform);
		if(!this.player.IsFindingBuildingLocation()) {
			this.CreateBuilding(actionToPerform);
		}
	}

	// --------------------------------------------------------------------------------------------
	// Starts moving the worker and resets the current construction
	public override void StartMove(Vector3 destination) {
		base.StartMove(destination);
		this.amountBuilt = 0.0f;
		this.isBuilding = false;
	}

	// --------------------------------------------------------------------------------------------
	// Create a new building
	private void CreateBuilding(string buildingName) {
		if(this.player) {
			Vector3 buildPoint = new Vector3(transform.position.x, transform.position.y, 
				transform.position.z + 10);
			this.player.CreateBuilding(buildingName, buildPoint, this, playingArea);
		}
	}

	// --------------------------------------------------------------------------------------------
	// Handle the mouse click on a building to resume construction
	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		bool doBase = true;
		if(this.player && this.player.isHuman && this.currentlySelected && hitObject 
			&& hitObject.name != "Ground") {
			Building building = hitObject.transform.parent.GetComponent<Building>();
			if(building) {
				if(building.UnderConstruction()) {
					this.SetBuilding(building);
					doBase = false;
				}
			}
		}
		if(doBase) {
			base.MouseClick(hitObject, hitPoint, controller);
		}
	}
}
