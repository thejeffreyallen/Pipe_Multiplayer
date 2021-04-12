//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using Tools;
	using Models;
	using UnityEngine;

	/// <summary>
	/// Represent a map representation
	/// </summary>
	class MapWidget
	{
		/// <summary>
		/// construct a map widget
		/// </summary>
		/// <param name="map"></param>
		/// <param name="even"></param>
		public MapWidget(Map map, bool even, bool current)
		{
			Map = map;

			//change backgrounds alternatively
			if (current) {
				_containerStyle.normal.background = _currentMapBackground;
				_containerStyle.hover = new GUIStyleState() { };
			}
			else if (even) _containerStyle.normal.background = _lightMapBackground;
			else _containerStyle.normal.background = _darkMapBackground;

			Rect editButtonRect;

			using (new GUILayout.HorizontalScope(_containerStyle)) {
				//Infos
				using (new GUILayout.VerticalScope(GUILayout.ExpandWidth(true))) {
					using (new GUILayout.HorizontalScope()) {
						GUILayout.Label(map.DisplayName, _nameStyle);
						if (!string.IsNullOrEmpty(map.Authors)) GUILayout.Label($"by {map.Authors}");
						GUILayout.FlexibleSpace();
					}

					using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(false))) {
						GUILayout.Label($"Rating : {map.Rating.ToString().PadLeft(3, ' ')}");
						GUILayout.Label($"  |   {map.Flags.ToString().Replace(",", " /")}");
						GUILayout.FlexibleSpace();
					}
				}

				//edit button
				using (new GUILayout.VerticalScope()) {
					GUILayout.Space(5);
					if (GUILayout.Button("edit", GUILayout.Width(50), GUILayout.Height(30))) {
						Action = MAP_EDIT;
					}
					editButtonRect = GUILayoutUtility.GetLastRect();
				}
			}

			//make whole area clickable
			Rect containerRect = GUILayoutUtility.GetLastRect();
			if (Event.current != null && Event.current.isMouse) {
				var mousePos = Event.current.mousePosition;
				if (containerRect.Contains(mousePos)) {

					//left click to load map
					if (Input.GetMouseButtonDown(0) && !editButtonRect.Contains(mousePos)) {
						Action = MAP_LOAD;
					}

					//right click to edit
					if (Input.GetMouseButtonDown(1)) {
						Action = MAP_EDIT;
					}
				}
			}
		}

		/// <summary>
		/// Storing selected map reference
		/// </summary>
		public readonly Map Map;

		/// <summary>
		/// What bouton was pressed
		/// </summary>
		public string Action { get; private set; }

		/// <summary>
		/// consts for edit action
		/// </summary>
		public const string MAP_EDIT = "edit";

		/// <summary>
		/// const for load action
		/// </summary>
		public const string MAP_LOAD = "load";

		/// <summary>
		/// style for container
		/// </summary>
		private GUIStyle _containerStyle = new GUIStyle {
			padding = new RectOffset() { top = 10, left = 10, bottom = 10, right = 10 },
			hover = new GUIStyleState() { background = _hoverBackground }
		};

		/// <summary>
		/// style for name
		/// </summary>
		private static readonly GUIStyle _nameStyle = new GUIStyle {
			fontSize = 20,
			normal = new GUIStyleState() { textColor = Color.white }
		};

		/// <summary>
		/// 'dark' background color
		/// </summary>
		/// private static readonly Texture2D _darkMapBackground = TextureHelpers.MakeTexture(16, 16, new Color(.54f, .36f, .21f, .35f));
		private static readonly Texture2D _darkMapBackground = TextureHelpers.MakeTexture(16, 16, new Color(.47f, .32f, .19f, .35f));

		/// <summary>
		/// 'light' background color
		/// </summary>
		/// private static readonly Texture2D _lightMapBackground = TextureHelpers.MakeTexture(16, 16, new Color(.8f, .62f, .44f, .3f));
		private static readonly Texture2D _lightMapBackground = TextureHelpers.MakeTexture(16, 16, new Color(.81f, .65f, .50f, .35f));


		/// <summary>
		/// 'current' background color
		/// </summary>
		private static readonly Texture2D _currentMapBackground = TextureHelpers.MakeTexture(16, 16, new Color(.29f, .10f, .08f, .40f));

		/// <summary>
		/// Color when mouse over
		/// </summary>
		private static Texture2D _hoverBackground = TextureHelpers.MakeTexture(2, 2, new Color(.0f, .43f, .0f, .5f));

	}
}
