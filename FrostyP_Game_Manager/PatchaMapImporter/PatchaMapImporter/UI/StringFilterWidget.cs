//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using Tools;
	using UnityEngine;

	/// <summary>
	/// Represent the string filter
	/// </summary>
	class StringFilterWidget
	{
		/// <summary>
		/// Construct with existing flags
		/// </summary>
		/// <param name="flags"></param>
		public StringFilterWidget(string search)
		{
			using (new GUILayout.HorizontalScope(_containerStyle)) {
				Value = new TextEditWidget("Filter by names : ", search).Value;
				GUILayout.Space(5);
				if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false))) {
					Value = "";
				}
			}

			if (search != Value) {
				Log.Write("StringFilterWidget: has changed ! [" + search + "] => [" + Value + "]");
			}
		}

		/// <summary>
		/// Return current value
		/// </summary>
		public string Value { get; private set; }

		private static readonly GUIStyle _containerStyle = new GUIStyle {
			fontSize = 15,
			normal = new GUIStyleState {
				textColor = Color.white
			}
		};
	}
}
