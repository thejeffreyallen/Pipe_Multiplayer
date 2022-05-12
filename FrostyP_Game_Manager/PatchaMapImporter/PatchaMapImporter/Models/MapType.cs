namespace PatchaMapImporter.Models
{
	using System;

	/// <summary>
	/// Types of maps
	/// </summary>
	[Flags]
	public enum MapType
	{
		None = 0,
		Trail = 1,
		Park = 2,
		Bowl = 4,
		Street = 8,
		Mega = 16,
		Race = 32,
		Other = 64,
		All = Trail | Park | Bowl | Street | Mega | Race | Other
	}

	/// <summary>
	/// check if at least one flag is matching
	/// </summary>
	public static class MapTypeExtensions
	{

		/// <summary>
		/// check if at least a flag in "type" is in "other"
		/// </summary>
		/// <param name="type"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		static public bool HasAtLeastOneOf(this MapType type, MapType other)
		{
			foreach (MapType t in Enum.GetValues(typeof(MapType))) {
				if (t != MapType.None && t != MapType.All && (type & t) == t && (other & t) == t) return true;
			}

			return false;
		}
	}
}
