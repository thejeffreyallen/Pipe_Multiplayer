using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using PIPE_Valve_Console_Client;

namespace FrostyP_Game_Manager
{

	public class FrostyPGamemanager : MonoBehaviour
	{
		public static FrostyPGamemanager instance;

		public GUIStyle style;
		Vector2 popupscroll;


		// Menu Controller
		delegate void Menu();
		Dictionary<int, Menu> Menus = new Dictionary<int, Menu>();
		public int MenuShowing;

		// GUI
		public GUIStyle Generalstyle = new GUIStyle();
		public GameObject Onlineobj;
		
		// menu paramters
		public bool OpenMenu;

		List<string> popups = new List<string>();
		bool Popup;
		Dictionary<int, string> ScreenModeDisplay = new Dictionary<int, string>();
		int currentscreen = 0;


		void Start()
		{
			
			style = new GUIStyle();
			style.normal.background = InGameUI.instance.BlackTex;
			style.alignment = TextAnchor.MiddleCenter;
			style.normal.textColor = Color.white;
			style.fontStyle = FontStyle.Bold;
			style.hover.background = InGameUI.instance.GreenTex;
			style.hover.textColor = Color.black;
			style.onNormal.background = InGameUI.instance.RedTex;
			style.onHover.background = InGameUI.instance.GreenTex;

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
				{1,CharacterModding.instance.Show},
				{2,Teleport.instance.Show},
				{3,CameraSettings.instance.Show},
				{4,RiderPhysics.instance.Show},
				{5,InGameUI.instance.Show },
				{6,ShowAbout },

			};


			
			Generalstyle.normal.background = InGameUI.instance.whiteTex; 
			Generalstyle.normal.textColor = Color.black;

			Generalstyle.alignment = TextAnchor.MiddleCenter;
			Generalstyle.fontStyle = FontStyle.Bold;

			ScreenModeDisplay = new Dictionary<int, string>
			{
				{0,"Exclusive" },
				{1,"Full screen" },
				{2,"Max window" },
				{3,"Windowed" },
			};
			
			OpenMenu = true;
		}

		private void Update()
		{
			/// toggle menu with G
			if (Input.GetKeyDown(KeyCode.G))
			{
				OpenMenu = !OpenMenu;
                if (MainManager.instance.isOpen)
                {
					OpenMenu = false;
                }
				if (InGameUI.instance.Connected && InGameUI.instance.Minigui)
				{
					InGameUI.instance.Minigui = false;
					InGameUI.instance.OnlineMenu = true;
				}


			}

			// Teleport shortcut
			if (MGInputManager.LeftStick_Down())
            {
                if (MGInputManager.LTrigger() > 0.5f)
                {
                    if (MGInputManager.RTrigger() > 0.5f)
                    {
                        if (MGInputManager.RightStick_Hold())
                        {
							Teleport.instance.Open();
                        }
                    }
                }
            }

            

		}

		public void OnGUI()
		{
			
			if (OpenMenu)
			{
                try
                {
				GUI.skin = InGameUI.instance.skin;
				

				GUILayout.Space(2);

				GUILayout.BeginArea(new Rect(new Vector2(Screen.width/4,5),new Vector2(Screen.width/2,Screen.height/50)));
				GUILayout.BeginHorizontal();
				
				GUILayout.Space(5);
				if (GUILayout.Button("About", style))
				{
					if(MenuShowing != 6)
                    {
					MenuShowing = 6;
                    }
                    else
                    {
						MenuShowing = 0;
                    }
				}
				GUILayout.Space(5);
				if (GUILayout.Button("Debugger", style))
                {
			      gameObject.GetComponent<Consolelog>().enabled = !gameObject.GetComponent<Consolelog>().enabled;
                }
				GUILayout.Space(5);
				// parkbuilder tab
				if (GUILayout.Button("Park Builder",style))
                {
					GetComponent<ParkBuilder>().Open(LocalPlayer.instance.DaryienOriginal.transform.position);
					OpenMenu = false;
                }
				GUILayout.Space(5);
				// Replay mode
				if (GUILayout.Button("Replay", style))
                {
					ReplayMode.instance.Open();
					OpenMenu = false;
                }
				GUILayout.Space(5);
				// Rider Setup Tab
				if (GUILayout.Button("Rider", style))
				{
					if(LocalPlayer.instance.RiderRoot.GetComponentsInChildren<Transform>().Length < 70)
                    {
					if(MenuShowing != 1)
                    {
					CharacterModding.instance.RiderSetupOpen();
                    }
                    else
                    {
						CharacterModding.instance.Close();
                    }

                    }
                    else
                    {
						PopUpMessage(("Rider setup only available for Daryien"));
                    }
				}
				GUILayout.Space(5);
				// Bmx Setup Tab
				if (GUILayout.Button("Teleport", style))
                {
					
					GUILayout.Space(20);
					GUILayout.Label("Teleport:", Generalstyle);
					Teleport.instance.Open();
					GUILayout.Space(20);
                }
				GUILayout.Space(5);
				// camera tab
				if (GUILayout.Button("Camera", style))
                {
					
					if(MenuShowing != 3)
                    {
					MenuShowing = 3;

                    }
					else
					{
					MenuShowing = 0;
					}
				}
				GUILayout.Space(5);
				// rider tab
				if (GUILayout.Button("Physics", style))
                {
					if(MenuShowing != 4)
                    {
					MenuShowing = 4;
                    }
					else
					{
						MenuShowing = 0;
					}
				}
				GUILayout.Space(5);
				// online tab
				if (GUILayout.Button("Online", style))
                {
					if(MenuShowing != 5)
                    {
					MenuShowing = 5;
                    }
                    else
                    {
					MenuShowing = 0;
                    }
                }


				GUILayout.EndHorizontal();

				GUILayout.EndArea();


                if (MenuShowing != 0)
                {
				 Menus[MenuShowing]();
                }

                if (Popup)
                {
					PopupShow();
                }

                }
                catch (System.Exception x)
                {
					Debug.Log($"Main Frosty Manager picked up GUI error : " + x);

                }



			}



		}

