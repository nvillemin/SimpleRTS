using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using RTS;

public class Building : WorldObject {
	// --------------------------------------------------------------------------------------------
	// UNITY VARIABLES
	public float maxBuildProgress;

	// --------------------------------------------------------------------------------------------
	// PROTECTED VARIABLES
	protected Queue<string> buildQueue;

	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private float currentBuildProgress = 0.0f;
	private Vector3 spawnPoint;

	// --------------------------------------------------------------------------------------------
	// Initialization
	protected override void Awake() {
		base.Awake();
		this.buildQueue = new Queue<string>();
		float spawnX = this.selectionBounds.center.x + this.transform.forward.x 
			* this.selectionBounds.extents.x + this.transform.forward.x * 10;
		float spawnZ = this.selectionBounds.center.z + this.transform.forward.z 
			+ this.selectionBounds.extents.z + this.transform.forward.z * 10;
		this.spawnPoint = new Vector3(spawnX, 0.0f, spawnZ);
	}

	// --------------------------------------------------------------------------------------------
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
					this.player.AddUnit(this.buildQueue.Dequeue(), this.spawnPoint, this.transform.rotation);
				}
				this.currentBuildProgress = 0.0f;
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Get all units from queue
	public string[] getBuildQueueValues() {
		string[] values = new string[this.buildQueue.Count];
		int pos = 0;
		foreach(string unit in this.buildQueue) {
			values[pos++] = unit;
		}
		return values;
	}

	// --------------------------------------------------------------------------------------------
	// Get current build percentage
	public float getBuildPercentage() {
		return this.currentBuildProgress / this.maxBuildProgress;
	}
}
