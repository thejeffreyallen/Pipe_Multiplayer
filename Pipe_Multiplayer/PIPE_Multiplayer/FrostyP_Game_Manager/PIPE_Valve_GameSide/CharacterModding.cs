
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Rendering.PostProcessing;



namespace PIPE_Valve_Console_Client
{
	public class CharacterModding : MonoBehaviour
	{
		public static CharacterModding instance;
		public bool RiderIsShowing;
		
		// root directory
		public string Texturerootdir = Application.dataPath + "/FrostyPGameManager/Textures/";
        #region Riders Directories
        // directories of rider textures
        public string HeadDir = Application.dataPath + "/FrostyPGameManager/Textures/Head/";
		public string HatDir = Application.dataPath + "/FrostyPGameManager/Textures/Hat/";
		public string ShirtDir = Application.dataPath + "/FrostyPGameManager/Textures/Shirt/";
		public string BottomsDir = Application.dataPath + "/FrostyPGameManager/Textures/Pants/";
		public string ShoeDir = Application.dataPath + "/FrostyPGameManager/Textures/Shoes/";
		public string HandsFeetDir = Application.dataPath + "/FrostyPGameManager/Textures/Hands-Feet/";
		public string BodyDir = Application.dataPath + "/FrostyPGameManager/Textures/Body/";
		public string RidersaveDir = Application.dataPath + "/FrostyPGameManager/Textures/Save/";
		#endregion

		// Custom Tex's found
		public FileInfo[] Shirts;
		public FileInfo[] Bottoms;
		public FileInfo[] Hats;
		public FileInfo[] Shoes;
		public FileInfo[] Heads;
		public FileInfo[] Bodies;
		public FileInfo[] Hands_feet;
		public FileInfo savedTex;

		/// <summary>
		/// Riders renderers accesible by CharacterPart.DisplayName
		/// </summary>
		public Dictionary<string, Renderer> Rider_Materials = new Dictionary<string, Renderer>();
		/// <summary>
		/// Current part being shown and edited, changed by clicking a button
		/// </summary>
		CharacterPart CurrentPart;
		Vector2 TexScroll;

		// Random Choice numbers
		private float shirts;
		private float bottoms;
		private float hats;
		private float shoes;


		// Auto Cam movement
		public GameObject Cam;
		GameObject HatPos;
		GameObject ShirtPos;
		GameObject BottomsPos;
		GameObject ShoesPos;
		GameObject HeadPos;
		GameObject HandsfeetPos;
		GameObject BodyPos;
		Dictionary<int, GameObject> CamPositions;
		int ShowingCampos = 2;
		Dictionary<int, GameObject> LookPositions;
		GameObject Gamecam;

		/*
			"shirt_geo",
			"pants_geo",
			"hat_geo_",
			"shoes_geo",
			"Baseball Cap_R",
			"Daryien_Head",
			"Daryien_Body",
			"Daryien_HandsFeet"
		*/


		public Vector3 CapForwardPos;
		public Vector3 CapBackwardPos;
		public Vector3 CapForwardRot;
		public Vector3 CapBackwardRot;
		public bool CapisForward = true;



		void Awake()
        {
			// add all directories to list and create all if needed =================================================
			List<string> directories = new List<string>();
			
			directories.Add(HeadDir);
			directories.Add(HatDir);
			directories.Add(ShoeDir);
			directories.Add(ShirtDir);
			directories.Add(BottomsDir);
			directories.Add(HandsFeetDir);
			directories.Add(BodyDir);
			directories.Add(RidersaveDir);

			foreach (string s in directories)
			{
				if (!Directory.Exists(s))
				{
					Directory.CreateDirectory(s);

				}
			}

			// creates static reference to this class as their only need be 1.
			if (instance == null)
			{
				instance = this;
			}

			Cam = Instantiate(Camera.main.gameObject);
			Cam.SetActive(false);
			DontDestroyOnLoad(Cam);
			HatPos = new GameObject();
			ShirtPos = new GameObject();
			BottomsPos = new GameObject();
			ShoesPos = new GameObject();
			HeadPos = new GameObject();
			HandsfeetPos = new GameObject();
			BodyPos = new GameObject();
			DontDestroyOnLoad(HatPos);
			DontDestroyOnLoad(ShirtPos);
			DontDestroyOnLoad(BottomsPos);
			DontDestroyOnLoad(HeadPos);
			DontDestroyOnLoad(ShoesPos);
			DontDestroyOnLoad(HandsfeetPos);
			DontDestroyOnLoad(BodyPos);

			// grab riders renderers
			Rider_Materials.Add("Bottoms", GameObject.Find("pants_geo").GetComponent<SkinnedMeshRenderer>());
			Rider_Materials.Add("Shirt", GameObject.Find("shirt_geo").GetComponent<SkinnedMeshRenderer>());
			Rider_Materials.Add("Body", GameObject.Find("body_geo").GetComponent<SkinnedMeshRenderer>());
			Rider_Materials.Add("Cap", GameObject.Find("Baseball Cap_R").GetComponent<MeshRenderer>());
			Rider_Materials.Add("Shoes", GameObject.Find("shoes_geo").GetComponent<SkinnedMeshRenderer>());



		}

