using UnityEngine;
using System.Collections;
using RTS;

public class Unit : WorldObject {
	/*** Game Engine methods, all can be overridden by subclass ***/

	// --------------------------------------------------------------------------------------------
	protected override void Awake() {
		base.Awake();
	}

	// --------------------------------------------------------------------------------------------
	protected override void Start() {
		base.Start();
	}

	// --------------------------------------------------------------------------------------------
	protected override void Update() {
		base.Update();
	}

	// --------------------------------------------------------------------------------------------
	protected override void OnGUI() {
		base.OnGUI();
	}

	// --------------------------------------------------------------------------------------------
	// Change cursor to move if selected
	public override void SetHoverState(GameObject hoverObject) {
		base.SetHoverState(hoverObject);
		// only handle input if owned by a human player and currently selected
		if(player && player.isHuman && currentlySelected) {
			if(hoverObject.name == "Ground")
				player.hud.SetCursorState(CursorState.Move);
		}
	}
}
