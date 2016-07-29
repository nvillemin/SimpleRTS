﻿using UnityEngine;
using RTS;
using System.Collections.Generic;

public class HUD : MonoBehaviour {
	// --------------------------------------------------------------------------------------------
	// CONSTANTS
	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40, SELECTION_NAME_HEIGHT = 15,
		ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;

	// --------------------------------------------------------------------------------------------
	// UNITY VARIABLES
	public GUISkin resourcesSkin, ordersSkin, selectBoxSkin, mouseCursorSkin;
	public Texture2D activeCursor, selectCursor, leftCursor, rightCursor, upCursor, downCursor;
	public Texture2D[] moveCursors, attackCursors, harvestCursors, resources;

	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private Player player;
	private CursorState activeCursorState;
	private int currentFrame = 0;
	private Dictionary<ResourceType, int> resourceValues, resourceLimits;
	private Dictionary<ResourceType, Texture2D> resourceImages;

	// --------------------------------------------------------------------------------------------
	// Initialization
	void Start() {
		this.player = this.transform.root.GetComponent<Player>();
		ResourceManager.StoreSelectBoxItems(this.selectBoxSkin);
		this.SetCursorState(CursorState.Select);
		this.resourceValues = new Dictionary<ResourceType, int>();
		this.resourceLimits = new Dictionary<ResourceType, int>();
		resourceImages = new Dictionary<ResourceType, Texture2D>();
		for(int i = 0; i < this.resources.Length; i++) {
			switch(this.resources[i].name) {
				case "Money":
					resourceImages.Add(ResourceType.Money, this.resources[i]);
					resourceValues.Add(ResourceType.Money, 0);
					resourceLimits.Add(ResourceType.Money, 0);
					break;
				case "Power":
					resourceImages.Add(ResourceType.Power, this.resources[i]);
					resourceValues.Add(ResourceType.Power, 0);
					resourceLimits.Add(ResourceType.Power, 0);
					break;
				default:
					break;
			}
		}
	}

	// --------------------------------------------------------------------------------------------
	// Draw bars and cursor once per frame
	void OnGUI() {
		if(this.player && this.player.isHuman) {
			this.DrawOrdersBar();
			this.DrawResourcesBar();
			this.DrawMouseCursor();
		}
	}

	// --------------------------------------------------------------------------------------------
	// Draw the bar for the orders
	private void DrawOrdersBar() {
		GUI.skin = this.ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width - ORDERS_BAR_WIDTH, RESOURCE_BAR_HEIGHT,
			ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");
		if(this.player.SelectedObject) {
			string selectionName = this.player.SelectedObject.objectName;
			GUI.Label(new Rect(0, 10, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
		}
		GUI.EndGroup();
	}

	// --------------------------------------------------------------------------------------------
	// Draw the bar for the resources
	private void DrawResourcesBar() {
		GUI.skin = this.resourcesSkin;
		GUI.BeginGroup(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");
		int topPos = 4, iconLeft = 4, textLeft = 20;
		this.DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);
		iconLeft += TEXT_WIDTH;
		textLeft += TEXT_WIDTH;
		this.DrawResourceIcon(ResourceType.Power, iconLeft, textLeft, topPos);
		GUI.EndGroup();
	}

	// --------------------------------------------------------------------------------------------
	// Draw the resource icon
	private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos) {
		Texture2D icon = this.resourceImages[type];
		string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();
		GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
		GUI.Label(new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
	}

	// --------------------------------------------------------------------------------------------
	// Check if the mouse is inside the game screen without the bars
	public bool MouseInBounds() {
		// Screen coordinates start in the lower-left corner of the screen
		// not the top-left of the screen like the drawing coordinates do
		Vector3 mousePos = Input.mousePosition;
		bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
		bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;

		return insideWidth && insideHeight;
	}

	// --------------------------------------------------------------------------------------------
	// Draw the mouse cursor according to HUD
	private void DrawMouseCursor() {
		bool mouseOverHud = !MouseInBounds() && this.activeCursorState != CursorState.PanRight 
			&& this.activeCursorState != CursorState.PanUp;
		if(mouseOverHud) {
			Cursor.visible = true;
		} else {
			Cursor.visible = false;
			GUI.skin = this.mouseCursorSkin;
			GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
			this.UpdateCursorAnimation();
			Rect cursorPosition = this.GetCursorDrawPosition();
			GUI.Label(cursorPosition, this.activeCursor);
			GUI.EndGroup();
		}
	}

	// --------------------------------------------------------------------------------------------
	// Sequence animation for cursor (based on more than one image for the cursor)
	// Change once per second, loops through array of images
	private void UpdateCursorAnimation() {
		switch(this.activeCursorState) {
			case CursorState.Move :
				this.currentFrame = (int)Time.time % this.moveCursors.Length;
				this.activeCursor = this.moveCursors[this.currentFrame];
				break;
			case CursorState.Attack:
				this.currentFrame = (int)Time.time % this.attackCursors.Length;
				this.activeCursor = this.attackCursors[this.currentFrame];
				break;
			case CursorState.Harvest:
				this.currentFrame = (int)Time.time % this.harvestCursors.Length;
				this.activeCursor = this.harvestCursors[this.currentFrame];
				break;
		}
	}

	// --------------------------------------------------------------------------------------------
	private Rect GetCursorDrawPosition() {
		// Set base position for custom cursor image
		float leftPos = Input.mousePosition.x;
		float topPos = Screen.height - Input.mousePosition.y; // screen draw coordinates are inverted
		topPos -= this.activeCursor.height / 2;
		leftPos -= this.activeCursor.width / 2;

		// Adjust position based on the type of cursor being shown
		if(this.activeCursorState == CursorState.PanRight) {
			leftPos = Screen.width - this.activeCursor.width;
		} else if(this.activeCursorState == CursorState.PanDown) {
			topPos = Screen.height - this.activeCursor.height;
		}

		return new Rect(leftPos, topPos, this.activeCursor.width, this.activeCursor.height);
	}

	// --------------------------------------------------------------------------------------------
	// Change cursor image
	public void SetCursorState(CursorState newState) {
		this.activeCursorState = newState;
		switch(newState) {
			case CursorState.Select:
				this.activeCursor = this.selectCursor;
				break;
			case CursorState.Attack:
				this.currentFrame = (int)Time.time % this.attackCursors.Length;
				this.activeCursor = this.attackCursors[this.currentFrame];
				break;
			case CursorState.Harvest:
				this.currentFrame = (int)Time.time % harvestCursors.Length;
				this.activeCursor = this.harvestCursors[this.currentFrame];
				break;
			case CursorState.Move:
				this.currentFrame = (int)Time.time % this.moveCursors.Length;
				this.activeCursor = this.moveCursors[this.currentFrame];
				break;
			case CursorState.PanLeft:
				this.activeCursor = this.leftCursor;
				break;
			case CursorState.PanRight:
				this.activeCursor = this.rightCursor;
				break;
			case CursorState.PanUp:
				this.activeCursor = this.upCursor;
				break;
			case CursorState.PanDown:
				this.activeCursor = this.downCursor;
				break;
			default:
				break;
		}
	}

	// --------------------------------------------------------------------------------------------
	// Get the game rectangle without the bars
	public Rect GetPlayingArea() {
		return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH,
			Screen.height - RESOURCE_BAR_HEIGHT);
	}

	// --------------------------------------------------------------------------------------------
	// Update the resource values
	public void SetResourceValues(Dictionary<ResourceType, int> resourceValues, Dictionary<ResourceType, int> resourceLimits) {
		this.resourceValues = resourceValues;
		this.resourceLimits = resourceLimits;
	}
}