		private void Start()
		{
			// support alpha channel
			GameObject.Find("pants_geo").GetComponent<SkinnedMeshRenderer>().material.EnableKeyword("_ALPHATEST_ON");
			GameObject.Find("shirt_geo").GetComponent<SkinnedMeshRenderer>().material.EnableKeyword("_ALPHATEST_ON");

			
			CapBackwardPos = new Vector3(-0.0005f, 0.16f, 0.0235f);
			CapBackwardRot = new Vector3(-107.39f, -1.525879e-05f, 181.24f);
			CapForwardPos = new Vector3(-9.062365f, 0.165f, 0.025f);
			CapForwardRot = new Vector3(-107.39f, 0f,0f);

			LoadRiderSetup();

			CamPositions = new Dictionary<int, GameObject>
			{
				{1,HatPos },
				{2,ShirtPos},
				{3,BottomsPos },
				{4,ShoesPos },
				{5,HeadPos },
				{6,HandsfeetPos },
				{7,BodyPos },

			};
			LookPositions = new Dictionary<int, GameObject>
			{
				{1,LocalPlayer.instance.Riders_Transforms[22].gameObject },
				{2,LocalPlayer.instance.Riders_Transforms[9].gameObject},
				{3,LocalPlayer.instance.Riders_Transforms[4].gameObject },
				{4,LocalPlayer.instance.Riders_Transforms[6].gameObject },
				{5,LocalPlayer.instance.Riders_Transforms[22].gameObject },
				{6,LocalPlayer.instance.Riders_Transforms[17].gameObject },
				{7,LocalPlayer.instance.Riders_Transforms[7].gameObject },


				
			};
			

			HatPos.transform.position = LocalPlayer.instance.Riders_Transforms[22].position + (LocalPlayer.instance.Riders_Transforms[4].forward / 2) + (Vector3.up / 2);
			ShirtPos.transform.position = LocalPlayer.instance.Riders_Transforms[7].position + (LocalPlayer.instance.Riders_Transforms[0].forward * 1.2f) + (Vector3.up / 1.1f);
			BottomsPos.transform.position = LocalPlayer.instance.Riders_Transforms[4].position + (LocalPlayer.instance.Riders_Transforms[4].forward) + (Vector3.up / 2f);
			HeadPos.transform.position = LocalPlayer.instance.Riders_Transforms[22].position + (LocalPlayer.instance.Riders_Transforms[4].forward / 2) + (Vector3.up / 5);
			ShoesPos.transform.position = LocalPlayer.instance.Riders_Transforms[6].position + (LocalPlayer.instance.Riders_Transforms[6].forward / 2) + (Vector3.down/4) + (-LocalPlayer.instance.Riders_Transforms[6].right / 3);
			HandsfeetPos.transform.position = LocalPlayer.instance.Riders_Transforms[17].position + (LocalPlayer.instance.Riders_Transforms[17].forward / 2) + (Vector3.up) + (LocalPlayer.instance.Riders_Transforms[17].right / 3);
			BodyPos.transform.position = LocalPlayer.instance.Riders_Transforms[7].position + (LocalPlayer.instance.Riders_Transforms[0].forward * 1.7f) + (Vector3.up / 1.0f);

			HatPos.transform.parent = LocalPlayer.instance.Riders_Transforms[22];
			ShirtPos.transform.parent = LocalPlayer.instance.Riders_Transforms[7];
			BottomsPos.transform.parent = LocalPlayer.instance.Riders_Transforms[4];
			HeadPos.transform.parent = LocalPlayer.instance.Riders_Transforms[22];
			ShoesPos.transform.parent = LocalPlayer.instance.Riders_Transforms[6];
			HandsfeetPos.transform.parent = LocalPlayer.instance.Riders_Transforms[17];
			BodyPos.transform.parent = LocalPlayer.instance.Riders_Transforms[7];

			HatPos.SetActive(false);
			ShirtPos.SetActive(false);
			BottomsPos.SetActive(false);
			HeadPos.SetActive(false);
			ShoesPos.SetActive(false);
			HandsfeetPos.SetActive(false);
			BodyPos.SetActive(false);

		}

