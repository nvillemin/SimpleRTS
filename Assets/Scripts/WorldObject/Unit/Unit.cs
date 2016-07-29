using UnityEngine;
using System.Collections;
using RTS;

public class Unit : WorldObject {
	// --------------------------------------------------------------------------------------------
	// UNITY VARIABLES
	public float moveSpeed, rotateSpeed;

	// --------------------------------------------------------------------------------------------
	// PROTECTED VARIABLES
	protected bool moving, rotating;

	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private Vector3 destination;
	private Quaternion targetRotation;

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
	// Called each frame, move or rotate
	protected override void Update() {
		base.Update();
		if(this.rotating) {
			this.TurnToTarget();
		} else if(this.moving) {
			this.MakeMove();
		}
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
		if(this.player && this.player.isHuman && this.currentlySelected) {
			if(hoverObject.name == "Ground")
				this.player.hud.SetCursorState(CursorState.Move);
		}
	}

	// --------------------------------------------------------------------------------------------
	public override void MouseClick(GameObject hitObject, Vector3 hitPoint, Player controller) {
		base.MouseClick(hitObject, hitPoint, controller);
		//only handle input if owned by a human player and currently selected
		if(this.player && this.player.isHuman && this.currentlySelected) {
			if(hitObject.name == "Ground" && hitPoint != ResourceManager.InvalidPosition) {
				float x = hitPoint.x;
				//makes sure that the unit stays on top of the surface it is on
				float y = hitPoint.y + this.player.SelectedObject.transform.position.y;
				float z = hitPoint.z;
				Vector3 destination = new Vector3(x, y, z);
				this.StartMove(destination);
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Starts the moving command, rotating first
	public void StartMove(Vector3 destination) {
		this.destination = destination;
		this.targetRotation = Quaternion.LookRotation(destination - this.transform.position);
		this.rotating = true;
		this.moving = false;
	}

	// --------------------------------------------------------------------------------------------
	// Rotate towards the destination
	private void TurnToTarget() {
		this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, targetRotation
			, rotateSpeed);
		// sometimes it gets stuck exactly 180 degrees out in the calculation and does nothing
		// this check fixes that
		Quaternion inverseTargetRotation = new Quaternion(-targetRotation.x, -targetRotation.y
			, -targetRotation.z, -targetRotation.w);
		if(this.transform.rotation == targetRotation 
			|| this.transform.rotation == inverseTargetRotation) {
			this.rotating = false;
			this.moving = true;
		}
		this.CalculateBounds();
	}

	// --------------------------------------------------------------------------------------------
	// Move unit towards the destination
	private void MakeMove() {
		this.transform.position = Vector3.MoveTowards(this.transform.position, destination
			, Time.deltaTime * moveSpeed);
		if(this.transform.position == destination) {
			this.moving = false;
		}
		this.CalculateBounds();
	}
}
