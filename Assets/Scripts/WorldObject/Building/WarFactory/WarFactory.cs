using UnityEngine;
using System.Collections;

public class WarFactory : Building {

	// --------------------------------------------------------------------------------------------
	// Initialization
	protected override void Start() {
		base.Start();
		this.actions = new string[] { "Tank" };
	}

	// --------------------------------------------------------------------------------------------
	// Make the war factory perform an action
	public override void PerformAction(string actionToPerform) {
		base.PerformAction(actionToPerform);
		this.CreateUnit(actionToPerform);
	}
}
