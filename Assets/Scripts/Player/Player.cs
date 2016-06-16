using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	public HUD hud;
	public WorldObject SelectedObject { get; set; }
	public string username;
    public bool isHuman;

    // Use this for initialization
    void Start () {
		this.hud = GetComponentInChildren<HUD>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
