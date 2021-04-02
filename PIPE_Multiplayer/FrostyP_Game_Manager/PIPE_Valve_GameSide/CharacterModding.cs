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
        public string HeadDir = Application.dataPath + "/FrostyPGameManager/Textures/Head/";
		public string HatDir = Application.dataPath + "/FrostyPGameManager/Textures/Head/";
		public string ShirtDir = Application.dataPath + "/FrostyPGameManager/Textures/Shirt/";
		public string BottomsDir = Application.dataPath + "/FrostyPGameManager/Textures/Pants/";
		public string ShoeDir = Application.dataPath + "/FrostyPGameManager/Textures/Shoes/";
		public string HandsFeetDir = Application.dataPath + "/FrostyPGameManager/Textures/Hands-Feet/";
		public string BodyDir = Application.dataPath + "/FrostyPGameManager/Textures/Body/";
		public string RidersaveDir = Application.dataPath + "/FrostyPGameManager/Textures/Save/";
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
		bool stemtoggle;
		bool rimstoggle;
		bool Frimtoggle;
		bool Rrimtoggle;
		bool wheelstoggle;
		bool barstoggle;
		bool texturetoggle;
		bool NormalToggle;
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
		public string Tirenormaldir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/TireNormals/";
		public string Framenormaldir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/FrameNormals/";
		public string Forksnormaldir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/ForksNormals/";
		public string Barsnormaldir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/BarsNormals/";
		public string Seatnormaldir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/SeatNormals/";
		public string Stemdir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/Stems/";
		public string Stemnormaldir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/StemNormals/";
		public string Rimdir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/Rims/";
		public string Rimnormaldir = Application.dataPath + "/FrostyPGameManager/Textures/Bmx/RimNormals/";
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

		




		// Bmx Tex
		private Texture2D[] Frames;
		private Texture2D[] Forks;
		private Texture2D[] Seats;
		public Texture2D[] Stems;
		private Texture2D[] Bars;
		private Texture2D[] Tires;
		public Texture2D[] Rims;

		public Texture2D[] TireNormals;
		public Texture2D[] FrameNormals;
		public Texture2D[] ForksNormals;
		public Texture2D[] BarsNormals;
		public Texture2D[] SeatNormals;
		public Texture2D[] RimNormals;
		public Texture2D[] StemNormals;


		public Dictionary<string, Texture2D> currentTexs;
		/// <summary>
		/// bikes materials accessible via gameobject name
		/// </summary>
		public Dictionary<string, MeshRenderer> BMX_Materials = new Dictionary<string, MeshRenderer>();
		/// <summary>
		/// Riders materials accesible by Gameobject name
		/// </summary>
		public Dictionary<string, Renderer> Rider_Materials = new Dictionary<string, Renderer>();
		Texture2D[] Currentshowingarray;
		string currentarrayname;
		   public MeshRenderer FrameRen;
		public	MeshRenderer ForksRen;
			public MeshRenderer BarsRen;
			public MeshRenderer SeatRen;
			public MeshRenderer FTireRen;
		    public MeshRenderer RTireRen;
		    public MeshRenderer FRimRen;
		    public MeshRenderer RRimRen;
		    public MeshRenderer StemRen;
		


		// if theres more than one material being edited at once
		bool saving = false;




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
			directories.Add(Stemdir);
			directories.Add(BarsDir);
			directories.Add(TiresDir);
			directories.Add(Rimdir);
			directories.Add(HeadDir);
			directories.Add(HatDir);
			directories.Add(ShoeDir);
			directories.Add(ShirtDir);
			directories.Add(BottomsDir);
			directories.Add(HandsFeetDir);
			directories.Add(BodyDir);
			directories.Add(Tirenormaldir);
			directories.Add(Framenormaldir);
			directories.Add(Forksnormaldir);
			directories.Add(Barsnormaldir);
			directories.Add(Seatnormaldir);
			directories.Add(Stemnormaldir);
			directories.Add(Rimnormaldir);


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
			if (!Directory.Exists(Application.dataPath + "/FrostyPGameManager/Textures/Save/"))
			{
				Directory.CreateDirectory(Application.dataPath + "/FrostyPGameManager/Textures/Save/");
			}
			
			

			
				Rider_Materials.Add("pants_geo", GameObject.Find("pants_geo").GetComponent<SkinnedMeshRenderer>());
			Rider_Materials.Add("shirt_geo", GameObject.Find("shirt_geo").GetComponent<SkinnedMeshRenderer>());
			Rider_Materials.Add("body_geo", GameObject.Find("body_geo").GetComponent<SkinnedMeshRenderer>());
			Rider_Materials.Add("Baseball Cap_R", GameObject.Find("Baseball Cap_R").GetComponent<MeshRenderer>());
			Rider_Materials.Add("shoes_geo", GameObject.Find("shoes_geo").GetComponent<SkinnedMeshRenderer>());




				LoadRiderSetup();
			#endregion




			#region Setup Bmx Textures

			currentTexs = new Dictionary<string, Texture2D>();

			Bmx = UnityEngine.GameObject.Find("BMX");
			Frames = SearchDirectory(FrameDir);
			Forks = SearchDirectory(ForksDir);
			Stems = SearchDirectory(Stemdir);
			Seats = SearchDirectory(SeatDir);
			Tires = SearchDirectory(TiresDir);
			Rims = SearchDirectory(Rimdir);
			Bars = SearchDirectory(BarsDir);
			TireNormals = SearchDirectory(Tirenormaldir);
			FrameNormals = SearchDirectory(Framenormaldir);
			ForksNormals = SearchDirectory(Forksnormaldir);
			BarsNormals = SearchDirectory(Barsnormaldir);
			SeatNormals = SearchDirectory(Seatnormaldir);
			StemNormals = SearchDirectory(Stemnormaldir);
			RimNormals = SearchDirectory(Rimnormaldir);

			FrameRen = UnityEngine.GameObject.Find("BMX:Frame_Joint").transform.Find("Frame Mesh").gameObject.GetComponent<MeshRenderer>();
			ForksRen = UnityEngine.GameObject.Find("Forks Mesh").GetComponent<MeshRenderer>();
			BarsRen = UnityEngine.GameObject.Find("Bars Mesh").GetComponent<MeshRenderer>();
			SeatRen = UnityEngine.GameObject.Find("Seat Mesh").GetComponent<MeshRenderer>();
			FTireRen = UnityEngine.GameObject.Find("BMX:Wheel").transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
			RTireRen = UnityEngine.GameObject.Find("BMX:Wheel 1").transform.Find("Tire Mesh").gameObject.GetComponent<MeshRenderer>();
			StemRen = UnityEngine.GameObject.Find("BMX:Bars_Joint").transform.Find("Stem Mesh").gameObject.GetComponent<MeshRenderer>();
			FRimRen = UnityEngine.GameObject.Find("BMX:Wheel").transform.Find("Rim Mesh").gameObject.GetComponent<MeshRenderer>();
			RRimRen = UnityEngine.GameObject.Find("BMX:Wheel 1").transform.Find("Rim Mesh").gameObject.GetComponent<MeshRenderer>();



			BMX_Materials = new Dictionary<string, MeshRenderer>();
			BMX_Materials.Add("framemesh",FrameRen);
			BMX_Materials.Add("Forks Mesh",ForksRen);
			BMX_Materials.Add("Seat Mesh",SeatRen);
			BMX_Materials.Add("Bars Mesh",BarsRen);
			BMX_Materials.Add("Tire Mesh Front",FTireRen);
			BMX_Materials.Add("Tire Mesh Back",RTireRen);
			BMX_Materials.Add("Stem Mesh", StemRen);
			BMX_Materials.Add("Front Rim", FRimRen);
			BMX_Materials.Add("Rear Rim", RRimRen);

			StartCoroutine(Initialiseafterwait());

			// to be done

			//BMX_Materials.Add("Spokes Mesh", UnityEngine.GameObject.Find("Spokes Mesh").GetComponent<MeshRenderer>());
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
			if (GUILayout.Button("Save rider/Live Update"))
			{
				if(GameManager.instance._localplayer.RiderModelname == "Daryien")
                {
				SaveRiderSetup();

                }
                if (InGameUI.instance.Connected && GameManager.instance._localplayer.RiderModelname == "Daryien")
                {
				GameManager.instance.SendQuickRiderUpdate();
                }
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
			stemtoggle = GUILayout.Toggle(stemtoggle, " Stem ");
			wheelstoggle = GUILayout.Toggle(wheelstoggle, " Tires ");
			rimstoggle = GUILayout.Toggle(rimstoggle, " Rims ");
			GUILayout.Space(5);
		
			if (GUILayout.Button("Save Bike/LiveUpdate") && !saving)
            {

                if (InGameUI.instance.Connected)
                {
				GameManager.instance.SendQuickBikeUpdate();
                }
        
					
			    SaveBmxSetup();
					
                
            }

			if (frametoggle)
            {
				GUILayout.Space(20);
				GUILayout.Label("Frame Settings:");
				texturetoggle = GUILayout.Toggle(texturetoggle, " Base Texture ");
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
				NormalToggle = GUILayout.Toggle(NormalToggle, " Frame Normal ");
				if (NormalToggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						Material m = BMX_Materials["framemesh"].material;
						

						m.EnableKeyword("_NORMALMAP");
						

						m.SetTexture("_BumpMap", null);
						
						BMXNetLoadout.instance.FrameNormal = new byte[0];
						BMXNetLoadout.instance.FrameNormalName = "";
					}
					currentarrayname = "Frames Normal";
					Currentshowingarray = FrameNormals;
				}


				materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
                if (materialstoggle)
                {
					Renderer m = FrameRen;
					
					
					GUILayout.Label("RGB");
					BMXNetLoadout.instance.FrameColour[0] = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FrameColour[0], 0, 1);
					BMXNetLoadout.instance.FrameColour[1] = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FrameColour[1], 0, 1);
					BMXNetLoadout.instance.FrameColour[2] = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FrameColour[2], 0, 1);
					m.material.color = new Color(BMXNetLoadout.instance.FrameColour[0], BMXNetLoadout.instance.FrameColour[1], BMXNetLoadout.instance.FrameColour[2]);

					GUILayout.Label("Smoothness");
					BMXNetLoadout.instance.FrameSmooth = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FrameSmooth, 0, 1);
					m.material.SetInt("_SmoothnessTextureChannel", 0);
					m.material.SetFloat("_Glossiness", BMXNetLoadout.instance.FrameSmooth);
					GUILayout.Label("Metallic");
					BMXNetLoadout.instance.FrameMetallic = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FrameMetallic, 0, 1);
					m.material.SetFloat("_Metallic", BMXNetLoadout.instance.FrameMetallic);
					
				}
				GUILayout.Space(20);



			}
			if (forkstoggle)
			{
				GUILayout.Space(20);
					GUILayout.Label("Forks Settings:");
				texturetoggle = GUILayout.Toggle(texturetoggle, " Base Texture ");
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




				NormalToggle = GUILayout.Toggle(NormalToggle, " Forks Normal ");
				if (NormalToggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						Material m = BMX_Materials["Forks Mesh"].material;


						m.EnableKeyword("_NORMALMAP");


						m.SetTexture("_BumpMap", null);

						BMXNetLoadout.instance.ForksNormal = new byte[0];
						BMXNetLoadout.instance.ForksNormalName = "";
					}
					currentarrayname = "Forks Normal";
					Currentshowingarray = ForksNormals;
				}





				materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
				if (materialstoggle)
				{
					Renderer m = BMX_Materials["Forks Mesh"];
					


					GUILayout.Label("RGB");
					BMXNetLoadout.instance.ForksColour[0] = GUILayout.HorizontalSlider(BMXNetLoadout.instance.ForksColour[0], 0, 1);
					BMXNetLoadout.instance.ForksColour[1] = GUILayout.HorizontalSlider(BMXNetLoadout.instance.ForksColour[1], 0, 1);
					BMXNetLoadout.instance.ForksColour[2] = GUILayout.HorizontalSlider(BMXNetLoadout.instance.ForksColour[2], 0, 1);
					m.material.color = new Color(BMXNetLoadout.instance.ForksColour[0], BMXNetLoadout.instance.ForksColour[1], BMXNetLoadout.instance.ForksColour[2]);

					GUILayout.Label("Smoothness");
					BMXNetLoadout.instance.ForksSmooth = GUILayout.HorizontalSlider(BMXNetLoadout.instance.ForksSmooth, 0, 1);
					m.material.SetInt("_SmoothnessTextureChannel", 0);
					m.material.SetFloat("_Glossiness", BMXNetLoadout.instance.ForksSmooth);
					GUILayout.Label("Metallic");
					BMXNetLoadout.instance.ForksMetallic = GUILayout.HorizontalSlider(BMXNetLoadout.instance.ForksMetallic, 0, 1);
					m.material.SetFloat("_Metallic", BMXNetLoadout.instance.ForksMetallic);


				}
				GUILayout.Space(20);



			}
			if (barstoggle)
			{
				GUILayout.Space(20);
					GUILayout.Label("Bars Settings:");
				texturetoggle = GUILayout.Toggle(texturetoggle, " Base Texture ");
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


				NormalToggle = GUILayout.Toggle(NormalToggle, " Bars Normal ");
				if (NormalToggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						Material m = BMX_Materials["Bars Mesh"].material;


						m.EnableKeyword("_NORMALMAP");


						m.SetTexture("_BumpMap", null);

						BMXNetLoadout.instance.BarsNormal = new byte[0];
						BMXNetLoadout.instance.BarsNormalName = "";
					}
					currentarrayname = "Bars Normal";
					Currentshowingarray = BarsNormals;
				}




				materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
				if (materialstoggle)
				{
					Renderer m = BMX_Materials["Bars Mesh"];
				




					GUILayout.Label("RGB");
					BMXNetLoadout.instance.BarsColour.x = GUILayout.HorizontalSlider(BMXNetLoadout.instance.BarsColour.x, 0, 1);
					BMXNetLoadout.instance.BarsColour.y = GUILayout.HorizontalSlider(BMXNetLoadout.instance.BarsColour.y, 0, 1);
					BMXNetLoadout.instance.BarsColour.z = GUILayout.HorizontalSlider(BMXNetLoadout.instance.BarsColour.z, 0, 1);
					m.material.color = new Color(BMXNetLoadout.instance.BarsColour.x, BMXNetLoadout.instance.BarsColour.y, BMXNetLoadout.instance.BarsColour.z);

					GUILayout.Label("Smoothness");
					BMXNetLoadout.instance.BarsSmooth = GUILayout.HorizontalSlider(BMXNetLoadout.instance.BarsSmooth, 0, 1);
					m.material.SetInt("_SmoothnessTextureChannel", 0);
					m.material.SetFloat("_Glossiness", BMXNetLoadout.instance.BarsSmooth);
					GUILayout.Label("Metallic");
					BMXNetLoadout.instance.BarsMetallic = GUILayout.HorizontalSlider(BMXNetLoadout.instance.BarsMetallic, 0, 1);
					m.material.SetFloat("_Metallic", BMXNetLoadout.instance.BarsMetallic);

				}

				GUILayout.Space(20);
			}
			if (seattoggle)
			{
				GUILayout.Space(20);
					GUILayout.Label("Seat Settings:");
				texturetoggle = GUILayout.Toggle(texturetoggle, " Base Texture ");
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


				NormalToggle = GUILayout.Toggle(NormalToggle, " Seat Normal ");
				if (NormalToggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						Material m = BMX_Materials["Seat Mesh"].material;


						m.EnableKeyword("_NORMALMAP");


						m.SetTexture("_BumpMap", null);

						BMXNetLoadout.instance.SeatNormal = new byte[0];
						BMXNetLoadout.instance.SeatNormalName = "";
					}
					currentarrayname = "Seat Normal";
					Currentshowingarray = SeatNormals;
				}







				materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
				if (materialstoggle)
				{
					Renderer m = BMX_Materials["Seat Mesh"];
					
					GUILayout.Label("RGB");
					BMXNetLoadout.instance.SeatColour.x = GUILayout.HorizontalSlider(BMXNetLoadout.instance.SeatColour.x, 0, 1);
					BMXNetLoadout.instance.SeatColour.y = GUILayout.HorizontalSlider(BMXNetLoadout.instance.SeatColour.y, 0, 1);
					BMXNetLoadout.instance.SeatColour.z = GUILayout.HorizontalSlider(BMXNetLoadout.instance.SeatColour.z, 0, 1);
					m.material.color = new Color(BMXNetLoadout.instance.SeatColour.x, BMXNetLoadout.instance.SeatColour.y, BMXNetLoadout.instance.SeatColour.z);

					GUILayout.Label("Smoothness");
					BMXNetLoadout.instance.SeatSmooth = GUILayout.HorizontalSlider(BMXNetLoadout.instance.SeatSmooth, 0, 1);
					m.material.SetInt("_SmoothnessTextureChannel", 0);
					m.material.SetFloat("_Metallic", 0.0f);
					m.material.SetFloat("_Glossiness", BMXNetLoadout.instance.SeatSmooth);
					
				}

				GUILayout.Space(20);
			}
			if (stemtoggle)
			{
				GUILayout.Space(20);
				GUILayout.Label("Stem Settings:");
				texturetoggle = GUILayout.Toggle(texturetoggle, " Base Texture ");
				if (texturetoggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						BMX_Materials["Stem Mesh"].material.mainTexture = null;
						BMXNetLoadout.instance.StemTex = new byte[0];
						BMXNetLoadout.instance.StemTexName = "";
					}
					currentarrayname = "Stems";
					Currentshowingarray = Stems;
				}


				NormalToggle = GUILayout.Toggle(NormalToggle, " Stem Normal ");
				if (NormalToggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						Material m = BMX_Materials["Stem Mesh"].material;


						m.EnableKeyword("_NORMALMAP");


						m.SetTexture("_BumpMap", null);

						BMXNetLoadout.instance.StemNormal = new byte[0];
						BMXNetLoadout.instance.StemNormalName = "";
					}
					currentarrayname = "Stem Normal";
					Currentshowingarray = StemNormals;
				}




				materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
				if (materialstoggle)
				{
					Material m = BMX_Materials["Stem Mesh"].material;

					GUILayout.Label("RGB");
					BMXNetLoadout.instance.StemColour.x = GUILayout.HorizontalSlider(BMXNetLoadout.instance.StemColour.x, 0, 1);
					BMXNetLoadout.instance.StemColour.y = GUILayout.HorizontalSlider(BMXNetLoadout.instance.StemColour.y, 0, 1);
					BMXNetLoadout.instance.StemColour.z = GUILayout.HorizontalSlider(BMXNetLoadout.instance.StemColour.z, 0, 1);
					m.color = new Color(BMXNetLoadout.instance.StemColour.x, BMXNetLoadout.instance.StemColour.y, BMXNetLoadout.instance.StemColour.z);

					GUILayout.Label("Smoothness");
					BMXNetLoadout.instance.StemSmooth = GUILayout.HorizontalSlider(BMXNetLoadout.instance.StemSmooth, 0, 1);

					GUILayout.Label("Metallic");
					BMXNetLoadout.instance.StemMetallic = GUILayout.HorizontalSlider(BMXNetLoadout.instance.StemMetallic, 0, 1);

					m.SetInt("_SmoothnessTextureChannel", 0);
					m.SetFloat("_Metallic", BMXNetLoadout.instance.StemMetallic);
					m.SetFloat("_Glossiness", BMXNetLoadout.instance.StemSmooth);

				}

				GUILayout.Space(20);
			}
			if (wheelstoggle)
			{
				GUILayout.Space(20);
				texturetoggle = GUILayout.Toggle(texturetoggle, " Tire Base ");
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
				NormalToggle = GUILayout.Toggle(NormalToggle, " Tire Normal ");
				if (NormalToggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						Material m = BMX_Materials["Tire Mesh Front"].materials[1];
						Material _m = BMX_Materials["Tire Mesh Back"].materials[1];
						Material __m = BMX_Materials["Tire Mesh Front"].materials[0];
						Material ___m = BMX_Materials["Tire Mesh Back"].materials[0];

						BMX_Materials["Tire Mesh Front"].materials[1].EnableKeyword("_NORMALMAP");
						BMX_Materials["Tire Mesh Back"].materials[1].EnableKeyword("_NORMALMAP");
						BMX_Materials["Tire Mesh Front"].materials[0].EnableKeyword("_NORMALMAP");
						BMX_Materials["Tire Mesh Back"].materials[0].EnableKeyword("_NORMALMAP");

						m.SetTexture("_BumpMap",null);
						_m.SetTexture("_BumpMap", null);
						__m.SetTexture("_BumpMap", null);
						___m.SetTexture("_BumpMap",null);
						BMXNetLoadout.instance.TiresNormal = new byte[0];
						BMXNetLoadout.instance.TireNormalName = "";
					}
					currentarrayname = "Tires Normal";
					Currentshowingarray = TireNormals;
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
					BMXNetLoadout.instance.FTireColour.x = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FTireColour.x, 0, 1);
					BMXNetLoadout.instance.FTireColour.y = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FTireColour.y, 0, 1);
					BMXNetLoadout.instance.FTireColour.z = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FTireColour.z, 0, 1);
					m.materials[0].color = new Color(BMXNetLoadout.instance.FTireColour.x, BMXNetLoadout.instance.FTireColour.y, BMXNetLoadout.instance.FTireColour.z);
					GUILayout.Space(5);

					GUILayout.Label("Sidewall:");
					BMXNetLoadout.instance.FTireSideColour.x = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FTireSideColour.x, 0, 1);
					BMXNetLoadout.instance.FTireSideColour.y = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FTireSideColour.y, 0, 1);
					BMXNetLoadout.instance.FTireSideColour.z = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FTireSideColour.z, 0, 1);
					m.materials[1].color = new Color(BMXNetLoadout.instance.FTireSideColour.x, BMXNetLoadout.instance.FTireSideColour.y, BMXNetLoadout.instance.FTireSideColour.z);
					GUILayout.Space(10);

					GUILayout.Label("Smoothness");
					BMXNetLoadout.instance.FTireSmooth = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FTireSmooth, 0, 1);
					m.material.SetInt("_SmoothnessTextureChannel", 0);
					m.material.SetFloat("_Metallic", 0.27f);
					m.material.SetFloat("_Glossiness", BMXNetLoadout.instance.FTireSmooth);



					Renderer _m = BMX_Materials["Tire Mesh Back"];
					//_m.material.mainTexture = null;
					GUILayout.Label("Back Tire:");
					GUILayout.Space(5);
					GUILayout.Label("RGB");
					GUILayout.Label("Top:");
					BMXNetLoadout.instance.RTireColour.x = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RTireColour.x, 0, 1);
					BMXNetLoadout.instance.RTireColour.y = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RTireColour.y, 0, 1);
					BMXNetLoadout.instance.RTireColour.z = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RTireColour.z, 0, 1);
					_m.materials[0].color = new Color(BMXNetLoadout.instance.RTireColour.x, BMXNetLoadout.instance.RTireColour.y, BMXNetLoadout.instance.RTireColour.z);
					GUILayout.Space(5);

					GUILayout.Label("Sidewall:");
					BMXNetLoadout.instance.RTireSideColour.x = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RTireSideColour.x, 0, 1);
					BMXNetLoadout.instance.RTireSideColour.y = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RTireSideColour.y, 0, 1);
					BMXNetLoadout.instance.RTireSideColour.z = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RTireSideColour.z, 0, 1);
					_m.materials[1].color = new Color(BMXNetLoadout.instance.RTireSideColour.x, BMXNetLoadout.instance.RTireSideColour.y, BMXNetLoadout.instance.RTireSideColour.z);
					GUILayout.Space(10);

					GUILayout.Label("Smoothness");
					BMXNetLoadout.instance.RTireSmooth = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RTireSmooth, 0, 1);
					_m.material.SetInt("_SmoothnessTextureChannel", 0);
					_m.material.SetFloat("_Metallic", 0.27f);
					_m.material.SetFloat("_Glossiness", BMXNetLoadout.instance.RTireSmooth);

					

				}

				GUILayout.Space(20);
			}

			if (rimstoggle)
			{
				GUILayout.Space(20);
				Frimtoggle = GUILayout.Toggle(Frimtoggle, " Front Rim ");
                if (Frimtoggle)
                {
				GUILayout.Space(20);
				texturetoggle = GUILayout.Toggle(texturetoggle, " Rim Base ");
				if (texturetoggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						BMX_Materials["Front Rim"].material.mainTexture = null;
						
						BMXNetLoadout.instance.FRimTex = new byte[0];
						BMXNetLoadout.instance.FRimTexName = "";
					}
					currentarrayname = "FRim";
					Currentshowingarray = Rims;
				}


				NormalToggle = GUILayout.Toggle(NormalToggle, " Rim Normal ");
				if (NormalToggle)
				{
					if (GUILayout.Button("Remove Texture"))
					{
						Material m = BMX_Materials["Front Rim"].material;
						
						

						m.EnableKeyword("_NORMALMAP");
						m.SetTexture("_BumpMap", null);
						
						
						BMXNetLoadout.instance.FRimNormal = new byte[0];
						BMXNetLoadout.instance.FRimNormalName = "";
					}
					currentarrayname = "FRim Normal";
					Currentshowingarray = RimNormals;
				}


				materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
				if (materialstoggle)
				{
					Renderer m = BMX_Materials["Front Rim"];


					//m.material.mainTexture = null;
					GUILayout.Label("Front Rim:");
					GUILayout.Space(5);
					GUILayout.Label("RGB");
					
					BMXNetLoadout.instance.FRimColour.x = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FRimColour.x, 0, 1);
					BMXNetLoadout.instance.FRimColour.y = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FRimColour.y, 0, 1);
					BMXNetLoadout.instance.FRimColour.z = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FRimColour.z, 0, 1);
					m.material.color = new Color(BMXNetLoadout.instance.FRimColour.x, BMXNetLoadout.instance.FRimColour.y, BMXNetLoadout.instance.FRimColour.z);
					GUILayout.Space(5);


					GUILayout.Label("Smoothness");
					BMXNetLoadout.instance.FRimSmooth = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FRimSmooth, 0, 1);
						GUILayout.Label("Metallic");
						BMXNetLoadout.instance.FRimMetallic = GUILayout.HorizontalSlider(BMXNetLoadout.instance.FRimMetallic, 0, 1);
						m.material.SetInt("_SmoothnessTextureChannel", 0);
					m.material.SetFloat("_Metallic", BMXNetLoadout.instance.FRimMetallic);
					m.material.SetFloat("_Glossiness", BMXNetLoadout.instance.FRimSmooth);


						GUILayout.Space(20);
				}

                }
				GUILayout.Space(20);





				Rrimtoggle = GUILayout.Toggle(Rrimtoggle, " Rear Rim ");
				if (Rrimtoggle)
				{
					GUILayout.Space(20);
					texturetoggle = GUILayout.Toggle(texturetoggle, " Rim Base ");
					if (texturetoggle)
					{
						if (GUILayout.Button("Remove Texture"))
						{
							BMX_Materials["Rear Rim"].material.mainTexture = null;
							BMXNetLoadout.instance.RRimTex = new byte[0];
							BMXNetLoadout.instance.RRimTexName = "";
						}
						currentarrayname = "RRim";
						Currentshowingarray = Rims;
					}


					NormalToggle = GUILayout.Toggle(NormalToggle, " Rim Normal ");
					if (NormalToggle)
					{
						if (GUILayout.Button("Remove Texture"))
						{
							Material m = BMX_Materials["Front Rim"].material;
							Material _m = BMX_Materials["Rear Rim"].material;


							m.EnableKeyword("_NORMALMAP");
							m.EnableKeyword("_NORMALMAP");


							m.SetTexture("_BumpMap", null);
							_m.SetTexture("_BumpMap", null);

							BMXNetLoadout.instance.FRimNormal = new byte[0];
							BMXNetLoadout.instance.TireNormalName = "";
						}
						currentarrayname = "RRim Normal";
						Currentshowingarray = RimNormals;
					}


					materialstoggle = GUILayout.Toggle(materialstoggle, " Edit Material ");
					if (materialstoggle)
					{
						Renderer m = BMX_Materials["Rear Rim"];


						//m.material.mainTexture = null;
						GUILayout.Label("Rear Rim:");
						GUILayout.Space(5);
						GUILayout.Label("RGB");
						BMXNetLoadout.instance.RRimColour.x = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RRimColour.x, 0, 1);
						BMXNetLoadout.instance.RRimColour.y = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RRimColour.y, 0, 1);
						BMXNetLoadout.instance.RRimColour.z = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RRimColour.z, 0, 1);
						m.material.color = new Color(BMXNetLoadout.instance.RRimColour.x, BMXNetLoadout.instance.RRimColour.y, BMXNetLoadout.instance.RRimColour.z);
						GUILayout.Space(5);


						GUILayout.Label("Smoothness");
						BMXNetLoadout.instance.RRimSmooth = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RRimSmooth, 0, 1);

						GUILayout.Label("Metallic");
						BMXNetLoadout.instance.RRimMetallic = GUILayout.HorizontalSlider(BMXNetLoadout.instance.RRimMetallic, 0, 1);

						m.material.SetInt("_SmoothnessTextureChannel", 0);
						m.material.SetFloat("_Metallic", BMXNetLoadout.instance.RRimMetallic);
						m.material.SetFloat("_Glossiness", BMXNetLoadout.instance.RRimSmooth);


					}

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
					Rider_Materials["pants_geo"].material.mainTexture = texArray[texNum];
					
		    }
				if (name == "shirt_geo")
				{
					Rider_Materials["shirt_geo"].material.mainTexture = texArray[texNum];

				}
				if (name == "shoes_geo")
				{
					Rider_Materials["shoes_geo"].material.mainTexture = texArray[texNum];

				}
				if (name == "Daryien_Body")
			{
					Rider_Materials["body_geo"].materials[1].mainTexture = texArray[texNum];

				}
			if (name == "Daryien_Head")
			{
					Rider_Materials["body_geo"].materials[0].mainTexture = texArray[texNum];

				}
			if (name == "Baseball Cap_R")
			{
					Rider_Materials["Baseball Cap_R"].material.mainTexture = texArray[texNum];
					Rider_Materials["Baseball Cap_R"].material.color = Color.white;
					
				}
			if (name == "Daryien_HandsFeet")
			{
					Rider_Materials["body_geo"].materials[2].mainTexture = texArray[texNum];


				}
				
			if(name == "Frames")
                {
					Material m = FrameRen.material;
					m.SetTexture("_MainTex",(Texture)texArray[texNum]);
					BMXNetLoadout.instance.FrameTex = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.FrameTexname = texArray[texNum].name;
				}
			if (name == "Forks")
			{
				Material m = BMX_Materials["Forks Mesh"].material;
					m.SetTexture("_MainTex", (Texture)texArray[texNum]);
					BMXNetLoadout.instance.ForksTex = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.ForkTexname = texArray[texNum].name;
			}
				if (name == "Stems")
				{
					Material m = BMX_Materials["Stem Mesh"].material;
					m.SetTexture("_MainTex", (Texture)texArray[texNum]);
					BMXNetLoadout.instance.StemTex = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.StemTexName = texArray[texNum].name;
				}

				if (name == "Bars")
				{
					Material m = BMX_Materials["Bars Mesh"].material;
					m.SetTexture("_MainTex", (Texture)texArray[texNum]);
					BMXNetLoadout.instance.BarsTex = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.BarTexName = texArray[texNum].name;
				}
				if (name == "Seats")
				{
					Material m = BMX_Materials["Seat Mesh"].material;
					m.SetTexture("_MainTex", (Texture)texArray[texNum]);
					BMXNetLoadout.instance.SeatTex = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.SeatTexname = texArray[texNum].name;
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
					BMXNetLoadout.instance.TireTexName = texArray[texNum].name;
				}

				if (name == "RRim")
				{
					Material m = BMX_Materials["Rear Rim"].material;
					m.SetTexture("_MainTex", (Texture)texArray[texNum]);
					BMXNetLoadout.instance.RRimTex = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.RRimTexName = texArray[texNum].name;
				}
				if (name == "FRim")
				{
					Material m = BMX_Materials["Front Rim"].material;
					m.SetTexture("_MainTex", (Texture)texArray[texNum]);
					BMXNetLoadout.instance.FRimTex = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.FRimTexName = texArray[texNum].name;
				}


				if (name == "Tires Normal")
				{

					Material m = BMX_Materials["Tire Mesh Front"].materials[1];
					Material _m = BMX_Materials["Tire Mesh Back"].materials[1];
					Material __m = BMX_Materials["Tire Mesh Front"].materials[0];
					Material ___m = BMX_Materials["Tire Mesh Back"].materials[0];

					BMX_Materials["Tire Mesh Front"].materials[1].EnableKeyword("_NORMALMAP"); 
					BMX_Materials["Tire Mesh Back"].materials[1].EnableKeyword("_NORMALMAP"); 
					BMX_Materials["Tire Mesh Front"].materials[0].EnableKeyword("_NORMALMAP");
					BMX_Materials["Tire Mesh Back"].materials[0].EnableKeyword("_NORMALMAP"); 

					m.SetTexture("_BumpMap", (Texture)texArray[texNum]);
					_m.SetTexture("_BumpMap", (Texture)texArray[texNum]);
					__m.SetTexture("_BumpMap", (Texture)texArray[texNum]);
					___m.SetTexture("_BumpMap", (Texture)texArray[texNum]);

					BMXNetLoadout.instance.TiresNormal = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.TireNormalName = texArray[texNum].name;
				}

				if (name == "Frames Normal")
				{

					Material m = BMX_Materials["framemesh"].material;


					m.EnableKeyword("_NORMALMAP");

					m.SetTexture("_BumpMap", (Texture)texArray[texNum]);
					
					

					BMXNetLoadout.instance.FrameNormal = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.FrameNormalName = texArray[texNum].name;
				}

				if (name == "Forks Normal")
				{

					Material m = BMX_Materials["Forks Mesh"].material;


					m.EnableKeyword("_NORMALMAP");

					m.SetTexture("_BumpMap", (Texture)texArray[texNum]);



					BMXNetLoadout.instance.ForksNormal = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.ForksNormalName = texArray[texNum].name;
				}

				if (name == "Bars Normal")
				{

					Material m = BMX_Materials["Bars Mesh"].material;


					m.EnableKeyword("_NORMALMAP");

					m.SetTexture("_BumpMap", (Texture)texArray[texNum]);



					BMXNetLoadout.instance.BarsNormal = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.BarsNormalName = texArray[texNum].name;
				}

				if (name == "Seat Normal")
				{

					Material m = BMX_Materials["Seat Mesh"].material;


					m.EnableKeyword("_NORMALMAP");

					m.SetTexture("_BumpMap", (Texture)texArray[texNum]);



					BMXNetLoadout.instance.SeatNormal = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.SeatNormalName = texArray[texNum].name;
				}

				if (name == "Stem Normal")
				{

					Material m = BMX_Materials["Stem Mesh"].material;


					m.EnableKeyword("_NORMALMAP");

					m.SetTexture("_BumpMap", (Texture)texArray[texNum]);



					BMXNetLoadout.instance.StemNormal = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.StemNormalName = texArray[texNum].name;
				}

				if (name == "FRim Normal")
				{

					Material m = BMX_Materials["Front Rim"].material;


					m.EnableKeyword("_NORMALMAP");

					m.SetTexture("_BumpMap", (Texture)texArray[texNum]);



					BMXNetLoadout.instance.FRimNormal = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.FRimNormalName = texArray[texNum].name;
				}

				if (name == "RRim Normal")
				{

					Material m = BMX_Materials["Rear Rim"].material;


					m.EnableKeyword("_NORMALMAP");

					m.SetTexture("_BumpMap", (Texture)texArray[texNum]);



					BMXNetLoadout.instance.RRimNormal = texArray[texNum].EncodeToPNG();
					BMXNetLoadout.instance.RRimNormalName = texArray[texNum].name;
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

		
		

		public int LoadBmxSetup()
        {
			FileStream file;
			BMXSaveData load;
			
			if (File.Exists(BmxSaveDir + "BMXSAVE.FrostyPreset")) file = File.OpenRead(BmxSaveDir + "BMXSAVE.FrostyPreset");
			else
			{
				
				Debug.Log("Couldnt find file in there");
				return 0;
			}


			BinaryFormatter bf = new BinaryFormatter();
			load = (BMXSaveData)bf.Deserialize(File.OpenRead(BmxSaveDir + "BMXSAVE.FrostyPreset"));
			file.Close();
			
            try
            {
			BMXNetLoadout.instance.FrameColour = new Vector3(load.FrameColour[0],load.FrameColour[1],load.FrameColour[2]);
			
			BMXNetLoadout.instance.FrameSmooth = load.FrameSmooth;
		    BMXNetLoadout.instance.FrameMetallic = load.FrameMetallic;
			BMXNetLoadout.instance.FrameTex = load.FrameTex;
				BMXNetLoadout.instance.FrameTexname = load.FrameTexname;
				BMXNetLoadout.instance.FrameNormal = load.FrameNormal;
				BMXNetLoadout.instance.FrameNormalName = load.FrameNormalname;



			BMXNetLoadout.instance.ForksColour = new Vector3(load.ForksColour[0], load.ForksColour[1],load.ForksColour[2]);
			
			BMXNetLoadout.instance.ForksSmooth = load.ForksSmooth;
			BMXNetLoadout.instance.ForksMetallic = load.Forksmetallic;
			BMXNetLoadout.instance.ForksTex = load.ForksTex;
				BMXNetLoadout.instance.ForkTexname = load.ForksTexname;
				BMXNetLoadout.instance.ForksNormal = load.ForksNormal;
				BMXNetLoadout.instance.ForksNormalName = load.ForksNormalname;


			BMXNetLoadout.instance.BarsColour = new Vector3(load.BarsColour[0],load.BarsColour[1],load.BarsColour[2]);
			
			BMXNetLoadout.instance.BarsSmooth = load.BarsSmooth;
			BMXNetLoadout.instance.BarsMetallic = load.BarsMetallic;
			BMXNetLoadout.instance.BarsTex = load.BarsTex;
				BMXNetLoadout.instance.BarTexName = load.BarsTexname;
				BMXNetLoadout.instance.BarsNormal = load.BarsNormal;
				BMXNetLoadout.instance.BarsNormalName = load.BarsNormalname;



				BMXNetLoadout.instance.SeatColour = new Vector3(load.SeatColour[0],load.SeatColour[1],load.SeatColour[2]);
			
			BMXNetLoadout.instance.SeatSmooth = load.SeatSmooth;
			BMXNetLoadout.instance.SeatTex = load.SeatTex;
				BMXNetLoadout.instance.SeatTexname = load.SeatTexname;
				BMXNetLoadout.instance.SeatNormal = load.SeatNormal;
				BMXNetLoadout.instance.SeatNormalName = load.SeatNormalname;


				BMXNetLoadout.instance.FTireColour = new Vector3(load.FTireColour[0], load.FTireColour[1],load.FTireColour[2]);
				
				BMXNetLoadout.instance.RTireColour = new Vector3(load.RTireColour[0],load.RTireColour[1],load.RTireColour[2]);
			
			BMXNetLoadout.instance.FTireSideColour = new Vector3(load.FTireSideColour[0],load.FTireSideColour[1],load.FTireSideColour[2]);
			
			BMXNetLoadout.instance.RTireSideColour = new Vector3(load.RTireSideColour[0],load.RTireSideColour[1],load.RTireSideColour[2]);
			
			BMXNetLoadout.instance.TiresTex = load.TiresTex;
			BMXNetLoadout.instance.TiresNormal = load.TiresNormal;
				BMXNetLoadout.instance.TireTexName = load.TiresTexname;
				BMXNetLoadout.instance.TireNormalName = load.TiresNormalname;



				BMXNetLoadout.instance.StemColour = new Vector3(load.StemColour[0],load.StemColour[1],load.StemColour[2]);
				BMXNetLoadout.instance.StemMetallic = load.StemMetallic;
				BMXNetLoadout.instance.StemSmooth = load.StemSmooth;
				BMXNetLoadout.instance.StemTex = load.StemTex;
				BMXNetLoadout.instance.StemTexName = load.StemTexName;
				BMXNetLoadout.instance.StemNormal = load.StemNormal;
				BMXNetLoadout.instance.StemNormalName = load.StemNormalName;

				BMXNetLoadout.instance.FRimColour = new Vector3(load.FRimColour[0], load.FRimColour[1], load.FRimColour[2]);
				BMXNetLoadout.instance.FRimMetallic= load.FRimMetallic;
				BMXNetLoadout.instance.FRimSmooth = load.FRimSmooth;
				BMXNetLoadout.instance.FRimTex = load.FRimTex;
				BMXNetLoadout.instance.FRimTexName = load.FRimTexName;
				BMXNetLoadout.instance.FRimNormal = load.FRimNormal;
				BMXNetLoadout.instance.FRimNormalName = load.FRimNormalName;


				BMXNetLoadout.instance.RRimColour = new Vector3(load.RRimColour[0], load.RRimColour[1], load.RRimColour[2]);
				BMXNetLoadout.instance.RRimMetallic = load.RRimMetallic;
				BMXNetLoadout.instance.RRimSmooth = load.RRimSmooth;
				BMXNetLoadout.instance.RRimTex = load.RRimTex;
				BMXNetLoadout.instance.RRimTexName = load.RRimTexName;
				BMXNetLoadout.instance.RRimNormal = load.RRimNormal;
				BMXNetLoadout.instance.RRimNormalName = load.RRimNormalName;



			

				
				//=============================================================================================== carry on from here
				

				FrameRen.material.SetInt("_SmoothnessTextureChannel", 0);
				FrameRen.material.SetFloat("_Metallic", BMXNetLoadout.instance.FrameMetallic);
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
				if (BMXNetLoadout.instance.FrameNormal.Length > 1)
				{
					Texture2D image = new Texture2D(2, 2);
					image.LoadImage(BMXNetLoadout.instance.FrameNormal);
					image.Apply();
					image.name = load.FrameNormalname;
					FrameRen.material.EnableKeyword("_NORMALMAP");
					FrameRen.material.SetTexture("_BumpMap", image);
					

				}
				else
				{
					FrameRen.material.EnableKeyword("_NORMALMAP");
					FrameRen.material.SetTexture("_BumpMap", null);
					BMXNetLoadout.instance.FrameNormalName = "e";
				}



				ForksRen.material.color = new Color(BMXNetLoadout.instance.ForksColour.x, BMXNetLoadout.instance.ForksColour.y, BMXNetLoadout.instance.ForksColour.z);
				ForksRen.material.SetInt("_SmoothnessTextureChannel", 0);
				ForksRen.material.SetFloat("_Metallic", BMXNetLoadout.instance.ForksMetallic);
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

				if (BMXNetLoadout.instance.ForksNormal.Length > 1)
				{
					Texture2D image = new Texture2D(2, 2);
					image.LoadImage(BMXNetLoadout.instance.ForksNormal);
					image.Apply();
					image.name = load.ForksNormalname;
					ForksRen.material.EnableKeyword("_NORMALMAP");
					ForksRen.material.SetTexture("_BumpMap", image);


				}
				else
				{
					ForksRen.material.EnableKeyword("_NORMALMAP");
					ForksRen.material.SetTexture("_BumpMap", null);
					BMXNetLoadout.instance.ForksNormalName = "e";
				}




				BarsRen.material.color = new Color(BMXNetLoadout.instance.BarsColour.x, BMXNetLoadout.instance.BarsColour.y, BMXNetLoadout.instance.BarsColour.z);
				BarsRen.material.SetInt("_SmoothnessTextureChannel", 0);
				BarsRen.material.SetFloat("_Metallic", BMXNetLoadout.instance.BarsMetallic);
				BarsRen.material.SetFloat("_Glossiness", BMXNetLoadout.instance.BarsSmooth);
			if (BMXNetLoadout.instance.BarsTex.Length > 1)
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


				if (BMXNetLoadout.instance.BarsNormal.Length > 1)
				{
					Texture2D image = new Texture2D(2, 2);
					image.LoadImage(BMXNetLoadout.instance.BarsNormal);
					image.Apply();
					image.name = load.BarsNormalname;
					BarsRen.material.EnableKeyword("_NORMALMAP");
					BarsRen.material.SetTexture("_BumpMap", image);


				}
				else
				{
					BarsRen.material.EnableKeyword("_NORMALMAP");
					BarsRen.material.SetTexture("_BumpMap", null);
					BMXNetLoadout.instance.BarsNormalName = "e";
				}



				SeatRen.material.color = new Color(BMXNetLoadout.instance.SeatColour.x, BMXNetLoadout.instance.SeatColour.y, BMXNetLoadout.instance.SeatColour.z);
				SeatRen.material.SetInt("_SmoothnessTextureChannel", 0);
				SeatRen.material.SetFloat("_Metallic", 0.0f);
				SeatRen.material.SetFloat("_Glossiness", 0.3f);
			if (BMXNetLoadout.instance.SeatTex.Length > 1)
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


				if (BMXNetLoadout.instance.SeatNormal.Length > 1)
				{
					Texture2D image = new Texture2D(2, 2);
					image.LoadImage(BMXNetLoadout.instance.SeatNormal);
					image.Apply();
					image.name = load.SeatNormalname;
					SeatRen.material.EnableKeyword("_NORMALMAP");
					SeatRen.material.SetTexture("_BumpMap", image);


				}
				else
				{
					SeatRen.material.EnableKeyword("_NORMALMAP");
					SeatRen.material.SetTexture("_BumpMap", null);
					BMXNetLoadout.instance.SeatNormalName = "e";
				}




				StemRen.material.color = new Color(BMXNetLoadout.instance.StemColour.x, BMXNetLoadout.instance.StemColour.y, BMXNetLoadout.instance.StemColour.z);
				StemRen.material.SetInt("_SmoothnessTextureChannel", 0);
				StemRen.material.SetFloat("_Metallic",BMXNetLoadout.instance.StemMetallic);
				StemRen.material.SetFloat("_Glossiness", BMXNetLoadout.instance.StemSmooth);
				if (BMXNetLoadout.instance.StemTex.Length > 1)
				{
					Texture2D image = new Texture2D(2, 2);
					image.LoadImage(BMXNetLoadout.instance.StemTex);
					image.Apply();
					image.name = load.StemTexName;
					StemRen.material.mainTexture = image;

				}
				else
				{
					StemRen.material.mainTexture = null;
					BMXNetLoadout.instance.StemTexName = "e";
				}


				if (BMXNetLoadout.instance.StemNormal.Length > 1)
				{
					Texture2D image = new Texture2D(2, 2);
					image.LoadImage(BMXNetLoadout.instance.StemNormal);
					image.Apply();
					image.name = load.StemNormalName;
					StemRen.material.EnableKeyword("_NORMALMAP");
					StemRen.material.SetTexture("_BumpMap", image);


				}
				else
				{
					StemRen.material.EnableKeyword("_NORMALMAP");
					StemRen.material.SetTexture("_BumpMap", null);
					BMXNetLoadout.instance.StemNormalName = "e";
				}




				FRimRen.material.color = new Color(BMXNetLoadout.instance.FRimColour.x, BMXNetLoadout.instance.FRimColour.y, BMXNetLoadout.instance.FRimColour.z);
				FRimRen.material.SetInt("_SmoothnessTextureChannel", 0);
				FRimRen.material.SetFloat("_Metallic", BMXNetLoadout.instance.FRimMetallic);
				FRimRen.material.SetFloat("_Glossiness", BMXNetLoadout.instance.FRimSmooth);
				if (BMXNetLoadout.instance.FRimTex.Length > 1)
				{
					Texture2D image = new Texture2D(2, 2);
					image.LoadImage(BMXNetLoadout.instance.FRimTex);
					image.Apply();
					image.name = load.FRimTexName;
					FRimRen.material.mainTexture = image;

				}
				else
				{
					FRimRen.material.mainTexture = null;
					BMXNetLoadout.instance.FRimTexName = "e";
				}


				if (BMXNetLoadout.instance.FRimNormal.Length > 1)
				{
					Texture2D image = new Texture2D(2, 2);
					image.LoadImage(BMXNetLoadout.instance.FRimNormal);
					image.Apply();
					image.name = load.FRimNormalName;
					FRimRen.material.EnableKeyword("_NORMALMAP");
					FRimRen.material.SetTexture("_BumpMap", image);


				}
				else
				{
					FRimRen.material.EnableKeyword("_NORMALMAP");
					FRimRen.material.SetTexture("_BumpMap", null);
					BMXNetLoadout.instance.FRimNormalName = "e";
				}





				RRimRen.material.color = new Color(BMXNetLoadout.instance.RRimColour.x, BMXNetLoadout.instance.RRimColour.y, BMXNetLoadout.instance.RRimColour.z);
				RRimRen.material.SetInt("_SmoothnessTextureChannel", 0);
				RRimRen.material.SetFloat("_Metallic", BMXNetLoadout.instance.RRimMetallic);
				RRimRen.material.SetFloat("_Glossiness", BMXNetLoadout.instance.RRimSmooth);
				if (BMXNetLoadout.instance.RRimTex.Length > 1)
				{
					Texture2D image = new Texture2D(2, 2);
					image.LoadImage(BMXNetLoadout.instance.RRimTex);
					image.Apply();
					image.name = load.RRimTexName;
					RRimRen.material.mainTexture = image;

				}
				else
				{
					RRimRen.material.mainTexture = null;
					BMXNetLoadout.instance.RRimTexName = "e";
				}


				if (BMXNetLoadout.instance.RRimNormal.Length > 1)
				{
					Texture2D image = new Texture2D(2, 2);
					image.LoadImage(BMXNetLoadout.instance.RRimNormal);
					image.Apply();
					image.name = load.RRimNormalName;
					RRimRen.material.EnableKeyword("_NORMALMAP");
					RRimRen.material.SetTexture("_BumpMap", image);


				}
				else
				{
					RRimRen.material.EnableKeyword("_NORMALMAP");
					RRimRen.material.SetTexture("_BumpMap", null);
					BMXNetLoadout.instance.RRimNormalName = "e";
				}













				FTireRen.materials[0].color = new Color(BMXNetLoadout.instance.FTireColour.x, BMXNetLoadout.instance.FTireColour.y, BMXNetLoadout.instance.FTireColour.z);
			RTireRen.materials[0].color = new Color(BMXNetLoadout.instance.RTireColour.x, BMXNetLoadout.instance.RTireColour.y, BMXNetLoadout.instance.RTireColour.z);
				FTireRen.materials[1].color = new Color(BMXNetLoadout.instance.FTireSideColour.x, BMXNetLoadout.instance.FTireSideColour.y, BMXNetLoadout.instance.FTireSideColour.z);
				RTireRen.materials[1].color = new Color(BMXNetLoadout.instance.RTireSideColour.x, BMXNetLoadout.instance.RTireSideColour.y, BMXNetLoadout.instance.RTireSideColour.z);


				if (BMXNetLoadout.instance.TiresTex.Length > 1)
			    {
				Texture2D image = new Texture2D(2, 2);
				image.LoadImage(BMXNetLoadout.instance.TiresTex);
				image.Apply();
					image.name = load.TiresTexname;
				FTireRen.materials[1].mainTexture = image;
					RTireRen.materials[1].mainTexture = image;
					FTireRen.materials[0].mainTexture = image;
					RTireRen.materials[0].mainTexture = image;
				}
                else
                {
					FTireRen.material.mainTexture = null;
					RTireRen.material.mainTexture = null;
					BMXNetLoadout.instance.TireTexName = "e";
				}

				if (BMXNetLoadout.instance.TiresNormal.Length > 1)
				{
					Texture2D image = new Texture2D(2, 2);
					image.LoadImage(BMXNetLoadout.instance.TiresNormal);
					image.Apply();
					image.name = load.TiresNormalname;

					FTireRen.materials[1].EnableKeyword("_NORMALMAP");
					RTireRen.materials[1].EnableKeyword("_NORMALMAP");
					FTireRen.materials[0].EnableKeyword("_NORMALMAP");
					RTireRen.materials[0].EnableKeyword("_NORMALMAP");

					FTireRen.materials[1].SetTexture("_BumpMap",image);
					RTireRen.materials[1].SetTexture("_BumpMap", image);
					FTireRen.materials[0].SetTexture("_BumpMap", image);
					RTireRen.materials[0].SetTexture("_BumpMap", image);
				}
				else
				{
					FTireRen.materials[1].SetTexture("_BumpMap", null);
					RTireRen.materials[1].SetTexture("_BumpMap", null);
					FTireRen.materials[0].SetTexture("_BumpMap", null);
					RTireRen.materials[0].SetTexture("_BumpMap", null);
					BMXNetLoadout.instance.TireNormalName = "e";
				
				}
				
				Debug.Log("Loaded BMX!");
            }
            catch (UnityException x)
            {
				Debug.Log("Didnt load bmx: " + x);

            }
			
			return 1;

		}



		public void SaveBmxSetup()
        {
			saving = true;

            try
            {
			BMXSaveData Data = new BMXSaveData();
			
			Data.FrameColour = new float[3];
			Data.FrameColour[0] = BMXNetLoadout.instance.FrameColour.x;
			Data.FrameColour[1] = BMXNetLoadout.instance.FrameColour.y;
			Data.FrameColour[2] = BMXNetLoadout.instance.FrameColour.z;
			Data.FrameSmooth = BMXNetLoadout.instance.FrameSmooth;
			Data.FrameMetallic = BMXNetLoadout.instance.FrameMetallic;
			Data.FrameTex = BMXNetLoadout.instance.FrameTex;
			Data.FrameTexname = BMXNetLoadout.instance.FrameTexname;
				Data.FrameNormal = BMXNetLoadout.instance.FrameNormal;
				Data.FrameNormalname = BMXNetLoadout.instance.FrameNormalName;

			Data.ForksColour = new float[3];
			Data.ForksColour[0] = BMXNetLoadout.instance.ForksColour.x;
			Data.ForksColour[1] = BMXNetLoadout.instance.ForksColour.y;
			Data.ForksColour[2] = BMXNetLoadout.instance.ForksColour.z;
			Data.ForksSmooth = BMXNetLoadout.instance.ForksSmooth;
			Data.Forksmetallic = BMXNetLoadout.instance.ForksMetallic;
			Data.ForksTex = BMXNetLoadout.instance.ForksTex;
				Data.ForksTexname = BMXNetLoadout.instance.ForkTexname;
				Data.ForksNormal = BMXNetLoadout.instance.ForksNormal;
				Data.ForksNormalname = BMXNetLoadout.instance.ForksNormalName;

				Data.BarsColour = new float[3];
			Data.BarsColour[0] = BMXNetLoadout.instance.BarsColour.x;
			Data.BarsColour[1] = BMXNetLoadout.instance.BarsColour.y;
			Data.BarsColour[2] = BMXNetLoadout.instance.BarsColour.z;
			Data.BarsSmooth = BMXNetLoadout.instance.BarsSmooth;
			Data.BarsMetallic = BMXNetLoadout.instance.BarsMetallic;
			Data.BarsTex = BMXNetLoadout.instance.BarsTex;
				Data.BarsTexname = BMXNetLoadout.instance.BarTexName;
				Data.BarsNormal = BMXNetLoadout.instance.BarsNormal;
				Data.BarsNormalname = BMXNetLoadout.instance.BarsNormalName;



				Data.SeatColour = new float[3];
			Data.SeatColour[0] = BMXNetLoadout.instance.SeatColour.x;
			Data.SeatColour[1] = BMXNetLoadout.instance.SeatColour.y;
			Data.SeatColour[2] = BMXNetLoadout.instance.SeatColour.z;
			Data.SeatSmooth = BMXNetLoadout.instance.SeatSmooth;
			Data.SeatTex = BMXNetLoadout.instance.SeatTex;
				Data.SeatTexname = BMXNetLoadout.instance.SeatTexname;
				Data.SeatNormal = BMXNetLoadout.instance.SeatNormal;
				Data.SeatNormalname = BMXNetLoadout.instance.SeatNormalName;



				Data.StemColour = new float[3];
				Data.StemColour[0] = BMXNetLoadout.instance.StemColour.x;
				Data.StemColour[1] = BMXNetLoadout.instance.StemColour.y;
				Data.StemColour[2] = BMXNetLoadout.instance.StemColour.z;
				Data.StemSmooth = BMXNetLoadout.instance.StemSmooth;
				Data.StemTex = BMXNetLoadout.instance.StemTex;
				Data.StemMetallic = BMXNetLoadout.instance.StemMetallic;
				Data.StemTexName = BMXNetLoadout.instance.StemTexName;
				Data.StemNormal = BMXNetLoadout.instance.StemNormal;
				Data.StemNormalName = BMXNetLoadout.instance.StemNormalName;





				Data.FRimColour = new float[3];
				Data.FRimColour[0] = BMXNetLoadout.instance.FRimColour.x;
				Data.FRimColour[1] = BMXNetLoadout.instance.FRimColour.y;
				Data.FRimColour[2] = BMXNetLoadout.instance.FRimColour.z;
				Data.FRimSmooth = BMXNetLoadout.instance.FRimSmooth;
				Data.FRimMetallic = BMXNetLoadout.instance.FRimMetallic;
				Data.FRimTex = BMXNetLoadout.instance.FRimTex;
				Data.FRimTexName = BMXNetLoadout.instance.FRimTexName;
				Data.FRimNormal = BMXNetLoadout.instance.FRimNormal;
				Data.FRimNormalName = BMXNetLoadout.instance.FRimNormalName;



				Data.RRimColour = new float[3];
				Data.RRimColour[0] = BMXNetLoadout.instance.RRimColour.x;
				Data.RRimColour[1] = BMXNetLoadout.instance.RRimColour.y;
				Data.RRimColour[2] = BMXNetLoadout.instance.RRimColour.z;
				Data.RRimSmooth = BMXNetLoadout.instance.RRimSmooth;
				Data.RRimMetallic = BMXNetLoadout.instance.RRimMetallic;
				Data.RRimTex = BMXNetLoadout.instance.RRimTex;
				Data.RRimTexName = BMXNetLoadout.instance.RRimTexName;
				Data.RRimNormal = BMXNetLoadout.instance.RRimNormal;
				Data.RRimNormalName = BMXNetLoadout.instance.RRimNormalName;



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
			
			Data.TiresTexname = BMXNetLoadout.instance.TireTexName;
			Data.TiresNormalname = BMXNetLoadout.instance.TireNormalName;

			if (!Directory.Exists(BmxSaveDir))
			{
				Directory.CreateDirectory(BmxSaveDir);
			}

		

			
			
			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(File.OpenWrite(BmxSaveDir + "BMXSAVE.FrostyPreset"), Data);
			
			Debug.Log("Completed Saving BMX");

			saving = false;

            }
            catch (System.Exception x)
            {
				Debug.Log("Already saving" + x);
				saving = false;
            }

         




		}

		public void SaveRiderSetup()
        {
			RiderSaveData data = new RiderSaveData();
			data.Shirtbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["shirt_geo"].material.mainTexture);
			data.shirtimagename = Rider_Materials["shirt_geo"].material.mainTexture.name;
			data.shirtParentname = "shirt_geo";

			data.bottomsbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["pants_geo"].material.mainTexture);
			data.bottomsimagename = Rider_Materials["pants_geo"].material.mainTexture.name;
			data.bottomsParentname = "pants_geo";

			data.shoesbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["shoes_geo"].material.mainTexture);
			data.shoesimagename = Rider_Materials["shoes_geo"].material.mainTexture.name;
			data.shoesParentname = "shoes_geo";

			data.hatbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["Baseball Cap_R"].material.mainTexture);
			data.hatimagename = Rider_Materials["Baseball Cap_R"].material.mainTexture.name;
			data.hatParentname = "Baseball Cap_R";

			data.headbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["body_geo"].materials[0].mainTexture);
			data.headimagename = Rider_Materials["body_geo"].materials[0].mainTexture.name;
			data.headParentname = "body_geo";

			data.bodybytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["body_geo"].materials[1].mainTexture);
			data.bodyimagename = Rider_Materials["body_geo"].materials[1].mainTexture.name;
			data.bodyParentname = "body_geo";

			data.handsfeetbytes = ImageConversion.EncodeToPNG((Texture2D)Rider_Materials["body_geo"].materials[2].mainTexture);
			data.handsfeetimagename = Rider_Materials["body_geo"].materials[2].mainTexture.name;
			data.handsfeetParentname = "body_geo";


			if (!Directory.Exists(RidersaveDir))
			{
				Directory.CreateDirectory(RidersaveDir);
			}



			


			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(File.OpenWrite(RidersaveDir + "RiderSave.FrostyPreset"), data);

			Debug.Log("Completed Saving Rider");



		}


		public int LoadRiderSetup()
        {
			
			RiderSaveData load;

			if (File.Exists(RidersaveDir + "RiderSave.FrostyPreset"))
            {
			BinaryFormatter bf = new BinaryFormatter();
			load = (RiderSaveData)bf.Deserialize(File.OpenRead(RidersaveDir + "RiderSave.FrostyPreset"));
			


			Texture2D Shirttex = new Texture2D(1024, 1024);
			Texture2D bottomstex = new Texture2D(1024, 1024);
			Texture2D shoestex = new Texture2D(1024, 1024);
			Texture2D hattex = new Texture2D(1024, 1024);
			Texture2D headtex = new Texture2D(1024, 1024);
			Texture2D bodytex = new Texture2D(1024, 1024);
			Texture2D handsfeettex = new Texture2D(1024, 1024);

			ImageConversion.LoadImage(Shirttex, load.Shirtbytes);
			ImageConversion.LoadImage(bottomstex, load.bottomsbytes);
			ImageConversion.LoadImage(shoestex, load.shoesbytes);
			ImageConversion.LoadImage(hattex, load.hatbytes);
			ImageConversion.LoadImage(headtex, load.headbytes);
			ImageConversion.LoadImage(bodytex, load.bodybytes);
			ImageConversion.LoadImage(handsfeettex, load.handsfeetbytes);


			Shirttex.name = load.shirtimagename;
			bottomstex.name = load.bottomsimagename;
			shoestex.name = load.shoesimagename;
			headtex.name = load.headimagename;
			bodytex.name = load.bodyimagename;
			handsfeettex.name = load.handsfeetimagename;
			hattex.name = load.hatimagename;



			Rider_Materials["shirt_geo"].material.mainTexture = Shirttex;
			Rider_Materials["pants_geo"].material.mainTexture = bottomstex;
			Rider_Materials["shoes_geo"].material.mainTexture = shoestex;
			Rider_Materials["Baseball Cap_R"].material.mainTexture = hattex;
	        Rider_Materials["Baseball Cap_R"].material.color = Color.white;
			Rider_Materials["body_geo"].materials[0].mainTexture = headtex;
			Rider_Materials["body_geo"].materials[1].mainTexture = bodytex;
			Rider_Materials["body_geo"].materials[2].mainTexture = handsfeettex;

			return 1;
            }
            else
            {
				return 0;
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

			if (NormalToggle)
			{
				GUILayout.BeginArea(new Rect(new Vector2(imagesidebuffer, 0f), new Vector2(imagewidth, (float)Screen.height)));
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, new GUILayoutOption[0]);
				GUILayout.BeginVertical("Box", new GUILayoutOption[0]);
				for (int T = 0; T < Currentshowingarray.Length; T++)
				{
					ShowATexButton(T, Currentshowingarray, currentarrayname);
				}


				GUILayout.EndVertical();
				GUILayout.EndScrollView();
				GUILayout.EndArea();
			}








		}



		/// <summary>
		/// After mash have done there thing
		/// </summary>
		/// <returns></returns>
		IEnumerator Initialiseafterwait()
		{
			// makes sure bike is ready without having to save it manually
			yield return new WaitForSeconds(1);
			LoadBmxSetup();
			SaveBmxSetup();
			
			yield return null;
		}






		}
}
