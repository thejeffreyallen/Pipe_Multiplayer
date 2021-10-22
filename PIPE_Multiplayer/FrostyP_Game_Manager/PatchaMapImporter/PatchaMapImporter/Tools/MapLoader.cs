//copyrights
//Mayhem/Lineryder @ Volution Modding team
//herve3527 / herve_patcha@3527trail.com

namespace PatchaMapImporter.Tools
{
	using BMXS_AStates;
	using Models;
	using System.Collections;
	using System.IO;
	using System.Reflection;
	using UnityEngine;
	using UnityEngine.SceneManagement;

	/// <summary>
	/// Map loader for pipe
	/// Full credits to lineryder / mayhem & Volution modding team for the original PipeworksMapImporter
	/// https://discord.gg/S89swkZ
	/// </summary>
	public class MapLoader
	{
		/// <summary>
		/// Construct with the MonoBehavior script class
		/// </summary>
		/// <param name="parent"></param>
		public MapLoader(string mapDirectory, MonoBehaviour parent)
		{
			_mapDirectory = mapDirectory;
			_parent = parent;

			//change lifetime of some objects
			UnityEngine.Object.DontDestroyOnLoad(GameObject.Find("Game World"));
			UnityEngine.Object.DontDestroyOnLoad(GameObject.Find("BMXS Player Components"));
			UnityEngine.Object.DontDestroyOnLoad(parent.gameObject);

			//fix camera parameters
			Camera component = GameObject.Find("Main Camera").GetComponent<Camera>();
			component.allowMSAA = true;
			component.allowHDR = true;
			component.renderingPath = RenderingPath.DeferredShading;

			//remove existing sessions markers
			foreach (object obj in UnityEngine.Object.FindObjectOfType<SessionMarker>().transform) {
				GameObject g = obj as GameObject;
				if (g && g.name == "Orientation") UnityEngine.Object.Destroy(g);
			}
		}

		/// <summary>
		/// Load a map
		/// </summary>
		/// <param name="map"></param>
		public void Load(Map map)
		{
			var mapFilename = Path.Combine(_mapDirectory, map.Filename);
			var dll = Path.Combine(_mapDirectory + "/DLLs", $"{map.Filename}.dll");

			if (_bundle != null) {
				Log.Write($"ML: unloading previous bundle...");
				_bundle.Unload(true);
			}
			mapimporter._loadedMap = map;
			mapimporter.currentMap = map.Filename;

			//load assets
			Log.Write($"ML: loading assets from '{mapFilename}'...");
			_bundle = AssetBundle.LoadFromFile(mapFilename);

			//load managed script
			if (File.Exists(dll)) {
				Log.Write($"ML: loading scripts from '{dll}'...");
				Assembly.LoadFile(dll);
			}

			//wait for spawning
			if (_bundle != null) {
				Log.Write($"ML: loading scene...");
				_async = SceneManager.LoadSceneAsync(_bundle.GetAllScenePaths()[0], LoadSceneMode.Single);

				Log.Write($"ML: wait for respawn...");
				_parent.StartCoroutine(WaitForSpawn());
			}
            PIPE_Multiplayer.GameManager.instance.GetLevelName();
        }

		/// <summary>
		/// Wait for player to spawn
		/// </summary>
		/// <returns></returns>
		private IEnumerator WaitForSpawn()
		{
			while (!GameObject.Find("CustomMap_SpawnPoint")) yield return new WaitForEndOfFrame();

			VehicleManager vehicleManager = UnityEngine.Object.FindObjectOfType<VehicleManager>();
			Standing stand = _parent.GetComponent<Standing>();

			vehicleManager.GetSessionMarker().SetMarker(GameObject.Find("CustomMap_SpawnPoint").transform);
			yield return new WaitForSeconds(1f);

			vehicleManager.GetSessionMarker().ResetPlayerAtMarker();
			UnityEngine.Object.Destroy(GameObject.Find("CustomMap_SpawnPoint"));

			_async.allowSceneActivation = true;
			vehicleManager.timeSinceLastTransition = 0f;
			stand.Start();

			yield break;
		}

		//privates
		private readonly string _mapDirectory;
		private readonly MonoBehaviour _parent;
		private AssetBundle _bundle = null;
		private AsyncOperation _async = null;
		public PatchaMapImporter mapimporter;
	}
}
