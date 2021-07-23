using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PIPE_Valve_Console_Client;

namespace FrostyP_Game_Manager
{

	public class FrostyPGamemanager : MonoBehaviour
	{
		public static FrostyPGamemanager instance;
		
        bool consoleon = false;
		
		
		// Menu Controller
		delegate void Menu();
		Dictionary<int, Menu> Menus = new Dictionary<int, Menu>();
		public int MenuShowing;



        
		// GUI
		public GUISkin skin = ScriptableObject.CreateInstance<GUISkin>();
		public GUIStyle Generalstyle = new GUIStyle();
		public GameObject Onlineobj;
		GUIStyle OffStyle = new GUIStyle();
		GUIStyle OnStyle = new GUIStyle();
		

		// menu paramters
		public bool OpenMenu;


		
		

		void Start()
		{
			

			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				Debug.Log("Instance already exists, destroying object!");
				Destroy(this);
			}

			Menus = new Dictionary<int, Menu>()
			{
				{1,CharacterModding.instance.RiderSetupShow},
				{2,Teleport.instance.Show},
				{3,CameraSettings.instance.CameraSettingsShow},
				{4,RiderPhysics.instance.RiderPhysicsShow},
				{5,InGameUI.instance.OnlineShow },

			};

			
			DirectoryInfo Presetdirectoryinfo = new DirectoryInfo(RiderPhysics.instance.PresetDirectory);

			if (!Presetdirectoryinfo.Exists)
			{
				Presetdirectoryinfo.Create();
			}


			OffStyle.alignment = TextAnchor.MiddleCenter;
			OffStyle.normal.textColor = Color.black;
			OffStyle.normal.background = InGameUI.instance.GreenTex;
			OffStyle.hover.textColor = Color.green;
			OffStyle.hover.background = InGameUI.instance.GreyTex;

			OnStyle.alignment = TextAnchor.MiddleCenter;
			OnStyle.normal.textColor = Color.green;
			OnStyle.normal.background = InGameUI.instance.GreyTex;
			OnStyle.hover.textColor = Color.red;
			OnStyle.hover.background = InGameUI.instance.GreenTex;







			Generalstyle.normal.background = InGameUI.instance.whiteTex; 
			Generalstyle.normal.textColor = Color.black;

			Generalstyle.alignment = TextAnchor.MiddleCenter;
			Generalstyle.fontStyle = FontStyle.Bold;
			//Generalstyle.fontSize = 16;
			//Generalstyle.border.left = 5;
			//Generalstyle.border.right = 5;
			//Generalstyle.margin.left = 5;
			//Generalstyle.margin.right = 5;

			skin.label.normal.textColor = Color.black;
			skin.label.fontSize = 13;
			skin.label.fontStyle = FontStyle.Bold;
			skin.label.alignment = TextAnchor.MiddleCenter;
			skin.label.normal.background = InGameUI.instance.TransTex;



			skin.textField.alignment = TextAnchor.MiddleCenter;
			skin.textField.normal.textColor = Color.red;
			skin.textField.hover.textColor = Color.white;
			skin.textField.normal.background = Texture2D.whiteTexture;
			skin.textField.focused.background = Texture2D.blackTexture;
			skin.textField.focused.textColor = Color.white;
			skin.textField.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
			skin.textField.padding = new RectOffset(5, 5, 2, 2);



			skin.button.alignment = TextAnchor.MiddleCenter;
			skin.button.normal.textColor = Color.black;
			skin.button.normal.background = InGameUI.instance.GreenTex;
			skin.button.hover.textColor = Color.green;
			skin.button.hover.background = InGameUI.instance.GreyTex;
			skin.button.normal.background.wrapMode = TextureWrapMode.Clamp;
			skin.button.onNormal.background = InGameUI.instance.RedTex;
			



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
			skin.horizontalSliderThumb.fixedWidth = 10;
			skin.horizontalSliderThumb.fixedHeight = 13;
			skin.horizontalSlider.fixedHeight = 10;
			//skin.horizontalSlider.padding = new RectOffset(10, 10, 0, 0);
			skin.horizontalSliderThumb.hover.background = InGameUI.instance.BlackTex;

			
			skin.verticalScrollbarThumb.normal.background = InGameUI.instance.GreenTex;
			skin.verticalScrollbarThumb.hover.background = InGameUI.instance.GreyTex;
			//skin.verticalScrollbarThumb.fixedHeight = 20;
			skin.verticalScrollbarThumb.fixedWidth = 14;
			skin.verticalScrollbarThumb.normal.background.wrapMode = TextureWrapMode.Clamp;
			skin.verticalScrollbar.alignment = TextAnchor.MiddleRight;
			skin.verticalScrollbarThumb.alignment = TextAnchor.MiddleRight;

