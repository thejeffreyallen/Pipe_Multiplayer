namespace PatchaMapImporter.Models
{
	using System.Collections.Generic;

	/// <summary>
	/// Store filter and map infos
	/// (going to be serialized into json inside the map directory)
	/// </summary>
	public class Store
	{
		/// <summary>
		/// Active Filter
		/// </summary>
		public Filter Filter = new Filter { Flags = MapType.All, Search = string.Empty };

		/// <summary>
		/// Toutes les infos
		/// </summary>
		public List<Map> Maps = new List<Map>();
	}
}
