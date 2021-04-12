//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using Tools;
	using Models;
	using System;
	using UnityEngine;

	/// <summary>
	/// Represent the Flags filter
	/// </summary>
	class FlagsFilterWidget
	{
		/// <summary>
		/// Construct with existing flags
		/// </summary>
		/// <param name="flags"></param>
		public FlagsFilterWidget(string label, MapType flags)
		{
			Value = flags;

			using (new GUILayout.HorizontalScope(_containerStyle)) {
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
				foreach (MapType t in Enum.GetValues(typeof(MapType))) {
					if (t != MapType.None && t != MapType.All) {
						var initial_value = (flags & t) == t;
						var current_value = GUILayout.Toggle(initial_value, t.ToString());
						if (initial_value != current_value) {
							if (current_value) Value |= t;
							else Value &= ~t;
						}
					}
				}

				if (GUILayout.Button("All")) {
					Value = MapType.All;
				}
			}

			if (flags != Value) {
				Log.Write("FlagsFilterWidget: has changed ! [" + flags + "] => [" + Value + "]");
			}
		}

		/// <summary>
		/// Return current value
		/// </summary>
		public MapType Value { get; private set; }

		/// <summary>
		/// Container style
		/// </summary>
		private static readonly GUIStyle _containerStyle = new GUIStyle {
			fontSize = 30,
			normal = new GUIStyleState {
				textColor = Color.white
			}
		};
	}
}
