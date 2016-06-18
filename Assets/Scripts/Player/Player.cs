using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	// --------------------------------------------------------------------------------------------
	// UNITY VARIABLES
	public HUD hud;
	public WorldObject SelectedObject { get; set; }
	public string username;
    public bool isHuman;

	// --------------------------------------------------------------------------------------------
	// Initialization
	void Start () {
		this.hud = GetComponentInChildren<HUD>();
	}
}
