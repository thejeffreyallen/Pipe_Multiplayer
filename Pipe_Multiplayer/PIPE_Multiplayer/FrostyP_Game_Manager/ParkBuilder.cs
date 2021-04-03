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
		GUISkin skin = (GUISkin)ScriptableObject.CreateInstance("GUISkin");
		GUIStyle Generalstyle = new GUIStyle();

		MGInputManager inputman;
		ParkSaver Saver = new ParkSaver();

		
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

				
				

			}

			public int Change(int currentspeed)
            {
                if(currentspeed == 3)
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
		float Camdistance = 20;
		Camspeed camspeed;


		string SaveparkName = "Default name park";

		public bool openflag = false;
		
		bool snappingtofloor = true;
		bool placedonce = false;
		bool Saveopen = false;
		bool switched;
		bool Helpmenu;
		bool Loadopen = false;
		bool lookingforobjects = false;
		bool REgrabbed = false;
		Vector3 regrabbed_pos;
		Vector3 regrabbed_rot;



		string AssetbundlesDirectory = Application.dataPath + "/FrostyPGameManager/ParkBuilder/Assetbundles";
		string ParksDirectory= Application.dataPath + "/FrostyPGameManager/ParkBuilder/Parks/";


		List<GameObject> itemsfound = new List<GameObject>();
		List<GameObject> placedobjects = new List<GameObject>();

		Texture2D RedTex;
		Texture2D BlackTex;
		Texture2D GreyTex;
		Texture2D GreenTex;
		Texture2D whiteTex;
		Texture2D TransTex;
		


		GameObject Player;
		Camera buildercam;
		GameObject camobj;
		Vector3 camFarPos;
		GameObject controlobjforcam;
		
		GameObject Activeobj;
		

		public FileInfo[] availableassetbundles;
		public List<AssetBundle> bundlesloaded;


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


			// make ghost object that rotates to camera obj only on Y
			controlobjforcam = new GameObject();


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

			skin.button.normal.textColor = Color.black;
			skin.button.alignment = TextAnchor.MiddleCenter;
			skin.scrollView.normal.background = GreenTex;
			skin.verticalScrollbarThumb.normal.background = GreenTex;
			skin.scrollView.alignment = TextAnchor.MiddleCenter;
			skin.scrollView.fixedWidth = Screen.width / 4;



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
				
				
				if (buildercam == null)
				{
					buildercam = camobj.AddComponent<Camera>();
					buildercam.transform.position = Camera.current.transform.position;
					buildercam.transform.rotation = Camera.current.transform.rotation;
					
					buildercam.enabled = true;
				}
				
				
				if (Activeobj != null)
				{
					camFarPos = new Vector3(Activeobj.transform.position.x, Activeobj.transform.position.y + 10, Activeobj.transform.position.z - 50);
					//ShowAxisofObject();

				}



			}



		}






        void OnGUI()
        {
			if (openflag)
            {
				
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




				

				if (placedobjects.Count != 0)
				{

					ShowCurrentBuild(SaveparkName);
				}


				// -------------------------------------------------------------------------------------------------------------------------      Actual GUI     -----------------------------------------------------------------------------------------------------------------

				scrollPosition = GUILayout.BeginScrollView(scrollPosition, Generalstyle);
				GUILayout.Space(10);
				GUILayout.Label("Park Builder Menu *Limited Functionlity*", Generalstyle);
				
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
					GUILayout.Label(" put in ParkBuilder/AssetBundles/, they should then show below");

				}
				if (camobj != null && Activeobj != null)
				{
					GUILayout.Space(10);
					GUILayout.Label(" Controls: ");
					Controls();
				}
				GUILayout.Space(30);
				GUILayout.Label("Movement Speed at " + currentcamspeed, Generalstyle);
				GUILayout.Label("Placed objects : " + placedobjects.Count, Generalstyle);
				snappingtofloor = GUILayout.Toggle(snappingtofloor, " Snapping to floor");

				if(Activeobj != null && !Activeobj.name.Contains("Pointer"))
                {
					if(GUILayout.Button("Destroy " + Activeobj.gameObject.name))
					{
						//Vector3 pos = Activeobj.transform.position;
						//Vector3 rot = Activeobj.transform.eulerAngles;
						REgrabbed = false;
						Destroy(Activeobj);
						
						
						
                    }
                }





				// Save current objects
				GUILayout.Space(10);
				Saveopen = GUILayout.Toggle(Saveopen, "Toggle Save Menu");
                if (Saveopen)
                {
					SaveparkName = GUILayout.TextField(SaveparkName);
                    if(GUILayout.Button("Save")){

						Saver.Save(Saver.GiveNamesAndTransformData(placedobjects), ParksDirectory + SaveparkName);


                    }
                }



				// load a setup
				GUILayout.Space(10);
				Loadopen = GUILayout.Toggle(Loadopen, "Toggle Load Menu");
				if (Loadopen)
				{
					DirectoryInfo info = new DirectoryInfo(ParksDirectory);
					FileInfo[] files = info.GetFiles();

                    foreach(FileInfo file in files)
                    {
						if(GUILayout.Button("Load "+ file.Name))
						{
							Saver.LoadObjectSetup(file.FullName);
                        }
                    }
					GUILayout.Space(10);
				}

				GUILayout.Space(10);





				// list Bundles found and give load button
				foreach (FileInfo file in availableassetbundles)
                {
                    if (GUILayout.Button("Load now : " + file.Name))
                    {
						GUILayout.Space(10);
						AssetBundle bundle = AssetBundle.LoadFromFile(file.FullName);
						bundlesloaded.Add(bundle);

                    }

                    
                }

				GUILayout.Space(20);

				if (bundlesloaded != null)
                {
					GUILayout.Label(" Loaded Assets: ");
					GUILayout.Space(10);
					foreach (AssetBundle bundle in bundlesloaded)
                    {

						GameObject[] items = bundle.LoadAllAssets<GameObject>();
						foreach (GameObject item in items)
						{
							if (GUILayout.Button(item.name))
							{
								Destroy(Activeobj.gameObject);
								GameObject obj = bundle.LoadAsset(item.name) as GameObject;
								TargetObjectandInstatiate(obj);
							}
						}
					}
                }









				GUILayout.Space(20);
				if (GUILayout.Button("Return to game"))
            {
					Destroy(Activeobj);
					buildercam.enabled = false;
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
			Player = UnityEngine.GameObject.Find("BMXS Player Components");
			Player.SetActive(false);
			openflag = true;
			bundlesloaded = new List<AssetBundle>();

			if (inputman == null)
			{
				inputman = new MGInputManager();

			}

			if (buildercam != null)
			{
				buildercam.enabled = true;
			}

			if (camobj == null)
			{
				camobj = new GameObject();
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



		// called by clicking a button, button sends the gameobject its refering to
		void TargetObjectandInstatiate(GameObject obj)
        {
			
			Activeobj = GameObject.Instantiate(obj);
			
        }



		/// <summary>
		/// kills activeobj and makes given obj the target
		/// </summary>
		/// <param name="obj"></param>
		void TargetPlacedObject(GameObject obj)
        {
			if(Activeobj != null)
            {
				Destroy(Activeobj);
            }
				Activeobj = obj;

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

				camobj.transform.RotateAround(Activeobj.transform.position, Vector3.up, MGInputManager.RStickX());
				camobj.transform.RotateAround(Activeobj.transform.position, Vector3.right, MGInputManager.RStickY());



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

				Camdistance = Mathf.Clamp(Camdistance, 10, 100);
				Camdistance = Mathf.Lerp(Camdistance, Camdistance + (-MGInputManager.RStickY()), 1);

			}


			// if R Trigger is on
			if (MGInputManager.RTrigger() > 0.4 && MGInputManager.LTrigger() < 0.2f)
			{
				GUILayout.Label("Fine Tune Rotate Object -- Lstick to rotate Y, Rstick to rotate Z ");

				// fine tuning rotate
				Activeobj.transform.Rotate(0, MGInputManager.LStickY() * Time.deltaTime * camspeed.speeds[currentcamspeed].value, MGInputManager.RStickX() * Time.deltaTime * camspeed.speeds[currentcamspeed].value);


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




		// instatiates a clone of current object, adds PLACED to its name for later reference then re-targets activeobj to the live version
		void PlaceObject()
        {
			GameObject obj = GameObject.Instantiate(UnityEngine.GameObject.Find(Activeobj.name));
			obj.transform.DetachChildren();
			obj.name = Activeobj.gameObject.name + " PLACED";
			obj.transform.position = Activeobj.transform.position;
			obj.transform.rotation = Activeobj.transform.rotation;
			placedobjects.Add(obj);
			
			placedonce = true;

            // send to server if online
            if (InGameUI.instance.Connected)
            {
			
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


		void ShowCurrentBuild(string Buildname)
        {
			GameObject g = null;
			bool clicked = false;
			GUILayout.Space(50);
			GUILayout.Label("Placed Objects:");
			GUILayout.Space(20);
			foreach(GameObject obj in placedobjects)
            {
                if (GUILayout.Button(obj.name))
                {
					
					regrabbed_pos = obj.transform.position;
					regrabbed_rot = obj.transform.eulerAngles;
					REgrabbed = true;
					g = obj;
					clicked = true;
					
                }

            }

            if (clicked && g != null)
            {
				placedobjects.Remove(g);
				TargetPlacedObject(g);
			}

        }


		void ReplaceOnBButton()
        {
            if (MGInputManager.B_Down())
            {
				Activeobj.transform.position = regrabbed_pos;
				Activeobj.transform.eulerAngles = regrabbed_rot;
				Activeobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			REgrabbed = false;
            }
            if (MGInputManager.A_Down())
            {
				REgrabbed = false;
            }

        }

        #endregion



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

		public NetGameObject(string _nameofobject,string _nameoffile, string _nameofassetbundle,Vector3 _rotation,Vector3 _position, Vector3 _scale, bool _IsPhysicsenabled)
        {
			NameofObject = _nameofobject;
			NameOfFile = _nameoffile;
			NameofAssetBundle = _nameofassetbundle;
			Rotation = _rotation;
			Position = _position;
			Scale = _scale;
			IsPhysics = _IsPhysicsenabled;

        }


    }




}