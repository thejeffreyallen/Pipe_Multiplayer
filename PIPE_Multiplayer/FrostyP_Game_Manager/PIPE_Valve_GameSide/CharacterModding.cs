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

		// root directory
		public string Texturerootdir = Application.dataPath + "/FrostyPGameManager/Textures/";
        #region Riders Directories
        // directories of rider textures
        public string HeadDir = Application.dataPath + "/FrostyPGameManager/Textures/Head";
		public string HatDir = Application.dataPath + "/FrostyPGameManager/Textures/Head";
		public string ShirtDir = Application.dataPath + "/FrostyPGameManager/Textures/Shirt";
		public string BottomsDir = Application.dataPath + "/FrostyPGameManager/Textures/Pants";
		public string ShoeDir = Application.dataPath + "/FrostyPGameManager/Textures/Shoes";
		public string HandsFeetDir = Application.dataPath + "/FrostyPGameManager/Textures/Hands-Feet";
		public string BodyDir = Application.dataPath + "/FrostyPGameManager/Textures/Body";
        #endregion

        #region Toggles
        // Toggles
        bool headtoggle;
		bool hattoggle;
		bool shirttoggle;
		bool bottomstoggle;
		bool shoetoggle;
		bool handsfeettoggle;
		bool bodytoggle;
		bool frametoggle;
		bool forkstoggle;
		bool seattoggle;
		bool wheelstoggle;
		bool barstoggle;
		bool texturetoggle;
		bool materialstoggle;
        #endregion


        #region Bikes Directories
        // directories of Bmx textures
        public GameObject Bmx;
		public string FrameDir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/Frames/";
		public string ForksDir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/Forks/";
		public string BarsDir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/Bars/";
		public string TiresDir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/Tires/";
		public string SeatDir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/Seats/";
		public string BmxSaveDir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/Saved/";
        #endregion

        float imagesidebuffer = Screen.width - 200f;
		float imagewidth = 150;



		// Riders Texs
		private Texture2D[] Shirts;
		private Texture2D[] Bottoms;
		private Texture2D[] Hats;
		private Texture2D[] Shoes;
		private Texture2D[] Heads;
		private Texture2D[] Bodies;
		private Texture2D[] Hands_feet;
		public Texture2D savedTex;

		SkinnedMeshRenderer shirtren;
		SkinnedMeshRenderer bottomsren;
		SkinnedMeshRenderer hatren;
		SkinnedMeshRenderer shoesren;
		SkinnedMeshRenderer headren;
		SkinnedMeshRenderer bodyren;
		SkinnedMeshRenderer handsfeetren;




		// Bmx Tex
		private Texture2D[] Frames;
		private Texture2D[] Forks;
		private Texture2D[] Seats;
		private Texture2D[] Bars;
		private Texture2D[] Tires;
		public Dictionary<string, Texture2D> currentTexs;
		/// <summary>
		/// bikes materials accessible via gameobject name
		/// </summary>
		public Dictionary<string, MeshRenderer> BMX_Materials = new Dictionary<string, MeshRenderer>();
		/// <summary>
		/// Riders materials accesible by Gameobject name
		/// </summary>
		public Dictionary<string, SkinnedMeshRenderer> Rider_Materials = new Dictionary<string, SkinnedMeshRenderer>();
		Texture2D[] Currentshowingarray;
		string currentarrayname;
		   public MeshRenderer FrameRen;
		public	MeshRenderer ForksRen;
			public MeshRenderer BarsRen;
			public MeshRenderer SeatRen;
			public MeshRenderer FTireRen;
		    public MeshRenderer RTireRen;


		// if theres more than one material being edited at once
		float _r;
		float _g;
		float _b;
		float _r2;
		float _g2;
		float _b2;
		float _r3;
		float _g3;
		float _b3;
		float _r4;
		float _g4;
		float _b4;
		float smoothness;
		float smoothness2;
		



		public Vector2 scrollPosition;

		private bool _Random;
		private float shirts;
		private float bottoms;
		private float hats;
		private float shoes;


		private Material[] skinMats;



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


		public string savePath;
		public bool isSkin;



		private void Start()
		{
			// add all directories to list and create all if needed =================================================
			List<string> directories = new List<string>();
			directories.Add(FrameDir);
			directories.Add(ForksDir);
			directories.Add(SeatDir);
			directories.Add(BarsDir);
			directories.Add(TiresDir);
			directories.Add(HeadDir);
			directories.Add(HatDir);
			directories.Add(ShoeDir);
			directories.Add(ShirtDir);
			directories.Add(BottomsDir);
			directories.Add(HandsFeetDir);
			directories.Add(BodyDir);

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

			#region Setup Rider textures
			// grab all clothing in folders
			Shirts = SearchDirectory(Texturerootdir + "Shirt");
			Bottoms = SearchDirectory(Texturerootdir + "Pants");
			Hats = SearchDirectory(Texturerootdir + "Hat");
			Shoes = SearchDirectory(Texturerootdir + "Shoes");
			Heads = SearchDirectory(Texturerootdir + "Head");
			Bodies = SearchDirectory(Texturerootdir + "Body");
			Hands_feet = SearchDirectory(Texturerootdir + "Hands-Feet");
			// support alpha channel
			GameObject.Find("pants_geo").GetComponent<SkinnedMeshRenderer>().material.EnableKeyword("_ALPHATEST_ON");
			GameObject.Find("shirt_geo").GetComponent<SkinnedMeshRenderer>().material.EnableKeyword("_ALPHATEST_ON");
			skinMats = GameObject.Find("body_geo").GetComponent<SkinnedMeshRenderer>().materials;

			// load last used
			if (!Directory.Exists(Application.dataPath + "/Textures/Save/"))
			{
				Directory.CreateDirectory(Application.dataPath + "/Textures/Save/");
			}
			for (int i = 0; i < RiderPartnames.Length; i++)
			{
				savePath = Application.dataPath + "//Textures/Save/" + RiderPartnames[i] + ".png";
				LoadRiderSetup(RiderPartnames[i], savePath);
			}

			

			#endregion

			#region Setup Bmx Textures

			currentTexs = new Dictionary<string, Texture2D>();

			Bmx = UnityEngine.GameObject.Find("BMX");
			Frames = SearchDirectory(FrameDir);
			Forks = SearchDirectory(ForksDir);
			Seats = SearchDirectory(SeatDir);
			Tires = SearchDirectory(TiresDir);
			Bars = SearchDirectory(BarsDir);

			FrameRen = UnityEngine.GameObject.Find("BMX:Frame_Joint").transform.Find("Frame Mesh").gameObject.GetComponent<MeshRenderer>();
			ForksRen = UnityEngine.GameObject.Find("Forks Mesh").GetComponent<MeshRenderer>();
			BarsRen = UnityEngine.GameObject.Find("Bars Mesh").GetComponent<MeshRenderer>();
			SeatRen = UnityEngine.GameObject.Find("Seat Mesh").GetComponent<MeshRenderer>();
			FTireRen = UnityEngine.GameObject.Find("BMX:Wheel").transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
			RTireRen = UnityEngine.GameObject.Find("BMX:Wheel 1").transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();



			BMX_Materials = new Dictionary<string, MeshRenderer>();
			BMX_Materials.Add("framemesh",FrameRen);
			BMX_Materials.Add("Forks Mesh",ForksRen);
			BMX_Materials.Add("Seat Mesh",SeatRen);
			BMX_Materials.Add("Bars Mesh",BarsRen);
			BMX_Materials.Add("Tire Mesh Front",FTireRen);
			BMX_Materials.Add("Tire Mesh Back",RTireRen);
			LoadBmxSetup();

			// to be done

			//BMX_Materials.Add("Spokes Mesh", UnityEngine.GameObject.Find("Spokes Mesh").GetComponent<MeshRenderer>());
			//BMX_Materials.Add("Stem Mesh", UnityEngine.GameObject.Find("Stem Mesh").GetComponent<MeshRenderer>());
			//BMX_Materials.Add("Rim Mesh", UnityEngine.GameObject.Find("Rim Mesh").GetComponent<MeshRenderer>());
			//BMX_Materials.Add("Hub Mesh", UnityEngine.GameObject.Find("Hub Mesh").GetComponent<MeshRenderer>());
			//BMX_Materials.Add("Nipple Mesh", UnityEngine.GameObject.Find("Nipple Mesh").GetComponent<MeshRenderer>());
		
			

			


			#endregion



		}


        /// <summary>
        /// Main loop ran by FrostyPGameManagers OnGUI(), this classes OnGUI() frees functions from boundaries of FrostyPGamemangers OnGUI() to allow images to display out the way of the manager
        /// </summary>
        public void RiderTexturesOpen()
		{
			GUILayout.Space(20);
			if (GUILayout.Button("Live Update on Net"))
			{
				GameManager.instance.SendQuickRiderUpdate();
			}
			GUILayout.Space(5);
			shirttoggle = GUILayout.Toggle(shirttoggle, " Shirts ");
			bottomstoggle = GUILayout.Toggle(bottomstoggle, " Bottoms ");
			hattoggle = GUILayout.Toggle(hattoggle, " Hats ");
			shoetoggle = GUILayout.Toggle(shoetoggle, " Shoes ");
			headtoggle = GUILayout.Toggle(headtoggle, " Heads ");
			bodytoggle = GUILayout.Toggle(bodytoggle,  " Bodies ");
			handsfeettoggle = GUILayout.Toggle(handsfeettoggle, " Hands/Feet ");

			
					
				
				if (GUILayout.Button("Random generate"))
				{
					_Random = !_Random;
				}
				if (_Random)
				{
					shirts = Random.Range(0f, (float)Shirts.Length);
					bottoms = Random.Range(0f, (float)Bottoms.Length);
					hats = Random.Range(0f, (float)Hats.Length);
					shoes = Random.Range(0f, (float)Shoes.Length);
					RandomConfig((int)shirts, Shirts, "shirt_geo");
					RandomConfig((int)bottoms, Bottoms, "pants_geo");
					RandomConfig((int)hats, Hats, "Baseball Cap_R");
					RandomConfig((int)shoes, Shoes, "shoes_geo");
					
					_Random = false;
				}
				
				
				


			GUILayout.Space(20);
		}


		public void BMXSetupOpen()
        {
           

			GUILayout.Space(20);
			// tabs
			if (GUILayout.Button(" Load Last Setup "))
			{
				LoadBmxSetup();

			}

			frametoggle = GUILayout.Toggle(frametoggle, " Frames ");
			forkstoggle = GUILayout.Toggle(forkstoggle, " Forks ");
			barstoggle = GUILayout.Toggle(barstoggle, " Bars ");
			seattoggle = GUILayout.Toggle(seattoggle, " Seat ");
			wheelstoggle = GUILayout.Toggle(wheelstoggle, " Wheels ");
			GUILayout.Space(5);
			if(GUILayout.Button(" Save Current Bike/LiveUpdate"))
            {
				SaveBmxSetup();
                if (InGameUI.instance.Connected)
                {
				GameManager.instance.SendQuickBikeUpdate();
                }
            }

			if (frametoggle)
            {
				GUILayout.Space(20);
				GUILayout.Label("Frame Settings:");
				texturetoggle = GUILayout.Toggle(texturetoggle, " Textures ");
                if (texturetoggle)
                {
					if(GUILayout.Button("Remove Texture"))
                    {
						FrameRen.material.mainTexture = null;
						BMXNetLoadout.instance.FrameTex = new byte[0];
						BMXNetLoadout.instance.FrameTexname = "";
					}
					Currentshowingarray = Frames;
					currentarrayname = "Frames";
                }

				materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
                if (materialstoggle)
                {
					Renderer m = FrameRen;

					
					GUILayout.Label("RGB");
					_r = GUILayout.HorizontalSlider(_r, 0, 1);
					_g = GUILayout.HorizontalSlider(_g, 0, 1);
					_b = GUILayout.HorizontalSlider(_b, 0, 1);
					m.material.color = new Color(_r, _g, _b);

					GUILayout.Label("Smoothness");
					smoothness = GUILayout.HorizontalSlider(smoothness, 0, 1);
					m.material.SetInt("_SmoothnessTextureChannel", 0);
					m.material.SetFloat("_Metallic", 0.27f);
					m.material.SetFloat("_Glossiness", smoothness);
					BMXNetLoadout.instance.FrameColour.x = _r;
					BMXNetLoadout.instance.FrameColour.y = _g;
					BMXNetLoadout.instance.FrameColour.z = _b;
					BMXNetLoadout.instance.FrameSmooth = smoothness;
				}
				GUILayout.Space(20);
			}
			if (forkstoggle)
			{
				GUILayout.Space(20);
					GUILayout.Label("Forks Settings:");
				texturetoggle = GUILayout.Toggle(texturetoggle, " Textures ");
				if (texturetoggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						BMX_Materials["Forks Mesh"].material.mainTexture = null;
						BMXNetLoadout.instance.ForksTex = new byte[0];
						BMXNetLoadout.instance.ForkTexname = "";
					}
					currentarrayname = "Forks";
					Currentshowingarray = Forks;
				}
				materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
				if (materialstoggle)
				{
					Renderer m = BMX_Materials["Forks Mesh"];

					GUILayout.Label("RGB");
					_r = GUILayout.HorizontalSlider(_r, 0, 1);
					_g = GUILayout.HorizontalSlider(_g, 0, 1);
					_b = GUILayout.HorizontalSlider(_b, 0, 1);
					m.material.color = new Color(_r, _g, _b);

					GUILayout.Label("Smoothness");
					smoothness = GUILayout.HorizontalSlider(smoothness, 0, 1);
					m.material.SetInt("_SmoothnessTextureChannel", 0);
					m.material.SetFloat("_Metallic", 0.27f);
					m.material.SetFloat("_Glossiness", smoothness);
					BMXNetLoadout.instance.ForksColour.x = _r;
					BMXNetLoadout.instance.ForksColour.y = _g;
					BMXNetLoadout.instance.ForksColour.z = _b;
					BMXNetLoadout.instance.ForksSmooth = smoothness;

				}
				GUILayout.Space(20);
			}
			if (barstoggle)
			{
				GUILayout.Space(20);
					GUILayout.Label("Bars Settings:");
				texturetoggle = GUILayout.Toggle(texturetoggle, " Textures ");
				if (texturetoggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						BMX_Materials["Bars Mesh"].material.mainTexture = null;
						BMXNetLoadout.instance.BarsTex = new byte[0];
						BMXNetLoadout.instance.BarTexName = "";
					}
					currentarrayname = "Bars";
					Currentshowingarray = Bars;
				}
				materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
				if (materialstoggle)
				{
					Renderer m = BMX_Materials["Bars Mesh"];

					GUILayout.Label("RGB");
					_r = GUILayout.HorizontalSlider(_r, 0, 1);
					_g = GUILayout.HorizontalSlider(_g, 0, 1);
					_b = GUILayout.HorizontalSlider(_b, 0, 1);
					m.material.color = new Color(_r, _g, _b);

					GUILayout.Label("Smoothness");
					smoothness = GUILayout.HorizontalSlider(smoothness, 0, 1);
					m.material.SetInt("_SmoothnessTextureChannel", 0);
					m.material.SetFloat("_Metallic", 0.2f);
					m.material.SetFloat("_Glossiness", smoothness);
					BMXNetLoadout.instance.BarsColour.x = _r;
					BMXNetLoadout.instance.BarsColour.y = _g;
					BMXNetLoadout.instance.BarsColour.z = _b;
					BMXNetLoadout.instance.BarsSmooth = smoothness;
				}

				GUILayout.Space(20);
			}
			if (seattoggle)
			{
				GUILayout.Space(20);
					GUILayout.Label("Seat Settings:");
				texturetoggle = GUILayout.Toggle(texturetoggle, " Textures ");
				if (texturetoggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						BMX_Materials["Seat Mesh"].material.mainTexture = null;
						BMXNetLoadout.instance.SeatTex = new byte[0];
						BMXNetLoadout.instance.SeatTexname = "";
					}
					currentarrayname = "Seats";
					Currentshowingarray = Seats;
				}
				materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
				if (materialstoggle)
				{
					Renderer m = BMX_Materials["Seat Mesh"];

					GUILayout.Label("RGB");
					_r = GUILayout.HorizontalSlider(_r, 0, 1);
					_g = GUILayout.HorizontalSlider(_g, 0, 1);
					_b = GUILayout.HorizontalSlider(_b, 0, 1);
					m.material.color = new Color(_r, _g, _b);

					GUILayout.Label("Smoothness");
					smoothness = GUILayout.HorizontalSlider(smoothness, 0, 1);
					m.material.SetInt("_SmoothnessTextureChannel", 0);
					m.material.SetFloat("_Metallic", 0.27f);
					m.material.SetFloat("_Glossiness", smoothness);
					BMXNetLoadout.instance.SeatColour.x = _r;
					BMXNetLoadout.instance.SeatColour.y = _g;
					BMXNetLoadout.instance.SeatColour.z = _b;
					BMXNetLoadout.instance.SeatSmooth = smoothness;
				}

				GUILayout.Space(20);
			}
			if (wheelstoggle)
			{
				GUILayout.Space(20);
				texturetoggle = GUILayout.Toggle(texturetoggle, " Textures ");
				if (texturetoggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						BMX_Materials["Tire Mesh Front"].materials[0].mainTexture = null;
						BMX_Materials["Tire Mesh Front"].materials[1].mainTexture = null;
						BMX_Materials["Tire Mesh Back"].materials[0].mainTexture = null;
						BMX_Materials["Tire Mesh Back"].materials[1].mainTexture = null;
						BMXNetLoadout.instance.TiresTex = new byte[0];
						BMXNetLoadout.instance.TireTexName = "";
					}
					currentarrayname = "Tires";
					Currentshowingarray = Tires;
				}
				materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
				if (materialstoggle)
				{
					Renderer m = BMX_Materials["Tire Mesh Front"];
					//m.material.mainTexture = null;
					GUILayout.Label("Front Tire:");
					GUILayout.Space(5);
					GUILayout.Label("RGB");
					GUILayout.Label("Top:");
					_r = GUILayout.HorizontalSlider(_r, 0, 1);
					_g = GUILayout.HorizontalSlider(_g, 0, 1);
					_b = GUILayout.HorizontalSlider(_b, 0, 1);
					m.materials[0].color = new Color(_r, _g, _b);
					GUILayout.Space(5);
					GUILayout.Label("Sidewall:");
					_r2 = GUILayout.HorizontalSlider(_r2, 0, 1);
					_g2 = GUILayout.HorizontalSlider(_g2, 0, 1);
					_b2 = GUILayout.HorizontalSlider(_b2, 0, 1);
					m.materials[1].color = new Color(_r2, _g2, _b2);
					GUILayout.Space(10);
					GUILayout.Label("Smoothness");
					smoothness = GUILayout.HorizontalSlider(smoothness, 0, 1);
					m.material.SetInt("_SmoothnessTextureChannel", 0);
					m.material.SetFloat("_Metallic", 0.27f);
					m.material.SetFloat("_Glossiness", smoothness);

					Renderer _m = BMX_Materials["Tire Mesh Back"];
					//_m.material.mainTexture = null;
					GUILayout.Label("Back Tire:");
					GUILayout.Space(5);
					GUILayout.Label("RGB");
					GUILayout.Label("Top:");
					_r3 = GUILayout.HorizontalSlider(_r3, 0, 1);
					_g3 = GUILayout.HorizontalSlider(_g3, 0, 1);
					_b3 = GUILayout.HorizontalSlider(_b3, 0, 1);
					_m.materials[0].color = new Color(_r3, _g3, _b3);
					GUILayout.Space(5);
					GUILayout.Label("Sidewall:");
					_r4 = GUILayout.HorizontalSlider(_r4, 0, 1);
					_g4 = GUILayout.HorizontalSlider(_g4, 0, 1);
					_b4 = GUILayout.HorizontalSlider(_b4, 0, 1);
					_m.materials[1].color = new Color(_r4, _g4, _b4);
					GUILayout.Space(10);
					GUILayout.Label("Smoothness");
					smoothness2 = GUILayout.HorizontalSlider(smoothness2, 0, 1);
					_m.material.SetInt("_SmoothnessTextureChannel", 0);
					_m.material.SetFloat("_Metallic", 0.27f);
					_m.material.SetFloat("_Glossiness", smoothness2);

					BMXNetLoadout.instance.FTireColour.x = _r;
					BMXNetLoadout.instance.FTireColour.y = _g;
					BMXNetLoadout.instance.FTireColour.z = _b;
					BMXNetLoadout.instance.FTireSideColour.x = _r2;
					BMXNetLoadout.instance.FTireSideColour.y = _g2;
					BMXNetLoadout.instance.FTireSideColour.z = _b2;


					BMXNetLoadout.instance.RTireColour.x = _r3;
					BMXNetLoadout.instance.RTireColour.y = _g3;
					BMXNetLoadout.instance.RTireColour.z = _b3;
					BMXNetLoadout.instance.RTireSideColour.x = _r4;
					BMXNetLoadout.instance.RTireSideColour.y = _g4;
					BMXNetLoadout.instance.RTireSideColour.z = _b4;

				}

				GUILayout.Space(20);
			}

			GUILayout.Space(20);
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

		
		public void ShowATexButton(int texNum, Texture2D[] texArray, string name)
		{
			GUIStyle style = new GUIStyle();
			style.fixedHeight = imagewidth;
			style.fixedWidth = imagewidth;
			if(GUILayout.Button(texArray[texNum], style))
            {
				
			if (name == "pants_geo")
			{
					GameObject g = UnityEngine.GameObject.Find("Skeleton").transform.GetChild(0).gameObject;
					g.transform.Find("pants_geo").gameObject.GetComponent<SkinnedMeshRenderer>().material.mainTexture = texArray[texNum];
					SaveTextures(texNum, texArray, name);
		    }
				if (name == "shirt_geo")
				{
					GameObject g = GameManager.instance._localplayer.Rider_Root;
					g.transform.Find(name).gameObject.GetComponent<SkinnedMeshRenderer>().material.mainTexture = texArray[texNum];
					SaveTextures(texNum, texArray, name);
				}
				if (name == "shoes_geo")
				{
					GameObject g = GameManager.instance._localplayer.Rider_Root;
					g.transform.Find(name).gameObject.GetComponent<SkinnedMeshRenderer>().material.mainTexture = texArray[texNum];
					SaveTextures(texNum, texArray, name);
				}
				if (name == "Daryien_Body")
			{
					GameObject g = UnityEngine.GameObject.Find("Skeleton").transform.GetChild(0).gameObject;
					g.transform.Find("body_geo").gameObject.GetComponent<SkinnedMeshRenderer>().materials[1].mainTexture = texArray[texNum];
					SaveTextures(texNum, texArray, name);
				}
			if (name == "Daryien_Head")
			{
					GameObject g = UnityEngine.GameObject.Find("Skeleton").transform.GetChild(0).gameObject;
					g.transform.Find("body_geo").gameObject.GetComponent<SkinnedMeshRenderer>().materials[0].mainTexture = texArray[texNum];
					SaveTextures(texNum, texArray, name);
				}
			if (name == "Baseball Cap_R")
			{
					GameObject g = UnityEngine.GameObject.Find("Skeleton").transform.GetChild(0).gameObject;
					g.transform.FindDeepChild("Baseball Cap_R").GetComponent<MeshRenderer>().material.mainTexture = texArray[texNum];
					g.transform.FindDeepChild("Baseball Cap_R").GetComponent<MeshRenderer>().material.color = Color.white;
					SaveTextures(texNum, texArray, name);
				}
			if (name == "Daryien_HandsFeet")
			{
					GameObject g = UnityEngine.GameObject.Find("Skeleton").transform.GetChild(0).gameObject;
					g.transform.Find("body_geo").gameObject.GetComponent<SkinnedMeshRenderer>().materials[2].mainTexture = texArray[texNum];
					SaveTextures(texNum, texArray, name);
				}
				
			if(name == "Frames")
                {
					Material m = FrameRen.material;
					BMXNetLoadout.instance.FrameTex = texArray[texNum].EncodeToPNG();
					m.SetTexture("_MainTex",(Texture)texArray[texNum]);
				}
			if (name == "Forks")
			{
				Material m = BMX_Materials["Forks Mesh"].material;
					BMXNetLoadout.instance.ForksTex = texArray[texNum].EncodeToPNG();
					m.SetTexture("_MainTex", (Texture)texArray[texNum]);
			}
				if (name == "Bars")
				{
					Material m = BMX_Materials["Bars Mesh"].material;
					BMXNetLoadout.instance.BarsTex = texArray[texNum].EncodeToPNG();
					m.SetTexture("_MainTex", (Texture)texArray[texNum]);
				}
				if (name == "Seats")
				{
					Material m = BMX_Materials["Seat Mesh"].material;
					BMXNetLoadout.instance.SeatTex = texArray[texNum].EncodeToPNG();
					m.SetTexture("_MainTex", (Texture)texArray[texNum]);
				}
				if (name == "Tires")
				{
					Material m = BMX_Materials["Tire Mesh Front"].materials[1];
					Material _m = BMX_Materials["Tire Mesh Back"].materials[1];
					Material __m = BMX_Materials["Tire Mesh Front"].materials[0];
					Material ___m = BMX_Materials["Tire Mesh Back"].materials[0];
					m.mainTexture = (Texture)texArray[texNum];
					_m.mainTexture = (Texture)texArray[texNum];
					__m.mainTexture = (Texture)texArray[texNum];
					___m.mainTexture = (Texture)texArray[texNum];
					BMXNetLoadout.instance.TiresTex = texArray[texNum].EncodeToPNG();
				}


			}


		}



	

		
		public void RandomConfig(int texNum, Texture2D[] texArray, string name)
		{
			if (!texArray.Equals(Hats))
			{
				GameObject.Find(name).GetComponent<SkinnedMeshRenderer>().material.mainTexture = texArray[texNum];
			}
			if (texArray == Hats)
			{
				GameObject.Find("Baseball Cap_R").GetComponent<MeshRenderer>().material.color = Color.white;
				GameObject.Find(name).GetComponent<MeshRenderer>().material.mainTexture = texArray[texNum];
			}
			_Random = false;
		}

		
		public void LoadRiderSetup(string name, string path)
		{
			if (File.Exists(path))
			{
				
				byte[] array = File.ReadAllBytes(path);
				savedTex = new Texture2D(1024, 1024);
				savedTex.name = name;
				ImageConversion.LoadImage(savedTex, array);

				if (name != "Baseball Cap_R" && name != "Daryien_Head" && name != "Daryien_Body" && name != "Daryien_HandsFeet")
				{
					GameObject.Find(name).GetComponent<SkinnedMeshRenderer>().material.mainTexture = savedTex;
					return;
				}
				if (name == "Daryien_Head")
				{
					skinMats[0].mainTexture = savedTex;
				}
				if (name == "Daryien_Body")
				{
					skinMats[1].mainTexture = savedTex;
				}
				if (name == "Daryien_HandsFeet")
				{
					skinMats[2].mainTexture = savedTex;
				}
				if (name == "BaseBall Cap_R")
				{
					GameObject.Find("Baseball Cap_R").GetComponent<MeshRenderer>().material.color = Color.white;
					GameObject.Find(name).GetComponent<MeshRenderer>().material.mainTexture = savedTex;
				}
			}
		}


		public void LoadBmxSetup()
        {
			FileStream file;
			BMXSaveData load;
			
			if (File.Exists(BmxSaveDir + "BMXSAVE.FrostyPreset")) file = File.OpenRead(BmxSaveDir + "BMXSAVE.FrostyPreset");
			else
			{
				Debug.Log("Couldnt find file in there");
				return;
			}


			BinaryFormatter bf = new BinaryFormatter();
			load = (BMXSaveData)bf.Deserialize(File.OpenRead(BmxSaveDir + "BMXSAVE.FrostyPreset"));
			file.Close();

            try
            {
			BMXNetLoadout.instance.FrameColour = new Vector3(load.FrameColour[0],load.FrameColour[1],load.FrameColour[2]);
			
			BMXNetLoadout.instance.FrameSmooth = load.FrameSmooth;
			BMXNetLoadout.instance.FrameTex = load.FrameTex;

			BMXNetLoadout.instance.ForksColour = new Vector3(load.FrameColour[0],load.FrameColour[1],load.FrameColour[2]);
			
			BMXNetLoadout.instance.ForksSmooth = load.ForksSmooth;
			BMXNetLoadout.instance.ForksTex = load.ForksTex;

			BMXNetLoadout.instance.BarsColour = new Vector3(load.FrameColour[0],load.FrameColour[1],load.FrameColour[2]);
			
			BMXNetLoadout.instance.BarsSmooth = load.BarsSmooth;
			BMXNetLoadout.instance.BarsTex = load.BarsTex;

			BMXNetLoadout.instance.SeatColour = new Vector3(load.FrameColour[0],load.FrameColour[1],load.FrameColour[2]);
			
			BMXNetLoadout.instance.SeatSmooth = load.SeatSmooth;
			BMXNetLoadout.instance.SeatTex = load.SeatTex;

			BMXNetLoadout.instance.FTireColour = new Vector3(load.FrameColour[0],load.FrameColour[1],load.FrameColour[2]);
			
			BMXNetLoadout.instance.RTireColour = new Vector3(load.FrameColour[0],load.FrameColour[1],load.FrameColour[2]);
			
			BMXNetLoadout.instance.FTireSideColour = new Vector3(load.FrameColour[0],load.FrameColour[1],load.FrameColour[2]);
			
			BMXNetLoadout.instance.RTireSideColour = new Vector3(load.FrameColour[0],load.FrameColour[1],load.FrameColour[2]);
			
			BMXNetLoadout.instance.TiresTex = load.TiresTex;
			BMXNetLoadout.instance.TiresNormal = load.TiresNormal;

				BMXNetLoadout.instance.FrameTexname = load.FrameTexname;
				BMXNetLoadout.instance.ForkTexname = load.ForksTexname;
				BMXNetLoadout.instance.BarTexName = load.BarsTexname;
				BMXNetLoadout.instance.SeatTexname = load.SeatTexname;
				BMXNetLoadout.instance.TireTexName = load.TiresTexname;
				BMXNetLoadout.instance.TireNormalName = load.TiresNormalname;


				FrameRen.material.SetInt("_SmoothnessTextureChannel", 0);
				FrameRen.material.SetFloat("_Metallic", 0.27f);
				FrameRen.material.SetFloat("_Glossiness", BMXNetLoadout.instance.FrameSmooth);
				FrameRen.material.color = new Color(BMXNetLoadout.instance.FrameColour.x, BMXNetLoadout.instance.FrameColour.y, BMXNetLoadout.instance.FrameColour.z);
            if (BMXNetLoadout.instance.FrameTex.Length > 1)
            {
				Texture2D image = new Texture2D(2, 2);
				image.LoadImage(BMXNetLoadout.instance.FrameTex);
				image.Apply();
					image.name = load.FrameTexname;
					FrameRen.material.mainTexture = image;

			}
                else
                {
					FrameRen.material.mainTexture = null;
					BMXNetLoadout.instance.FrameTexname = "e";
				}

			ForksRen.material.color = new Color(BMXNetLoadout.instance.ForksColour.x, BMXNetLoadout.instance.ForksColour.y, BMXNetLoadout.instance.ForksColour.z);
				ForksRen.material.SetInt("_SmoothnessTextureChannel", 0);
				ForksRen.material.SetFloat("_Metallic", 0.27f);
				ForksRen.material.SetFloat("_Glossiness", BMXNetLoadout.instance.ForksSmooth);
			if (BMXNetLoadout.instance.ForksTex.Length > 1)
			{
				Texture2D image = new Texture2D(2, 2);
				image.LoadImage(BMXNetLoadout.instance.ForksTex);
				image.Apply();
					image.name = load.ForksTexname;
					ForksRen.material.mainTexture = image;

			}
                else
                {
					ForksRen.material.mainTexture = null;
					BMXNetLoadout.instance.ForkTexname = "e";
				}


			BarsRen.material.color = new Color(BMXNetLoadout.instance.BarsColour.x, BMXNetLoadout.instance.BarsColour.y, BMXNetLoadout.instance.BarsColour.z);
				BarsRen.material.SetInt("_SmoothnessTextureChannel", 0);
				BarsRen.material.SetFloat("_Metallic", 0.27f);
				BarsRen.material.SetFloat("_Glossiness", BMXNetLoadout.instance.BarsSmooth);
			if (BMXNetLoadout.instance.BarsTex.Length > 0)
			{
				Texture2D image = new Texture2D(2, 2);
				image.LoadImage(BMXNetLoadout.instance.BarsTex);
				image.Apply();
					image.name = load.BarsTexname;
					BarsRen.material.mainTexture = image;

			}
                else
                {
					BarsRen.material.mainTexture = null;
					BMXNetLoadout.instance.BarTexName = "e";
				}


			SeatRen.material.color = new Color(BMXNetLoadout.instance.SeatColour.x, BMXNetLoadout.instance.SeatColour.y, BMXNetLoadout.instance.SeatColour.z);
				SeatRen.material.SetInt("_SmoothnessTextureChannel", 0);
				SeatRen.material.SetFloat("_Metallic", 0.27f);
				SeatRen.material.SetFloat("_Glossiness", BMXNetLoadout.instance.SeatSmooth);
			if (BMXNetLoadout.instance.SeatTex.Length > 0)
			{
				Texture2D image = new Texture2D(2, 2);
				image.LoadImage(BMXNetLoadout.instance.SeatTex);
				image.Apply();
					image.name = load.SeatTexname;
					SeatRen.material.mainTexture = image;

			}
                else
                {
					SeatRen.material.mainTexture = null;
					BMXNetLoadout.instance.SeatTexname = "e";
				}


			FTireRen.material.color = new Color(BMXNetLoadout.instance.FTireColour.x, BMXNetLoadout.instance.FTireColour.y, BMXNetLoadout.instance.FTireColour.z);
			RTireRen.material.color = new Color(BMXNetLoadout.instance.RTireColour.x, BMXNetLoadout.instance.RTireColour.y, BMXNetLoadout.instance.RTireColour.z);
		
			if (BMXNetLoadout.instance.TiresTex.Length > 0)
			{
				Texture2D image = new Texture2D(2, 2);
				image.LoadImage(BMXNetLoadout.instance.TiresTex);
				image.Apply();
					image.name = load.TiresTexname;
				FTireRen.material.mainTexture = image;
					RTireRen.material.mainTexture = image;
				}
                else
                {
					FTireRen.material.mainTexture = null;
					RTireRen.material.mainTexture = null;
					BMXNetLoadout.instance.TireTexName = "e";
				}


			Debug.Log("Loaded BMX!");
            }
            catch (UnityException x)
            {
				Debug.Log("Didnt load bmx: " + x);

            }


		}

		public void SaveBmxSetup()
        {
			BMXSaveData Data = new BMXSaveData();
			
			Data.FrameColour = new float[3];
			Data.FrameColour[0] = BMXNetLoadout.instance.FrameColour.x;
			Data.FrameColour[1] = BMXNetLoadout.instance.FrameColour.y;
			Data.FrameColour[2] = BMXNetLoadout.instance.FrameColour.z;
			Data.FrameSmooth = BMXNetLoadout.instance.FrameSmooth;
			Data.FrameTex = BMXNetLoadout.instance.FrameTex;

			Data.ForksColour = new float[3];
			Data.ForksColour[0] = BMXNetLoadout.instance.ForksColour.x;
			Data.ForksColour[1] = BMXNetLoadout.instance.ForksColour.y;
			Data.ForksColour[2] = BMXNetLoadout.instance.ForksColour.z;
			Data.ForksSmooth = BMXNetLoadout.instance.ForksSmooth;
			Data.ForksTex = BMXNetLoadout.instance.ForksTex;

			Data.BarsColour = new float[3];
			Data.BarsColour[0] = BMXNetLoadout.instance.BarsColour.x;
			Data.BarsColour[1] = BMXNetLoadout.instance.BarsColour.y;
			Data.BarsColour[2] = BMXNetLoadout.instance.BarsColour.z;
			Data.BarsSmooth = BMXNetLoadout.instance.BarsSmooth;
			Data.BarsTex = BMXNetLoadout.instance.BarsTex;

			Data.SeatColour = new float[3];
			Data.SeatColour[0] = BMXNetLoadout.instance.SeatColour.x;
			Data.SeatColour[1] = BMXNetLoadout.instance.SeatColour.y;
			Data.SeatColour[2] = BMXNetLoadout.instance.SeatColour.z;
			Data.SeatSmooth = BMXNetLoadout.instance.SeatSmooth;
			Data.SeatTex = BMXNetLoadout.instance.SeatTex;

			Data.FTireColour = new float[3];
			Data.FTireColour[0] = BMXNetLoadout.instance.FTireColour.x;
			Data.FTireColour[1] = BMXNetLoadout.instance.FTireColour.y;
			Data.FTireColour[2] = BMXNetLoadout.instance.FTireColour.z;
			Data.RTireColour = new float[3];
			Data.RTireColour[0] = BMXNetLoadout.instance.RTireColour.x;
			Data.RTireColour[1] = BMXNetLoadout.instance.RTireColour.y;
			Data.RTireColour[2] = BMXNetLoadout.instance.RTireColour.z;
			Data.FTireSideColour = new float[3];
			Data.FTireSideColour[0] = BMXNetLoadout.instance.FTireSideColour.x;
			Data.FTireSideColour[1] = BMXNetLoadout.instance.FTireSideColour.y;
			Data.FTireSideColour[2] = BMXNetLoadout.instance.FTireSideColour.z;
			Data.RTireSideColour = new float[3];
			Data.RTireSideColour[0] = BMXNetLoadout.instance.RTireSideColour.x;
			Data.RTireSideColour[1] = BMXNetLoadout.instance.RTireSideColour.y;
			Data.RTireSideColour[2] = BMXNetLoadout.instance.RTireSideColour.z;
			Data.TiresTex = BMXNetLoadout.instance.TiresTex;
			Data.TiresNormal = BMXNetLoadout.instance.TiresNormal;
			Data.FrameTexname = BMXNetLoadout.instance.FrameTexname;
			Data.ForksTexname = BMXNetLoadout.instance.ForkTexname;
			Data.BarsTexname = BMXNetLoadout.instance.BarTexName;
			Data.SeatTexname = BMXNetLoadout.instance.SeatTexname;
			Data.TiresTexname = BMXNetLoadout.instance.TireTexName;
			Data.TiresNormalname = BMXNetLoadout.instance.TireNormalName;

			if (!Directory.Exists(BmxSaveDir))
			{
				Directory.CreateDirectory(BmxSaveDir);
			}

			FileStream file;

			if (File.Exists(BmxSaveDir + "BMXSAVE.FrostyPreset"))
			{
				file = File.OpenWrite(BmxSaveDir + "BMXSAVE.FrostyPreset");
			}
			else file = File.Create(BmxSaveDir + "BMXSAVE.FrostyPreset");
			
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(file, Data);
			file.Close();
			Debug.Log("Completed Saving BMX");
			return;




		}


		public void SaveTextures(int texNum, Texture2D[] texArray, string name)
		{
			//texArray[texNum].GetRawTextureData();
			//byte[] bytes = ImageConversion.EncodeToPNG(texArray[texNum]);

			
			byte[] bytes = ImageConversion.EncodeToPNG(texArray[texNum]);
            try
            {
				File.WriteAllBytes(Application.dataPath + "/Textures/Save/" + name + ".png", bytes );
            }
            catch (System.Exception x)
            {
				Debug.Log(x);
            }
			
		}

		
		
		/// <summary>
		/// Used for displaying images without barriers of manager
		/// </summary>
		private void OnGUI()
        {
			GUI.skin = FrostyP_Game_Manager.FrostyPGamemanager.instance.skin;
			// for rider
			if (headtoggle)
			{
				bodytoggle = false;
				handsfeettoggle = false;
				shirttoggle = false;
				shoetoggle = false;
				hattoggle = false;
				bottomstoggle = false;
				GUILayout.BeginArea(new Rect(new Vector2(imagesidebuffer, 0), new Vector2(imagewidth, (float)Screen.height)));
				scrollPosition = GUILayout.BeginScrollView(scrollPosition);
				GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
				for (int i = 0; i < Heads.Length; i++)
				{
					ShowATexButton(i, Heads, "Daryien_Head");
					
				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.EndArea();
				
			}
			if (bodytoggle)
			{
				headtoggle = false;
				handsfeettoggle = false;
				shirttoggle = false;
				shoetoggle = false;
				hattoggle = false;
				bottomstoggle = false;
				GUILayout.BeginArea(new Rect(new Vector2(imagesidebuffer, 0f), new Vector2(imagewidth, (float)Screen.height)));
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[0]);
				GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
				for (int j = 0; j < Bodies.Length; j++)
				{
					ShowATexButton(j, Bodies, "Daryien_Body");
					
				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.EndArea();
				
			}
			if (handsfeettoggle)
			{
				headtoggle = false;
				bodytoggle = false;
				shirttoggle = false;
				shoetoggle = false;
				hattoggle = false;
				bottomstoggle = false;

				GUILayout.BeginArea(new Rect(new Vector2(imagesidebuffer, 0f), new Vector2(imagewidth, (float)Screen.height)));
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[0]);
				GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
				for (int k = 0; k < Hands_feet.Length; k++)
				{
					ShowATexButton(k, Hands_feet, "Daryien_HandsFeet");
					
				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.EndArea();
				
			}
			if (shirttoggle)
			{
				headtoggle = false;
				handsfeettoggle = false;
				bodytoggle = false;
				shoetoggle = false;
				hattoggle = false;
				bottomstoggle = false;

				GUILayout.BeginArea(new Rect(new Vector2(imagesidebuffer, 0f), new Vector2(imagewidth, (float)Screen.height)));
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[0]);
				GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
				for (int m = 0; m < Shirts.Length; m++)
				{
					ShowATexButton(m, Shirts, "shirt_geo");
					
				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.EndArea();
				
			}
			if (shoetoggle)
			{
				headtoggle = false;
				handsfeettoggle = false;
				shirttoggle = false;
				bodytoggle = false;
				hattoggle = false;
				bottomstoggle = false;

				GUILayout.BeginArea(new Rect(new Vector2(imagesidebuffer, 0f), new Vector2(imagewidth, (float)Screen.height)));
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[0]);
				GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
				for (int n = 0; n < Shoes.Length; n++)
				{
					ShowATexButton(n, Shoes, "shoes_geo");
					
				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.EndArea();
				
			}
			if (hattoggle)
			{
				headtoggle = false;
				handsfeettoggle = false;
				shirttoggle = false;
				bodytoggle = false;
				shoetoggle = false;
				bottomstoggle = false;

				GUILayout.BeginArea(new Rect(new Vector2(imagesidebuffer, 0f), new Vector2(imagewidth, (float)Screen.height)));
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[0]);
				GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
				for (int num = 0; num < Hats.Length; num++)
				{
					ShowATexButton(num, Hats, "Baseball Cap_R");
					
				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.EndArea();
				
			}
			if (bottomstoggle)
			{
				headtoggle = false;
				handsfeettoggle = false;
				shirttoggle = false;
				bodytoggle = false;
				hattoggle = false;
				shoetoggle = false;
				GUILayout.BeginArea(new Rect(new Vector2(imagesidebuffer, 0f), new Vector2(imagewidth, (float)Screen.height)));
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[0]);
				GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
				for (int l = 0; l < Bottoms.Length; l++)
				{
					ShowATexButton(l, Bottoms, "pants_geo");
					
				}
				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.EndArea();

			}

			// for bike
			if (texturetoggle)
			{
				GUILayout.BeginArea(new Rect(new Vector2(imagesidebuffer, 0f), new Vector2(imagewidth, (float)Screen.height)));
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[0]);
				GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
                for (int T = 0; T < Currentshowingarray.Length ; T++)
                {
					ShowATexButton(T, Currentshowingarray, currentarrayname);
                }


				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.EndArea();
			}



          


           


		}

		
		
		
		
	}
}
