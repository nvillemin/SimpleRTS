using UnityEngine;
using RTS;
using System.Collections.Generic;
using System;

public class HUD : MonoBehaviour {
	// --------------------------------------------------------------------------------------------
	// CONSTANTS
	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40, SELECTION_NAME_HEIGHT = 15,
		ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32, 
		BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64, BUTTON_SPACING = 7, SCROLL_BAR_WIDTH = 22,
		BUILD_IMAGE_PADDING = 8;

	// --------------------------------------------------------------------------------------------
	// UNITY VARIABLES
	public GUISkin resourcesSkin, ordersSkin, selectBoxSkin, mouseCursorSkin;
	public Texture2D activeCursor, selectCursor, leftCursor, rightCursor, upCursor, downCursor,
		buttonHover, buttonClick, buildFrame, buildMask, smallButtonHover, smallButtonClick,
		rallyPointCursor, healthHealthy, healthDamaged, healthCritical;
	public Texture2D[] moveCursors, attackCursors, harvestCursors, resources;

	// --------------------------------------------------------------------------------------------
	// PRIVATE VARIABLES
	private Player player;
	private CursorState activeCursorState, previousCursorState;
	private int currentFrame = 0, buildAreaHeight = 0;
	private Dictionary<ResourceType, int> resourceValues, resourceLimits;
	private Dictionary<ResourceType, Texture2D> resourceImages;
	private WorldObject lastSelection;
	private float sliderValue;

