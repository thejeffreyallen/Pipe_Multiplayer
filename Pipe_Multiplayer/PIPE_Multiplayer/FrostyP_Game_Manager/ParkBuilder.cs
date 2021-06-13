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

		MGInputManager inputman;
		ParkSaveLoad _ParkSaveLoad = new ParkSaveLoad();

		
		Vector2 scrollPosition;
		

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
		bool placedonce = false;
		bool SaveLoadOpen = false;
		bool switched;
		bool Helpmenu;
		bool REgrabbed = false;
		bool StreamingObjectData = false;
		Vector3 regrabbed_pos;
		Vector3 regrabbed_rot;



		// Myobjects Menu
		Vector2 MyobjectsScroll;




		public string AssetbundlesDirectory = Application.dataPath + "/FrostyPGameManager/ParkBuilder/Assetbundles/";
		public string ParksDirectory = Application.dataPath + "/FrostyPGameManager/ParkBuilder/Parks/";


		
		public List<PlacedObject> placedobjects = new List<PlacedObject>();
		public List<NetGameObject> NetgameObjects = new List<NetGameObject>();
		
		

		Texture2D RedTex;
		Texture2D BlackTex;
		Texture2D GreyTex;
		Texture2D GreenTex;
		Texture2D whiteTex;
		Texture2D TransTex;
		


		public GameObject Player;
		Camera buildercam;
		GameObject camobj;
		Vector3 camFarPos;
		GameObject controlobjforcam;
		
		GameObject Activeobj;
		

		public FileInfo[] availableassetbundles;
		public List<BundleData> bundlesloaded;


		BundleData ActiveBundleData;
		PlacedObject ActivePlacedObject;

		// send
		float KeepAlivetimer;



		// SaveLoad Menu
		public List<PlacedObject> ObjectstoSave = new List<PlacedObject>();
		public List<string> CreatorsList = new List<string>();
		bool Save_Load_Toggle = false;
		string SaveloadMode;
		Vector2 SaveloadScroll;
		public bool ShowLoadedspot;
		public SavedSpot ActiveSavedspot;
		List<string> LoadedSpotCreators;
		List<string> Loadedspotpackneeded;
		


		void Awake()
        {
			instance = this;

			
				bundlesloaded = new List<BundleData>();
			

			// check both directories and create if needed
			DirectoryInfo info = new DirectoryInfo(ParksDirectory);
			if (!info.Exists)
			{
				info.Create();
			}
			DirectoryInfo info1 = new DirectoryInfo(AssetbundlesDirectory);
			if (!info1.Exists)
			{
				info1.Create();
			}


			camobj = new GameObject();
			camobj.AddComponent<FMOD_Listener>();
			buildercam = camobj.AddComponent<Camera>();
			camobj.SetActive(false);
			DontDestroyOnLoad(camobj);
			// make ghost object that rotates to camera obj only on Y
			controlobjforcam = new GameObject();
			DontDestroyOnLoad(controlobjforcam);
		}

		// Use this for initialization
		void Start()
        {
			

			// create instance of camspeed and run setup once, this adds 3 presets for Y button to scroll through
			camspeed = new Camspeed();
			camspeed.Setup();

			// check both directories and create if needed
			DirectoryInfo info = new DirectoryInfo(ParksDirectory);
            if (!info.Exists)
            {
				info.Create();
            }
			DirectoryInfo info1 = new DirectoryInfo(AssetbundlesDirectory);
			if (!info.Exists)
			{
				info.Create();
			}




			RedTex = new Texture2D(Screen.width / 6, Screen.height / 4); ;
			Color[] colorarray = RedTex.GetPixels();
			Color newcolor = new Color(0.5f, 0, 0, 1);
			for (var i = 0; i < colorarray.Length; ++i)
			{
				colorarray[i] = newcolor;
			}

			RedTex.SetPixels(colorarray);

			RedTex.Apply();

			BlackTex = Texture2D.blackTexture;
			Color[] colorarray2 = BlackTex.GetPixels();
			Color newcolor2 = new Color(0, 0, 0, 0.4f);
			for (var i = 0; i < colorarray2.Length; ++i)
			{
				colorarray2[i] = newcolor2;
			}

			BlackTex.SetPixels(colorarray2);

			BlackTex.Apply();

			GreyTex = Texture2D.blackTexture;
			Color[] colorarray3 = GreyTex.GetPixels();
			Color newcolor3 = new Color(0.5f, 0.5f, 0.5f, 1);
			for (var i = 0; i < colorarray3.Length; ++i)
			{
				colorarray3[i] = newcolor3;
			}

			GreyTex.SetPixels(colorarray3);

			GreyTex.Apply();


			GreenTex = new Texture2D(20, 10);
			Color[] colorarray4 = GreenTex.GetPixels();
			Color newcolor4 = new Color(0.2f, 0.6f, 0.2f, 1f);
			for (var i = 0; i < colorarray4.Length; ++i)
			{
				colorarray4[i] = newcolor4;
			}

			GreenTex.SetPixels(colorarray4);

			GreenTex.Apply();



			whiteTex = new Texture2D(20, 10);
			Color[] colorarray5 = whiteTex.GetPixels();
			Color newcolor5 = new Color(1f, 1f, 1f, 0.3f);
			for (var i = 0; i < colorarray5.Length; ++i)
			{
				colorarray5[i] = newcolor5;
			}

			whiteTex.SetPixels(colorarray5);

			whiteTex.Apply();


			TransTex = new Texture2D(20, 10);
			Color[] colorarray6 = TransTex.GetPixels();
			Color newcolor6 = new Color(1f, 1f, 1f, 0f);
			for (var i = 0; i < colorarray6.Length; ++i)
			{
				colorarray6[i] = newcolor6;
			}

			TransTex.SetPixels(colorarray6);

			TransTex.Apply();







			Generalstyle.normal.background = whiteTex;
			Generalstyle.normal.textColor = Color.black;

			Generalstyle.alignment = TextAnchor.MiddleCenter;
			Generalstyle.fontStyle = FontStyle.Bold;
			//Generalstyle.fontSize = 16;
			//Generalstyle.border.left = 5;
			//Generalstyle.border.right = 5;
			//Generalstyle.margin.left = 5;
			//Generalstyle.margin.right = 5;

			skin.label.normal.textColor = Color.black;
			skin.label.fontSize = 15;
			skin.label.fontStyle = FontStyle.Bold;
			skin.label.alignment = TextAnchor.MiddleCenter;
			skin.label.normal.background = TransTex;



			skin.textField.alignment = TextAnchor.MiddleCenter;
			skin.textField.normal.textColor = Color.red;
			skin.textField.normal.background = GreyTex;
			skin.textField.focused.background = BlackTex;
			skin.textField.focused.textColor = Color.white;
			skin.textField.font = Font.CreateDynamicFontFromOSFont("Arial", 14);



			skin.button.normal.textColor = Color.black;
			skin.button.alignment = TextAnchor.MiddleCenter;
			skin.button.normal.background = GreenTex;
			skin.button.onNormal.background = GreyTex;
			skin.button.onNormal.textColor = Color.red;
			skin.button.onHover.background = GreenTex;
			skin.button.hover.textColor = Color.green;
			skin.button.normal.background.wrapMode = TextureWrapMode.Clamp;
			skin.button.hover.background = GreyTex;



			skin.toggle.normal.textColor = Color.black;
			skin.toggle.alignment = TextAnchor.MiddleCenter;
			skin.toggle.normal.background = GreenTex;
			skin.toggle.onNormal.background = GreyTex;
			skin.toggle.onNormal.textColor = Color.black;
			skin.toggle.onHover.background = GreenTex;
			skin.toggle.hover.textColor = Color.green;
			skin.toggle.normal.background.wrapMode = TextureWrapMode.Clamp;
			skin.toggle.hover.background = GreyTex;


			skin.horizontalSlider.alignment = TextAnchor.MiddleCenter;
			skin.horizontalSlider.normal.textColor = Color.black;
			skin.horizontalSlider.normal.background = GreyTex;
			skin.horizontalSliderThumb.normal.background = GreenTex;
			skin.horizontalSliderThumb.normal.background.wrapMode = TextureWrapMode.Clamp;
			skin.horizontalSliderThumb.normal.textColor = Color.white;
			skin.horizontalSliderThumb.fixedWidth = 20;
			skin.horizontalSliderThumb.fixedHeight = 20;
			skin.horizontalSliderThumb.hover.background = BlackTex;

			
			//skin.scrollView.normal.background = GreenTex;
			skin.verticalScrollbarThumb.normal.background = GreenTex;
			skin.scrollView.alignment = TextAnchor.MiddleCenter;
			//skin.scrollView.fixedWidth = Screen.width / 4;



		}

        // Update is called once per frame
        void Update()
        {

            if (openflag)
            {

                if (REgrabbed && Activeobj != null)
                {
					ReplaceOnBButton();
                }





				// check everything is there, if not load/find it/turn it on, destroying activeobj will always result in it being re-instatiated as pointer object
				
				
				if (buildercam == null && camobj != null)
				{
					buildercam = camobj.AddComponent<Camera>();
					camobj.AddComponent<FMOD_Listener>();
					buildercam.transform.position = Camera.current.transform.position;
					buildercam.transform.rotation = Camera.current.transform.rotation;
					
					buildercam.enabled = true;
				}
				
				
				if (Activeobj != null)
				{
					camFarPos = new Vector3(Activeobj.transform.position.x, Activeobj.transform.position.y + 10, Activeobj.transform.position.z - 50);
					//ShowAxisofObject();

				}


				// send keepactive packet every 2 seconds to stop timeout on server
				if (InGameUI.instance.Connected)
				{
					// keep connection active while no rider is streaming
					if (KeepAlivetimer < 2)
					{
						KeepAlivetimer = KeepAlivetimer + Time.deltaTime;
					}
					else if (KeepAlivetimer >= 2)
					{
						ClientSend.KeepActive();
						KeepAlivetimer = 0;
					}


				}


			}



		}


		void FixedUpdate()
        {
            if (StreamingObjectData)
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

           




        }




        void OnGUI()
        {
			if (openflag)
            {

				// things that decide theyre own GUI layout ---===
				
				if (placedobjects.Count != 0)
				{

					MyObjectsMenu();
				}


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



				//--------------------------------------------


			skin.textField.margin.left = Screen.width / 12;
			skin.textField.margin.right = Screen.width / 12;
			skin.button.margin.left = Screen.width / 12;
			skin.button.margin.right = Screen.width / 12;
			skin.toggle.margin.left = Screen.width / 12;
			skin.toggle.margin.right = Screen.width / 12;
			skin.horizontalSlider.margin.left = Screen.width / 16;
			skin.horizontalSlider.margin.right = Screen.width / 16;
			skin.label.padding.right = 10;
			//skin.label.margin.left = Screen.width / 10;
			//skin.label.margin.right = Screen.width / 10;


			GUI.skin = skin;




				



				// -------------------------------------------------------------------------------------------------------------------------      Things contained in Primary GUI     -----------------------------------------------------------------------------------------------------------------

				scrollPosition = GUILayout.BeginScrollView(scrollPosition, Generalstyle);
				GUILayout.Space(10);
				GUILayout.Label("Park Builder", Generalstyle);
				
				GUILayout.Space(10);
				Helpmenu = GUILayout.Toggle(Helpmenu, " put my objects into parkbuilder? ");
                if (Helpmenu)
                {
					GUILayout.Label(" parkbuilder will recognise and load any Assetbundles that");
					GUILayout.Label(" do not contain Scenes. To do this follow the steps");
					GUILayout.Label(" for creating a map, using the mapscript that creates bundles,");
					GUILayout.Label(" but dont mark your scene as a bundle item, make each object in");
					GUILayout.Label(" your a scene a prefab by dragging from hierarchy to assets,");
					GUILayout.Label(" then mark each prefab as part of your assetbundle, build and");
					GUILayout.Label(" put in ParkBuilder/AssetBundles/, they should then show below.");
					GUILayout.Label(" make your objects positions 0,0,0 and square so that theyre root");
					GUILayout.Label(" position can be found again later");

				}
				if (camobj != null && Activeobj != null)
				{
					GUILayout.Space(10);
					GUILayout.Label(" Controls: ");
					Controls();
				}
				GUILayout.Space(30);
				GUILayout.Label("Movement Speed at " + camspeed.speeds[currentcamspeed].name, Generalstyle);



				if (GUILayout.Button("Clear All ") && placedobjects.Count>0)
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
					if(Activeobj != null)
                    {
						pos = Activeobj.transform.position;
						Destroy(Activeobj);
                    }
					Activeobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					Activeobj.transform.position = pos;
				}




				if (Activeobj != null && !Activeobj.name.Contains("Pointer"))
                {
					if(GUILayout.Button("Destroy " + Activeobj.gameObject.name.Replace("(Clone)","")))
					{
						DestroyObject();
						
						
                    }
                }





				// Save current objects
				GUILayout.Space(10);
				SaveLoadOpen = GUILayout.Toggle(SaveLoadOpen, "Toggle Save/Load Menu");
               

				GUILayout.Space(10);




				GUILayout.Label("Loadable bundles:");
				// list Bundles found and give load button
				foreach (FileInfo file in availableassetbundles)
                {
					bool loaded = false;
					foreach(BundleData bun in bundlesloaded)
                    {
						if(bun.FileName == file.Name)
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
						bundlesloaded.Add(new BundleData(bundle,file.Name));

                    }

                    }

                    
                }

				GUILayout.Space(20);

				if (bundlesloaded != null)
                {
					GUILayout.Label(" Loaded Assets: ");
					GUILayout.Space(20);
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
								Vector3 pos = new Vector3( Activeobj.transform.position.x, Activeobj.transform.position.y, Activeobj.transform.position.z);
								Destroy(Activeobj.gameObject);
								
								SpawnNewObject(obj, bundledata, pos);
							}
						}
					}
                }









				GUILayout.Space(20);
				if (GUILayout.Button("Return to game"))
            {
					Destroy(Activeobj);
					camobj.SetActive(false);
				    Player.SetActive(true);
				    openflag = false;
					
					
            }

			GUILayout.EndScrollView();
            }
        }

        #region Functions





	
		/// <summary>
		/// Call to open ParkBuilder
		/// </summary>
		public void Open(Vector3 _playerscurrentpos)
        {
			Activeobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Activeobj.name = "Pointer";
			Activeobj.transform.position = _playerscurrentpos;
			
			Player = UnityEngine.Component.FindObjectOfType<PlayerInfo>().gameObject;
			Player.SetActive(false);
			openflag = true;
			

			if (inputman == null)
			{
				inputman = new MGInputManager();

			}


			    camobj.SetActive(true);
				camobj.transform.position = _playerscurrentpos + (Vector3.back * 5);
			

			if(buildercam == null)
            {
				
				buildercam = camobj.AddComponent<Camera>();
				camobj.GetComponent<FMOD_Listener>().enabled = true;
			}
		
			//give list of found files
		availableassetbundles = LoadAssets();
			


		}




        /// <summary>
        /// gives list of fileinfo's from Assetbundles directory
        /// </summary>
        /// <returns></returns>
        FileInfo[] LoadAssets()
        {
			DirectoryInfo listofbundles = new DirectoryInfo(AssetbundlesDirectory);
            if (!listofbundles.Exists)
            {
				listofbundles.Create();
            }


			return listofbundles.GetFiles();

        }



		// called by clicking a button
		void SpawnNewObject(GameObject obj, BundleData _bunddata, Vector3 pos)
        {
			ActiveBundleData = _bunddata;
			Activeobj = GameObject.Instantiate(obj);
			Activeobj.transform.position = pos;
			Activeobj.name.Replace("(Clone)", "");
			if (obj.GetComponent<Rigidbody>())
			{
				obj.layer = 25;
				obj.GetComponent<Rigidbody>().isKinematic = true;
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







		void Controls()
        {
			controlobjforcam.transform.position = camobj.transform.position;
			controlobjforcam.transform.eulerAngles = new Vector3(0, camobj.transform.eulerAngles.y, 0);


			camobj.transform.LookAt(Activeobj.transform, Vector3.up);


			var Zofobject = controlobjforcam.transform.up;
			var Xofobject = controlobjforcam.transform.right;
			var YofObject = controlobjforcam.transform.forward;




			// if L trigger is off, Lstick moves object
			if (MGInputManager.LTrigger() < 0.2f && MGInputManager.RTrigger() < 0.2f)
			{
				GUILayout.Space(20);
				GUILayout.Label("L Stick to move, Rstick to rotate cam around object, A to place - Y to change speed");
				GUILayout.Space(10);
				GUILayout.Label(" RB an LB to spin object 45 - Lstick click to reset rotation");
				GUILayout.Label(" RT or LT For other functions");



				// move function for live object

				MoveObject();
				CamFollow();

				float speed = 40;
				if(MGInputManager.RStickX()>0.2f | MGInputManager.RStickX() < -0.2f)
                {
				camobj.transform.RotateAround(Activeobj.transform.position, Vector3.up, -MGInputManager.RStickX() * Time.deltaTime * speed);

                }

				if (MGInputManager.RStickY() > 0.2f | MGInputManager.RStickY() < -0.2f)
				{
					camobj.transform.RotateAround(Activeobj.transform.position, camobj.transform.right, MGInputManager.RStickY() * Time.deltaTime * speed);
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
				GUILayout.Label("L Stick for Height adjust, Rstick for pan in/out");
				Activeobj.transform.position = Activeobj.transform.position + controlobjforcam.transform.up * MGInputManager.LStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value;



				// pan in and out with R stick
				if (Vector3.Distance(camobj.transform.position, Activeobj.transform.position) > Camdistance)
				{
					camobj.transform.position = Vector3.MoveTowards(camobj.transform.position, Activeobj.transform.position, Time.deltaTime * 5);

				}
				if (Vector3.Distance(camobj.transform.position, Activeobj.transform.position) < Camdistance)
				{
					Vector3 dir = (camobj.transform.position - Activeobj.transform.position).normalized;
					camobj.transform.position = camobj.transform.position + dir * 10 * Time.deltaTime;


				}

				Camdistance = Mathf.Clamp(Camdistance, 5, 100);
				Camdistance = Mathf.Lerp(Camdistance, Camdistance + (-MGInputManager.RStickY()), 15 * Time.fixedDeltaTime);

			}


			// if R Trigger is on
			if (MGInputManager.RTrigger() > 0.4 && MGInputManager.LTrigger() < 0.2f)
			{
				GUILayout.Label("Fine Tune Rotate Object -- Lstick to rotate Y, Rstick to rotate Z and X ");

				// fine tuning rotate
				Activeobj.transform.Rotate(MGInputManager.RStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value, MGInputManager.LStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value, -MGInputManager.RStickX() * Time.deltaTime * camspeed.speeds[currentcamspeed].value);


			}



			if (MGInputManager.LTrigger() < 0.2f && Activeobj != null && MGInputManager.A_Down() && placedonce == false)
			{
				PlaceObject();
			}
			if (MGInputManager.A_Up())
			{
				placedonce = false;
			}

			if (MGInputManager.Y_Down() && switched == false)
			{

				currentcamspeed = camspeed.Change(currentcamspeed);
				switched = true;
			}
			if (MGInputManager.Y_Up())
			{
				switched = false;
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



		// instantiates a clone of current object, adds PLACED to its name for later reference then re-targets activeobj to the live version
		void PlaceObject()
        {
            if (!Activeobj.name.Contains("Pointer"))
            {
			StreamingObjectData = false;

            if (!REgrabbed)
            {
			GameObject obj = GameObject.Instantiate(UnityEngine.GameObject.Find(Activeobj.name));
			//obj.transform.DetachChildren();
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

				NetGameObject _newobj = new NetGameObject(Activeobj.name.Replace("(Clone)",""), ActiveBundleData.FileName, ActiveBundleData.Bundle.name, Activeobj.transform.eulerAngles, Activeobj.transform.position, Activeobj.transform.localScale, false, ID, obj);
				NetgameObjects.Add(_newobj);

            if (InGameUI.instance.Connected && NetgameObjects.Count < 10)
            {
				ClientSend.SpawnObjectOnServer(_newobj);
			

			}


            }
            catch (UnityException x)
            {
				Debug.Log(x);
            }


			placedobjects.Add(_new);
			
			placedonce = true;

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
				placedonce = true;
            }


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

			Activeobj.transform.position = Activeobj.transform.position + transform.TransformDirection(controlobjforcam.transform.forward) * Y + transform.TransformDirection(controlobjforcam.transform.right) * X;
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
            if (Vector3.Distance(camobj.transform.position, Activeobj.transform.position) > Camdistance)
            {
				Vector3 dir = -(camobj.transform.position - Activeobj.transform.position).normalized;
				camobj.transform.position = camobj.transform.position + dir * 10 * Time.deltaTime;

			}
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

        #endregion




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
			Rect box = new Rect(new Vector2(Screen.width / 6 * 5, Screen.height / 12), new Vector2(Screen.width / 6f, Screen.height / 3));
			GUI.skin = skin;
			GUILayout.BeginArea(box);
			GUILayout.Label($"My Objects: {placedobjects.Count} placed", Generalstyle);


			MyobjectsScroll = GUILayout.BeginScrollView(MyobjectsScroll);

			GUILayout.Space(20);
			foreach(PlacedObject obj in placedobjects)
            {

				// pick up object, remember where it was, replace on B, let go with A
                if (GUILayout.Button(obj.Object.name.Replace("(Clone)","")))
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
			Rect box = new Rect(new Vector2(Screen.width / 2 - 10, Screen.height / 1.7f), new Vector2(Screen.width / 2f, Screen.height / 2));
			GUI.skin = skin;
			GUILayout.BeginArea(box);
			Save_Load_Toggle = GUILayout.Toggle(Save_Load_Toggle, $"{SaveloadMode}");
			GUILayout.Space(15);
			// Load Mode
			if (Save_Load_Toggle)
            {
				SaveloadScroll = GUILayout.BeginScrollView(SaveloadScroll);
				foreach(FileInfo file in new DirectoryInfo(ParksDirectory).GetFiles())
                {
                    if(GUILayout.Button($"{file.Name}"))
					{
						_ParkSaveLoad.LoadSpot($"{file.FullName}");
                    }
                }
				GUILayout.EndScrollView();
			}

			// Save Mode
            if (!Save_Load_Toggle)
            {
			GUILayout.Label($"{ObjectstoSave.Count} placed in Save List", Generalstyle);
				GUILayout.Space(5);
				SaveparkName = GUILayout.TextField(SaveparkName);
				GUILayout.Space(5);
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

				SaveloadScroll = GUILayout.BeginScrollView(SaveloadScroll);

				GUILayout.Space(5);
				GUILayout.Label("My Objects");
				GUILayout.Space(5);
				foreach (PlacedObject myobj in placedobjects)
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
                        if (GUILayout.Button($"Add {myobj.Object.name.Replace("(Clone)","")}"))
                        {
							
							ObjectstoSave.Add(myobj);
							
                        }
                    }
                    if (found)
                    {
						if (GUILayout.Button($"Remove {myobj.Object.name.Replace("(Clone)", "")}"))
						{
							ObjectstoSave.Remove(myobj);
							
						}

					}

					GUILayout.Space(5);

				}
				GUILayout.Space(15);
				if (InGameUI.instance.Connected)
                {
				foreach(RemotePlayer player in GameManager.Players.Values)
                {
						PlacedObject _foundobj = null;
					GUILayout.Space(5);
					GUILayout.Label($"{player.username}'s Objects");
					foreach(NetGameObject rmObj in GameManager.PlayersObjects[player.id])
                    {
						PlacedObject _RMPlacedObject = new PlacedObject(rmObj._Gameobject, new BundleData(rmObj.AssetBundle, rmObj.NameOfFile));
							_RMPlacedObject.ObjectId = rmObj.ObjectID;
							_RMPlacedObject.OwnerID = rmObj.OwnerID;
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
							if (GUILayout.Button($"Add {_RMPlacedObject.Object.name.Replace("(Clone)", "")}"))
							{
								ObjectstoSave.Add(_RMPlacedObject);
							}
						}
						if (found)
						{
							if (GUILayout.Button($"Remove {_RMPlacedObject.Object.name.Replace("(Clone)", "")}"))
							{
								ObjectstoSave.Remove(_foundobj);
							}

						}


					}
					GUILayout.Space(5);
				}

                }

			GUILayout.EndScrollView();


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
					if (asbun.name == _savedObj.AssetBundleName)
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
						PlacedObject _new = new PlacedObject(_thisobj, new BundleData(asbun, _savedObj.FileName));

						if (InGameUI.instance.Connected)
						{
						
								// send to server if online
								int ID = GiveUniqueNumber();
								_new.ObjectId = ID;

								NetGameObject _newobj = new NetGameObject(_savedObj.nameofGameObject,_savedObj.FileName,_savedObj.AssetBundleName, new Vector3(_savedObj.rotation[0], _savedObj.rotation[1], _savedObj.rotation[2]), new Vector3(_savedObj.position[0], _savedObj.position[1], _savedObj.position[2]),_thisobj.transform.localScale, false, ID, _thisobj);
								NetgameObjects.Add(_newobj);

								if (InGameUI.instance.Connected && NetgameObjects.Count < 10)
								{
									ClientSend.SpawnObjectOnServer(_newobj);


								}

							
						}


						found = true;
						placedobjects.Add(_new);
					}
                }


                // if not returned, check file directory
                if (!found)
                {
				foreach(FileInfo file in new DirectoryInfo(AssetbundlesDirectory).GetFiles())
                {
					if(file.Name == _savedObj.FileName)
                    {
						AssetBundle asbun = AssetBundle.LoadFromFile(file.FullName);
							bundlesloaded.Add(new BundleData(asbun, file.Name));
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

							PlacedObject _new = new PlacedObject(_thisobj, new BundleData(asbun, _savedObj.FileName));

							if (InGameUI.instance.Connected)
							{

								// send to server if online
								int ID = GiveUniqueNumber();
								_new.ObjectId = ID;

								NetGameObject _newobj = new NetGameObject(_savedObj.nameofGameObject, _savedObj.FileName, _savedObj.AssetBundleName, new Vector3(_savedObj.rotation[0], _savedObj.rotation[1], _savedObj.rotation[2]), new Vector3(_savedObj.position[0], _savedObj.position[1], _savedObj.position[2]), _thisobj.transform.localScale, false, ID, _thisobj);
								NetgameObjects.Add(_newobj);

								if (InGameUI.instance.Connected && NetgameObjects.Count < 10)
								{
									ClientSend.SpawnObjectOnServer(_newobj);


								}


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

				GUIStyle BackG = new GUIStyle();
				BackG.alignment = TextAnchor.MiddleCenter;
				BackG.padding = new RectOffset(5, 5, 5, 5);
				BackG.normal.background = whiteTex;
				BackG.normal.textColor = Color.black;
				
				
				

				GUIStyle buttonsstyle = new GUIStyle();
				buttonsstyle.alignment = TextAnchor.MiddleCenter;
				buttonsstyle.normal.textColor = Color.grey;
				buttonsstyle.hover.textColor = Color.white;
				buttonsstyle.normal.background = whiteTex;
				buttonsstyle.hover.background = GreenTex;
				buttonsstyle.fontStyle = FontStyle.Bold;


				GUILayout.BeginArea(box,BackG);
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

				GUILayout.Label(ActiveSavedspot.NameOfSpot);
				GUILayout.Label($"Built by: ", BackG);
				foreach(string c in LoadedSpotCreators)
                {
					GUILayout.Label(c, buttonsstyle);
                }
				GUILayout.Label($"For map: {ActiveSavedspot.MapName}", BackG);
				GUILayout.Label($"Object count: {ActiveSavedspot.Objects.Count}", BackG);
				GUILayout.Label($"Packs needed:", BackG);
				foreach(string need in Loadedspotpackneeded)
                {
					GUILayout.Label(need, buttonsstyle);
                }

				GUILayout.EndArea();
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

		public NetGameObject(string _nameofobject,string _nameoffile, string _nameofassetbundle,Vector3 _rotation,Vector3 _position, Vector3 _scale, bool _IsPhysicsenabled, int objectid, GameObject GO)
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
		public string FullFileName;


		public BundleData(AssetBundle _bundle, string _filename)
        {
			Bundle = _bundle;
			FileName = _filename;
			

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



}