		public void ShowAbout()
        {
			GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 6, 10 + (Screen.height/40)), new Vector2(Screen.width / 3 * 2, Screen.height / 2)));
			GUILayout.BeginHorizontal();

			GUILayout.Label($"PIPE Manager : {PIPE_Valve_Console_Client.GameNetworking.instance.VERSIONNUMBER}", Generalstyle);
			GUILayout.Space(10);
			GUILayout.Label("G toggles Menu: L toggles Patcha, B toggles Volution Garage, P toggles Pipeworks Player Importer", Generalstyle);
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			if(GUILayout.Button($"Screen Mode: {ScreenModeDisplay[currentscreen]}"))
            {
				Debug.Log($"Change screen from {ScreenModeDisplay[currentscreen]}");
				SetFullScreen(Screen.fullScreen);
				

			}
			GUILayout.EndHorizontal();
			GUILayout.Space(20);
			GUILayout.Label("Creating a Server Basic Setup");
			GUILayout.Space(10);
			GUILayout.Label("1) Start Server App, ensure it's allowed through you Firewall", Generalstyle);
			GUILayout.Label("2) Enter a player limit, a tick rate (60 - 120), any port number (7777 standard) and any password, then press enter to boot", Generalstyle);
			GUILayout.Label("3) In game, if Server is on your PC connect to 127.0.0.1(this PC), with chosen port number", Generalstyle);
			GUILayout.Label("You should then see the Server app responding and logging your data, Your Online", Generalstyle);
			GUILayout.Space(10);
			GUILayout.Label("If the server is on your local network, connect to Server Pc's local IP (192.168.???.???) found in Server Pc's Network Settings (IPv4)", Generalstyle);
			GUILayout.Label("Any other local players connected to your LAN can also connect to Server Pc's local IP", Generalstyle);
			GUILayout.Space(10);

			GUILayout.Label("4) To Accept Remote players, you must tell your router that any data received on chosen port should be directed straight to Local IP of Server Pc", Generalstyle);
			GUILayout.Label("This option should be called PortForwarding in your Router's Browser App", Generalstyle);
			GUILayout.Label("PortForwarding allows anyone through your Routers security and straight on to your Pc's Firewalls, security etc", Generalstyle);
			GUILayout.Label("Sharing an open Socket (Ip and port combination) publically without protection should be avoided", Generalstyle);
			GUILayout.Label("Even if sharing with trusted people, i'd advise being comfortable with turning the port forward off when not in use and varying the Port used", Generalstyle);
			GUILayout.Space(10);
			GUILayout.Label("Once you have a port forward setup, Remote players can connect to your Router's External IP with chosen Port number", Generalstyle);

			GUILayout.EndArea();

		}

		void SetFullScreen(bool fullScreenValue)
		{
			if (!fullScreenValue)
			{
				Resolution resolution = Screen.currentResolution;
				Screen.SetResolution(resolution.width, resolution.height, fullScreenValue);
			}
            else
            {
			 Screen.fullScreen = fullScreenValue;

            }


			try
			{

				if (currentscreen == 3)
				{
					currentscreen = 0;
				}
				else
				{
					currentscreen++;
				}

				if (currentscreen != 0 && currentscreen != 1)
				{
					Screen.fullScreen = false;
				}
				Screen.fullScreenMode = (FullScreenMode)currentscreen;
				Debug.Log($"to screen {ScreenModeDisplay[currentscreen]}");
			}
			catch (System.Exception x)
			{
				Debug.Log("Screen mode :  " + x);
			}

		}

		public void PopUpMessage(string message)
        {
			popups.Add(message);
			Popup = true;
			StartCoroutine(PopUpMessageEnum(message));
        }

		IEnumerator PopUpMessageEnum(string mess)
        {
			yield return new WaitForSeconds(2);
			popups.Remove(mess);
			Popup = false;

        }

		void PopupShow()
        {
            if (popups.Count > 0)
            {
				GUILayout.BeginArea(new Rect(new Vector2(Screen.width/8*3,Screen.height/8*3), new Vector2(Screen.width/5,150)),InGameUI.BoxStyle);
				popupscroll = GUILayout.BeginScrollView(popupscroll);
                for (int i = 0; i < popups.Count; i++)
                {
				GUILayout.Label(popups[i]);
                }
				GUILayout.EndScrollView();
				GUILayout.EndArea();

            }
        }


	}
}







		






       