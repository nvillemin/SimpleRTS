using UnityEngine;
using System.Collections;

namespace RTS {
    public static class ResourceManager {
		// --------------------------------------------------------------------------------------------
		// Picking
		private static Vector3 invalidPosition = new Vector3(-99999, -99999, -99999);
		public static Vector3 InvalidPosition { get { return invalidPosition; } }

		// --------------------------------------------------------------------------------------------
		// Camera
		public static int ScrollWidth { get { return 15; } }
		public static float ScrollSpeed { get { return 25; } }
		public static float RotateAmount { get { return 10; } }
		public static float RotateSpeed { get { return 100; } }
		public static float MinCameraHeight { get { return 10; } } // Change if terrain isn't flat
		public static float MaxCameraHeight { get { return 40; } } // Change if terrain isn't flat

		// --------------------------------------------------------------------------------------------
		// Selection box
		private static Bounds invalidBounds = new Bounds(new Vector3(-99999, -99999, -99999),
			new Vector3(0, 0, 0));
		public static Bounds InvalidBounds { get { return invalidBounds; } }
		private static GUISkin selectBoxSkin;
		public static GUISkin SelectBoxSkin { get { return selectBoxSkin; } }
		public static void StoreSelectBoxItems(GUISkin skin) {
			selectBoxSkin = skin;
		}

		// --------------------------------------------------------------------------------------------
		// Building
		public static int BuildSpeed { get { return 2; } }
	}
}
