using UnityEngine.UI;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;



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

      

		#region Toggles
		// Toggles
		bool texturetoggle;
		bool NormalToggle;
		bool materialstoggle;
        #endregion


		// Riders Texs
		public Texture2D[] Shirts;
		public Texture2D[] Bottoms;
		public Texture2D[] Hats;
		public Texture2D[] Shoes;
		public Texture2D[] Heads;
		public Texture2D[] Bodies;
		public Texture2D[] Hands_feet;
		public Texture2D savedTex;

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




		public string[] RiderPartnames = new string[]
		{
			"shirt_geo",
			"pants_geo",
			"hat_geo_",
			"shoes_geo",
			"Baseball Cap_R",
			"Daryien_Head",
			"Daryien_Body",
			"Daryien_HandsFeet"
		};


		
		



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
			else if (instance != this)
			{
				Debug.Log("character modding already exists, destroying old character modding now");
				Destroy(this);
			}
		}

		private void Start()
		{
			
			#region Setup Rider textures
			
			// support alpha channel
			GameObject.Find("pants_geo").GetComponent<SkinnedMeshRenderer>().material.EnableKeyword("_ALPHATEST_ON");
			GameObject.Find("shirt_geo").GetComponent<SkinnedMeshRenderer>().material.EnableKeyword("_ALPHATEST_ON");
			

			
			// grab riders renderers
			Rider_Materials.Add("Bottoms", GameObject.Find("pants_geo").GetComponent<SkinnedMeshRenderer>());
			Rider_Materials.Add("Shirt", GameObject.Find("shirt_geo").GetComponent<SkinnedMeshRenderer>());
			Rider_Materials.Add("Body", GameObject.Find("body_geo").GetComponent<SkinnedMeshRenderer>());
			Rider_Materials.Add("Cap", GameObject.Find("Baseball Cap_R").GetComponent<MeshRenderer>());
			Rider_Materials.Add("Shoes", GameObject.Find("shoes_geo").GetComponent<SkinnedMeshRenderer>());




				LoadRiderSetup();
			#endregion



		}

        /// <summary>
        /// Main loop ran by FrostyPGameManagers OnGUI(), this classes OnGUI() frees functions from boundaries of FrostyPGamemangers OnGUI() to allow images to display out the way of the manager
        /// </summary>
        public void Show()
		{

			

			GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 4, 70), new Vector2(Screen.width / 2, Screen.height / 20)), InGameUI.BoxStyle);
			GUILayout.Label("Rider Setup");
			GUILayout.Space(5);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Close"))
			{
				Shirts = null;
				Bottoms = null;
				Hats = null;
				Shoes = null;
				Heads = null;
				Bodies = null;
				Hands_feet = null;
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
				part.NormalTexs = null;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Body"];
				part.MatNum = 2;
				CurrentPart = part;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Head "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Head";
				part.BaseTexs = Heads;
				part.NormalTexs = null;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Body"];
				part.MatNum = 0;
				CurrentPart = part;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Body "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Body";
				part.BaseTexs = Bodies;
				part.NormalTexs = null;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Body"];
				part.MatNum = 1;
				CurrentPart = part;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Shirt "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Shirt";
				part.BaseTexs = Shirts;
				part.NormalTexs = null;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Shirt"];
				part.MatNum = 0;
				CurrentPart = part;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Bottoms "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Bottoms";
				part.BaseTexs = Bottoms;
				part.NormalTexs = null;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Bottoms"];
				part.MatNum = 0;
				CurrentPart = part;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Shoes "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Shoes";
				part.BaseTexs = Shoes;
				part.NormalTexs = null;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Shoes"];
				part.MatNum = 0;
				CurrentPart = part;
			}
			GUILayout.Space(2);
			if (GUILayout.Button(" Hat "))
			{
				CharacterPart part = new CharacterPart();
				part.DisplayName = "Cap";
				part.BaseTexs = Hats;
				part.NormalTexs = null;
				part.MaterialCount = 1;
				part.Renderer = Rider_Materials["Cap"];
				part.MatNum = 0;
				CurrentPart = part;
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
			RiderIsShowing = true;
		}

		void ShowPartMenu(CharacterPart part)
        {
			GUILayout.Label($"{part.DisplayName}");
			GUILayout.Space(10);
            if (GUILayout.Button("Remove"))
            {
				if(part.DisplayName != "Hands/Feet" && part.DisplayName != "Head" && part.DisplayName != "Body")
                {
				part.Renderer.material.mainTexture = null;
				part.Renderer.enabled = false;
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

		public void ShowATexButton(int texNum, CharacterPart part,bool isnormal)
		{
			GUIStyle style = new GUIStyle();
			style.fixedHeight = Screen.width/6;
			style.fixedWidth = Screen.width/6;

           
			   if (GUILayout.Button(part.BaseTexs[texNum], style))
			   {
				part.Renderer.enabled = true;
			    part.Renderer.materials[part.MatNum].mainTexture = part.BaseTexs[texNum];
					part.Renderer.materials[part.MatNum].color = new Color(1, 1, 1, 1);
			   }


		}

		public void RandomConfig(int texNum, Texture2D[] texArray, string name)
		{
		 Rider_Materials[name].material.mainTexture = texArray[texNum];
		 Rider_Materials[name].material.color = Color.white;
			
		}

		/// <summary>
		/// Takes a Directory and gives all textures in itself and all subs
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public Texture2D[] SearchDirectory(string filePath)
		{
			string[] files = Directory.GetFiles(filePath, "*.png", SearchOption.AllDirectories);
			
			Texture2D[] array = new Texture2D[files.Length];
			for (int i = 0; i < files.Length; i++)
			{
				FileInfo info = new FileInfo(files[i]);

				byte[] array2 = File.ReadAllBytes(files[i]);
				
				Texture2D texture2D = new Texture2D(1024, 1024);
				ImageConversion.LoadImage(texture2D, array2);
				texture2D.name = info.Name;
				array[i] = texture2D;

			}
			return array;
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
		public Texture2D[] BaseTexs;
		public Texture2D[] NormalTexs;
		public string DisplayName;
		public int MatNum;
		public bool isMetallic;
		public string[] Matnames;

    }

	/// <summary>
	/// Built when sending or receiving an Update to net
	/// </summary>
	public class GearUpdate
    {
		public bool isRiderUpdate;
		public List<TextureInfo> RiderTextures;
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
