using System.Collections;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using Unity;
using PIPE_Valve_Console_Client;




namespace FrostyP_Game_Manager
{
	
    public class ParkBuilder : MonoBehaviour
    {

		public static ParkBuilder instance;


		GUISkin skin = (GUISkin)ScriptableObject.CreateInstance("GUISkin");
		GUIStyle Generalstyle = new GUIStyle();
		GUIStyle Myobjectstyle = new GUIStyle();
		GUIStyle headerstyle = new GUIStyle();

		MGInputManager inputman;
		ParkSaveLoad _ParkSaveLoad = new ParkSaveLoad();

		

		public class Camspeed
        {
			
			
			public List<Speed> speeds = new List<Speed>();

            public void Setup()
            {
				Speed Highest = new Speed();
				Highest.name = "Highest";
				Highest.value = 15;
				speeds.Add(Highest);


				Speed High = new Speed();
				High.name = "High";
				High.value = 10;
				speeds.Add(High);


				Speed Medium = new Speed();
				Medium.name = "Medium";
				Medium.value = 5;
				speeds.Add(Medium);


				Speed Slow = new Speed();
				Slow.name = "Slow";
				Slow.value = 2;
				speeds.Add(Slow);

				Speed Slower = new Speed();
				Slower.name = "Slower";
				Slower.value = 1;
				speeds.Add(Slower);

				Speed Slowest = new Speed();
				Slowest.name = "Slowest";
				Slowest.value = 0.5f;
				speeds.Add(Slowest);


			}

			public int Change(int currentspeed)
            {
                if(currentspeed == 5)
                {

					return 0;
                }
                else
                {
					
					return currentspeed + 1;
					
                }
            }


			public struct Speed
            {
				public string name;
				public float value;
            }



		}
		int currentcamspeed = 0;
		float Camdistance = 10;
		Camspeed camspeed;


		string SaveparkName = "Default name park";

		public bool openflag = false;
		bool snappingtofloor = true;
		bool SaveLoadOpen = false;
		bool Helpmenu;
		bool controlmenu;
		bool objectswindow;
		bool objectsloadtoggle;
		bool SceneWindowToggle;
		string objectsloaddisplay;
		bool REgrabbed = false;
		bool StreamingObjectData = false;
		Vector3 regrabbed_pos;
		Vector3 regrabbed_rot;



		// Myobjects Menu
		Vector2 MyobjectsScroll;




		public string AssetbundlesDirectory = Application.dataPath + "/FrostyPGameManager/ParkBuilder/Assetbundles/";
		public string ParksDirectory = Application.dataPath + "/FrostyPGameManager/ParkBuilder/Parks/";
		public string SkyboxDirectory = Application.dataPath + "/FrostyPGameManager/ParkBuilder/SkyBoxes/";




		public List<PlacedObject> placedobjects = new List<PlacedObject>();
		public List<NetGameObject> NetgameObjects = new List<NetGameObject>();
		
		
		Camera buildercam;
		GameObject camobj;
		Vector3 camFarPos;
		GameObject controlobjforcam;
		
		GameObject Activeobj;
		

		public FileInfo[] availableassetbundles;
		public List<BundleData> bundlesloaded;


		BundleData ActiveBundleData;
		PlacedObject ActivePlacedObject;

		
		// SaveLoad Menu
		public List<PlacedObject> ObjectstoSave = new List<PlacedObject>();
		public List<string> CreatorsList = new List<string>();
		bool Save_Load_Toggle = false;
		string SaveloadMode;
		Vector2 objectsscroll;
		Vector2 SaveloadScroll;
		Vector2 MyObjectsSaveScroll;
		Vector2 OtherPlayersScroll;
		
		public bool ShowLoadedspot;
		public SavedSpot ActiveSavedspot;
		List<string> LoadedSpotCreators;
		List<string> Loadedspotpackneeded;


		// Scene Adjustment
		Light ShowingLight = null;
		GameObject LightSphere;
		Vector2 Scenescroll;
		bool LightsToggle;
		bool skyboxToggle;
		float Skyr;
		float Skyg;
		float Skyb;
		Vector2 Skyboxscroll;
		Material DefaultSky;
		List<RotatingLight> Rotatinglights = new List<RotatingLight>();
		string Xval = "0";
		string Yval = "0";
		string Zval = "0";


		// Directional light panel
		float x;
		float y;
		float z;


		void Awake()
        {
			instance = this;
			bundlesloaded = new List<BundleData>();
			

			// check both directories and create if needed
			if (!Directory.Exists(ParksDirectory))
			{
			 Directory.CreateDirectory(ParksDirectory);
			}
			
			if (!Directory.Exists(AssetbundlesDirectory))
			{
			 Directory.CreateDirectory(AssetbundlesDirectory);
			}
			if (!Directory.Exists(SkyboxDirectory))
			{
				Directory.CreateDirectory(SkyboxDirectory);
			}


			camobj = Instantiate(Camera.main.gameObject);
			//camobj.AddComponent<FMOD_Listener>();
			buildercam = camobj.GetComponent<Camera>();
			camobj.SetActive(false);
			DontDestroyOnLoad(camobj);
			// make ghost object that rotates to camera obj only on Y
			controlobjforcam = new GameObject();
			DontDestroyOnLoad(controlobjforcam);
			LightSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			DontDestroyOnLoad(LightSphere);
			LightSphere.SetActive(false);
		}

		// Use this for initialization
		void Start()
        {
			

			// create instance of camspeed and run setup once, this adds 3 presets for Y button to scroll through
			camspeed = new Camspeed();
			camspeed.Setup();

			// check both directories and create if needed
			
            if (!Directory.Exists(ParksDirectory))
            {
				Directory.CreateDirectory(ParksDirectory);
            }
			
			if (!Directory.Exists(AssetbundlesDirectory))
			{
				Directory.CreateDirectory(AssetbundlesDirectory);
			}

			SetupGuis();


			DefaultSky = RenderSettings.skybox;


		}

        // Update is called once per frame
        void Update()
        {
            
            if (openflag)
            {
                if (!SceneWindowToggle)
                {

				Controls();

				if (REgrabbed && Activeobj != null)
                {
					ReplaceOnBButton();
                }

				if (Activeobj != null)
				{
					camFarPos = new Vector3(Activeobj.transform.position.x, Activeobj.transform.position.y + 10, Activeobj.transform.position.z - 50);
					//ShowAxisofObject();

				}



				}
                else
                {
					YtoChangeSpeed();
				}






				// send keepactive packet every 2 seconds to stop timeout on server
				if (InGameUI.instance.Connected)
				{
					GameManager.KeepNetworkActive();
				}


			}



		}

		void FixedUpdate()
        {
            if (StreamingObjectData && InGameUI.instance.Connected)
            {
                try
                {
					if(Activeobj != null && ActivePlacedObject != null)
                    {

				       ClientSend.ObjectTransformUpdate(ActivePlacedObject);

                    }

                }
                catch (System.Exception x)
                {
					Debug.Log("stream object transform error   : " + x);

                }
                
            }

			if (Rotatinglights.Count > 0)
			{
				
                for (int i = 0; i < Rotatinglights.ToArray().Length; i++)
                {
                    if (Rotatinglights[i].light)
                    {
						Rotatinglights[i].light.gameObject.transform.Rotate(Rotatinglights[i].Rotateamount.x * Time.fixedDeltaTime, Rotatinglights[i].Rotateamount.y * Time.fixedDeltaTime, Rotatinglights[i].Rotateamount.z * Time.fixedDeltaTime);
                    }
                    else
                    {
						Rotatinglights.RemoveAt(i);
                    }
                }

			}




		}

