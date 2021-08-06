using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.IO;
using PIPE_Valve_Console_Client;

namespace FrostyP_Game_Manager
{
    public class RiderPhysics : MonoBehaviour
    {
        public static RiderPhysics instance;

		#region Rider Settings
		// Game settings
		public Vector3 Gravityvalue;
		private StateHopSetting[] arrayofhopsettings;
		private StateHopSetting groundedsettings;
		private StateHopSetting Smithsettings;
		private StateHopSetting Feeblesettings;
		private StateHopSetting Cranksettings;
		private StateHopSetting Crooksettings;
		private StateHopSetting Mannyhopsettings;
		private StateHopSetting LipTrickhopsettings;
		private StateHopSetting doublepegssettings;
		private StateHopSetting Noseysettings;
		private StateHopSetting Pedalfeeblesettings;
		private StateHopSetting Icesettings;
		private StateHopSetting Airhopsettings;
		private StateHopSetting lucesettings;
		private StateHopSetting Toothsettings;
		private StateHopSetting unlucesettings;

		private MannyNoseyPopInControl mannynoseyController;

		private Pedaling pedalingmanager;
		public int MaxPedalV;

		//set by GUI, override statehopsettings

		public float Gravity;
		public float nollieMin;
		public float nollieMax;
		public float hopMin;
		public float hopMax;
		public float SmithNollieMin;
		public float SmithNollieMax;
		public float SmithhopMin;
		public float SmithhopMax;
		public float CrankhopMin;
		public float CrankhopMax;
		public float CrankNollieMin;
		public float CrankNollieMax;
		public float LucehopMin;
		public float LucehopMax;
		public float LuceNollieMin;
		public float LuceNollieMax;
		public float ToothNollieMin;
		public float ToothNollieMax;
		public float CrookhopMin;
		public float CrookhopMax;
		public float CrookNollieMin;
		public float CrookNollieMax;
		public float MannyhopMin;
		public float MannyhopMax;
		public float LipTrickhopMin;
		public float LipTrickhopMax;
		public float LipTrickNollieMin;
		public float LipTrickNollieMax;
		public float PegshopMin;
		public float PegshopMax;
		public float PegsNollieMin;
		public float PegsNollieMax;
		public float NoseyNollieMin;
		public float NoseyNollieMax;
		public float PedalFeebshopMin;
		public float PedalFeebshopMax;
		public float PedalFeebsNollieMin;
		public float PedalFeebsNollieMax;
		public float IcehopMin;
		public float IcehopMax;
		public float FeeblehopMin;
		public float FeeblehopMax;
		public float FeebleNollieMin;
		public float FeebleNollieMax;
		public float MannySensitivityMax;
		public float MannySensitivityMin;
		public float MannysensitivityThreshold;
		public float Spindrag;
		public float Flipdrag;


		bool havesavedhopsettings;
		bool havesavedpedalforce;

		private BMXS_PStates.MannyState mannystate;
		private BMXS_PStates.Air air;
		
		#endregion


		bool opennollies;
		Vector2 NollieScroll;
		bool openhops;
		Vector2 HopsScroll;
		bool openmannysensitivity;
		bool loadpresetsbool;

		public string PresetDirectory = Application.dataPath + "/FrostyPGameManager/Presets/Physics/";
		public string LastLoadedDir = Application.dataPath + "/FrostyPGameManager/Presets/Physics/ActiveProfile/";
		public string SelectedProfile = "Default";
		string userpresetname = "Preset Name for Forces here..";

		GUIStyle Labelstyle = new GUIStyle();
		
		
		void Awake()
        {
            if (!Directory.Exists(PresetDirectory))
            {
				Directory.CreateDirectory(PresetDirectory);
            }
			if (!Directory.Exists(LastLoadedDir))
			{
				Directory.CreateDirectory(LastLoadedDir);
			}
		}



