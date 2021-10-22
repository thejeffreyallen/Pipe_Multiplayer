//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using Models;
	using System;
	using System.Collections.Generic;
	using UnityEngine;

	/// <summary>
	/// Represent the map list
	/// </summary>
	class MapListWidget
	{
		/// <summary>
		/// Construct with maps list and delegate for load or edit
		/// </summary>
		/// <param name="scrollPos">scroll position to keep</param>
		/// <param name="maps">map list to show</param>
		/// <param name="onLoad">Action to call when a map wants to be loaded</param>
		/// <param name="onEdit">Action to call when a map wants to be edited</param>
		public MapListWidget(ref Vector2 scrollPos, List<Map> maps, Map currentMap, Action<Map> onLoad, Action<Map> onEdit)
		{
			GUILayout.Label($"{maps.Count} maps.");

			scrollPos = GUILayout.BeginScrollView(scrollPos, _containerStyle);
			{
				int i = 0;
				foreach (var map in maps) {
					var even = (++i % 2) == 0;
					var widget = new MapWidget(map, even, map == currentMap);

					//call actions
					if (widget.Action == MapWidget.MAP_EDIT) onEdit(map);
					else if (widget.Action == MapWidget.MAP_LOAD) onLoad(map);

					GUILayout.Space(5);
				}
			}
			GUILayout.EndScrollView();
		}

		/// <summary>
		/// Background for all map list
		/// </summary>
		private static readonly Texture2D _scrollBackground = TextureHelpers.MakeTexture(16, 16, new Color(.3f, .3f, .3f, .5f));

		/// <summary>
		/// Style for list
		/// </summary>
		private static readonly GUIStyle _containerStyle = new GUIStyle {
			normal = new GUIStyleState() {
				background = _scrollBackground
			}
		};
	}



}