        void OnGUI()
        {
            try
            {
			  if (openflag)
              {
				GUI.skin = skin;

				
				
				
			    MyObjectsMenu();
				


                if (SaveLoadOpen)
                {
					SaveLoadMenu();
					if(Save_Load_Toggle == false)
                    {
						SaveloadMode = "Save Mode";
                    }
                    else
                    {
						SaveloadMode = "Load Mode";
                    }
                }

                if (ShowLoadedspot)
                {
					ShowLoadedSpot();
                }

                if (SceneWindowToggle)
                {
				 SceneSetupShow();
                }
                    else
                    {
						LightSphere.SetActive(false);
                    }

					//--------------------------------------------





					// -------------------------------------------------------------------------------------------------------------------------      Things contained in Primary GUI     -----------------------------------------------------------------------------------------------------------------
					GUILayout.BeginArea(new Rect(new Vector2(Screen.width/4,5), new Vector2(Screen.width/2,30)));
					GUILayout.BeginHorizontal();
				
				Helpmenu = GUILayout.Toggle(Helpmenu, " Help ", FrostyPGamemanager.instance.style);
			    GUILayout.Space(2);
				controlmenu = GUILayout.Toggle(controlmenu,"Controls:", FrostyPGamemanager.instance.style);
				GUILayout.Space(5);
				GUILayout.Label("Movement Speed at " + camspeed.speeds[currentcamspeed].name, Generalstyle);
				GUILayout.Space(5);
				if (GUILayout.Button("Clear All ", FrostyPGamemanager.instance.style) && placedobjects.Count>0)
				{
					Vector3 pos = new Vector3(0, 0, 0);
                    for (int i = 0; i < placedobjects.Count; i++)
                    {
						if(placedobjects[i].ObjectId != 0)
                        {
						ClientSend.DestroyAnObject(placedobjects[i].ObjectId);
                        }
						Destroy(placedobjects[i].Object);
						
                    }
					foreach(PlacedObject p in placedobjects)
                    {
					  foreach(PlacedObject m in ObjectstoSave.ToArray())
                        {
							if(p == m)
                            {
								ObjectstoSave.Remove(m);
                            }
                        }
                    }
					placedobjects.Clear();
				    NetgameObjects.Clear();


					if(Activeobj != null)
                    {
						pos = Activeobj.transform.position;
						Destroy(Activeobj);
                    }
					Activeobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					Activeobj.transform.position = pos;
				}


				       if (Activeobj != null)
                       {
						GUILayout.Space(2);
						if (!Activeobj.name.Contains("Pointer"))
                        {
					       if(GUILayout.Button("Destroy " + Activeobj.gameObject.name.Replace("(Clone)",""), FrostyPGamemanager.instance.style))
					       {
						   DestroyObject();
						
                           }
                        }
                       }


				    // Save current objects
				    GUILayout.Space(2);
				    SaveLoadOpen = GUILayout.Toggle(SaveLoadOpen, "Save/Load", FrostyPGamemanager.instance.style);
					GUILayout.Space(2);
					objectswindow = GUILayout.Toggle(objectswindow, "Objects/Bundles", FrostyPGamemanager.instance.style);
					GUILayout.Space(2);
					SceneWindowToggle = GUILayout.Toggle(SceneWindowToggle, "Scene", FrostyPGamemanager.instance.style);
					GUILayout.Space(2);
				if (GUILayout.Button("Return to game", FrostyPGamemanager.instance.style))
                {
				Close();
				}
					GUILayout.EndHorizontal();
					GUILayout.EndArea();

					if (objectswindow)
					{
						ObjectsWindowShow();
					}

					if (Helpmenu)
					{
					controlmenu = false;
					 HelpShow();
					}

					if (controlmenu)
					{
						Helpmenu = false;
						ControlHelp();
					}


				}

			}
			catch(UnityException x)
            {
				Debug.Log($"ParkBuilder GUI error : " + x);
            }
        }

		/// <summary>
		/// Call to open ParkBuilder
		/// </summary>
		public void Open(Vector3 _playerscurrentpos)
        {
			GameManager.TogglePlayerComponents(false);
			Activeobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Activeobj.name = "Pointer";
			Activeobj.transform.position = _playerscurrentpos + Vector3.up;
			controlobjforcam.transform.position = Activeobj.transform.position;

			

			if (inputman == null)
			{
				inputman = new MGInputManager();

			}


				camobj.transform.position = _playerscurrentpos + (Vector3.back * 5) + (Vector3.up * 2);
			    camobj.SetActive(true);
			

			
		
			//give list of found files
		availableassetbundles = LoadAssets();
			

			openflag = true;

		}

		public void Close()
        {
			GameManager.TogglePlayerComponents(true);
			Destroy(Activeobj);
			camobj.SetActive(false);
			openflag = false;
			FrostyPGamemanager.instance.OpenMenu = true;
		}

        /// <summary>
        /// gives list of fileinfo's from Assetbundles directory
        /// </summary>
        /// <returns></returns>
        FileInfo[] LoadAssets()
        {
			if (!Directory.Exists(AssetbundlesDirectory)) Directory.CreateDirectory(AssetbundlesDirectory);

			DirectoryInfo listofbundles = new DirectoryInfo(AssetbundlesDirectory);
            
			return listofbundles.GetFiles("*.*",SearchOption.AllDirectories);

        }

		// called by clicking a button
		void SpawnNewObject(GameObject obj, BundleData _bunddata, Vector3 pos)
        {
            try
            {
			Activeobj = GameObject.Instantiate(obj);
			ActiveBundleData = _bunddata;
			Activeobj.transform.position = pos;
			Activeobj.name.Replace("(Clone)", "");
			if (obj.GetComponent<Rigidbody>())
			{
				obj.layer = 25;
				obj.GetComponent<Rigidbody>().isKinematic = true;
			}


            }
            catch (UnityException x)
            {
				Debug.Log(x);
            }
		}

		/// <summary>
		/// kills activeobj and makes given placed obj the target
		/// </summary>
		/// <param name="obj"></param>
		void TargetPlacedObject(PlacedObject _placedobj)
        {
			if(Activeobj != null)
            {
				Destroy(Activeobj);
            }
				Activeobj = _placedobj.Object;
			ActiveBundleData = _placedobj.BundleData;
			ActivePlacedObject = _placedobj;

			foreach(NetGameObject n in NetgameObjects)
            {
				if(n.ObjectID == _placedobj.ObjectId)
                {
                    // move on net
                    if (InGameUI.instance.Connected)
                    {
						StreamingObjectData = true;
                    }

                }
            }


		}

		void ObjectsWindowShow()
        {
			GUILayout.BeginArea(new Rect(new Vector2(10,150), new Vector2(Screen.width/3-200,Screen.height - 300)),InGameUI.BoxStyle);
			objectsloadtoggle = GUILayout.Toggle(objectsloadtoggle, objectsloaddisplay);
			GUILayout.Space(10);
			objectsscroll = GUILayout.BeginScrollView(objectsscroll);

            if (objectsloadtoggle)
            {
				objectsloaddisplay = "Load Bundles";

			GUILayout.Label("Loadable bundles:");
			// list Bundles found and give load button
			foreach (FileInfo file in availableassetbundles)
			{
				bool loaded = false;
				foreach (BundleData bun in bundlesloaded)
				{
					if (bun.FileName == file.Name)
					{
						loaded = true;
					}
				}

				if (!loaded)
				{
					if (GUILayout.Button("Load now : " + file.Name))
					{
						GUILayout.Space(10);
						AssetBundle bundle = AssetBundle.LoadFromFile(file.FullName);
						bundlesloaded.Add(new BundleData(bundle, file.Name,file.DirectoryName));

					}

				}


			}

            }
            else
            {
				objectsloaddisplay = "Spawn Asset";

				GUILayout.Space(20);

			if (bundlesloaded != null)
			{
				foreach (BundleData bundledata in bundlesloaded)
				{
					GUILayout.Label(bundledata.Bundle.name + " :");
					GUILayout.Space(10);
					GameObject[] items = bundledata.Bundle.LoadAllAssets<GameObject>();
					foreach (GameObject item in items)
					{
						if (GUILayout.Button(item.name))
						{
							GameObject obj = bundledata.Bundle.LoadAsset(item.name) as GameObject;
							Vector3 pos = new Vector3(Activeobj.transform.position.x, Activeobj.transform.position.y, Activeobj.transform.position.z);
							Destroy(Activeobj.gameObject);

							SpawnNewObject(obj, bundledata, pos);
						}
					}
				}
			}



            }






			GUILayout.EndScrollView();
			GUILayout.EndArea();
        }