		void Start()
        {

			instance = this;


			

			Labelstyle.normal.background = InGameUI.instance.TransTex;
			Labelstyle.border = new RectOffset(0, 0, 0, 0);
			Labelstyle.alignment = TextAnchor.MiddleLeft;
			Labelstyle.margin = new RectOffset(0, 0, 0, 0);
			Labelstyle.fixedWidth = 220;
			Labelstyle.fontSize = 14;
			Labelstyle.fontStyle = FontStyle.Bold;
			Labelstyle.normal.textColor = Color.black;

			


			mannynoseyController = UnityEngine.Component.FindObjectOfType<MannyNoseyPopInControl>();
			arrayofhopsettings = UnityEngine.Component.FindObjectsOfType<StateHopSetting>();
			pedalingmanager = UnityEngine.Component.FindObjectOfType<Pedaling>();
			air = FindObjectOfType<BMXS_PStates.Air>();
			if (arrayofhopsettings != null)
			{
				foreach (StateHopSetting set in arrayofhopsettings)
				{
					if (set.gameObject.name.Contains("Grounded"))
					{
						groundedsettings = set;
					}
					if (set.gameObject.name.Contains("Smith"))
					{
						Smithsettings = set;
					}
					if (set.gameObject.name.Contains("Feeble") && !set.gameObject.name.Contains("Pedal"))
					{
						Feeblesettings = set;
					}
					if (set.gameObject.name.Contains("Crook"))
					{
						Crooksettings = set;
					}
					if (set.gameObject.name.Contains("Ice"))
					{
						Icesettings = set;
					}
					if (set.gameObject.name.Contains("Tooth"))
					{
						Toothsettings = set;
					}
					if (set.gameObject.name.Contains("Crank"))
					{
						Cranksettings = set;
					}
					if (set.gameObject.name.Contains("Air"))
					{
						Airhopsettings = set;
					}
					if (set.gameObject.name.Contains("Lip"))
					{
						LipTrickhopsettings = set;
					}
					if (set.gameObject.name.Contains("Double"))
					{
						doublepegssettings = set;
					}
					if (set.gameObject.name.Contains("Manny"))
					{
						Mannyhopsettings = set;
					}
					if (set.gameObject.name.Contains("Nosey"))
					{
						Noseysettings = set;
					}
					if (set.gameObject.name.Contains("Pedal"))
					{
						Pedalfeeblesettings = set;
					}
					if (set.gameObject.name.Contains("UnLucky"))
					{
						unlucesettings = set;
					}
					if (set.gameObject.name.Contains("Lucky"))
					{
						lucesettings = set;
					}

				}
			}




			if (!File.Exists(PresetDirectory + "Default.Physics"))
			{
				Save("Default");
				Load(new DirectoryInfo(LastLoadedDir + "Active.Physics").GetFiles()[0]);
			}
            else if(File.Exists(LastLoadedDir + "Active.Physics"))
            {
				Load(new DirectoryInfo(LastLoadedDir + "Active.Physics").GetFiles()[0]);

				pedalingmanager.maxPedalVel = MaxPedalV;
				groundedsettings.nollieMinForce = nollieMin;
				groundedsettings.nollieMaxForce = nollieMax;

				groundedsettings.jHopMaxForce = hopMax;
				groundedsettings.jHopMinForce = hopMin;

				Smithsettings.jHopMaxForce = SmithhopMax;
				Smithsettings.jHopMinForce = SmithhopMin;
				Smithsettings.nollieMaxForce = SmithNollieMax;
				Smithsettings.nollieMinForce = SmithNollieMin;

				Icesettings.jHopMinForce = IcehopMin;
				Icesettings.jHopMaxForce = IcehopMax;

				Toothsettings.nollieMinForce = ToothNollieMin;
				Toothsettings.nollieMaxForce = ToothNollieMax;

				Feeblesettings.jHopMaxForce = FeeblehopMax;
				Feeblesettings.jHopMinForce = FeeblehopMin;
				Feeblesettings.nollieMaxForce = FeebleNollieMax;
				Feeblesettings.nollieMinForce = FeebleNollieMin;

				Crooksettings.jHopMaxForce = CrookhopMax;
				Crooksettings.jHopMinForce = CrookhopMin;
				Crooksettings.nollieMaxForce = CrookNollieMax;
				Crooksettings.nollieMinForce = CrookNollieMin;

				Cranksettings.jHopMaxForce = CrankhopMax;
				Cranksettings.jHopMinForce = CrankhopMin;
				Cranksettings.nollieMaxForce = CrankNollieMax;
				Cranksettings.nollieMinForce = CrankNollieMin;

				LipTrickhopsettings.jHopMaxForce = LipTrickhopMax;
				LipTrickhopsettings.jHopMinForce = LipTrickhopMin;
				LipTrickhopsettings.nollieMaxForce = LipTrickNollieMax;
				LipTrickhopsettings.nollieMinForce = LipTrickNollieMin;

				doublepegssettings.jHopMaxForce = PegshopMax;
				doublepegssettings.jHopMinForce = PegshopMin;
				doublepegssettings.nollieMaxForce = PegsNollieMax;
				doublepegssettings.nollieMinForce = PegsNollieMin;

				Mannyhopsettings.jHopMaxForce = MannyhopMax;
				Mannyhopsettings.jHopMinForce = MannyhopMin;
				Noseysettings.nollieMaxForce = NoseyNollieMax;
				Noseysettings.nollieMinForce = NoseyNollieMin;

				Pedalfeeblesettings.jHopMaxForce = PedalFeebshopMax;
				Pedalfeeblesettings.jHopMinForce = PedalFeebshopMin;
				Pedalfeeblesettings.nollieMaxForce = PedalFeebsNollieMax;
				Pedalfeeblesettings.nollieMinForce = PedalFeebsNollieMin;

				lucesettings.jHopMaxForce = LucehopMax;
				lucesettings.jHopMinForce = LucehopMin;
				lucesettings.nollieMaxForce = LuceNollieMax;
				lucesettings.nollieMinForce = LuceNollieMin;

				mannynoseyController.maxPos = MannySensitivityMax;
				mannynoseyController.minPos = MannySensitivityMin;
				mannynoseyController.holdTime = MannysensitivityThreshold;
				air.spinDrag = Spindrag;
				air.flipDrag = Flipdrag;

				Physics.gravity = new Vector3(0, Gravity, 0);

			}

			


		}