		void Update()
        {
            if (RiderIsShowing)
            {
			
				if(FrostyP_Game_Manager.FrostyPGamemanager.instance.MenuShowing != 1)
                {
					Close();
                }


				float angleX = -MGInputManager.RStickX() * 110;
				float angleY = MGInputManager.RStickY() * 110;
				Cam.transform.position = Vector3.Lerp(Cam.transform.position, CamPositions[ShowingCampos].transform.position,2 * Time.deltaTime);
				Cam.transform.LookAt(LookPositions[ShowingCampos].transform);
				if(MGInputManager.RStickX()>0.1f | MGInputManager.RStickY()>0.1f| MGInputManager.RStickX()<-0.1f| MGInputManager.RStickY() < -0.1f)
                {
				CamPositions[ShowingCampos].transform.RotateAround(LookPositions[ShowingCampos].transform.position, LocalPlayer.instance.Riders_Transforms[0].up, angleX * Time.deltaTime);
				CamPositions[ShowingCampos].transform.RotateAround(LookPositions[ShowingCampos].transform.position, Cam.transform.right, angleY * Time.deltaTime);

                }
			}
        }


        /// <summary>
        /// Main loop ran by FrostyPGameManagers OnGUI(), this classes OnGUI() frees functions from boundaries of FrostyPGamemangers OnGUI() to allow images to display out the way of the manager
        /// </summary>
        public void Show()
		{

			if (Camera.current.gameObject.GetComponent<PostProcessVolume>())
			{
				if (Camera.current.gameObject.GetComponent<PostProcessVolume>().profile.TryGetSettings(out DepthOfField DF))
				{
					DF.SetAllOverridesTo(false);
					DF.enabled.value = false;
				}
			}


			GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 4, 10 + (Screen.height/45)), new Vector2(Screen.width / 2, Screen.height / 20)), InGameUI.BoxStyle);
			GUILayout.Label("Rider Setup");
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Close"))
			{
				ShowingCampos = 2;
				Shirts = null;
				Bottoms = null;
				Hats = null;
				Shoes = null;
				Heads = null;
				Bodies = null;
				Hands_feet = null;
				Cam.SetActive(false);
				FrostyP_Game_Manager.FrostyPGamemanager.instance.MenuShowing = 0;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Load Last Setup "))
			{
				LoadRiderSetup();

			}
			GUILayout.Space(2);
			if (GUILayout.Button("Save rider"))
			{
				if(LocalPlayer.instance.RiderModelname == "Daryien")
                {
				SaveRiderSetup();

                }
                if (InGameUI.instance.Connected && LocalPlayer.instance.RiderModelname == "Daryien")
                {
					ClientSend.SendGearUpdate(GameManager.instance.GetMyGear(true));
                }
			}
			GUILayout.Space(2);
			if (GUILayout.Button("Random generate"))
			{
				shirts = Random.Range(0f, (float)Shirts.Length);
				bottoms = Random.Range(0f, (float)Bottoms.Length);
				hats = Random.Range(0f, (float)Hats.Length);
				shoes = Random.Range(0f, (float)Shoes.Length);
				RandomConfig((int)shirts, Shirts, "Shirt");
				RandomConfig((int)bottoms, Bottoms, "Bottoms");
				RandomConfig((int)hats, Hats, "Cap");
				RandomConfig((int)shoes, Shoes, "Shoes");
			}
				
					
					
				
			GUILayout.Space(10);

			if (GUILayout.Button(" Hands/Feet "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Hands/Feet";
				part.BaseTexs = Hands_feet;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Body"];
				part.MatNum = 2;
				CurrentPart = part;
				ShowingCampos = 6;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Head "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Head";
				part.BaseTexs = Heads;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Body"];
				part.MatNum = 0;
				CurrentPart = part;
				ShowingCampos = 5;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Body "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Body";
				part.BaseTexs = Bodies;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Body"];
				part.MatNum = 1;
				CurrentPart = part;
				ShowingCampos = 7;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Shirt "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Shirt";
				part.BaseTexs = Shirts;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Shirt"];
				part.MatNum = 0;
				CurrentPart = part;
				ShowingCampos = 2;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Bottoms "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Bottoms";
				part.BaseTexs = Bottoms;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Bottoms"];
				part.MatNum = 0;
				CurrentPart = part;
				ShowingCampos = 3;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Shoes "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Shoes";
				part.BaseTexs = Shoes;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Shoes"];
				part.MatNum = 0;
				CurrentPart = part;
				ShowingCampos = 4;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Hat "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Cap";
				part.BaseTexs = Hats;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Cap"];
				part.MatNum = 0;
				CurrentPart = part;
				ShowingCampos = 1;
			}
			GUILayout.Space(2);
			
			GUILayout.EndHorizontal();
			GUILayout.EndArea();


			if (CurrentPart != null)
			{
				GUILayout.BeginArea(new Rect(new Vector2(50, 150), new Vector2(Screen.width / 5, Screen.height - 300)), InGameUI.BoxStyle);
				ShowPartMenu(CurrentPart);
				GUILayout.EndArea();
			}

			GUILayout.Space(20);
		}
		public void RiderSetupOpen()
		{
			Shirts = SearchDirectory(Texturerootdir + "Shirt");
			Bottoms = SearchDirectory(Texturerootdir + "Pants");
			Hats = SearchDirectory(Texturerootdir + "Hat");
			Shoes = SearchDirectory(Texturerootdir + "Shoes");
			Heads = SearchDirectory(Texturerootdir + "Head");
			Bodies = SearchDirectory(Texturerootdir + "Body");
			Hands_feet = SearchDirectory(Texturerootdir + "Hands-Feet");
			FrostyP_Game_Manager.FrostyPGamemanager.instance.MenuShowing = 1;
			
			Cam.SetActive(true);
			HatPos.SetActive(true);
			ShirtPos.SetActive(true);
			BottomsPos.SetActive(true);
			HeadPos.SetActive(true);
			ShoesPos.SetActive(true);
			HandsfeetPos.SetActive(true);
			BodyPos.SetActive(true);
			Cam.transform.position = Camera.main.gameObject.transform.position;
			Gamecam = Camera.main.gameObject;
			Gamecam.SetActive(false);
			RiderIsShowing = true;
           


		}

		public void Close()
        {
			HatPos.SetActive(false);
			ShirtPos.SetActive(false);
			BottomsPos.SetActive(false);
			Cam.SetActive(false);
			HeadPos.SetActive(false);
			ShoesPos.SetActive(false);
			HandsfeetPos.SetActive(false);
			BodyPos.SetActive(false);
			Gamecam.SetActive(true);
			RiderIsShowing = false;
        }

		void ShowPartMenu(CharacterPart part)
        {
			GUILayout.Label($"{part.DisplayName}");
			GUILayout.Space(10);

			if (part.DisplayName != "Hands/Feet" && part.DisplayName != "Head" && part.DisplayName != "Body" && part.DisplayName != "Cap")
			{
				if (GUILayout.Button("Remove"))
				{
					part.Renderer.material.mainTexture = null;
					part.Renderer.enabled = false;
				}
			}
			if(part.DisplayName == "Cap")
            {
                if (GUILayout.Button("Flip"))
                {
					FlipCap(CapisForward);
                }
            }



				TexScroll = GUILayout.BeginScrollView(TexScroll);
				GUILayout.Space(20);
				for (int i = 0; i < part.BaseTexs.Length; i++)
				{
					ShowATexButton(i, part, false);
					GUILayout.Space(5);
				}
				GUILayout.Space(20);
				GUILayout.EndScrollView();

			
		}

		public void FlipCap(bool forward)
        {
			CapBackwardPos = new Vector3(-0.0005f, 0.16f, 0.0235f);
			CapBackwardRot = new Vector3(-107.39f, -1.525879e-05f, 181.24f);
			CapForwardPos = new Vector3(-0.0005f, 0.165f, 0.025f);
			CapForwardRot = new Vector3(-107.39f, 0f, 0f);


			if (forward)
            {
				Rider_Materials["Cap"].gameObject.transform.localPosition = CapBackwardPos;
				Rider_Materials["Cap"].gameObject.transform.localEulerAngles = CapBackwardRot;
				CapisForward = false;
			}
            else
            {
				Rider_Materials["Cap"].gameObject.transform.localPosition = CapForwardPos;
				Rider_Materials["Cap"].gameObject.transform.localEulerAngles = CapForwardRot;
				CapisForward = true;
			}
        }
		public void FlipCap(bool forward,RemotePlayer player)
		{
			CapBackwardPos = new Vector3(-0.0005f, 0.16f, 0.0235f);
			CapBackwardRot = new Vector3(-107.39f, -1.525879e-05f, 181.24f);
			CapForwardPos = new Vector3(-0.0005f, 0.165f, 0.025f);
			CapForwardRot = new Vector3(-107.39f, 0f, 0f);


			if (forward)
			{
				player.RiderModel.transform.FindDeepChild("Baseball Cap_R").localPosition = CapBackwardPos;
				player.RiderModel.transform.FindDeepChild("Baseball Cap_R").localEulerAngles = CapBackwardRot;
				
			}
			else
			{
				player.RiderModel.transform.FindDeepChild("Baseball Cap_R").localPosition = CapForwardPos;
				player.RiderModel.transform.FindDeepChild("Baseball Cap_R").localEulerAngles = CapForwardRot;
				
			}
		}

		public void ShowATexButton(int texNum, CharacterPart part,bool isnormal)
		{
			
			   if (GUILayout.Button(part.BaseTexs[texNum].Name))
			   {
				part.Renderer.enabled = true;
				part.Renderer.materials[part.MatNum].EnableKeyword("_ALPHATEST_ON");
				part.Renderer.materials[part.MatNum].mainTexture = SetTex(part.BaseTexs[texNum]);
				part.Renderer.materials[part.MatNum].color = new Color(1, 1, 1, 1);
			   }


		}

		public void RandomConfig(int texNum, FileInfo[] texArray, string name)
		{
		 Rider_Materials[name].material.mainTexture = SetTex(texArray[texNum]);
		 Rider_Materials[name].material.color = Color.white;
			
		}

		/// <summary>
		/// Takes a Directory and gives all textures in itself and all subs
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public FileInfo[] SearchDirectory(string filePath)
		{
			FileInfo[] files = new DirectoryInfo(filePath).GetFiles();
			
			return files;
		}

		public Texture2D SetTex(FileInfo file)
        {
		  Texture2D tex = new Texture2D(1024, 1024);
			byte[] bytes = File.ReadAllBytes(file.FullName);
		  ImageConversion.LoadImage(tex, bytes);
		  tex.name = file.Name;
			return tex;

			
		}

		public void SaveRiderSetup()
        {
			RiderSaveData data = new RiderSaveData();

            if (Rider_Materials["Shirt"].material.mainTexture)
            {
			data.Shirtbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["Shirt"].material.mainTexture);
			data.shirtimagename = Rider_Materials["Shirt"].material.mainTexture.name;
			data.shirtParentname = "Shirt";
            }
            else
            {
				data.Shirtbytes = new byte[1];
				data.shirtimagename = "e";
				data.shirtParentname = "Shirt";
			}

            if (Rider_Materials["Bottoms"].material.mainTexture)
            {
			data.bottomsbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["Bottoms"].material.mainTexture);
			data.bottomsimagename = Rider_Materials["Bottoms"].material.mainTexture.name;
			data.bottomsParentname = "Bottoms";
            }
            else
            {
				data.bottomsbytes = new byte[1];
				data.bottomsimagename = "e";
				data.bottomsParentname = "Bottoms";
			}

            if (Rider_Materials["Shoes"].material.mainTexture)
            {
			data.shoesbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["Shoes"].material.mainTexture);
			data.shoesimagename = Rider_Materials["Shoes"].material.mainTexture.name;
			data.shoesParentname = "Shoes";
            }
            else
            {
				data.shoesbytes = new byte[1];
				data.shoesimagename = "e";
				data.shoesParentname = "Shoes";
			}

            if (Rider_Materials["Cap"].material.mainTexture)
            {
			data.hatbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["Cap"].material.mainTexture);
			data.hatimagename = Rider_Materials["Cap"].material.mainTexture.name;
			data.hatParentname = "Cap";
            }
            else
            {
				data.hatbytes = new byte[1];
				data.hatimagename = "e";
				data.hatParentname = "Cap";
			}

            if (Rider_Materials["Body"].materials[0].mainTexture)
            {
			data.headbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["Body"].materials[0].mainTexture);
			data.headimagename = Rider_Materials["Body"].materials[0].mainTexture.name;
			data.headParentname = "Body";
            }
            else
            {
				data.headbytes = new byte[1];
				data.headimagename = "e";
				data.headParentname = "Body";
			}

            if (Rider_Materials["Body"].materials[1].mainTexture)
            {
			data.bodybytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["Body"].materials[1].mainTexture);
			data.bodyimagename = Rider_Materials["Body"].materials[1].mainTexture.name;
			data.bodyParentname = "Body";
            }
            else
            {
				data.bodybytes = new byte[1];
				data.bodyimagename = "e";
				data.bodyParentname = "Body";
			}

            if (Rider_Materials["Body"].materials[2].mainTexture)
            {
			data.handsfeetbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["Body"].materials[2].mainTexture);
			data.handsfeetimagename = Rider_Materials["Body"].materials[2].mainTexture.name;
			data.handsfeetParentname = "Body";
            }
            else
            {
				data.handsfeetbytes = new byte[1];
				data.handsfeetimagename = "e";
				data.handsfeetParentname = "Body";
			}

			data.CapisForward = CapisForward;

			if (!Directory.Exists(RidersaveDir))
			{
				Directory.CreateDirectory(RidersaveDir);
			}



			


			BinaryFormatter bf = new BinaryFormatter();
			Stream stream = File.OpenWrite(RidersaveDir + "RiderSave.FrostyPreset");
			bf.Serialize(stream, data);
			stream.Close();

			Debug.Log("Completed Saving Rider");



		}
		public int LoadRiderSetup()
        {
			
			RiderSaveData load;

			if (File.Exists(RidersaveDir + "RiderSave.FrostyPreset"))
            {
			BinaryFormatter bf = new BinaryFormatter();
				Stream stream = File.OpenRead(RidersaveDir + "RiderSave.FrostyPreset");
			load = (RiderSaveData)bf.Deserialize(stream);
				stream.Close();
			


			Texture2D Shirttex = new Texture2D(1024, 1024);
			Texture2D bottomstex = new Texture2D(1024, 1024);
			Texture2D shoestex = new Texture2D(1024, 1024);
			Texture2D hattex = new Texture2D(1024, 1024);
			Texture2D headtex = new Texture2D(1024, 1024);
			Texture2D bodytex = new Texture2D(1024, 1024);
			Texture2D handsfeettex = new Texture2D(1024, 1024);

                if (load.Shirtbytes.Length > 1)
                {
			     ImageConversion.LoadImage(Shirttex, load.Shirtbytes);
                }
                if (load.bottomsbytes.Length > 1)
                {
			     ImageConversion.LoadImage(bottomstex, load.bottomsbytes);
                }
                if (load.shoesbytes.Length > 1)
                {
			     ImageConversion.LoadImage(shoestex, load.shoesbytes);
                }
                if (load.hatbytes.Length > 1)
                {
			     ImageConversion.LoadImage(hattex, load.hatbytes);
                }
                if (load.headbytes.Length > 1)
                {
			     ImageConversion.LoadImage(headtex, load.headbytes);
                }
                if (load.bodybytes.Length > 1)
                {
			     ImageConversion.LoadImage(bodytex, load.bodybytes);
                }
                if (load.handsfeetbytes.Length > 1)
                {
			     ImageConversion.LoadImage(handsfeettex, load.handsfeetbytes);
                }


			  Shirttex.name = load.shirtimagename;
			  bottomstex.name = load.bottomsimagename;
			  shoestex.name = load.shoesimagename;
			  headtex.name = load.headimagename;
			  bodytex.name = load.bodyimagename;
			  handsfeettex.name = load.handsfeetimagename;
			  hattex.name = load.hatimagename;


				if(Shirttex.name != "e")
                {
			     Rider_Materials["Shirt"].material.mainTexture = Shirttex;
                }
                else
                {
				 Rider_Materials["Shirt"].material.mainTexture = null;
				 Rider_Materials["Shirt"].enabled = false;
				}

				if(bottomstex.name != "e")
                {
			     Rider_Materials["Bottoms"].material.mainTexture = bottomstex;
                }
                else
                {
					Rider_Materials["Bottoms"].material.mainTexture = null;
					Rider_Materials["Bottoms"].enabled = false;
				}

				if(shoestex.name != "e")
                {
			     Rider_Materials["Shoes"].material.mainTexture = shoestex;
                }
                else
                {
					Rider_Materials["Shoes"].material.mainTexture = null;
					Rider_Materials["Shoes"].enabled = false;
				}

				if(hattex.name != "e")
                {
			     Rider_Materials["Cap"].material.mainTexture = hattex;
	             Rider_Materials["Cap"].material.color = Color.white;
                }
                else
                {
					Rider_Materials["Cap"].material.mainTexture = null;
					Rider_Materials["Cap"].enabled = false;
				}

				if(headtex.name != "e")
                {
			     Rider_Materials["Body"].materials[0].mainTexture = headtex;
                }
                else
                {
					Rider_Materials["Body"].materials[0].mainTexture = null;
					Rider_Materials["Body"].enabled = false;
				}

				if(bodytex.name != "e")
                {
			     Rider_Materials["Body"].materials[1].mainTexture = bodytex;
                }
                else
                {
					Rider_Materials["Body"].materials[1].mainTexture = null;
					Rider_Materials["Body"].enabled = false;

				}

				if(handsfeettex.name != "e")
                {
			     Rider_Materials["Body"].materials[2].mainTexture = handsfeettex;
                }
                else
                {
					Rider_Materials["Body"].materials[2].mainTexture = null;
					Rider_Materials["Body"].enabled = false;
				}

                try
                {
				FlipCap(!load.CapisForward);
                }
                catch (System.Exception)
                {

                }



			return 1;
            }
            else
            {
				return 0;
            }
			






		}

    }



	public class CharacterPart
    {
		public int MaterialCount;
		public Renderer Renderer;
		public FileInfo[] BaseTexs;
		public string DisplayName;
		public int MatNum;
		public string[] Matnames;

    }

	/// <summary>
	/// Built when sending or receiving an Update to net
	/// </summary>
	public class GearUpdate
    {
		public bool isRiderUpdate;
		public List<TextureInfo> RiderTextures;
		public bool Capisforward;
		public byte[] GarageSave;



		/// <summary>
		/// to Send Just Riders gear
		/// </summary>
		/// <param name="ridertextures"></param>
		public GearUpdate(List<TextureInfo> ridertextures)
        {
			isRiderUpdate = true;
			RiderTextures = ridertextures;

        }

		public GearUpdate()
        {
        }

	}

	/// <summary>
	/// Used for keeping track of texture name and the gameobject its on for when it reaches remote players
	/// </summary>
	public class TextureInfo
	{
		public string Nameoftexture;
		public string NameofparentGameObject;
		public bool isNormal;
		public int Matnum;

		public TextureInfo(string nameoftex, string nameofG_O, bool isnormal, int matnum)
		{
			Nameoftexture = nameoftex;
			NameofparentGameObject = nameofG_O;
			isNormal = isnormal;
			Matnum = matnum;
		}


	}


}
