//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.UI
{
	using Models;
	using System;
	using UnityEngine;

	/// <summary>
	/// Represent a map 
	/// </summary>
	class EditMapWidget
	{
		/// <summary>
		/// construct a map widget and delegate for save or cancel
		/// </summary>
		/// <param name="map"></param>
		/// <param name="even"></param>
		public EditMapWidget(ref Map editedMapInfos, Action<Map> onSaveMapInfo, Action<Map> onCancel)
		{
			using (new GUILayout.VerticalScope()) {
				var displayName = new TextEditWidget("   Name :", editedMapInfos.DisplayName).Value;
				var authors = new TextEditWidget(    "Authors :", editedMapInfos.Authors).Value;
				var rating = new RatingWidget(       "  Rating :", editedMapInfos.Rating).Value;
				var flags = new FlagsFilterWidget(   "  Types : ", editedMapInfos.Flags).Value;
				var description = new TextAreaWidget("Description : ", editedMapInfos.Description).Value;

				editedMapInfos = new Map(editedMapInfos.Filename, displayName, authors, description, rating, flags);

				GUILayout.Space(5);

				//buttons
				using (new GUILayout.HorizontalScope(GUILayout.ExpandWidth(false))) {
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(new GUIContent("Cancel ", "discard any changes"))) onCancel(editedMapInfos);
					if (GUILayout.Button(new GUIContent("Save", "save new map properties"))) onSaveMapInfo(editedMapInfos);
				}
			}
		}
	}
}
