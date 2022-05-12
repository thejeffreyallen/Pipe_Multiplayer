//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using UnityEngine;

	/// <summary>
	/// Represent a title
	/// </summary>
	class TitleWidget
	{
		/// <summary>
		/// Construct title
		/// </summary>
		/// <param name="str"></param>
		/// <param name="size"></param>
		public TitleWidget(string str, int size)
		{
			using (new GUILayout.HorizontalScope()) {
				var style = new GUIStyle {
					fontSize = size,
					normal = new GUIStyleState {
						textColor = Color.white
					}
				};

				GUILayout.FlexibleSpace();
				GUILayout.Label(str, style, GUILayout.ExpandWidth(true));
				GUILayout.FlexibleSpace();
			}
		}
	}
}