			skin.scrollView.normal.background = InGameUI.instance.whiteTex;
			skin.scrollView.alignment = TextAnchor.UpperCenter;
			skin.verticalSliderThumb.normal.background = InGameUI.instance.GreenTex;
			skin.verticalSliderThumb.hover.background = InGameUI.instance.BlackTex;
			//skin.verticalSliderThumb.fixedHeight = 20;
			skin.verticalSliderThumb.fixedWidth = 14;
			skin.verticalSlider.alignment = TextAnchor.MiddleRight;
			skin.verticalSliderThumb.alignment = TextAnchor.MiddleRight;

			skin.box.padding = new RectOffset(15, 15, 0, 0);

			OpenMenu = true;
		}


		private void Update()
		{
			/// toggle menu with G
			if (Input.GetKeyDown(KeyCode.G))
			{
				OpenMenu = !OpenMenu;
			}


		}



		public void OnGUI()
		{
			
			if (OpenMenu)
			{

				

				GUI.skin = skin;

				
				GUILayout.Space(20);

				GUILayout.BeginArea(new Rect(new Vector2(Screen.width/4,10),new Vector2(Screen.width/2,Screen.height/20)));
				GUILayout.BeginHorizontal();
				
				GUILayout.Label($"PIPE Manager - {PIPE_Valve_Console_Client.GameNetworking.instance.VERSIONNUMBER}", Generalstyle);
				GUILayout.Space(10);
				GUILayout.Label("G to toggle Menu: L to toggle Patcha", Generalstyle);
				GUILayout.EndHorizontal();
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();

				///// CONSOLE: when its attached in FrostyPgamemanager.Main it can be toggled with this, when toggled on it records to Desktop/Your_logs, spacebar to see it recording
				consoleon = GUILayout.Toggle(consoleon, "Debug mode");
				if (!this.gameObject.GetComponent<Consolelog>())
				{
					this.gameObject.AddComponent<Consolelog>().enabled = false;

				}
				if (consoleon && this.gameObject.GetComponent<Consolelog>())
				{
					this.gameObject.GetComponent<Consolelog>().enabled = true;

				}
				if (!consoleon && this.gameObject.GetComponent<Consolelog>())
				{
					this.gameObject.GetComponent<Consolelog>().enabled = false;

				}

				GUILayout.Space(5);
				// parkbuilder tab
				if (GUILayout.Button("Park Builder"))
                {
					GetComponent<ParkBuilder>().Open(LocalPlayer.instance.DaryienOriginal.transform.position);
					OpenMenu = false;
                }
				GUILayout.Space(5);
				// Replay mode
				if (GUILayout.Button("Replay"))
                {
					ReplayMode.instance.Open();
					OpenMenu = false;
                }
				GUILayout.Space(5);
				// Rider Setup Tab
				if (GUILayout.Button("Rider"))
				{
					
					GUILayout.Space(20);
					GUILayout.Label("Rider Clothing:", Generalstyle);
					CharacterModding.instance.RiderSetupOpen();
					GUILayout.Space(20);
				}
				GUILayout.Space(5);
				// Bmx Setup Tab
				if (GUILayout.Button("Teleport"))
                {
					
					GUILayout.Space(20);
					GUILayout.Label("Teleport:", Generalstyle);
					Teleport.instance.Open();
					GUILayout.Space(20);
                }
				GUILayout.Space(5);
				// camera tab
				if (GUILayout.Button("Camera"))
                {
					
					MenuShowing = 3;
                }
				GUILayout.Space(5);
				// rider tab
				if (GUILayout.Button("Physics"))
                {
					

					MenuShowing = 4;
                }
				GUILayout.Space(5);
				// online tab
				if (GUILayout.Button("Online"))
                {
					
					MenuShowing = 5;
                }


				GUILayout.EndHorizontal();

				GUILayout.EndArea();


                if (MenuShowing != 0)
                {
				 Menus[MenuShowing]();
                }

			}



		}


	}
}







		






       