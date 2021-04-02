//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using System;
	using UnityEngine;

	/// <summary>
	/// Represent Rating
	/// </summary>
	class RatingWidget
	{
		/// <summary>
		/// Construct with label and rating value
		/// </summary>
		/// <param name="label"></param>
		/// /// <param name="rating"></param>
		public RatingWidget(string label, int rating)
		{
			using (new GUILayout.HorizontalScope()) {
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
				GUILayout.Space(3);
				Value = (int)Math.Round(GUILayout.HorizontalSlider((float)rating, 0f, 100f));
				GUILayout.Space(5);
				GUILayout.Label(Value.ToString());
			}
		}

		/// <summary>
		/// Return current value
		/// </summary>
		public int Value { get; private set; }
	}
}
