﻿using UnityEngine;
using System.Collections;
using System;
using RTS;
using System.Collections.Generic;

public class WorldObject : MonoBehaviour {
	// --------------------------------------------------------------------------------------------
	// UNITY VARIABLES
	public string objectName;
	public Texture2D buildImage;
	public int energyBuildCost, energyProd, energyCost, metalBuildCost, metalProd, metalCost, 
		sellValue, hitPoints, maxHitPoints;

	// --------------------------------------------------------------------------------------------
	// PROTECTED VARIABLES
	protected Player player;
	protected string[] actions = {};
	protected bool currentlySelected = false, needsBuilding = false;
	protected Bounds selectionBounds;
	protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
	protected GUIStyle healthStyle = new GUIStyle();
	protected float healthPercentage = 1.0f;

	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private List<Material> oldMaterials = new List<Material>();

	// --------------------------------------------------------------------------------------------
	protected virtual void Awake() {
		this.selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds();
	}

	// --------------------------------------------------------------------------------------------
	protected virtual void Start() {
		this.SetPlayer();
	}

	// --------------------------------------------------------------------------------------------
	protected virtual void Update() {
		if(this.player) {
			this.player.AddResource(ResourceType.Energy, (this.energyProd - this.energyCost)
				* Time.deltaTime);
			this.player.AddResource(ResourceType.Metal, (this.metalProd - this.metalCost)
				* Time.deltaTime);
		}
	}

	// --------------------------------------------------------------------------------------------
	protected virtual void OnGUI() {
		if(this.currentlySelected) {
			this.DrawSelection();
		}
	}

	// --------------------------------------------------------------------------------------------
	public void SetPlayer() {
		player = transform.root.GetComponentInChildren<Player>();
	}

	// --------------------------------------------------------------------------------------------
	public void CalculateBounds() {
		this.selectionBounds = new Bounds(transform.position, Vector3.zero);
		foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
			this.selectionBounds.Encapsulate(r.bounds);
		}
	}

	// --------------------------------------------------------------------------------------------
	private void DrawSelection() {
		GUI.skin = ResourceManager.SelectBoxSkin;
		Rect selectBox = WorkManager.CalculateSelectionBox(this.selectionBounds, this.playingArea);
		// Draw the selection box around the currently selected object
		GUI.BeginGroup(this.playingArea);
		this.DrawSelectionBox(selectBox);
		GUI.EndGroup();
	}

	// --------------------------------------------------------------------------------------------
	// Draw the selection box around the object when it's selected
	protected virtual void DrawSelectionBox(Rect selectBox) {
		GUI.Box(selectBox, "");
		this.CalculateCurrentHealth(0.35f, 0.65f);
		this.DrawHealthBar(selectBox, "");
	}

	// --------------------------------------------------------------------------------------------
	// Change the way the health is displayed according to the percentage left
	protected virtual void CalculateCurrentHealth(float lowSplit, float highSplit) {
		this.healthPercentage = (float)hitPoints / (float)maxHitPoints;
		if(this.healthPercentage > highSplit) {
			this.healthStyle.normal.background = ResourceManager.HealthyTexture;
		} else if(this.healthPercentage > lowSplit) {
			this.healthStyle.normal.background = ResourceManager.DamagedTexture;
		} else {
			this.healthStyle.normal.background = ResourceManager.CriticalTexture;
		}
	}

	// --------------------------------------------------------------------------------------------
	// Draw the health bar of the object according to the health percentage
	protected void DrawHealthBar(Rect selectBox, string label) {
		this.healthStyle.padding.top = -20;
		this.healthStyle.fontStyle = FontStyle.Bold;
		GUI.Label(new Rect(selectBox.x, selectBox.y - 7, selectBox.width * this.healthPercentage, 
			5),	label, this.healthStyle);
	}

	// --------------------------------------------------------------------------------------------
	public virtual void SetSelection(bool selected, Rect playingArea) {
		this.currentlySelected = selected;
		if(selected) {
			this.playingArea = playingArea;
		}
	}

	// --------------------------------------------------------------------------------------------
	public string[] GetActions() {
		return actions;
	}

	// --------------------------------------------------------------------------------------------
	public virtual void PerformAction(string actionToPerform) {
		// it's up to children with specific actions to determine what to do with each actions
	}

	// --------------------------------------------------------------------------------------------
	public virtual void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		// only handle input if currently selected
		if(currentlySelected && hitObject && hitObject.name != "Ground") {
			WorldObject worldObject = hitObject.transform.parent.GetComponent<WorldObject>();
			// clicked on another selectable object
			if(worldObject) {
				this.ChangeSelection(worldObject, controller);
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	private void ChangeSelection(WorldObject worldObject, Player controller) {
		// this should be called by the following line, but there is an outside chance it will not
		this.SetSelection(false, this.playingArea);
		if(controller.SelectedObject) {
			controller.SelectedObject.SetSelection(false, this.playingArea);
		}
		controller.SelectedObject = worldObject;
		worldObject.SetSelection(true, controller.hud.GetPlayingArea());
	}

	// --------------------------------------------------------------------------------------------
	public virtual void SetHoverState(GameObject hoverObject) {
		// only handle input if owned by a human player and currently selected
		if(player && player.isHuman && currentlySelected) {
			if(hoverObject.name != "Ground")
				player.hud.SetCursorState(CursorState.Select);
		}
	}

	// --------------------------------------------------------------------------------------------
	public bool IsOwnedBy(Player owner) {
		return player && player.Equals(owner);
	}

	// --------------------------------------------------------------------------------------------
	// Enable or disable colliders
	public void SetColliders(bool enabled) {
		Collider[] colliders = GetComponentsInChildren<Collider>();
		foreach(Collider collider in colliders) {
			collider.enabled = enabled;
		}
	}

	// --------------------------------------------------------------------------------------------
	public void SetTransparentMaterial(Material material, bool storeExistingMaterial) {
		if(storeExistingMaterial) {
			this.oldMaterials.Clear();
		}
		Component[] components = GetComponentsInChildren(typeof(Renderer));
		foreach(Component comp in components) {
			Renderer renderer = (Renderer)comp;
			if(storeExistingMaterial) {
				this.oldMaterials.Add(renderer.material);
			}
			renderer.material = material;
		}
	}

	// --------------------------------------------------------------------------------------------
	public void RestoreMaterials() {
		Component[] components = GetComponentsInChildren(typeof(Renderer));
		if(this.oldMaterials.Count == components.Length) {
			for(int i = 0; i < components.Length; i++) {
				Renderer renderer = (Renderer)components[i];
				renderer.material = this.oldMaterials[i];
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	public void SetPlayingArea(Rect playingArea) {
		this.playingArea = playingArea;
	}

	// --------------------------------------------------------------------------------------------
	public Bounds GetSelectionBounds() {
		return this.selectionBounds;
	}

	// --------------------------------------------------------------------------------------------
	public bool NeedsBuilding() {
		return this.needsBuilding;
	}

	// --------------------------------------------------------------------------------------------
	public int GetProductionValue(ResourceType type, bool getCost) {
		int value;
		if(getCost) {
			value = (type == ResourceType.Energy) ? this.energyCost : this.metalCost;
		} else {
			value = (type == ResourceType.Energy) ? this.energyProd : this.metalProd;
		}
		return value;
	}
}
