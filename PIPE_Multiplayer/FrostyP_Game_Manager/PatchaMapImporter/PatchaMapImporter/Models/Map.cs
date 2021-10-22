using System;

namespace PatchaMapImporter.Models
{
	/// <summary>
	/// Represent a map and it's attributes
	/// </summary>
	public class Map
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="filename"></param>
		/// <param name="displayname"></param>
		/// <param name="authors"></param>
		/// <param name="description"></param>
		/// <param name="rating"></param>
		/// <param name="flags"></param>
		public Map(string filename, string displayname = "", string authors = "", string description = "", int rating = 50, MapType flags = MapType.All)
		{
			Filename = filename;
			DisplayName = string.IsNullOrEmpty(displayname) ? filename : displayname;
			Authors = authors;
			Description = description;
			Rating = rating;
			Flags = flags;
		}

		/// <summary>
		/// Map filename
		/// </summary>
		public readonly string Filename = "";

		/// <summary>
		/// Displayed name
		/// </summary>
		public string DisplayName = "";

		/// <summary>
		/// Authors
		/// </summary>
		public string Authors = "";

		/// <summary>
		/// Map Description
		/// </summary>
		public string Description = "";

		/// <summary>
		/// the lower the greater
		/// </summary>
		public int Rating = 50;

		/// <summary>
		/// Map type (flags)
		/// </summary>
		public MapType Flags = MapType.All;

		/// <summary>
		/// copy all fields but the filename;
		/// </summary>
		/// <param name="map">source map</param>
		public void CopyEditableInfos(Map map)
		{
			DisplayName = map.DisplayName;
			Authors = map.Authors;
			Description = map.Description;
			Rating = map.Rating;
			Flags = map.Flags;
		}
	}
}
