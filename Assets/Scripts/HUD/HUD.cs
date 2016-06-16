using UnityEngine;
using RTS;

public class HUD : MonoBehaviour {
	private const int ORDERS_BAR_WIDTH = 150, RESOURCE_BAR_HEIGHT = 40, SELECTION_NAME_HEIGHT = 15;

	public GUISkin resourcesSkin, ordersSkin, selectBoxSkin, mouseCursorSkin;
	public Texture2D activeCursor;
	public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
	public Texture2D[] moveCursors, attackCursors, harvestCursors;

	private Player player;
	private CursorState activeCursorState;
	private int currentFrame = 0;

	// Use this for initialization
	void Start() {
		this.player = this.transform.root.GetComponent<Player>();
		ResourceManager.StoreSelectBoxItems(selectBoxSkin);
		this.SetCursorState(CursorState.Select);
	}

	// OnGUI is called once per frame
	void OnGUI() {
		if(this.player && this.player.isHuman) {
			this.DrawOrdersBar();
			this.DrawResourcesBar();
			this.DrawMouseCursor();
		}
	}

	private void DrawOrdersBar() {
		GUI.skin = ordersSkin;
		GUI.BeginGroup(new Rect(Screen.width - ORDERS_BAR_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");
		if(player.SelectedObject) {
			string selectionName = player.SelectedObject.objectName;
			GUI.Label(new Rect(0, 10, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
		}
		GUI.EndGroup();
	}

	private void DrawResourcesBar() {
		GUI.skin = resourcesSkin;
		GUI.BeginGroup(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT));
		GUI.Box(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");
		GUI.EndGroup();
	}

	public bool MouseInBounds() {
		// Screen coordinates start in the lower-left corner of the screen
		// not the top-left of the screen like the drawing coordinates do
		Vector3 mousePos = Input.mousePosition;
		bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
		bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;

		return insideWidth && insideHeight;
	}

	private void DrawMouseCursor() {
		bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;
		if(mouseOverHud) {
			Cursor.visible = true;
		} else {
			Cursor.visible = false;
			GUI.skin = mouseCursorSkin;
			GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
			this.UpdateCursorAnimation();
			Rect cursorPosition = GetCursorDrawPosition();
			GUI.Label(cursorPosition, activeCursor);
			GUI.EndGroup();
		}
	}

	private void UpdateCursorAnimation() {
		// sequence animation for cursor (based on more than one image for the cursor)
		// change once per second, loops through array of images
		switch(activeCursorState) {
			case CursorState.Move :
				currentFrame = (int)Time.time % moveCursors.Length;
				activeCursor = moveCursors[currentFrame];
				break;
			case CursorState.Attack:
				currentFrame = (int)Time.time % attackCursors.Length;
				activeCursor = attackCursors[currentFrame];
				break;
			case CursorState.Harvest:
				currentFrame = (int)Time.time % harvestCursors.Length;
				activeCursor = harvestCursors[currentFrame];
				break;
		}
	}

	private Rect GetCursorDrawPosition() {
		// set base position for custom cursor image
		float leftPos = Input.mousePosition.x;
		float topPos = Screen.height - Input.mousePosition.y; // screen draw coordinates are inverted

		// adjust position based on the type of cursor being shown
		if(activeCursorState == CursorState.PanRight)
			leftPos = Screen.width - activeCursor.width;
		else if(activeCursorState == CursorState.PanDown)
			topPos = Screen.height - activeCursor.height;
		else if(activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest) {
			topPos -= activeCursor.height / 2;
			leftPos -= activeCursor.width / 2;
		}
		return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
	}

	public void SetCursorState(CursorState newState) {
		this.activeCursorState = newState;
		switch(newState) {
			case CursorState.Select:
				activeCursor = selectCursor;
				break;
			case CursorState.Attack:
				currentFrame = (int)Time.time % attackCursors.Length;
				activeCursor = attackCursors[currentFrame];
				break;
			case CursorState.Harvest:
				currentFrame = (int)Time.time % harvestCursors.Length;
				activeCursor = harvestCursors[currentFrame];
				break;
			case CursorState.Move:
				currentFrame = (int)Time.time % moveCursors.Length;
				activeCursor = moveCursors[currentFrame];
				break;
			case CursorState.PanLeft:
				activeCursor = leftCursor;
				break;
			case CursorState.PanRight:
				activeCursor = rightCursor;
				break;
			case CursorState.PanUp:
				activeCursor = upCursor;
				break;
			case CursorState.PanDown:
				activeCursor = downCursor;
				break;
			default:
				break;
		}
	}

	public Rect GetPlayingArea() {
		return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
	}
}
