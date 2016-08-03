using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RTS;

public class Building : WorldObject {
	// --------------------------------------------------------------------------------------------
	// UNITY VARIABLES
	public float maxBuildProgress;
	public Texture2D rallyPointImage, sellImage;

	// --------------------------------------------------------------------------------------------
	// PROTECTED VARIABLES
	protected Queue<string> buildQueue;
	protected Vector3 rallyPoint;

	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private float currentBuildProgress = 0.0f;
	private Vector3 spawnPoint;
	private bool needsBuilding = false;

	// --------------------------------------------------------------------------------------------
	// Initialization (before Start)
	protected override void Awake() {
		base.Awake();
		this.buildQueue = new Queue<string>();
		float spawnX = this.selectionBounds.center.x + this.transform.forward.x 
			* this.selectionBounds.extents.x + this.transform.forward.x * 10;
		float spawnZ = this.selectionBounds.center.z + this.transform.forward.z 
			+ this.selectionBounds.extents.z + this.transform.forward.z * 10;
		this.spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);
		this.rallyPoint = this.spawnPoint;
	}

	// --------------------------------------------------------------------------------------------
	// Initialization
	protected override void Start() {
		base.Start();
	}

	// --------------------------------------------------------------------------------------------
	// Called each frame
	protected override void Update() {
		base.Update();
		this.ProcessBuildQueue();
	}

	// --------------------------------------------------------------------------------------------
	protected override void OnGUI() {
		base.OnGUI();
		if(this.needsBuilding) {
			this.DrawBuildProgress();
		}
	}

	// --------------------------------------------------------------------------------------------
	// Draw the current build progress
	private void DrawBuildProgress() {
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		// Draw the selection box around the currently selected object, within the bounds
		GUI.BeginGroup(playingArea);
		this.CalculateCurrentHealth(0.5f, 0.99f);
		this.DrawHealthBar(selectBox, "Building ...");
		GUI.EndGroup();
	}

	// --------------------------------------------------------------------------------------------
	// Add a new unit to the queue
	protected void CreateUnit(string unitName) {
		this.buildQueue.Enqueue(unitName);
	}

	// --------------------------------------------------------------------------------------------
	// Manage the queue each frame
	protected void ProcessBuildQueue() {
		if(this.buildQueue.Count > 0) {
			this.currentBuildProgress += Time.deltaTime * ResourceManager.BuildSpeed;
			if(this.currentBuildProgress > this.maxBuildProgress) {
				if(this.player) {
					this.player.AddUnit(this.buildQueue.Dequeue(), this.spawnPoint, 
						this.rallyPoint, this.transform.rotation, this);
				}
				this.currentBuildProgress = 0.0f;
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Get all units from queue
	public string[] GetBuildQueueValues() {
		string[] values = new string[this.buildQueue.Count];
		int pos = 0;
		foreach(string unit in this.buildQueue) {
			values[pos++] = unit;
		}
		return values;
	}

	// --------------------------------------------------------------------------------------------
	// Get current build percentage
	public float GetBuildPercentage() {
		return this.currentBuildProgress / this.maxBuildProgress;
	}

	// --------------------------------------------------------------------------------------------
	// Override WorldObject SetSelection
	public override void SetSelection(bool selected, Rect playingArea) {
		base.SetSelection(selected, playingArea);
		if(this.player) {
			RallyPoint flag = this.player.GetComponentInChildren<RallyPoint>();
			if(selected) {
				if(flag && this.player.isHuman && this.spawnPoint != ResourceManager.InvalidPosition
					&& this.rallyPoint != ResourceManager.InvalidPosition) {
					flag.transform.localPosition = this.rallyPoint;
					flag.transform.forward = this.transform.forward;
					flag.Enable();
				}
			} else {
				if(flag && this.player.isHuman)
					flag.Disable();
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Returns if the building has a spawn point
	public bool HasSpawnPoint() {
		return this.spawnPoint != ResourceManager.InvalidPosition 
			&& this.rallyPoint != ResourceManager.InvalidPosition;
	}

	// --------------------------------------------------------------------------------------------
	// Override WorldObject SetHoverState for the rally point
	public override void SetHoverState(GameObject hoverObject) {
		base.SetHoverState(hoverObject);
		// only handle input if owned by a human player and currently selected
		if(this.player && this.player.isHuman && this.currentlySelected) {
			if(hoverObject.name == "Ground") {
				if(this.player.hud.GetPreviousCursorState() == CursorState.RallyPoint)
					this.player.hud.SetCursorState(CursorState.RallyPoint);
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Override WorldObject MouseClick for the rally point
	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		base.MouseClick(hitObject, hitPoint, controller);
		// only handle iput if owned by a human player and currently selected
		if(this.player && this.player.isHuman && this.currentlySelected) {
			if(hitObject.name == "Ground") {
				if((this.player.hud.GetCursorState() == CursorState.RallyPoint 
					|| player.hud.GetPreviousCursorState() == CursorState.RallyPoint) 
					&& hitPoint != ResourceManager.InvalidPosition) {
					this.SetRallyPoint(hitPoint);
				}
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Place new rally point
	public void SetRallyPoint(Vector3 position) {
		this.rallyPoint = position;
		if(this.player && this.player.isHuman && this.currentlySelected) {
			RallyPoint flag = this.player.GetComponentInChildren<RallyPoint>();
			if(flag) {
				flag.transform.localPosition = this.rallyPoint;
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Sell this building
	public void Sell() {
		if(this.player) {
			this.player.AddResource(ResourceType.Energy, this.sellValue);
		}
		if(this.currentlySelected) {
			this.SetSelection(false, this.playingArea);
		}
		Destroy(this.gameObject);
	}

	// --------------------------------------------------------------------------------------------
	// Starts the construction of this building
	public void StartConstruction() {
		this.CalculateBounds();
		this.needsBuilding = true;
		this.hitPoints = 0;
	}

	// --------------------------------------------------------------------------------------------
	// Returns if the building is currently under construction
	public bool UnderConstruction() {
		return this.needsBuilding;
	}

	// --------------------------------------------------------------------------------------------
	// Construct the building
	public void Construct(int amount) {
		this.hitPoints += amount;
		if(this.hitPoints >= this.maxHitPoints) {
			this.hitPoints = this.maxHitPoints;
			this.needsBuilding = false;
			this.RestoreMaterials();
		}
	}
}