	// --------------------------------------------------------------------------------------------
	// Initialization
	void Start() {
		this.player = this.transform.root.GetComponent<Player>();
		ResourceManager.StoreSelectBoxItems(selectBoxSkin, this.healthHealthy, this.healthDamaged,
			this.healthCritical);
		this.SetCursorState(CursorState.Select);
		this.resourceValues = new Dictionary<ResourceType, int>();
		this.resourceLimits = new Dictionary<ResourceType, int>();
		this.resourceImages = new Dictionary<ResourceType, Texture2D>();
		for(int i = 0; i < this.resources.Length; i++) {
			switch(this.resources[i].name) {
				case "Energy":
					this.resourceImages.Add(ResourceType.Energy, this.resources[i]);
					this.resourceValues.Add(ResourceType.Energy, 0);
					this.resourceLimits.Add(ResourceType.Energy, 0);
					break;
				case "Metal":
					this.resourceImages.Add(ResourceType.Metal, this.resources[i]);
					this.resourceValues.Add(ResourceType.Metal, 0);
					this.resourceLimits.Add(ResourceType.Metal, 0);
					break;
				default:
					break;
			}
		}
		this.buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 
			* BUTTON_SPACING;
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
		GUI.BeginGroup(new Rect(Screen.width - ORDERS_BAR_WIDTH - BUILD_IMAGE_WIDTH, 
			RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH + BUILD_IMAGE_WIDTH, 
			Screen.height - RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH, 0, ORDERS_BAR_WIDTH, 
			Screen.height - RESOURCE_BAR_HEIGHT), "");
		if(this.player.SelectedObject) {
			string selectionName = this.player.SelectedObject.objectName;
			if(!selectionName.Equals("")) {
				int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH / 2;
				int topPos = this.buildAreaHeight + BUTTON_SPACING;
				GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT),
					selectionName);
			}
			if(this.player.SelectedObject.IsOwnedBy(player) && !this.player.SelectedObject.NeedsBuilding()) {
				// reset slider value if the selected object has changed
				if(this.lastSelection && this.lastSelection != this.player.SelectedObject) {
					this.sliderValue = 0.0f;
				}
				this.DrawActions(this.player.SelectedObject.GetActions());
				// store the current selection
				this.lastSelection = this.player.SelectedObject;

				Building selectedBuilding = this.lastSelection.GetComponent<Building>();
				if(selectedBuilding && !selectedBuilding.NeedsBuilding()) {
					this.DrawBuildQueue(selectedBuilding.GetBuildQueueValues(),
						selectedBuilding.GetBuildPercentage());
					this.DrawStandardBuildingOptions(selectedBuilding);
				}
			}
		}
		GUI.EndGroup();
	}

	// --------------------------------------------------------------------------------------------
	// Draw the build queue for the selected building
	private void DrawBuildQueue(string[] buildQueue, float buildPercentage) {
		for(int i = 0; i < buildQueue.Length; i++) {
			float topPos = i * BUILD_IMAGE_HEIGHT - (i + 1) * BUILD_IMAGE_PADDING;
			Rect buildPos = new Rect(BUILD_IMAGE_PADDING, topPos, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
			GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(buildQueue[i]));
			GUI.DrawTexture(buildPos, this.buildFrame);
			topPos += BUILD_IMAGE_PADDING;
			float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
			float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
			if(i == 0) {
				// shrink the build mask on the item currently being built to give an idea of progress
				topPos += height * buildPercentage;
				height *= (1 - buildPercentage);
			}
			GUI.DrawTexture(new Rect(2 * BUILD_IMAGE_PADDING, topPos, width, height), this.buildMask);
		}
	}

	// --------------------------------------------------------------------------------------------
	// Draw the bar for the resources
	private void DrawResourcesBar() {
		GUI.skin = this.resourcesSkin;
		GUI.BeginGroup(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");
		int topPos = 4, iconLeft = 4, textLeft = 20;
		this.DrawResourceIcon(ResourceType.Energy, iconLeft, textLeft, topPos);
		iconLeft += TEXT_WIDTH;
		textLeft += TEXT_WIDTH;
		this.DrawResourceIcon(ResourceType.Metal, iconLeft, textLeft, topPos);
		GUI.EndGroup();
	}

	// --------------------------------------------------------------------------------------------
	// Draw building options for the current selected building
	private void DrawStandardBuildingOptions(Building building) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = this.smallButtonHover;
		buttons.active.background = this.smallButtonClick;
		GUI.skin.button = buttons;
		int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + BUTTON_SPACING;
		int topPos = this.buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
		int width = BUILD_IMAGE_WIDTH / 2;
		int height = BUILD_IMAGE_HEIGHT / 2;
		if(GUI.Button(new Rect(leftPos, topPos, width, height), building.sellImage)) {
			building.Sell();
		}
		if(building.HasSpawnPoint()) {
			leftPos += width + BUTTON_SPACING;
			if(GUI.Button(new Rect(leftPos, topPos, width, height), building.rallyPointImage)) {
				if(this.activeCursorState != CursorState.RallyPoint 
					&& this.previousCursorState != CursorState.RallyPoint) {
					this.SetCursorState(CursorState.RallyPoint);
				} else {
					// dirty hack to ensure toggle works
					this.SetCursorState(CursorState.PanRight);
					this.SetCursorState(CursorState.Select);
				}
			}
		}
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
			if(!this.player.IsFindingBuildingLocation()) {
				GUI.skin = this.mouseCursorSkin;
				GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
				this.UpdateCursorAnimation();
				Rect cursorPosition = this.GetCursorDrawPosition();
				GUI.Label(cursorPosition, this.activeCursor);
				GUI.EndGroup();
			}
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
		} else if(this.activeCursorState == CursorState.RallyPoint) {
			topPos -= this.activeCursor.height;
		}

		return new Rect(leftPos, topPos, this.activeCursor.width, this.activeCursor.height);
	}

	// --------------------------------------------------------------------------------------------
	// Change cursor image
	public void SetCursorState(CursorState newState) {
		if(this.activeCursorState != newState) {
			this.previousCursorState = this.activeCursorState;
			this.activeCursorState = newState;
		}
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
			case CursorState.RallyPoint:
				this.activeCursor = this.rallyPointCursor;
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
	public void SetResourceValues(Dictionary<ResourceType, float> resourceValues, Dictionary<ResourceType, int> resourceLimits) {
		foreach(ResourceType resource in resourceValues.Keys) {
			this.resourceValues[resource] = (int)Math.Floor(resourceValues[resource]);
		}
		this.resourceLimits = resourceLimits;
	}

	// --------------------------------------------------------------------------------------------
	// Draw actions
	private void DrawActions(string[] actions) {
		GUIStyle buttons = new GUIStyle();
		buttons.hover.background = this.buttonHover;
		buttons.active.background = this.buttonClick;
		GUI.skin.button = buttons;
		int numActions = actions.Length;
		// define the area to draw the actions inside
		GUI.BeginGroup(new Rect(BUILD_IMAGE_WIDTH, 0, ORDERS_BAR_WIDTH, buildAreaHeight));
		// draw scroll bar for the list of actions if need be
		if(numActions >= this.MaxNumRows(this.buildAreaHeight))
			this.DrawSlider(this.buildAreaHeight, numActions / 2.0f);
		// display possible actions as buttons and handle the button click for each
		for(int i = 0; i < numActions; i++) {
			int column = i % 2;
			int row = i / 2;
			Rect pos = this.GetButtonPos(row, column);
			Texture2D action = ResourceManager.GetBuildImage(actions[i]);
			if(action) {
				// create the button and handle the click of that button
				if(GUI.Button(pos, action)) {
					if(this.player.SelectedObject) {
						this.player.SelectedObject.PerformAction(actions[i]);
					}
				}
			}
		}
		GUI.EndGroup();
	}

	// --------------------------------------------------------------------------------------------
	private int MaxNumRows(int areaHeight) {
		return areaHeight / BUILD_IMAGE_HEIGHT;
	}

	// --------------------------------------------------------------------------------------------
	private Rect GetButtonPos(int row, int column) {
		int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
		float top = row * BUILD_IMAGE_HEIGHT - this.sliderValue * BUILD_IMAGE_HEIGHT;
		return new Rect(left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
	}

	// --------------------------------------------------------------------------------------------
	// Draw orders slider
	private void DrawSlider(int groupHeight, float numRows) {
		//slider goes from 0 to the number of rows that do not fit on screen
		this.sliderValue = GUI.VerticalSlider(this.GetScrollPos(groupHeight), this.sliderValue, 
			0.0f, numRows - this.MaxNumRows(groupHeight));
	}

	// --------------------------------------------------------------------------------------------
	private Rect GetScrollPos(int groupHeight) {
		return new Rect(BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, groupHeight - 2 
			* BUTTON_SPACING);
	}

	// --------------------------------------------------------------------------------------------
	public CursorState GetPreviousCursorState() {
		return this.previousCursorState;
	}

	// --------------------------------------------------------------------------------------------
	public CursorState GetCursorState() {
		return this.activeCursorState;
	}
}