		public void Show()
        {

			try
			{

                

				if (arrayofhopsettings != null)
				{
					foreach (StateHopSetting set in arrayofhopsettings)
					{
						if (set.gameObject.name.Contains("Grounded"))
						{
							groundedsettings = set;
						}
						if (set.gameObject.name.Contains("Smith"))
						{
							Smithsettings = set;
						}
						if (set.gameObject.name.Contains("Feeble") && !set.gameObject.name.Contains("Pedal"))
						{
							Feeblesettings = set;
						}
						if (set.gameObject.name.Contains("Crook"))
						{
							Crooksettings = set;
						}
						if (set.gameObject.name.Contains("Ice"))
						{
							Icesettings = set;
						}
						if (set.gameObject.name.Contains("Tooth"))
						{
							Toothsettings = set;
						}
						if (set.gameObject.name.Contains("Crank"))
						{
							Cranksettings = set;
						}
						if (set.gameObject.name.Contains("Air"))
						{
							Airhopsettings = set;
						}
						if (set.gameObject.name.Contains("Lip"))
						{
							LipTrickhopsettings = set;
						}
						if (set.gameObject.name.Contains("Double"))
						{
							doublepegssettings = set;
						}
						if (set.gameObject.name.Contains("Manny"))
						{
							Mannyhopsettings = set;
						}
						if (set.gameObject.name.Contains("Nosey"))
						{
							Noseysettings = set;
						}
						if (set.gameObject.name.Contains("Pedal"))
						{
							Pedalfeeblesettings = set;
						}
						if (set.gameObject.name.Contains("UnLucky"))
						{
							unlucesettings = set;
						}
						if (set.gameObject.name.Contains("Lucky"))
						{
							lucesettings = set;
						}

					}
				}
				else
				{
					arrayofhopsettings = UnityEngine.Component.FindObjectsOfType<StateHopSetting>();
				}

				// if defaults have been saved, update forces to GUI slider values, if not Save them then mark bool true
				if (havesavedhopsettings)
				{
					groundedsettings.nollieMinForce = nollieMin;
					groundedsettings.nollieMaxForce = nollieMax;

					groundedsettings.jHopMaxForce = hopMax;
					groundedsettings.jHopMinForce = hopMin;

					Smithsettings.jHopMaxForce = SmithhopMax;
					Smithsettings.jHopMinForce = SmithhopMin;
					Smithsettings.nollieMaxForce = SmithNollieMax;
					Smithsettings.nollieMinForce = SmithNollieMin;

					Icesettings.jHopMinForce = IcehopMin;
					Icesettings.jHopMaxForce = IcehopMax;

					Toothsettings.nollieMinForce = ToothNollieMin;
					Toothsettings.nollieMaxForce = ToothNollieMax;

					Feeblesettings.jHopMaxForce = FeeblehopMax;
					Feeblesettings.jHopMinForce = FeeblehopMin;
					Feeblesettings.nollieMaxForce = FeebleNollieMax;
					Feeblesettings.nollieMinForce = FeebleNollieMin;

					Crooksettings.jHopMaxForce = CrookhopMax;
					Crooksettings.jHopMinForce = CrookhopMin;
					Crooksettings.nollieMaxForce = CrookNollieMax;
					Crooksettings.nollieMinForce = CrookNollieMin;

					Cranksettings.jHopMaxForce = CrankhopMax;
					Cranksettings.jHopMinForce = CrankhopMin;
					Cranksettings.nollieMaxForce = CrankNollieMax;
					Cranksettings.nollieMinForce = CrankNollieMin;

					LipTrickhopsettings.jHopMaxForce = LipTrickhopMax;
					LipTrickhopsettings.jHopMinForce = LipTrickhopMin;
					LipTrickhopsettings.nollieMaxForce = LipTrickNollieMax;
					LipTrickhopsettings.nollieMinForce = LipTrickNollieMin;

					doublepegssettings.jHopMaxForce = PegshopMax;
					doublepegssettings.jHopMinForce = PegshopMin;
					doublepegssettings.nollieMaxForce = PegsNollieMax;
					doublepegssettings.nollieMinForce = PegsNollieMin;

					Mannyhopsettings.jHopMaxForce = MannyhopMax;
					Mannyhopsettings.jHopMinForce = MannyhopMin;
					Noseysettings.nollieMaxForce = NoseyNollieMax;
					Noseysettings.nollieMinForce = NoseyNollieMin;

					Pedalfeeblesettings.jHopMaxForce = PedalFeebshopMax;
					Pedalfeeblesettings.jHopMinForce = PedalFeebshopMin;
					Pedalfeeblesettings.nollieMaxForce = PedalFeebsNollieMax;
					Pedalfeeblesettings.nollieMinForce = PedalFeebsNollieMin;

					lucesettings.jHopMaxForce = LucehopMax;
					lucesettings.jHopMinForce = LucehopMin;
					lucesettings.nollieMaxForce = LuceNollieMax;
					lucesettings.nollieMinForce = LuceNollieMin;

					mannynoseyController.maxPos = MannySensitivityMax;
					mannynoseyController.minPos = MannySensitivityMin;
					mannynoseyController.holdTime = MannysensitivityThreshold;


				}
				else
				{
					havesavedhopsettings = true;
				}

				if (mannystate == null)
				{
					mannystate = UnityEngine.Component.FindObjectOfType<BMXS_PStates.MannyState>();

				}
				if (mannynoseyController == null)
				{
					mannynoseyController = UnityEngine.Component.FindObjectOfType<MannyNoseyPopInControl>();
				}
				if (arrayofhopsettings == null)
				{
					arrayofhopsettings = UnityEngine.Component.FindObjectsOfType<StateHopSetting>();
				}
				if (pedalingmanager == null)
				{
					pedalingmanager = UnityEngine.Component.FindObjectOfType<Pedaling>();
				}

				//if default pedal force has been saved, update pedalforce to GUI sliders value, if not, Save them then mark bool true
				if (havesavedpedalforce)
				{
					pedalingmanager.maxPedalVel = MaxPedalV;
				}
				else
				{
					havesavedpedalforce = true;
				}

                if (air)
                {
					air.spinDrag = Spindrag;
					air.flipDrag = Flipdrag;
                }

				Physics.gravity = new Vector3(0,Gravity,0);


                



                


				GUILayout.BeginArea(new Rect(new Vector2(50, 80), new Vector2(Screen.width / 3, Screen.height - 150)), InGameUI.BoxStyle);



				if (GUILayout.Button("Close Physics Setup"))
				{
					FrostyPGamemanager.instance.MenuShowing = 0;
				}
				GUILayout.Space(20);
				GUILayout.Label(SelectedProfile + " Profile active");
				GUILayout.Space(20);

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Default Rider Forces"))
				{
					if(File.Exists(PresetDirectory + "Default.Physics"))
                    {
						Load(new DirectoryInfo(PresetDirectory).GetFiles("Default.Physics", SearchOption.TopDirectoryOnly)[0]);
                    }
				}
				GUILayout.Space(10);
				loadpresetsbool = GUILayout.Toggle(loadpresetsbool, "Load/Save Presets");
				GUILayout.EndHorizontal();

				if (loadpresetsbool)
				{
					//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////      SAVE GAME /////////////////////////////////////////////////////////


					GUILayout.Space(20);
					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Save As"))
					{
						if(userpresetname != "Default")
                        {
						Save(userpresetname);
                        }
					}

					userpresetname = GUILayout.TextField(userpresetname);
					GUILayout.EndHorizontal();
					GUILayout.Space(10);

					//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////      SAVE GAME ///////////////////////////////////////////////////////////////////////////////////////////////////////


					GUILayout.Space(30);
					GUILayout.Label("Saved Presets:", PIPE_Valve_Console_Client.InGameUI.instance.Generalstyle);
					GUILayout.Space(10);

					////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////   LOAD GAME  ///////////////////////////////////////////////////////////////////////////////////
					DirectoryInfo riderinfo = new DirectoryInfo(PresetDirectory);
					FileInfo[] riderfilestoload = riderinfo.GetFiles();
					//////////////////////////////////////////////// if file contains right extension, show button with name of file, on click, load presets from file /////////////////////////////////////////////////////////////////////////
					foreach (FileInfo file in riderfilestoload)
					{
						if (GUILayout.Button($"{file.Name.Replace(".Physics","")}"))
						{
							Load(file);
						}


						////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////   LOAD GAME  ///////////////////////////////////////////////////////////////////////////////////
						GUILayout.Space(5);

					}
				}
                else
                {


					GUILayout.Space(20);

               
					GUILayout.Space(5);
					GUILayout.BeginHorizontal();
					GUILayout.Label("Gravity = " + Physics.gravity.y, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle, GUILayout.MaxWidth(120));
					if (GUILayout.Button("Default gravity", GUILayout.MaxWidth(120)))
					{
						Gravity = -12f;
					}
					GUILayout.Space(5);
					Gravity = GUILayout.HorizontalSlider(Gravity, -1, -25);
					GUILayout.EndHorizontal();
					GUILayout.Space(20);

					GUILayout.BeginHorizontal();
					GUILayout.Label("Max Pedal Force : " + pedalingmanager.maxPedalVel.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle, GUILayout.MaxWidth(120));
				    if (GUILayout.Button("Default Pedal Force", GUILayout.MaxWidth(120)))
				    {
					   MaxPedalV = 7;
				    }
				    GUILayout.Space(5);
				    MaxPedalV = Mathf.RoundToInt(GUILayout.HorizontalSlider(MaxPedalV, 5, 50));
					GUILayout.EndHorizontal();

					GUILayout.Space(20);
					GUILayout.BeginHorizontal();
					GUILayout.Label("Spin drag : " + air.spinDrag, InGameUI.instance.MiniPanelStyle, GUILayout.MaxWidth(120));
					if (GUILayout.Button("Default Spin drag", GUILayout.MaxWidth(120)))
					{
						Spindrag = 3;
					}
					GUILayout.Space(5);
					Spindrag = GUILayout.HorizontalSlider(Spindrag, 0, 10);
					GUILayout.EndHorizontal();

					GUILayout.Space(20);
					GUILayout.BeginHorizontal();
					GUILayout.Label("Flip drag : " + air.flipDrag, InGameUI.instance.MiniPanelStyle, GUILayout.MaxWidth(120));
					if (GUILayout.Button("Default Flip drag", GUILayout.MaxWidth(120)))
					{
						Flipdrag = 2;
					}
					GUILayout.Space(5);
					Flipdrag = GUILayout.HorizontalSlider(Flipdrag, 0, 10);
					GUILayout.EndHorizontal();


					GUILayout.Space(20);
					GUILayout.BeginHorizontal();
					opennollies = GUILayout.Toggle(opennollies, "Nollie Forces");
					openhops = GUILayout.Toggle(openhops, "Hop Forces");
					openmannysensitivity = GUILayout.Toggle(openmannysensitivity, "Manny/Nosey");
					GUILayout.EndHorizontal();


					if (opennollies)
					{
						openhops = false;
						openmannysensitivity = false;
						GUILayout.Space(10);

						NollieScroll = GUILayout.BeginScrollView(NollieScroll);

						GUILayout.Label("Standard Nollie Min Force" + groundedsettings.nollieMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						nollieMin = GUILayout.HorizontalSlider(nollieMin, 0.5f, 5);
						GUILayout.Label("Standard Nollie Max Force" + groundedsettings.nollieMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						nollieMax = GUILayout.HorizontalSlider(nollieMax, 0.5f, 5);
						GUILayout.Space(30);
						GUILayout.Label("Nosey pop Max Force" + Noseysettings.nollieMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						NoseyNollieMax = GUILayout.HorizontalSlider(NoseyNollieMax, 0.5f, 5);
						GUILayout.Label("Nosey pop Min Force" + Noseysettings.nollieMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						NoseyNollieMin = GUILayout.HorizontalSlider(NoseyNollieMin, 0.5f, 5);
						GUILayout.Space(30);
						GUILayout.Label("Smith Nollie Max Force" + Smithsettings.nollieMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						SmithNollieMax = GUILayout.HorizontalSlider(SmithNollieMax, 0.5f, 5);
						GUILayout.Label("Smith Nollie Min Force" + Smithsettings.nollieMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						SmithNollieMin = GUILayout.HorizontalSlider(SmithNollieMin, 0.5f, 5);
						GUILayout.Space(30);
						GUILayout.Label("Feeble Nollie Max Force" + Feeblesettings.nollieMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						FeebleNollieMax = GUILayout.HorizontalSlider(FeebleNollieMax, 0.5f, 5);
						GUILayout.Label("Feeble Nollie Min Force" + Feeblesettings.nollieMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						FeebleNollieMin = GUILayout.HorizontalSlider(FeebleNollieMin, 0.5f, 5);
						GUILayout.Space(30);
						GUILayout.Label("Tooth Nollie Max Force" + Toothsettings.nollieMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						ToothNollieMax = GUILayout.HorizontalSlider(ToothNollieMax, 0.5f, 5);
						GUILayout.Label("Tooth Nollie Min Force" + Toothsettings.nollieMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						ToothNollieMin = GUILayout.HorizontalSlider(ToothNollieMin, 0.5f, 5);
						GUILayout.Space(30);
						GUILayout.Label("Crank Nollie Max Force" + Cranksettings.nollieMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						CrankNollieMax = GUILayout.HorizontalSlider(CrankNollieMax, 0.5f, 5);
						GUILayout.Label("Crank Nollie Min Force" + Cranksettings.nollieMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						CrankNollieMin = GUILayout.HorizontalSlider(CrankNollieMin, 0.5f, 5);
						GUILayout.Space(30);
						GUILayout.Label("Crook Nollie Max Force" + Crooksettings.nollieMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						CrookNollieMax = GUILayout.HorizontalSlider(CrookNollieMax, 0.5f, 5);
						GUILayout.Label("Crook Nollie Min Force" + Crooksettings.nollieMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						CrookNollieMin = GUILayout.HorizontalSlider(CrookNollieMin, 0.5f, 5);
						GUILayout.Space(30);
						GUILayout.Label("LipTrick Nollie Max Force" + LipTrickhopsettings.nollieMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						LipTrickNollieMax = GUILayout.HorizontalSlider(LipTrickNollieMax, 0.5f, 5);
						GUILayout.Label("LipTrick Nollie Min Force" + LipTrickhopsettings.nollieMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						LipTrickNollieMin = GUILayout.HorizontalSlider(LipTrickNollieMin, 0.5f, 5);
						GUILayout.Space(30);
						GUILayout.Label("Luce-E/Un-Luc-E Nollie Max Force" + lucesettings.nollieMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						LuceNollieMax = GUILayout.HorizontalSlider(LuceNollieMax, 0.5f, 5);
						GUILayout.Label("Luce-E/Un-Luc-E Nollie Min Force" + lucesettings.nollieMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						LuceNollieMin = GUILayout.HorizontalSlider(LuceNollieMin, 0.5f, 5);
						GUILayout.Space(30);
						GUILayout.Label("Pegs Nollie Max Force" + doublepegssettings.nollieMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						PegsNollieMax = GUILayout.HorizontalSlider(PegsNollieMax, 0.5f, 5);
						GUILayout.Label("Pegs Nollie Min Force" + doublepegssettings.nollieMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						PegsNollieMin = GUILayout.HorizontalSlider(PegsNollieMin, 0.5f, 5);
						GUILayout.Space(30);
						GUILayout.Label("Pedal Feebs Nollie Max Force" + Pedalfeeblesettings.nollieMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						PedalFeebsNollieMax = GUILayout.HorizontalSlider(PedalFeebsNollieMax, 0.5f, 5);
						GUILayout.Label("Pedal Feebs Nollie Min Force" + Pedalfeeblesettings.nollieMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						PedalFeebsNollieMin = GUILayout.HorizontalSlider(PedalFeebsNollieMin, 0.5f, 5);
						GUILayout.Space(30);
						GUILayout.EndScrollView();

					}

					if (openhops)
					{
						opennollies = false;
						openmannysensitivity = false;
						GUILayout.Space(10);

						HopsScroll = GUILayout.BeginScrollView(HopsScroll);
						GUILayout.Label("Hop Min Force" + groundedsettings.jHopMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						hopMin = GUILayout.HorizontalSlider(hopMin, 0.5f, 10);
						GUILayout.Label("Hop Max Force" + groundedsettings.jHopMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						hopMax = GUILayout.HorizontalSlider(hopMax, 0.5f, 10);

						GUILayout.Space(30);


						GUILayout.Label("Smith Hop Max Force" + Smithsettings.jHopMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						SmithhopMax = GUILayout.HorizontalSlider(SmithhopMax, 0.5f, 10);
						GUILayout.Label("Smith Hop Min Force" + Smithsettings.jHopMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						SmithhopMin = GUILayout.HorizontalSlider(SmithhopMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Feeble Hop Max Force" + Feeblesettings.jHopMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						FeeblehopMax = GUILayout.HorizontalSlider(FeeblehopMax, 0.5f, 10);
						GUILayout.Label("Feeble Hop Min Force" + Feeblesettings.jHopMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						FeeblehopMin = GUILayout.HorizontalSlider(FeeblehopMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Ice Hop Max Force" + Icesettings.jHopMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						IcehopMax = GUILayout.HorizontalSlider(IcehopMax, 0.5f, 10);
						GUILayout.Label("Ice Hop Min Force" + Icesettings.jHopMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						IcehopMin = GUILayout.HorizontalSlider(IcehopMin, 0.5f, 10);

						GUILayout.Space(30);
						GUILayout.Label("Crank Hop Max Force" + Cranksettings.jHopMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						CrankhopMax = GUILayout.HorizontalSlider(CrankhopMax, 0.5f, 10);
						GUILayout.Label("Crank Hop Min Force" + Cranksettings.jHopMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						CrankhopMin = GUILayout.HorizontalSlider(CrankhopMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Crook Hop Max Force" + Crooksettings.jHopMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						CrookhopMax = GUILayout.HorizontalSlider(CrookhopMax, 0.5f, 10);
						GUILayout.Label("Crook Hop Min Force" + Crooksettings.jHopMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						CrookhopMin = GUILayout.HorizontalSlider(CrookhopMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("LipTrick Hop Max Force" + LipTrickhopsettings.jHopMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						LipTrickhopMax = GUILayout.HorizontalSlider(LipTrickhopMax, 0.5f, 10);
						GUILayout.Label("LipTrick Hop Min Force" + LipTrickhopsettings.jHopMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						LipTrickhopMin = GUILayout.HorizontalSlider(LipTrickhopMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Luce-E/Un Luce-E Hop Max Force" + lucesettings.jHopMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						LucehopMax = GUILayout.HorizontalSlider(LucehopMax, 0.5f, 10);
						GUILayout.Label("Luce-E/Un Luce-E Hop Min Force" + lucesettings.jHopMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						LucehopMin = GUILayout.HorizontalSlider(LucehopMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Pegs Hop Max Force" + doublepegssettings.jHopMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						PegshopMax = GUILayout.HorizontalSlider(PegshopMax, 0.5f, 10);
						GUILayout.Label("Pegs Hop Min Force" + doublepegssettings.jHopMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						PegshopMin = GUILayout.HorizontalSlider(PegshopMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Manny Hop Max Force" + Mannyhopsettings.jHopMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						MannyhopMax = GUILayout.HorizontalSlider(MannyhopMax, 0.5f, 10);
						GUILayout.Label("Manny Hop Min Force" + Mannyhopsettings.jHopMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						MannyhopMin = GUILayout.HorizontalSlider(MannyhopMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Pedal Feebs Hop Max Force" + Pedalfeeblesettings.jHopMaxForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						PedalFeebshopMax = GUILayout.HorizontalSlider(PedalFeebshopMax, 0.5f, 10);
						GUILayout.Label("Pedal Feebs Hop Min Force" + Pedalfeeblesettings.jHopMinForce.ToString(), PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
						PedalFeebshopMin = GUILayout.HorizontalSlider(PedalFeebshopMin, 0.5f, 10);

						GUILayout.Space(30);
						GUILayout.EndScrollView();
					}

					if (openmannysensitivity)
					{
						openhops = false;
						opennollies = false;
						GUILayout.Space(10);

						GUILayout.Label("Controls engagement of manny, once engaged you have full roam");
						GUILayout.Label("0 is Rstick in the middle, 1 is at the top. mirrored for nollies.");
						GUILayout.Space(30);
						GUILayout.Label("Max Position of R Stick = " + mannynoseyController.maxPos);
						GUILayout.Space(10);
						MannySensitivityMax = GUILayout.HorizontalSlider(MannySensitivityMax, 0.4f, 1f);
						GUILayout.Space(30);
						GUILayout.Label("Min Position of R Stick = " + mannynoseyController.minPos);
						MannySensitivityMin = GUILayout.HorizontalSlider(MannySensitivityMin, 0.01f, 0.6f);
						GUILayout.Space(10);
						GUILayout.Label("Time holding within range before engagement" + mannynoseyController.holdTime);
						GUILayout.Space(10);
						MannysensitivityThreshold = GUILayout.HorizontalSlider(MannysensitivityThreshold, 0.01f, 0.5f);
						GUILayout.Space(30);
					}




                }
                

				


					GUILayout.Space(10);

					GUILayout.EndArea();

				




			}
			catch (UnityException x)
			{
				Debug.Log("Rider Options error  : " + x);
			}



		}


		public void Save(string name)
        {
			List<float> values = new List<float>
			{
				{Physics.gravity.y },
				{groundedsettings.nollieMinForce },
				{groundedsettings.nollieMaxForce },
				{groundedsettings.jHopMinForce },
				{groundedsettings.jHopMaxForce },
				{Smithsettings.nollieMinForce },
				{Smithsettings.nollieMaxForce },
				{Smithsettings.jHopMinForce },
				{Smithsettings.jHopMaxForce },
				{Feeblesettings.jHopMinForce },
				{Feeblesettings.jHopMaxForce },
				{Feeblesettings.nollieMinForce },
				{Feeblesettings.nollieMaxForce },
				{Cranksettings.jHopMinForce },
				{Cranksettings.jHopMaxForce },
				{Cranksettings.nollieMinForce },
				{Cranksettings.nollieMaxForce },
				{Crooksettings.jHopMinForce },
				{Crooksettings.jHopMaxForce },
				{Crooksettings.nollieMinForce },
				{Crooksettings.nollieMaxForce },
				{Pedalfeeblesettings.jHopMinForce },
				{Pedalfeeblesettings.jHopMaxForce },
				{Pedalfeeblesettings.nollieMinForce },
				{Pedalfeeblesettings.nollieMaxForce },
				{Icesettings.jHopMinForce },
				{Icesettings.jHopMaxForce },
				{Toothsettings.nollieMinForce },
				{Toothsettings.nollieMaxForce },
				{lucesettings.jHopMinForce },
				{lucesettings.jHopMaxForce },
				{lucesettings.nollieMinForce },
				{lucesettings.nollieMaxForce },
				{Mannyhopsettings.jHopMinForce },
				{Mannyhopsettings.jHopMaxForce },
				{Noseysettings.nollieMinForce },
				{Noseysettings.nollieMaxForce },
				{doublepegssettings.jHopMinForce },
				{doublepegssettings.jHopMaxForce },
				{doublepegssettings.nollieMinForce },
				{doublepegssettings.nollieMaxForce },
				{(float)pedalingmanager.maxPedalVel },
				{LipTrickhopsettings.jHopMinForce },
				{LipTrickhopsettings.jHopMaxForce },
				{LipTrickhopsettings.nollieMinForce },
				{LipTrickhopsettings.nollieMaxForce },
				{mannynoseyController.minPos },
				{mannynoseyController.maxPos },
				{mannynoseyController.holdTime },
				{air.spinDrag },
				{air.flipDrag },

			};

			SaveLoad.SavePhysics(new PhysicsProfile(values,name));
			SaveLoad.SaveActive(new PhysicsProfile(values, name));
			SelectedProfile = name;
        }
		public void Load(FileInfo file)
		{
			PhysicsProfile profile = SaveLoad.LoadPhysics(file);

			Gravity = profile.Values[0];
			nollieMin = profile.Values[1];
			nollieMax = profile.Values[2];
			hopMin = profile.Values[3];
			hopMax = profile.Values[4];
			SmithNollieMin = profile.Values[5];
			SmithNollieMax = profile.Values[6];
			SmithhopMin = profile.Values[7];
			SmithhopMax = profile.Values[8];
			FeeblehopMin = profile.Values[9];
			FeeblehopMax = profile.Values[10];
			FeebleNollieMin = profile.Values[11];
			FeebleNollieMax = profile.Values[12];
			CrankhopMin = profile.Values[13];
			CrankhopMax = profile.Values[14];
			CrankNollieMin = profile.Values[15];
			CrankNollieMax = profile.Values[16];
			CrookhopMin = profile.Values[17];
			CrookhopMax = profile.Values[18];
			CrookNollieMin = profile.Values[19];
			CrookNollieMax = profile.Values[20];
				PedalFeebshopMin = profile.Values[21];
				PedalFeebshopMax = profile.Values[22];
				PedalFeebsNollieMin = profile.Values[23];
				PedalFeebsNollieMax = profile.Values[24];
				IcehopMin = profile.Values[25];
				IcehopMax = profile.Values[26];
				ToothNollieMin = profile.Values[27];
				ToothNollieMax = profile.Values[28];
				LucehopMin = profile.Values[29];
				LucehopMax = profile.Values[30];
				LuceNollieMin = profile.Values[31];
				LuceNollieMax = profile.Values[32];
				MannyhopMin = profile.Values[33];
				MannyhopMax = profile.Values[34];
				NoseyNollieMin = profile.Values[35];
				NoseyNollieMax = profile.Values[36];
				PegshopMin = profile.Values[37];
				PegshopMax = profile.Values[38];
				PegsNollieMin = profile.Values[39];
				PegsNollieMax = profile.Values[40];
				MaxPedalV = Mathf.FloorToInt(profile.Values[41]);
				LipTrickhopMin = profile.Values[42];
				LipTrickhopMax = profile.Values[43];
				LipTrickNollieMin = profile.Values[44];
				LipTrickNollieMax = profile.Values[45];
				MannySensitivityMin = profile.Values[46];
				MannySensitivityMax = profile.Values[47];
				MannysensitivityThreshold = profile.Values[48];
			    Spindrag = profile.Values[49];
			    Flipdrag = profile.Values[50];
			

			SaveLoad.SaveActive(profile);
			SelectedProfile = profile.name;





		}


	}
}
