//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.Tools
{
	using Models;
	using System.Collections.Generic;
	using System.IO;

	/// <summary>
	/// Map information manager
	/// / Metadatas
	/// / filtering
	/// </summary>
	public class MapManager
	{
		/// <summary>
		/// Name for storing metadata file
		/// </summary>
		const string METADATA_FILENAME = "_pmi.json";

		/// <summary>
		/// Constructor 
		/// </summary>
		public MapManager(string mapDirectory)
		{
			_mapDirectory = mapDirectory;
			_metaDataFilename = Path.Combine(_mapDirectory, METADATA_FILENAME);
			Load();
		}


		/// <summary>
		/// load map list and associated metadatas from file and map directory
		/// </summary>
		public void Load()
		{
			Log.Write("MM: Loading maps...");

			//load additionnal datas
			_store = File.Exists(_metaDataFilename) ? TinyJson.JSONParser.FromJson<Store>(File.ReadAllText(_metaDataFilename)) : new Store();
			Log.Write($"MM: Store: found existing {_store.Maps.Count} maps metadata.");

			////gather map names
			var mapsNames = new List<string>();
			foreach (var file in Directory.GetFiles(_mapDirectory)) {
				var info = new FileInfo(file.ToLower());
				var name = info.Name;
				if (name != METADATA_FILENAME) mapsNames.Add(name);
			}

			////remove missing maps infos from metadata
			var missingMaps = _store.Maps.Subset(map => !mapsNames.Contains(map.Filename));
			foreach (var map in missingMaps) {
				Log.Write($"MM: Store: removing infos for '{map.Filename}'.");
				_store.Maps.Remove(map);
			}

			//add generic map infos(for new files) to metadata
			var newMaps = mapsNames.Subset(mapfilename => _store.Maps.FindIndex(map => map.Filename == mapfilename) == -1);
			foreach (var filename in newMaps) {
				Log.Write($"MM: Store: adding infos for '{filename}'.");
				_store.Maps.Add(new Map(filename));
			}

			//filters maps with the stored filter
			Log.Write("MM: Store: filter is " + _store.Filter.ToString());
			Filter(_store.Filter);
		}

		/// <summary>
		/// Save Current MetaData to file
		/// </summary>
		public void Save()
		{
			Log.Write($"MM: Saving {_store.Maps.Count} map metadata");
			File.WriteAllText(_metaDataFilename, TinyJson.JSONWriter.ToJson(_store));
		}

		/// <summary>
		/// Apply filtering
		/// search string used only if string length >= 3 chars
		/// </summary>
		/// <param name="types"></param>
		public void Filter(Filter filter)
		{
			Log.Write($"MM: Filtering using {filter}.");

			//filter using flags
			var maps = _store.Maps.FindAll(map => map.Flags.HasAtLeastOneOf(filter.Flags));

			//filter using search
			if (!string.IsNullOrEmpty(filter.Search)) {
				maps = maps.FindAll(map => map.DisplayName.Contains(filter.Search)
											|| map.Authors.Contains(filter.Search)
											|| map.Filename.Contains(filter.Search));
			}

			//order
			switch (filter.Ordering) {
				case Ordering.ByRating:
					maps.Sort(delegate (Map x, Map y) { 
						var comp = -x.Rating.CompareTo(y.Rating); //sort by rating
						if (comp == 0) return x.DisplayName.CompareTo(y.DisplayName); //then by name
						return comp;
					});
					break;
				case Ordering.ByAuthors:
					maps.Sort(delegate (Map x, Map y) { 
						var comp = x.Authors.CompareTo(y.Authors);  //sort by authors
						if(comp == 0) return x.DisplayName.CompareTo(y.DisplayName); //then by name
						return comp;
					});
					break;
				default: //ByName
					maps.Sort(delegate (Map x, Map y) { return x.DisplayName.CompareTo(y.DisplayName); });
					break;
			}

			//save current visible list
			Maps = maps;

			Log.Write("MM: after filtering, maps count = " + Maps.Count.ToString());

			//saving last filter
			_store.Filter = filter;
		}

		/// <summary>
		/// Map list to show for selection
		/// </summary>
		public List<Map> Maps { get; private set; } = new List<Map>();

		/// <summary>
		/// Get current filter
		/// </summary>
		public Filter CurrentFilter { get { return _store.Filter; } }


		//-privates-------------------------------------

		private readonly string _mapDirectory;
		private readonly string _metaDataFilename;
		private Store _store;
	}
}