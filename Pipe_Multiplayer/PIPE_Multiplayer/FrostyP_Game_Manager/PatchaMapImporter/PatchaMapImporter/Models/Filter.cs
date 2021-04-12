namespace PatchaMapImporter.Models
{
	/// <summary>
	/// Represent a map "filter"
	/// </summary>
	public class Filter
	{
		/// <summary>
		/// Type of map
		/// </summary>
		public MapType Flags = MapType.All;

		/// <summary>
		/// String in map name
		/// </summary>
		public string Search = "";

		/// <summary>
		/// Order type
		/// </summary>
		public Ordering Ordering = Ordering.ByName;

		/// <summary>
		/// for printing
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return $"Flags='{Flags}' Search='{Search}' Ordering='{Ordering}'";
		}
	}
}
