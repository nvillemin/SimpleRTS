using UnityEngine;
using System.Collections;
using RTS;
using System;

public class UserInput : MonoBehaviour {
	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private Player player;

	// --------------------------------------------------------------------------------------------
	// Initialization
	void Start() {
	    this.player = transform.root.GetComponent<Player>();
    }

	// --------------------------------------------------------------------------------------------
	// Update is called once per frame
	void Update() {
        if(this.player.isHuman) {
            this.MoveCamera();
            this.RotateCamera();
			this.MouseActivity();
        }
    }

	// --------------------------------------------------------------------------------------------
	// Camera movements
	private void MoveCamera() {
		float xPos = Input.mousePosition.x;
		float yPos = Input.mousePosition.y;
		Vector3 movement = new Vector3(0, 0, 0);
		bool mouseScroll = false;

		// horizontal camera movement
		if(xPos >= 0 && xPos < ResourceManager.ScrollWidth) {
			movement.x -= ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanLeft);
			mouseScroll = true;
		} else if(xPos <= Screen.width && xPos > Screen.width - ResourceManager.ScrollWidth) {
			movement.x += ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanRight);
			mouseScroll = true;
		}

		// vertical camera movement
		if(yPos >= 0 && yPos < ResourceManager.ScrollWidth) {
			movement.z -= ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanDown);
			mouseScroll = true;
		} else if(yPos <= Screen.height && yPos > Screen.height - ResourceManager.ScrollWidth) {
			movement.z += ResourceManager.ScrollSpeed;
			player.hud.SetCursorState(CursorState.PanUp);
			mouseScroll = true;
		}

		// make sure movement is in the direction the camera is pointing
		// but ignore the vertical tilt of the camera to get sensible scrolling
		movement = Camera.main.transform.TransformDirection(movement);
		movement.y = 0;

		// away from ground movement
		movement.y -= ResourceManager.ScrollSpeed * Input.GetAxis("Mouse ScrollWheel");

		// calculate desired camera position based on received input
		Vector3 origin = Camera.main.transform.position;
		Vector3 destination = origin + movement;

		// limit away from ground movement to be between a minimum and maximum distance
		if(destination.y > ResourceManager.MaxCameraHeight) {
			destination.y = ResourceManager.MaxCameraHeight;
		} else if(destination.y < ResourceManager.MinCameraHeight) {
			destination.y = ResourceManager.MinCameraHeight;
		}

		// if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.main.transform.position = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.ScrollSpeed);
		}

		// change the cursor if not panning
		if(!mouseScroll) {
			player.hud.SetCursorState(CursorState.Select);
		}
	}

	// --------------------------------------------------------------------------------------------
	// Rotate the camera with the ALT key
	private void RotateCamera() {
		Vector3 origin = Camera.main.transform.eulerAngles;
		Vector3 destination = origin;

		// detect rotation amount if ALT is being held and the Right mouse button is down
		if((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)) && Input.GetMouseButton(1)) {
			destination.x -= Input.GetAxis("Mouse Y") * ResourceManager.RotateAmount;
			destination.y += Input.GetAxis("Mouse X") * ResourceManager.RotateAmount;
		}

		// if a change in position is detected perform the necessary update
		if(destination != origin) {
			Camera.main.transform.eulerAngles = Vector3.MoveTowards(origin, destination, Time.deltaTime * ResourceManager.RotateSpeed);
		}
	}

	// --------------------------------------------------------------------------------------------
	// Used to detect when mouse buttons are pressed
	private void MouseActivity() {
		if(Input.GetMouseButtonDown(0)) {
			this.LeftMouseClick();
		} else if(Input.GetMouseButtonDown(1)) {
			this.RightMouseClick();
		}
		this.MouseHover();
	}

	// --------------------------------------------------------------------------------------------
	// Used to change the cursor when hovering a selectable object
	private void MouseHover() {
		if(this.player.hud.MouseInBounds()) {
			if(this.player.IsFindingBuildingLocation()) {
				this.player.FindBuildingLocation();
			} else {
				GameObject hoverObject = FindHitObject();
				if(hoverObject) {
					if(this.player.SelectedObject)
						this.player.SelectedObject.SetHoverState(hoverObject);
					else if(hoverObject.name != "Ground") {
						Player owner = hoverObject.transform.root.GetComponent<Player>();
						if(owner) {
							Unit unit = hoverObject.transform.parent.GetComponent<Unit>();
							Building building = hoverObject.transform.parent.GetComponent<Building>();
							if(owner.username == this.player.username && (unit || building)) {
								this.player.hud.SetCursorState(CursorState.Select);
							}
						}
					}
				}
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Triggered when the left mouse button is clicked
	private void LeftMouseClick() {
		if(this.player.hud.MouseInBounds()) {
			if(player.IsFindingBuildingLocation()) {
				if(player.CanPlaceBuilding()) {
					this.player.StartConstruction();
				}
			} else {
				GameObject hitObject = FindHitObject();
				Vector3 hitPoint = FindHitPoint();
				if(hitObject && hitPoint != ResourceManager.InvalidPosition) {
					if(this.player.SelectedObject) {
						this.player.SelectedObject.MouseClick(hitObject, hitPoint, this.player);
					} else if(hitObject.name != "Ground") {
						WorldObject worldObject = hitObject.transform.parent
							.GetComponent<WorldObject>();
						if(worldObject) {
							// we already know the player has no selected object
							this.player.SelectedObject = worldObject;
							worldObject.SetSelection(true, player.hud.GetPlayingArea());
						}
					}
				}
			}
			
		}
	}

	// --------------------------------------------------------------------------------------------
	// Triggered when the right mouse button is clicked
	private void RightMouseClick() {
		if(this.player.hud.MouseInBounds() && !Input.GetKey(KeyCode.LeftAlt) 
			&& this.player.SelectedObject) {
			if(this.player.IsFindingBuildingLocation()) {
				this.player.CancelBuildingPlacement();
			} else {
				this.player.SelectedObject.SetSelection(false, this.player.hud.GetPlayingArea());
				this.player.SelectedObject = null;
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Raycasting to find the hit object
	private GameObject FindHitObject() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit)) {
			return hit.collider.gameObject;
		}
		return null;
	}

	// --------------------------------------------------------------------------------------------
	// Raycasting to find the hit point
	private Vector3 FindHitPoint() {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit)) {
			return hit.point;
		}
		return ResourceManager.InvalidPosition;
	}
}
