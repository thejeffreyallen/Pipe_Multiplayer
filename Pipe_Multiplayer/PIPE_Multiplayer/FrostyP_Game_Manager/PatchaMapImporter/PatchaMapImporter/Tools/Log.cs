//copyrights herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.Tools
{
	using System.IO;
	using UnityEngine;

	/// <summary>
	/// quick and really dirty log
	/// </summary>
	class Log
	{
		/// <summary>
		/// Write data to log
		/// </summary>
		/// <param name="data"></param>
		public static void Write(string data)
		{
			//seems to go to unity but can't check it
			Debug.Log(data);

			//write directly in my Folder
			//File.AppendAllText(@"C:\Program Files (x86)\Steam\steamapps\content\app_815780\depot_815781\Mods\PatchaMapImporter\patchamap-importer.log", data + "\r\n");
		}
	}
}
