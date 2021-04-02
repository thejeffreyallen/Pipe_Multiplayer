//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using UnityEngine;

	/// <summary>
	/// Label +  TextField
	/// </summary>
	class TextEditWidget
	{
		/// <summary>
		/// Construct with existing flags
		/// </summary>
		/// <param name="flags"></param>
		public TextEditWidget(string label, string data)
		{
			using (new GUILayout.HorizontalScope()) {
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
				Value = GUILayout.TextField(data, GUILayout.Height(20));
			}
		}

		/// <summary>
		/// Return current value
		/// </summary>
		public string Value { get; private set; }
	}
}
