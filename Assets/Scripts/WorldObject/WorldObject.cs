using UnityEngine;
using System.Collections;
using System;
using RTS;

public class WorldObject : MonoBehaviour {
	// --------------------------------------------------------------------------------------------
	// UNITY VARIABLES
	public string objectName;
	public Texture2D buildImage;
	public int cost, sellValue, hitPoints, maxHitPoints;

	// --------------------------------------------------------------------------------------------
	// PROTECTED VARIABLES
	protected Player player;
	protected string[] actions = {};
	protected bool currentlySelected = false;
	protected Bounds selectionBounds;
	protected Rect playingArea = new Rect(0.0f, 0.0f, 0.0f, 0.0f);

	// --------------------------------------------------------------------------------------------
	protected virtual void Awake() {
		this.selectionBounds = ResourceManager.InvalidBounds;
		CalculateBounds();
	}

	// --------------------------------------------------------------------------------------------
	protected virtual void Start() {
		this.player = this.transform.root.GetComponentInChildren<Player>();
	}

	// --------------------------------------------------------------------------------------------
	protected virtual void Update() {

	}

	// --------------------------------------------------------------------------------------------
	protected virtual void OnGUI() {
		if(this.currentlySelected) {
			this.DrawSelection();
		}
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
		Rect selectBox = WorkManager.CalculateSelectionBox(selectionBounds, playingArea);
		// Draw the selection box around the currently selected object
		GUI.BeginGroup(playingArea);
		this.DrawSelectionBox(selectBox);
		GUI.EndGroup();
	}

	// --------------------------------------------------------------------------------------------
	protected virtual void DrawSelectionBox(Rect selectBox) {
		GUI.Box(selectBox, "");
	}

	// --------------------------------------------------------------------------------------------
	public void SetSelection(bool selected, Rect playingArea) {
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
}