		void HelpShow()
        {

			GUILayout.BeginArea(new Rect(new Vector2(Screen.width/4,Screen.height/4), new Vector2(Screen.width/2,Screen.height/2)),InGameUI.BoxStyle);

			GUILayout.Label("Make your own Assets");
			GUILayout.Space(20);
			GUILayout.Label(" parkbuilder will recognise and load any Assetbundles that do not contain Scenes. To do this follow the steps \n for creating a map using the mapscript that creates an assetbundle,");
			GUILayout.Label(" but dont mark your scene as a bundle item, make each object in");
			GUILayout.Label(" your scene a prefab by dragging from hierarchy to assets,");
			GUILayout.Label(" then mark each prefab as part of your assetbundle, build bundle and");
			GUILayout.Label(" put in ParkBuilder/AssetBundles/, they should then show in Objects/Bundles.");
			GUILayout.Space(10);
			GUILayout.Label(" make your objects positions 0,0,0 and square so that theyre root");
			GUILayout.Label(" position can be found again later");
			GUILayout.Space(10);
			GUILayout.Label("Select placed object from MyObjects to live stream movement when online.");
			GUILayout.EndArea();
		}

		void ControlHelp()
        {
			GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 4, Screen.height / 4), new Vector2(Screen.width / 2, Screen.height / 2)),InGameUI.BoxStyle);
			GUILayout.Label("Controls : (xbox Controller)");
			GUILayout.Space(20);
			GUILayout.Label("A to Place an object : B to Replace object where it was : Y to change speed");
			GUILayout.Space(5);
			GUILayout.Label("LB/RB to quick spin");
			GUILayout.Space(5);
			GUILayout.Label("Hold LT for height adjust (L stick) and Cam distance (R stick)");
			GUILayout.Space(5);
			GUILayout.Label("Hold RT for Fine Rotating");
			GUILayout.Space(5);
			GUILayout.Label("LStick click to Reset object to root position");
			GUILayout.Space(5);
			GUILayout.Label("Select placed object from MyObjects to live stream movement when online");
			GUILayout.EndArea();
		}

		void Controls()
        {
			camobj.transform.LookAt(Activeobj.transform, Vector3.up);
			CamFollow();

			Vector3 dir = (camobj.transform.position - Activeobj.transform.position).normalized;
			Vector3 vel = controlobjforcam.transform.position - Activeobj.transform.position;

			controlobjforcam.transform.position = Vector3.SmoothDamp(controlobjforcam.transform.position, Activeobj.transform.position + (dir * Camdistance), ref vel, 0.01f);


			// if L trigger is off, Lstick moves object
			if (MGInputManager.LTrigger() < 0.2f && MGInputManager.RTrigger() < 0.2f)
			{
				

				// move function for live object

				MoveObject();

				float speed = 80;
				if(MGInputManager.RStickX()>0.2f | MGInputManager.RStickX() < -0.2f)
                {
				controlobjforcam.transform.RotateAround(Activeobj.transform.position, Vector3.up, -MGInputManager.RStickX() * Time.deltaTime * speed);

                }

				if (MGInputManager.RStickY() > 0.2f | MGInputManager.RStickY() < -0.2f)
				{
					controlobjforcam.transform.RotateAround(Activeobj.transform.position, camobj.transform.right, MGInputManager.RStickY() * Time.deltaTime * speed);
				}



				// lstick down reset rotation
				if (MGInputManager.LeftStick_Down())
				{
					Activeobj.transform.eulerAngles = new Vector3(0, 0, 0);
				}

				// LB to spin 45
				if (MGInputManager.LB_Down())
				{
					Activeobj.transform.Rotate(Vector3.up, 22.5f);
				}

				// RB to spin 45
				if (MGInputManager.RB_Down())
				{
					Activeobj.transform.Rotate(Vector3.forward, 22.5f);
				}



			}


					

			// if Ltrigger is on, Lstick moves object up and down
			if (MGInputManager.LTrigger() > 0.4f && MGInputManager.RTrigger() < 0.2f)
			{
				//GUILayout.Label("L Stick for Height adjust, Rstick for pan in/out");
				Activeobj.transform.position = Activeobj.transform.position + Vector3.up * MGInputManager.LStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value;

				Camdistance = Mathf.Clamp(Camdistance, 2, 100);
				Camdistance = Camdistance + -MGInputManager.RStickY() * Time.deltaTime * 10;

				// pan in and out with R stick
				
			}


			// if R Trigger is on
			if (MGInputManager.RTrigger() > 0.4 && MGInputManager.LTrigger() < 0.2f)
			{
				//GUILayout.Label("Fine Tune Rotate Object -- Lstick to rotate Y, Rstick to rotate Z and X ");

				// fine tuning rotate
				Activeobj.transform.Rotate(MGInputManager.RStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value, MGInputManager.LStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value, -MGInputManager.RStickX() * Time.deltaTime * camspeed.speeds[currentcamspeed].value);


			}



			YtoChangeSpeed();



			if (MGInputManager.LTrigger() < 0.2f && Activeobj != null && MGInputManager.A_Down())
			{
				PlaceObject();
			}





		}

		void DestroyObject()
        {
			Vector3 pos = Activeobj.transform.position;
			StreamingObjectData = false;

			if(ActivePlacedObject.ObjectId != 0 && InGameUI.instance.Connected)
            {

			ClientSend.DestroyAnObject(ActivePlacedObject.ObjectId);

            }
			NetGameObject todel = null;
			// find corresponding netgameobject and remove
			foreach (NetGameObject n in NetgameObjects)
			{
				if (n.ObjectID == ActivePlacedObject.ObjectId)
				{
					todel = n;

				}
			}

			if (todel != null)
			{
				NetgameObjects.Remove(todel);
			}


			REgrabbed = false;
			Destroy(Activeobj);
			if (ActivePlacedObject != null)
			{
				placedobjects.Remove(ActivePlacedObject);
			}
			ActiveBundleData = null;

			Activeobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Activeobj.transform.position = pos;
			PlacedObject o = null;
			foreach(PlacedObject p in ObjectstoSave)
            {
				if(p == ActivePlacedObject)
                {
					o = p;
                }
            }

            if (o != null)
            {
				ObjectstoSave.Remove(o);
            }

		}

		
		void PlaceObject()
        {
            try
            {
              if (!Activeobj.name.Contains("Pointer"))
            {
			StreamingObjectData = false;

            if (!REgrabbed)
            {
						
			 GameObject obj = GameObject.Instantiate(Activeobj) as GameObject;
            if (obj.GetComponent<Rigidbody>())
            {
				obj.layer = 25;
				obj.GetComponent<Rigidbody>().isKinematic = false;
            }


			obj.name.Replace("(Clone)", "");
			obj.transform.position = Activeobj.transform.position;
			obj.transform.rotation = Activeobj.transform.rotation;
				DontDestroyOnLoad(obj);
				PlacedObject _new = new PlacedObject(obj, ActiveBundleData);

            try
            {
            // send to server if online
				int ID = GiveUniqueNumber();
				_new.ObjectId = ID;

				NetGameObject _newobj = new NetGameObject(Activeobj.name.Replace("(Clone)",""), ActiveBundleData.FileName, ActiveBundleData.Bundle.name, Activeobj.transform.eulerAngles, Activeobj.transform.position, Activeobj.transform.localScale, false, ID, obj,ActiveBundleData.FullDir);
				NetgameObjects.Add(_newobj);

            if (InGameUI.instance.Connected && NetgameObjects.Count < 30)
            {
				ClientSend.SpawnObjectOnServer(_newobj);
			

			}


            }
            catch (UnityException x)
            {
				Debug.Log(x);
            }


			placedobjects.Add(_new);
			
			

            }
            if (REgrabbed)
            {
				//GameObject g = Instantiate(GameObject.Find(Activeobj.name));
				REgrabbed = false;
				
				Activeobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					Activeobj.name = "Pointer";
				Activeobj.transform.position = ActivePlacedObject.Object.transform.position;
				ActiveBundleData = null;
				ActivePlacedObject = null;
				
            }


            }

            }
            catch (UnityException x)
            {
				Debug.Log(x + " : " + x.TargetSite);
            }


        }

		void MoveObject()
        {
			Vector3 worldup = transform.TransformDirection(Activeobj.transform.up);
			float X = 0;
			float Y = 0;
			if (MGInputManager.LStickX()>0.2f | MGInputManager.LStickX() < -0.2f)
            {
		    X = MGInputManager.LStickX() * Time.deltaTime * camspeed.speeds[currentcamspeed].value;
            }
			if(MGInputManager.LStickY() > 0.2f | MGInputManager.LStickY() < -0.2f)
            {
			Y = MGInputManager.LStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value;
            }

			Activeobj.transform.position = Activeobj.transform.position + (camobj.transform.forward * Y) + (camobj.transform.right * X);
        }

		void ShowAxisofObject()
        {
            if (!Activeobj.GetComponent<LineRenderer>())
            {
			Activeobj.AddComponent<LineRenderer>();
				Activeobj.AddComponent<LineRenderer>();
				Activeobj.AddComponent<LineRenderer>();
			}
			LineRenderer line_Y = Activeobj.GetComponents<LineRenderer>()[0];
			line_Y.startColor = Color.red;
			line_Y.endColor = Color.blue;
			line_Y.positionCount = 2;
			line_Y.startWidth = 0.1f;
			line_Y.endWidth = 1;
			line_Y.SetPosition(0,Activeobj.transform.position);
			line_Y.SetPosition(1, Activeobj.transform.position + (Activeobj.transform.up * 3));

			LineRenderer line_X = Activeobj.GetComponents<LineRenderer>()[1];
			line_X.startColor = Color.yellow;
			line_X.endColor = Color.yellow;
			line_X.positionCount = 2;
			line_X.startWidth = 0.1f;
			line_X.endWidth = 1;
			line_X.SetPosition(0, Activeobj.transform.position);
			line_X.SetPosition(1, Activeobj.transform.position + (Activeobj.transform.right * 3));



		}

		void SnappingtoFloor()
        {
			RaycastHit rayhit;
			Ray ray = new Ray();
			ray.origin = Activeobj.transform.position;
			
            
        }

		void CamFollow()
        {
          
				
		camobj.transform.position = Vector3.Lerp(camobj.transform.position, controlobjforcam.transform.position,100 * Time.deltaTime);

			
        }

		void ReplaceOnBButton()
        {
            if (MGInputManager.B_Down())
            {
				
				Activeobj.transform.position = regrabbed_pos;
				Activeobj.transform.eulerAngles = regrabbed_rot;
				StreamingObjectData = false;
				
				Activeobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				Activeobj.name = "Pointer";
				Activeobj.transform.position = regrabbed_pos;
			REgrabbed = false;
            }
            

        }

		int GiveUniqueNumber()
        {
			int Id = 0;
			bool matched = false;
			while(Id == 0)
            {
				int num;
				num = Mathf.RoundToInt(Random.Range(1, 500));
				foreach(NetGameObject obj in NetgameObjects)
                {
					if(num == obj.ObjectID)
                    {
						matched = true;
                    }
                }

                if (!matched)
                {
					Id = num;
                }
            }



			return Id;
        }

		void MyObjectsMenu()
        {
			Rect box = new Rect(new Vector2(Screen.width / 6 * 5 - 10, Screen.height / 12), new Vector2(Screen.width / 6f, Screen.height / 3));
			
			GUILayout.BeginArea(box);
			GUILayout.Label($"My Objects: {placedobjects.Count} placed",Generalstyle);


			MyobjectsScroll = GUILayout.BeginScrollView(MyobjectsScroll);

			GUILayout.Space(20);
			foreach(PlacedObject obj in placedobjects.ToArray())
            {

				// pick up object, remember where it was, replace on B, let go with A
                if (GUILayout.Button(obj.Object.name.Replace("(Clone)","") + $" Id: {obj.ObjectId}",Myobjectstyle))
                {
                    if (!REgrabbed)
                    {
					regrabbed_pos = obj.Object.transform.position;
					regrabbed_rot = obj.Object.transform.eulerAngles;
					REgrabbed = true;

					TargetPlacedObject(obj);

                    }

                }
				GUILayout.Space(5);




            }

			GUILayout.EndScrollView();
			GUILayout.EndArea();
           

        }

		void SaveLoadMenu()
        {

			Rect box = new Rect(new Vector2(Screen.width > 1600 ? Screen.width / 3f : Screen.width / 4, 50f), new Vector2(Screen.width>1600 ? Screen.width / 3f : Screen.width / 2 , Screen.height - 100));
			
			GUILayout.BeginArea(box,InGameUI.BoxStyle);
			Save_Load_Toggle = GUILayout.Toggle(Save_Load_Toggle, $"{SaveloadMode}");
			GUILayout.Space(15);
			
			// Load Mode
			if (Save_Load_Toggle)
            {
				SaveloadScroll = GUILayout.BeginScrollView(SaveloadScroll);
				GUILayout.Space(10);
				foreach (FileInfo file in new DirectoryInfo(ParksDirectory).GetFiles())
                {
                    if(GUILayout.Button($"{file.Name}"))
					{
						_ParkSaveLoad.LoadSpot($"{file.FullName}");
                    }
					GUILayout.Space(10);
                }
				GUILayout.EndScrollView();
			}

			// Save Mode
            if (!Save_Load_Toggle)
            {
				

				GUILayout.BeginHorizontal();
			GUILayout.Label($"{ObjectstoSave.Count} placed in Save List");
				//GUILayout.Space(5);
				SaveparkName = GUILayout.TextField(SaveparkName);
				//GUILayout.Space(5);
				if (GUILayout.Button("Save Spot"))
                {
					foreach(PlacedObject _p in ObjectstoSave)
                    {
						if(_p.ObjectId != 0 && _p.OwnerID!=0)
                        {
						bool gotname = false;
							foreach(string _s in CreatorsList)
                            {
								if(_s == GameManager.Players[_p.OwnerID].username)
                                {
									gotname = true;
                                }
                            }

                            if (!gotname)
                            {
								CreatorsList.Add(GameManager.Players[_p.OwnerID].username);
                            }


                        }

						if(_p.OwnerID == 0)
                        {
							bool gotname = false;
							foreach (string _s in CreatorsList)
							{
								if (_s == InGameUI.instance.Username)
								{
									gotname = true;
								}
							}

							if (!gotname)
							{
								CreatorsList.Add(InGameUI.instance.Username);
							}


						}

					}
					_ParkSaveLoad.SaveSpot(ObjectstoSave, SaveparkName, CreatorsList);
                }
				GUILayout.EndHorizontal();
				GUILayout.Space(15);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Add all My Objects"))
				{
					foreach (PlacedObject myobj in placedobjects)
					{
						bool found = false;
						foreach (PlacedObject alreadyin in ObjectstoSave)
						{
							if (alreadyin == myobj)
							{
								found = true;
							}
						}
						if (!found)
						{
							ObjectstoSave.Add(myobj);
						}


					}
				}
				if (GUILayout.Button("Add Everything"))
				{
					foreach (PlacedObject myobj in placedobjects)
					{
						bool found = false;
						foreach (PlacedObject alreadyin in ObjectstoSave)
						{
							if (alreadyin == myobj)
							{
								found = true;
							}
						}
						if (!found)
						{
							ObjectstoSave.Add(myobj);
						}


					}


                    if (InGameUI.instance.Connected)
                    {

					foreach (RemotePlayer player in GameManager.Players.Values)
					{
						
						
						foreach (NetGameObject rmObj in player.Objects)
						{
							PlacedObject _RMPlacedObject = new PlacedObject(rmObj._Gameobject, new BundleData(rmObj.AssetBundle, rmObj.NameOfFile, rmObj.Directory));
							_RMPlacedObject.ObjectId = rmObj.ObjectID;
							_RMPlacedObject.OwnerID = rmObj.OwnerID;
							bool found = false;
							foreach (PlacedObject alreadyin in ObjectstoSave.ToArray())
							{
								if (alreadyin.ObjectId == rmObj.ObjectID && alreadyin.OwnerID == rmObj.OwnerID)
								{
									found = true;
								}
							}


							if (!found)
							{
								ObjectstoSave.Add(_RMPlacedObject);
							}
							


						}
						
					}



                    }


				}
				GUILayout.Space(10);
				if (GUILayout.Button("Remove All of My Objects"))
				{
					foreach (PlacedObject myobj in placedobjects)
					{
						
						foreach (PlacedObject alreadyin in ObjectstoSave.ToArray())
						{
							if (alreadyin == myobj)
							{
								ObjectstoSave.Remove(alreadyin);
							}
						}
						

					}

				}
				if (GUILayout.Button("Remove Everything"))
				{
					ObjectstoSave.Clear();
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
				GUILayout.Label("My Objects");
				GUILayout.Space(5);
				MyObjectsSaveScroll = GUILayout.BeginScrollView(MyObjectsSaveScroll, GUILayout.MaxHeight(300), GUILayout.MinHeight(75));
				
				foreach (PlacedObject myobj in placedobjects.ToArray())
                {
					bool found = false;
					foreach(PlacedObject alreadyin in ObjectstoSave)
                    {
						if(alreadyin == myobj)
                        {
							found = true;
                        }
                    }
                    if (!found)
                    {
                        if (GUILayout.Button($"Add {myobj.Object.name.Replace("(Clone)","")}: ID:{myobj.ObjectId}"))
                        {
							
							ObjectstoSave.Add(myobj);
							
                        }
                    }
                    if (found)
                    {
						if (GUILayout.Button($"Remove {myobj.Object.name.Replace("(Clone)", "")}: ID:{myobj.ObjectId}"))
						{
							ObjectstoSave.Remove(myobj);
							
						}

					}

					GUILayout.Space(5);

				}
				GUILayout.Space(15);
			    GUILayout.EndScrollView();
				

				GUILayout.Space(15);
				GUILayout.Label("Online Players:");
				OtherPlayersScroll = GUILayout.BeginScrollView(OtherPlayersScroll,GUILayout.MaxHeight(300),GUILayout.MinHeight(75));
				if (InGameUI.instance.Connected)
                {
				foreach(RemotePlayer player in GameManager.Players.Values)
                {
						PlacedObject _foundobj = null;
					GUILayout.Space(5);
					GUILayout.Label($"{player.username}'s Objects");
					foreach(NetGameObject rmObj in player.Objects.ToArray())
                    {
						bool found = false;
						foreach (PlacedObject alreadyin in ObjectstoSave.ToArray())
						{
							if (alreadyin.ObjectId == rmObj.ObjectID && alreadyin.OwnerID == rmObj.OwnerID)
							{
									_foundobj = alreadyin;
									found = true;
							}
						}


						if (!found)
						{
								PlacedObject RemoteObject = new PlacedObject(rmObj._Gameobject, new BundleData(rmObj.AssetBundle, rmObj.NameOfFile, rmObj.Directory));
								RemoteObject.ObjectId = rmObj.ObjectID;
								RemoteObject.OwnerID = rmObj.OwnerID;

								if (GUILayout.Button($"Add {RemoteObject.Object.name.Replace("(Clone)", "")}"))
							    {
								ObjectstoSave.Add(RemoteObject);
							    }
						}
						if (found)
						{
							if (GUILayout.Button($"Remove {_foundobj.Object.name.Replace("(Clone)", "")}"))
							{
								ObjectstoSave.Remove(_foundobj);
							}

						}


					}
					GUILayout.Space(5);
				}

                }
				GUILayout.EndScrollView();

				GUILayout.Space(15);
            }


			GUILayout.EndArea();
		}

		public void LoadedSpotSetup(SavedSpot _Spot)
        {
			
			foreach (SavedGameObject _savedObj in _Spot.Objects)
            {
				bool found = false;
				Debug.Log(_savedObj.nameofGameObject + ":");
				// check loaded bundles
				foreach(AssetBundle asbun in AssetBundle.GetAllLoadedAssetBundles())
                {
					if (asbun.name.ToLower() == _savedObj.AssetBundleName.ToLower())
					{
						GameObject _thisobj = Instantiate(asbun.LoadAsset(_savedObj.nameofGameObject)) as GameObject;
						_thisobj.transform.position = new Vector3(_savedObj.position[0], _savedObj.position[1], _savedObj.position[2]);
						_thisobj.transform.eulerAngles = new Vector3(_savedObj.rotation[0], _savedObj.rotation[1], _savedObj.rotation[2]);
						if (_thisobj.GetComponent<Rigidbody>())
						{
							_thisobj.layer = 25;
							_thisobj.GetComponent<Rigidbody>().isKinematic = false;
						}


						_thisobj.name.Replace("(Clone)", "");
						DontDestroyOnLoad(_thisobj);
						PlacedObject _new = new PlacedObject(_thisobj, new BundleData(asbun, _savedObj.FileName, _savedObj.Directory));

						if (InGameUI.instance.Connected)
						{
						
								// send to server if online
								int ID = GiveUniqueNumber();
								_new.ObjectId = ID;

								NetGameObject _newobj = new NetGameObject(_savedObj.nameofGameObject,_savedObj.FileName,_savedObj.AssetBundleName, new Vector3(_savedObj.rotation[0], _savedObj.rotation[1], _savedObj.rotation[2]), new Vector3(_savedObj.position[0], _savedObj.position[1], _savedObj.position[2]),_thisobj.transform.localScale, false, ID, _thisobj,_savedObj.Directory);
								NetgameObjects.Add(_newobj);

								
							   ClientSend.SpawnObjectOnServer(_newobj);


							
						}


						found = true;
						placedobjects.Add(_new);
					}



                }

                // if not returned, check file directory
                if (!found)
                {
				  foreach(FileInfo file in new DirectoryInfo(_savedObj.Directory).GetFiles())
                  {
					if(file.Name.ToLower() == _savedObj.FileName.ToLower())
                    {
							Debug.Log("Found in files");
						AssetBundle loadedasbun = AssetBundle.LoadFromFile(file.FullName);
							bundlesloaded.Add(new BundleData(loadedasbun, file.Name,file.DirectoryName));
						GameObject _thisobj = Instantiate(loadedasbun.LoadAsset(_savedObj.nameofGameObject)) as GameObject;
						_thisobj.transform.position = new Vector3(_savedObj.position[0], _savedObj.position[1], _savedObj.position[2]);
						_thisobj.transform.eulerAngles = new Vector3(_savedObj.rotation[0], _savedObj.rotation[1], _savedObj.rotation[2]);
						if (_thisobj.GetComponent<Rigidbody>())
						{
							_thisobj.layer = 25;
							_thisobj.GetComponent<Rigidbody>().isKinematic = false;
						}


						_thisobj.name.Replace("(Clone)", "");
						DontDestroyOnLoad(_thisobj);

							PlacedObject _new = new PlacedObject(_thisobj, new BundleData(loadedasbun, _savedObj.FileName,file.DirectoryName));

							if (InGameUI.instance.Connected)
							{

								// send to server if online
								int ID = GiveUniqueNumber();
								_new.ObjectId = ID;

								NetGameObject _newobj = new NetGameObject(_savedObj.nameofGameObject, _savedObj.FileName, _savedObj.AssetBundleName, new Vector3(_savedObj.rotation[0], _savedObj.rotation[1], _savedObj.rotation[2]), new Vector3(_savedObj.position[0], _savedObj.position[1], _savedObj.position[2]), _thisobj.transform.localScale, false, ID, _thisobj,_savedObj.Directory);
								NetgameObjects.Add(_newobj);

								
							    ClientSend.SpawnObjectOnServer(_newobj);

							}



							placedobjects.Add(_new);

					}
                  }

                }





            }



        }

		public void LoadedSpotReceiveFromFile(SavedSpot _spot)
        {
			ActiveSavedspot = _spot;
			LoadedSpotCreators = new List<string>();
			Loadedspotpackneeded = new List<string>();
			for (int i = 0; i < _spot.Creators.Length; i++)
			{
				LoadedSpotCreators.Add(_spot.Creators[i]);
			}

			foreach(SavedGameObject ob in _spot.Objects)
            {
				bool found = false;
				foreach(string s in Loadedspotpackneeded)
                {
				if(ob.AssetBundleName == s)
                    {
						found = true;
                    }

                }

                if (!found)
                {
					Loadedspotpackneeded.Add(ob.AssetBundleName);
                }

            }


			ShowLoadedspot = true;
        }

		public void ShowLoadedSpot()
        {
			if(ActiveSavedspot != null)
            {
				Rect box = new Rect(new Vector2(Screen.width / 3, Screen.height / 4), new Vector2(Screen.width / 3f, Screen.height / 2));

				GUIStyle titles = new GUIStyle();
				titles.alignment = TextAnchor.MiddleRight;
				titles.padding = new RectOffset(5, 5, 5, 5);
				titles.normal.background = InGameUI.instance.BlackTex;
				titles.normal.textColor = Color.white;
				titles.fontStyle = FontStyle.Bold;
				
				GUIStyle buttonsstyle = new GUIStyle();
				buttonsstyle.alignment = TextAnchor.MiddleCenter;
				buttonsstyle.normal.textColor = Color.white;
				buttonsstyle.hover.textColor = Color.white;
				buttonsstyle.normal.background = InGameUI.instance.GreenTex;
				buttonsstyle.hover.background = InGameUI.instance.GreyTex;
				buttonsstyle.fontStyle = FontStyle.Bold;

				GUIStyle content = new GUIStyle();
				content.normal.textColor = Color.green;
				content.alignment = TextAnchor.MiddleCenter;
				content.padding = new RectOffset(5, 5, 5, 5);

				GUILayout.BeginArea(box,titles);
                if (GUILayout.Button("Setup",buttonsstyle))
                {
					LoadedSpotSetup(ActiveSavedspot);
					ShowLoadedspot = false;
                }
				if (GUILayout.Button("Close",buttonsstyle))
				{
					ActiveSavedspot = null;
					LoadedSpotCreators = null;
					Loadedspotpackneeded = null;
					ShowLoadedspot = false;
				}
				GUILayout.Space(20);

				GUILayout.Label(ActiveSavedspot.NameOfSpot, content);
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Built by: ", titles, GUILayout.MaxWidth(80));
				foreach(string c in LoadedSpotCreators)
                {
					GUILayout.Label(c, content);
                }
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				GUILayout.Label($"For map: ", titles, GUILayout.MaxWidth(80));
				GUILayout.Label($"{ActiveSavedspot.MapName}", content);
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				GUILayout.Label($"Object count:", titles, GUILayout.MaxWidth(100));
				GUILayout.Label($" {ActiveSavedspot.Objects.Count}", content, GUILayout.MaxWidth(30));
				GUILayout.EndHorizontal();
				GUILayout.Space(5);

				GUILayout.Label($"Packs needed:");
				foreach(string need in Loadedspotpackneeded)
                {
					GUILayout.Label(need, content);
                }

				GUILayout.EndArea();
			}
        }

		public void SceneSetupShow()
        {
			Light[] lights = FindObjectsOfType<Light>();
			GUIStyle buttonsstyle = new GUIStyle();
			buttonsstyle.alignment = TextAnchor.MiddleCenter;
			buttonsstyle.normal.textColor = Color.white;
			buttonsstyle.hover.textColor = Color.white;
			buttonsstyle.normal.background = InGameUI.instance.GreenTex;
			buttonsstyle.hover.background = InGameUI.instance.GreyTex;
			buttonsstyle.fontStyle = FontStyle.Bold;

			GUILayout.BeginArea(new Rect(new Vector2(10, Screen.height / 4), new Vector2(Screen.width / 3f, Screen.height / 2)),InGameUI.BoxStyle);
			Scenescroll = GUILayout.BeginScrollView(Scenescroll);
			if(lights != null && ShowingLight == null)
            {
				LightsToggle = GUILayout.Toggle(LightsToggle, "Lights");
                if (LightsToggle)
                {
				  GUILayout.Label("Lights");

			      foreach(Light light in lights)
                  {
				if(GUILayout.Button($"{light.name} : {light.type.ToString()}",buttonsstyle))
                {
						if(ShowingLight != light)
                        {
						  x = light.gameObject.transform.eulerAngles.x;
						  y = light.gameObject.transform.eulerAngles.y;
						  z = light.gameObject.transform.eulerAngles.z;

						 ShowingLight = light;
                        }
                        else
                        {
							ShowingLight = null;
                        }
                }
                  }

					GUILayout.Space(20);
                }


            }
            else
            {
                if (GUILayout.Button("close"))
                {
					ShowingLight = null;
                }
            }

            if (ShowingLight)
            {
			 LightAdjustment(ShowingLight);
            }

            else if(!LightsToggle)
            {
				GUILayout.Space(10);
				skyboxToggle = GUILayout.Toggle(skyboxToggle, "Environment");
                if (skyboxToggle)
                {
			     SetEnvironment();
                }
			    else
                {
			 	FreeCam();
                }

            }

			GUILayout.EndScrollView();
			GUILayout.EndArea();

        }

		void LightAdjustment(Light light)
        {
			LightSphere.SetActive(true);
			LightSphere.transform.position = light.gameObject.transform.position;
			
            


			GUILayout.Space(10);
			//label
            switch (light.type)
            {
				case LightType.Directional:
			    GUILayout.Label($" name:{light.name} | Type:{light.type.ToString()} | ShadowType:{light.shadows.ToString()} | Rotation:{light.gameObject.transform.eulerAngles.ToString()}");
					FreeCam();
					DirectionalLightRotatePanel(light);
				break;

				case LightType.Spot:
					GUILayout.Label($"{light.name} : {light.type.ToString()} : {light.shadows.ToString()} shadows : Rotation {light.gameObject.transform.eulerAngles.ToString()} : Position  {light.gameObject.transform.position.ToString()}");
					MoveLight(light);
					FocusCamOnLight();
					break;

				case LightType.Point:
					GUILayout.Label($"{light.name} : {light.type.ToString()} : {light.shadows.ToString()} shadows : Rotation {light.gameObject.transform.eulerAngles.ToString()} : Position  {light.gameObject.transform.position.ToString()}");
					MoveLight(light);
					FocusCamOnLight();
					break;

				case LightType.Area:
					GUILayout.Label($"{light.name} : {light.type.ToString()} : {light.shadows.ToString()} shadows : Rotation {light.gameObject.transform.eulerAngles.ToString()} : Position  {light.gameObject.transform.position.ToString()}");
					MoveLight(light);
					FocusCamOnLight();
					break;
				    
			}
			GUILayout.Space(20);

			LightIntensity(light);
			



        }

		void FreeCam()
        {
            
				if (MGInputManager.LTrigger() < 0.5f)
                {
                if (MGInputManager.LStickX() > 0.1f| MGInputManager.LStickX() < -0.1f| MGInputManager.RStickY()>0.1f| MGInputManager.RStickY()<-0.1f| MGInputManager.LStickY()>0.1f| MGInputManager.LStickY()<-0.1f)
                {
					camobj.transform.Translate(MGInputManager.LStickX() * Time.deltaTime * camspeed.speeds[currentcamspeed].value, MGInputManager.RStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value, MGInputManager.LStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value);
					camobj.transform.Rotate(0, MGInputManager.RStickX(), 0);
                }

				}
                else
                {
				camobj.transform.Rotate(MGInputManager.LStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value, MGInputManager.RStickX() * Time.deltaTime * camspeed.speeds[currentcamspeed].value, MGInputManager.LStickX() * Time.deltaTime * camspeed.speeds[currentcamspeed].value);
                }
			
        }

		void FocusCamOnLight()
        {
			camobj.transform.LookAt(ShowingLight.gameObject.transform);
            if (MGInputManager.RTrigger() > 0.5f)
            {
				camobj.transform.Rotate(ShowingLight.gameObject.transform.up, camspeed.speeds[currentcamspeed].value * Time.deltaTime * MGInputManager.RStickX());
            }
			Vector3 vel = camobj.transform.position - ShowingLight.transform.position;
			camobj.transform.position = Vector3.SmoothDamp(camobj.transform.position, ShowingLight.gameObject.transform.position + (-ShowingLight.gameObject.transform.right * 3),ref vel,0.1f);
        }

		void DirectionalLightRotatePanel(Light light)
        {
			GUILayout.Space(10);
			GUILayout.Label("Rotation");

			GUILayout.BeginHorizontal();
			GUILayout.Label("X:");
			x = GUILayout.HorizontalSlider(x, 0, 360);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Y:");
			y = GUILayout.HorizontalSlider(y, 0, 360);
			GUILayout.EndHorizontal();


			GUILayout.BeginHorizontal();
			GUILayout.Label("Z:");
			z = GUILayout.HorizontalSlider(z, 0, 360);
			GUILayout.EndHorizontal();

			GUILayout.Space(10);
			GUILayout.Label("Auto-Rotate");
			GUILayout.BeginHorizontal();
			GUILayout.Label("X:");
			
			bool Xb = float.TryParse(Xval = GUILayout.TextField(Xval),out float Xres);
			GUILayout.Label("Y:");
			bool Yb = float.TryParse(Yval = GUILayout.TextField(Yval), out float Yres);
			GUILayout.Label("Z:");
			bool Zb = float.TryParse(Zval = GUILayout.TextField(Zval), out float Zres);

			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Start/Update"))
            {
				if(Xb && Yb && Zb)
                {
					bool inalready = false;
                    for (int i = 0; i < Rotatinglights.Count; i++)
                    {
						if(Rotatinglights[i].light == light)
                        {
							Rotatinglights[i].Rotateamount = new Vector3(Xres, Yres, Zres);
							inalready = true;
                        }
                    }

                    if (!inalready)
                    {
						Rotatinglights.Add(new RotatingLight(light,new Vector3(Xres,Yres,Zres)));
                    }

                }

            }
			if (GUILayout.Button("Stop"))
			{
				for (int i = 0; i < Rotatinglights.ToArray().Length; i++)
				{
					if (Rotatinglights[i].light == light)
					{
						Rotatinglights.RemoveAt(i);
					}
				}
			}
			GUILayout.EndHorizontal();
			bool found = false;
            for (int i = 0; i < Rotatinglights.Count; i++)
            {
				if(Rotatinglights[i].light == light)
                {
					found = true;
                }
            }

            if (!found)
            {
			light.gameObject.transform.eulerAngles = new Vector3(x, y, z);

            }

        }

		void MoveLight(Light light)
        {
			
			if (MGInputManager.LTrigger() < 0.5f)
            {
				light.gameObject.transform.position = light.gameObject.transform.position + (camobj.transform.forward * MGInputManager.LStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value) + (camobj.transform.right * MGInputManager.LStickX() * Time.deltaTime * camspeed.speeds[currentcamspeed].value) + (camobj.transform.up * MGInputManager.RStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value);

			}
			//rotate current light if trigger is down
			if (MGInputManager.LTrigger() > 0.5f)
			{
				light.gameObject.transform.Rotate(MGInputManager.LStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value, MGInputManager.LStickX() * Time.deltaTime * camspeed.speeds[currentcamspeed].value, MGInputManager.RStickX() * Time.deltaTime * camspeed.speeds[currentcamspeed].value);
			}
		}

		void LightIntensity(Light light)
        {
			if(GUILayout.Button("Toggle On/Off"))
            {
				light.enabled = !light.enabled;
            }
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Intensity: ");
			light.intensity = GUILayout.HorizontalSlider(light.intensity, 0, 2);
			GUILayout.EndHorizontal();
			if(light.type != LightType.Directional)
            {
			GUILayout.BeginHorizontal();
			GUILayout.Label("Range: ");
			light.range = GUILayout.HorizontalSlider(light.range, 0, 100);
			GUILayout.EndHorizontal();


            }

			DynamicGI.UpdateEnvironment();
		}

		void SetEnvironment()
        {
			GUILayout.Label("Ambient Color");
			GUILayout.BeginHorizontal();
			GUILayout.Label("R:");
			Skyr = GUILayout.HorizontalSlider(Skyr, 0, 1);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("G:");
			Skyg = GUILayout.HorizontalSlider(Skyg, 0, 1);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("B:");
			Skyb = GUILayout.HorizontalSlider(Skyb, 0, 1);
			GUILayout.EndHorizontal();

			

			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Ambient Intensity:");
			RenderSettings.ambientIntensity = GUILayout.HorizontalSlider(RenderSettings.ambientIntensity,0,1);
			GUILayout.EndHorizontal();

			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Reflection intensity:");
			RenderSettings.reflectionIntensity = GUILayout.HorizontalSlider(RenderSettings.reflectionIntensity, 0, 1);
			GUILayout.EndHorizontal();

            if (RenderSettings.skybox)
            {

			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Label("SkyBox Exposure:");
			RenderSettings.skybox.SetFloat("_Exposure", GUILayout.HorizontalSlider(RenderSettings.skybox.GetFloat("_Exposure"), 0, 2));
			GUILayout.EndHorizontal();

            }




			GUILayout.Space(10);
			RenderSettings.fog = GUILayout.Toggle(RenderSettings.fog, "Toggle Fog");

			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			RenderSettings.fogMode = FogMode.Exponential;
			GUILayout.Label("Fog density");
			RenderSettings.fogDensity = GUILayout.HorizontalSlider(RenderSettings.fogDensity, 0, 0.1f);
			GUILayout.EndHorizontal();

			RenderSettings.ambientSkyColor = new Color(Skyr,Skyg,Skyb);
            if (RenderSettings.skybox)
            {
			RenderSettings.skybox.color = new Color(Skyr, Skyg, Skyb);

            }


			//RenderSettings.skybox.ma
			GUILayout.Space(20);
			GUILayout.Label("SkyBoxes:");
			GUILayout.Space(10);
			Skyboxscroll = GUILayout.BeginScrollView(Skyboxscroll);
            if (GUILayout.Button("Default"))
            {
				RenderSettings.skybox = DefaultSky;
				DynamicGI.UpdateEnvironment();
			}

            if (GUILayout.Button("Remove"))
            {
				RenderSettings.skybox = null;
				DynamicGI.UpdateEnvironment();
            }
			GUILayout.Space(10);
			foreach (FileInfo file in new DirectoryInfo(SkyboxDirectory).GetFiles())
            {
				if(GUILayout.Button($"Load {file.Name}"))
                {
					SetSkyboxTexture(file);
                }
            }
			GUILayout.EndScrollView();
			DynamicGI.UpdateEnvironment();



        }

		void SetSkyboxTexture(FileInfo file)
        {
			Texture2D image = new Texture2D(2, 2);
			byte[] bytes = File.ReadAllBytes(file.FullName);

			ImageConversion.LoadImage(image, bytes);
			image.name = file.Name;
			Material mat = GameManager.PanoMat;
			mat.mainTexture = image;

			RenderSettings.skybox = mat;
			DynamicGI.UpdateEnvironment();
			
        }

		void SetupGuis()
        {


			headerstyle.fontStyle = FontStyle.Bold;
			headerstyle.fontSize = 14;
			headerstyle.normal.textColor = Color.black;
			headerstyle.alignment = TextAnchor.MiddleCenter;

			Myobjectstyle.hover.background = InGameUI.instance.GreenTex;
			Myobjectstyle.normal.textColor = Color.black;
			Myobjectstyle.normal.background = InGameUI.instance.whiteTex;
			Myobjectstyle.alignment = TextAnchor.MiddleCenter;



			Generalstyle.normal.background = InGameUI.instance.whiteTex;
			Generalstyle.normal.textColor = Color.black;
			Generalstyle.alignment = TextAnchor.MiddleCenter;
			Generalstyle.fontStyle = FontStyle.Bold;
			

			skin.label.normal.textColor = Color.white;
			skin.label.fontSize = 12;
			skin.label.fontStyle = FontStyle.Bold;
			skin.label.alignment = TextAnchor.MiddleCenter;
			skin.label.normal.background = InGameUI.instance.TransTex;

			skin.textField.padding = new RectOffset(2, 2, 2, 2);
			skin.textField.alignment = TextAnchor.MiddleCenter;
			skin.textField.normal.textColor = Color.red;
			skin.textField.normal.background = Texture2D.whiteTexture;
			skin.textField.focused.background = InGameUI.instance.BlackTex;
			skin.textField.focused.textColor = Color.white;
			skin.textField.font = Font.CreateDynamicFontFromOSFont("Arial", 12);

			skin.button.normal.textColor = Color.black;
			skin.button.alignment = TextAnchor.MiddleCenter;
			skin.button.normal.background = InGameUI.instance.GreenTex;
			skin.button.onNormal.background = InGameUI.instance.GreyTex;
			skin.button.onNormal.textColor = Color.red;
			skin.button.onHover.background = InGameUI.instance.GreenTex;
			skin.button.hover.textColor = Color.green;
			skin.button.normal.background.wrapMode = TextureWrapMode.Clamp;
			skin.button.hover.background = InGameUI.instance.GreyTex;

			skin.toggle.normal.textColor = Color.black;
			skin.toggle.alignment = TextAnchor.MiddleCenter;
			skin.toggle.normal.background = InGameUI.instance.GreenTex;
			skin.toggle.onNormal.background = InGameUI.instance.GreyTex;
			skin.toggle.onNormal.textColor = Color.black;
			skin.toggle.onHover.background = InGameUI.instance.GreenTex;
			skin.toggle.hover.textColor = Color.green;
			skin.toggle.normal.background.wrapMode = TextureWrapMode.Clamp;
			skin.toggle.hover.background = InGameUI.instance.GreyTex;

			skin.horizontalSlider.alignment = TextAnchor.MiddleCenter;
			skin.horizontalSlider.normal.textColor = Color.black;
			skin.horizontalSlider.normal.background = InGameUI.instance.GreyTex;
			skin.horizontalSliderThumb.normal.background = InGameUI.instance.GreenTex;
			skin.horizontalSliderThumb.normal.background.wrapMode = TextureWrapMode.Clamp;
			skin.horizontalSliderThumb.normal.textColor = Color.white;
			skin.horizontalSliderThumb.fixedWidth = 20;
			skin.horizontalSliderThumb.fixedHeight = 20;
			skin.horizontalSliderThumb.hover.background = InGameUI.instance.BlackTex;

			skin.verticalScrollbarThumb.normal.background = InGameUI.instance.GreenTex;
			skin.verticalScrollbarThumb.alignment = TextAnchor.MiddleRight;
			skin.verticalScrollbar.alignment = TextAnchor.MiddleRight;
			skin.verticalScrollbar.normal.background = InGameUI.instance.GreyTex;
			skin.scrollView.alignment = TextAnchor.MiddleCenter;
			skin.scrollView.padding = new RectOffset(15, 15, 5, 5);
			skin.scrollView.normal.background = InGameUI.instance.whiteTex;

			



		}

		void YtoChangeSpeed()
        {
			if (MGInputManager.Y_Down())
			{
				currentcamspeed = camspeed.Change(currentcamspeed);
			}
		}

	}


	/// <summary>
	/// Structure for sending data to net
	/// </summary>
	public class NetGameObject
    {
		public string NameofObject;
		public string NameofAssetBundle;
		public string NameOfFile;
		public Vector3 Rotation;
		public Vector3 Position;
		public Vector3 Scale;
		public bool IsPhysics;
		public GameObject _Gameobject = null;
		public int ObjectID;
		public AssetBundle AssetBundle;
		public uint OwnerID;
		public string Directory;

		public NetGameObject(string _nameofobject,string _nameoffile, string _nameofassetbundle,Vector3 _rotation,Vector3 _position, Vector3 _scale, bool _IsPhysicsenabled, int objectid, GameObject GO,string dir)
        {
			NameofObject = _nameofobject;
			NameOfFile = _nameoffile;
			NameofAssetBundle = _nameofassetbundle;
			Rotation = _rotation;
			Position = _position;
			Scale = _scale;
			IsPhysics = _IsPhysicsenabled;
			_Gameobject = GO;
			ObjectID = objectid;
			Directory = dir;

        }

		


    }

	public class BundleData
    {
		
		public AssetBundle Bundle;
		/// <summary>
		/// file no with no path
		/// </summary>
		public string FileName;
		/// <summary>
		/// Full path to file
		/// </summary>
		public string FullDir;


		public BundleData(AssetBundle _bundle, string _filename,string fulldir)
        {
			Bundle = _bundle;
			FileName = _filename;
			FullDir = fulldir;
			

        }



    }

	public class PlacedObject
    {
	 public GameObject Object;
	 public BundleData BundleData;
	 public int ObjectId = 0;
	 public uint OwnerID = 0;

	  public PlacedObject(GameObject GO,BundleData Bundata)
      {
		Object = GO;
		BundleData = Bundata;
      }

    }

	public class RotatingLight
    {
		public Light light;
		public Vector3 Rotateamount;


		public RotatingLight(Light _light, Vector3 rotation)
        {
			light = _light;
			Rotateamount = rotation;
        }


    }

}