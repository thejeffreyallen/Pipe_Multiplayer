using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrostyP_Game_Manager
{
    public class ConfigMenu : FrostyModule
    {
		Dictionary<int, string> ScreenModeDisplay = new Dictionary<int, string>();
		int currentscreen = 0;

		void Awake()
        {
			Buttontext = "Config";
			ScreenModeDisplay = new Dictionary<int, string>
			{
				{0,"Exclusive" },
				{1,"Full screen" },
				{2,"Max window" },
				{3,"Windowed" },
			};
		}

		public override void Show()
        {
			GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 6, 10 + (Screen.height / 40)), new Vector2(Screen.width / 3 * 2, Screen.height / 2)));
			GUILayout.BeginHorizontal();

			GUILayout.Label($"PIPE Manager : {RiptideManager.instance.VERSIONNUMBER}", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Space(10);
			GUILayout.Label("G toggles Menu: L toggles Patcha, B toggles Volution Garage, P toggles Pipeworks Player Importer", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.EndHorizontal();
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button($"Screen Mode: {ScreenModeDisplay[currentscreen]}"))
			{
				Debug.Log($"Change screen from {ScreenModeDisplay[currentscreen]}");
				SetFullScreen(Screen.fullScreen);
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(20);
			GUILayout.Label("Creating a Server Basic Setup");
			GUILayout.Space(10);
			GUILayout.Label("1) Start Server App, ensure it's allowed through you Firewall", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Space(10);
			GUILayout.Label("2) Do intial boot up", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Space(10);
			GUILayout.Label("4) optionally open port for Webportal, default 5001, ideally only for your router's IP", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Label("3) Change configuration as needed in Config/ServerConfig or through the Webportal", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Label("If the server is on your local network, connect to Server Pc's local IP (192.168.???.???)", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Label("Any other local players connected to your LAN can also connect to Server Pc's local IP", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Space(10);

			GUILayout.Label("5) To Accept Remote players, you must forward a port from your router to the server pc", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Label("Once you have a port forward setup, Remote players can connect either using your Router's External IP with your chosen Port number", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Label("or through the public server menu if your server is posting to a hub", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Space(20);
			GUILayout.Label("Network information:", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Label("TransformDump: Per second, how many player position updates are being thrown away \n after use instead of returning to pool for reuse, upping the transformPool value in config may help performance", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Label("AudioDump: Per second, how many player audio updates are being thrown away \n after use instead of returning to pool for reuse, upping the AudioPool value in config may help performance", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Label("Pending reliable: How many messages are awaiting acknowledgement,\n position and most audio messages dont await any acknowledgement as it cant be resent any faster than the next update will get there", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Label("Fragments: Any message over 1250bytes will be fragmented, sent as seperate messages and reconstructed at the other end, value should generally be 0", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Label("Average delay:Taking into account every live players personal ping as well as yours, you and any individual player have higher/lower latency", FrostyPGamemanager.instance.Generalstyle);
			GUILayout.Label("R2R Ping: Your latency in relation to an individual player", FrostyPGamemanager.instance.Generalstyle);


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


	}
}
