using UnityEngine;
using System.Collections;

using RTS;

public class GameObjectList : MonoBehaviour {
	// --------------------------------------------------------------------------------------------
	// UNITY VARIABLES
	public GameObject[] buildings;
	public GameObject[] units;
	public GameObject[] worldObjects;
	public GameObject player;

	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private static bool created = false;

	// --------------------------------------------------------------------------------------------
	// Initialization
	void Awake() {
		if(!created) {
			DontDestroyOnLoad(this.transform.gameObject);
			ResourceManager.SetGameObjectList(this);
			created = true;
		} else {
			Destroy(this.gameObject);
		}
	}

	// --------------------------------------------------------------------------------------------
	// Building accessor
	public GameObject GetBuilding(string name) {
		for(int i = 0; i < buildings.Length; i++) {
			Building building = buildings[i].GetComponent<Building>();
			if(building && building.name == name)
				return buildings[i];
		}
		return null;
	}

	// --------------------------------------------------------------------------------------------
	// Unit accessor
	public GameObject GetUnit(string name) {
		for(int i = 0; i < units.Length; i++) {
			Unit unit = units[i].GetComponent<Unit>();
			if(unit && unit.name == name)
				return units[i];
		}
		return null;
	}

	// --------------------------------------------------------------------------------------------
	// WorldObject accessor
	public GameObject GetWorldObject(string name) {
		foreach(GameObject worldObject in worldObjects) {
			if(worldObject.name == name)
				return worldObject;
		}
		return null;
	}

	// --------------------------------------------------------------------------------------------
	// Player accessor
	public GameObject GetPlayerObject() {
		return player;
	}

	// --------------------------------------------------------------------------------------------
	// Build image accessor
	public Texture2D GetBuildImage(string name) {
		for(int i = 0; i < buildings.Length; i++) {
			Building building = buildings[i].GetComponent<Building>();
			if(building && building.name == name)
				return building.buildImage;
		}
		for(int i = 0; i < units.Length; i++) {
			Unit unit = units[i].GetComponent<Unit>();
			if(unit && unit.name == name)
				return unit.buildImage;
		}
		return null;
	}
}
