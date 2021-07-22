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
    class RiderPhysics : MonoBehaviour
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

		public float GravityMaxMin;
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
		public float Unluc_ehopMin;
		public float Unluc_ehopMax;
		public float Unluc_eNollieMin;
		public float Unluc_eNollieMax;
		public float FeeblehopMin;
		public float FeeblehopMax;
		public float FeebleNollieMin;
		public float FeebleNollieMax;
		public float MannySensitivityMax;
		public float MannySensitivityMin;
		public float MannysensitivityThresholdMax;



		// Store originals
		public int MaxPedalVsaved;
		public bool havesavedhopsettings = false;
		public bool havesavedpedalforce = false;
		public float nollieMind;
		public float nollieMaxd;
		public float hopMind;
		public float hopMaxd;
		public float SmithNollieMind;
		public float SmithNollieMaxd;
		public float SmithhopMind;
		public float SmithhopMaxd;

		public float FeeblehopMind;
		public float FeeblehopMaxd;
		public float FeebleNollieMind;
		public float FeebleNollieMaxd;
		public float CrankhopMind;
		public float CrankhopMaxd;
		public float CrankNollieMind;
		public float CrankNollieMaxd;
		public float LucehopMind;
		public float LucehopMaxd;
		public float LuceNollieMind;
		public float LuceNollieMaxd;
		public float ToothNollieMind;
		public float ToothNollieMaxd;
		public float CrookhopMind;
		public float CrookhopMaxd;
		public float CrookNollieMind;
		public float CrookNollieMaxd;
		public float MannyhopMind;
		public float MannyhopMaxd;
		public float LipTrickhopMind;
		public float LipTrickhopMaxd;
		public float LipTrickNollieMind;
		public float LipTrickNollieMaxd;
		public float PegshopMind;
		public float PegshopMaxd;
		public float PegsNollieMind;
		public float PegsNollieMaxd;

		public float NoseyNollieMind;
		public float NoseyNollieMaxd;
		public float PedalFeebshopMind;
		public float PedalFeebshopMaxd;
		public float PedalFeebsNollieMind;
		public float PedalFeebsNollieMaxd;
		public float IcehopMind;
		public float IcehopMaxd;

		public float AirNollieMind;
		public float AirNollieMaxd;
		public float Unluc_ehopMind;
		public float Unluc_ehopMaxd;
		public float Unluc_eNollieMind;
		public float Unluc_eNollieMaxd;
		private float MannySensitivityMaxd;
		private float MannySensitivityMind;
		private float MannysensitivityThresholdMaxd;

		private BMXS_PStates.MannyState mannystate;
		public float MaxMannyAngle;
		#endregion


		bool opennollies;
		Vector2 NollieScroll;
		bool openhops;
		Vector2 HopsScroll;
		bool openmannysensitivity;
		bool loadpresetsbool;
		Vector2 PresetScroll;

		public string PresetDirectory = Application.dataPath + "/FrostyPGameManager/Presets/";
		string lastpresetselected = "Default";
		string userpresetname = "Preset Name for Forces here..";

		private VehicleManager manager = null;

		GUIStyle boxstyle = new GUIStyle();
		GUIStyle Labelstyle = new GUIStyle();
		
		





		void Start()
        {

			instance = this;

			mannynoseyController = UnityEngine.Component.FindObjectOfType<MannyNoseyPopInControl>();

			boxstyle.normal.background = PIPE_Valve_Console_Client.InGameUI.instance.whiteTex;
			boxstyle.padding = new RectOffset(15, 15, 5, 5);

			Labelstyle.normal.background = InGameUI.instance.TransTex;
			Labelstyle.border = new RectOffset(0, 0, 0, 0);
			Labelstyle.alignment = TextAnchor.MiddleLeft;
			Labelstyle.margin = new RectOffset(0, 0, 0, 0);
			Labelstyle.fixedWidth = 220;
			Labelstyle.fontSize = 14;
			Labelstyle.fontStyle = FontStyle.Bold;
			Labelstyle.normal.textColor = Color.black;


			Gravityvalue = Physics.gravity;


			arrayofhopsettings = UnityEngine.Component.FindObjectsOfType<StateHopSetting>();
			pedalingmanager = UnityEngine.Component.FindObjectOfType<Pedaling>();
		}




		public void RiderPhysicsShow()
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

					unlucesettings.jHopMaxForce = Unluc_ehopMax;
					unlucesettings.jHopMinForce = Unluc_ehopMin;
					unlucesettings.nollieMaxForce = Unluc_eNollieMax;
					unlucesettings.nollieMinForce = Unluc_eNollieMin;

					lucesettings.jHopMaxForce = LucehopMax;
					lucesettings.jHopMinForce = LucehopMin;
					lucesettings.nollieMaxForce = LuceNollieMax;
					lucesettings.nollieMinForce = LuceNollieMin;

					mannynoseyController.maxPos = MannySensitivityMax;
					mannynoseyController.minPos = MannySensitivityMin;
					mannynoseyController.holdTime = MannysensitivityThresholdMax;



					mannystate.SetMaxMannyAngle(MaxMannyAngle, true);

				}
				else
				{
					nollieMind = groundedsettings.nollieMinForce;
					nollieMaxd = groundedsettings.nollieMaxForce;

					hopMaxd = groundedsettings.jHopMaxForce;
					hopMind = groundedsettings.jHopMinForce;

					SmithhopMaxd = Smithsettings.jHopMaxForce;
					SmithhopMind = Smithsettings.jHopMinForce;
					SmithNollieMaxd = Smithsettings.nollieMaxForce;
					SmithNollieMind = Smithsettings.nollieMinForce;

					IcehopMind = Icesettings.jHopMinForce;
					IcehopMaxd = Icesettings.jHopMaxForce;

					ToothNollieMind = Toothsettings.nollieMinForce;
					ToothNollieMaxd = Toothsettings.nollieMaxForce;

					FeeblehopMaxd = Feeblesettings.jHopMaxForce;
					FeeblehopMind = Feeblesettings.jHopMinForce;
					FeebleNollieMaxd = Feeblesettings.nollieMaxForce;
					FeebleNollieMind = Feeblesettings.nollieMinForce;

					CrookhopMaxd = Crooksettings.jHopMaxForce;
					CrookhopMind = Crooksettings.jHopMinForce;
					CrookNollieMaxd = Crooksettings.nollieMaxForce;
					CrookNollieMind = Crooksettings.nollieMinForce;

					CrankhopMaxd = Cranksettings.jHopMaxForce;
					CrankhopMind = Cranksettings.jHopMinForce;
					CrankNollieMaxd = Cranksettings.nollieMaxForce;
					CrankNollieMind = Cranksettings.nollieMinForce;



					LipTrickhopMaxd = LipTrickhopsettings.jHopMaxForce;
					LipTrickhopMind = LipTrickhopsettings.jHopMinForce;
					LipTrickNollieMaxd = LipTrickhopsettings.nollieMaxForce;
					LipTrickNollieMind = LipTrickhopsettings.nollieMinForce;

					PegshopMaxd = doublepegssettings.jHopMaxForce;
					PegshopMind = doublepegssettings.jHopMinForce;
					PegsNollieMaxd = doublepegssettings.nollieMaxForce;
					PegsNollieMind = doublepegssettings.nollieMinForce;

					MannyhopMaxd = Mannyhopsettings.jHopMaxForce;
					MannyhopMind = Mannyhopsettings.jHopMinForce;

					NoseyNollieMaxd = Noseysettings.nollieMaxForce;
					NoseyNollieMind = Noseysettings.nollieMinForce;

					PedalFeebshopMaxd = Pedalfeeblesettings.jHopMaxForce;
					PedalFeebshopMind = Pedalfeeblesettings.jHopMinForce;
					PedalFeebsNollieMaxd = Pedalfeeblesettings.nollieMaxForce;
					PedalFeebsNollieMind = Pedalfeeblesettings.nollieMinForce;

					Unluc_ehopMaxd = unlucesettings.jHopMaxForce;
					Unluc_ehopMind = unlucesettings.jHopMinForce;
					Unluc_eNollieMaxd = unlucesettings.nollieMaxForce;
					Unluc_eNollieMind = unlucesettings.nollieMinForce;

					LucehopMaxd = unlucesettings.jHopMaxForce;
					LucehopMind = unlucesettings.jHopMinForce;
					LuceNollieMaxd = unlucesettings.nollieMaxForce;
					LuceNollieMind = unlucesettings.nollieMinForce;

					MannySensitivityMaxd = mannynoseyController.maxPos;
					MannySensitivityMind = mannynoseyController.minPos;
					MannysensitivityThresholdMaxd = mannynoseyController.holdTime;



					nollieMin = nollieMind;
					nollieMax = nollieMaxd;
					hopMin = hopMind;
					hopMax = hopMaxd;
					SmithhopMin = SmithhopMind;
					SmithhopMax = SmithhopMaxd;
					SmithNollieMin = SmithNollieMind;
					SmithNollieMax = SmithNollieMaxd;
					IcehopMin = IcehopMind;
					IcehopMax = IcehopMaxd;
					ToothNollieMin = ToothNollieMind;
					ToothNollieMax = ToothNollieMaxd;


					FeeblehopMax = FeeblehopMaxd;
					FeeblehopMin = FeeblehopMind;
					FeebleNollieMax = FeebleNollieMaxd;
					FeebleNollieMin = FeebleNollieMind;
					CrookhopMax = CrookhopMaxd;
					CrookhopMin = CrookhopMind;
					CrookNollieMax = CrookNollieMaxd;
					CrookNollieMin = CrookNollieMind;
					CrankhopMax = CrankhopMaxd;
					CrankhopMin = CrankhopMind;
					CrankNollieMax = CrankNollieMaxd;
					CrankNollieMin = CrankNollieMind;

					LipTrickhopMax = LipTrickhopMaxd;
					LipTrickhopMin = LipTrickhopMind;
					LipTrickNollieMax = LipTrickNollieMaxd;
					LipTrickNollieMin = LipTrickNollieMind;
					PegshopMax = PegshopMaxd;
					PegshopMin = PegshopMind;
					PegsNollieMax = PegsNollieMaxd;
					PegsNollieMin = PegsNollieMind;
					MannyhopMax = MannyhopMaxd;
					MannyhopMin = MannyhopMind;
					NoseyNollieMax = NoseyNollieMaxd;
					NoseyNollieMin = NoseyNollieMind;
					PedalFeebshopMax = PedalFeebshopMaxd;
					PedalFeebshopMin = PedalFeebshopMind;
					PedalFeebsNollieMax = PedalFeebsNollieMaxd;
					PedalFeebsNollieMin = PedalFeebsNollieMind;
					Unluc_ehopMax = Unluc_ehopMaxd;
					Unluc_ehopMin = Unluc_ehopMind;
					Unluc_eNollieMax = Unluc_eNollieMaxd;
					Unluc_eNollieMin = Unluc_eNollieMind;
					LucehopMax = LucehopMaxd;
					LucehopMin = LucehopMind;
					LuceNollieMax = LuceNollieMaxd;
					LuceNollieMin = LuceNollieMind;
					MannySensitivityMax = MannySensitivityMaxd;
					MannySensitivityMin = MannySensitivityMind;
					MannysensitivityThresholdMax = MannysensitivityThresholdMaxd;



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
					MaxPedalVsaved = pedalingmanager.maxPedalVel;
					MaxPedalV = MaxPedalVsaved;
					havesavedpedalforce = true;
				}

				manager = UnityEngine.Component.FindObjectOfType<VehicleManager>();
				


				Physics.gravity = Gravityvalue;




				GUILayout.BeginArea(new Rect(new Vector2(50,100), new Vector2(Screen.width/3,Screen.height-150)),boxstyle);



				if (GUILayout.Button("Close Physics Setup"))
                {
					FrostyPGamemanager.instance.MenuShowing = 0;
                }
				GUILayout.Space(20);
				GUILayout.Label(lastpresetselected + " Profile active");
				GUILayout.Space(20);

				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Default Rider Forces"))
				{
					MaxPedalV = MaxPedalVsaved;
					nollieMin = nollieMind;
					nollieMax = nollieMaxd;
					hopMin = hopMind;
					hopMax = hopMaxd;
					SmithhopMin = SmithhopMind;
					SmithhopMax = SmithhopMaxd;
					SmithNollieMin = SmithNollieMind;
					SmithNollieMax = SmithNollieMaxd;
					IcehopMin = IcehopMind;
					IcehopMax = IcehopMaxd;
					ToothNollieMin = ToothNollieMind;
					ToothNollieMax = ToothNollieMaxd;


					FeeblehopMax = FeeblehopMaxd;
					FeeblehopMin = FeeblehopMind;
					FeebleNollieMax = FeebleNollieMaxd;
					FeebleNollieMin = FeebleNollieMind;
					CrookhopMax = CrookhopMaxd;
					CrookhopMin = CrookhopMind;
					CrookNollieMax = CrookNollieMaxd;
					CrookNollieMin = CrookNollieMind;
					CrankhopMax = CrankhopMaxd;
					CrankhopMin = CrankhopMind;
					CrankNollieMax = CrankNollieMaxd;
					CrankNollieMin = CrankNollieMind;

					LipTrickhopMax = LipTrickhopMaxd;
					LipTrickhopMin = LipTrickhopMind;
					LipTrickNollieMax = LipTrickNollieMaxd;
					LipTrickNollieMin = LipTrickNollieMind;
					PegshopMax = PegshopMaxd;
					PegshopMin = PegshopMind;
					PegsNollieMax = PegsNollieMaxd;
					PegsNollieMin = PegsNollieMind;
					MannyhopMax = MannyhopMaxd;
					MannyhopMin = MannyhopMind;
					NoseyNollieMax = NoseyNollieMaxd;
					NoseyNollieMin = NoseyNollieMind;
					PedalFeebshopMax = PedalFeebshopMaxd;
					PedalFeebshopMin = PedalFeebshopMind;
					PedalFeebsNollieMax = PedalFeebsNollieMaxd;
					PedalFeebsNollieMin = PedalFeebsNollieMind;
					Unluc_ehopMax = Unluc_ehopMaxd;
					Unluc_ehopMin = Unluc_ehopMind;
					Unluc_eNollieMax = Unluc_eNollieMaxd;
					Unluc_eNollieMin = Unluc_eNollieMind;
					LucehopMax = LucehopMaxd;
					LucehopMin = LucehopMind;
					LuceNollieMax = LuceNollieMaxd;
					LuceNollieMin = LuceNollieMind;
					MannySensitivityMax = MannySensitivityMaxd;
					MannySensitivityMin = MannySensitivityMind;
					MannysensitivityThresholdMax = MannysensitivityThresholdMaxd;
					lastpresetselected = "Default";
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


						///////// look through all public members in here, if its an int or float, add to list for saving
						List<PresetData> Rider_Save_List = new List<PresetData>();
						MemberInfo[] members = this.GetType().GetMembers();
						foreach (MemberInfo info in members)
						{

							if (info.MemberType == MemberTypes.Field)
							{
								FieldInfo field = typeof(RiderPhysics).GetField(info.Name);

								if (field.FieldType.ToString().Contains("Single") | field.FieldType.ToString().Contains("Int"))
								{
									if (!info.Name.EndsWith("d"))
									{
										if (info.Name.Contains("Max") | info.Name.Contains("Min"))

											if (field.FieldType.ToString().Contains("Single"))
											{
												float obj;
												obj = (float)field.GetValue(this);

												Rider_Save_List.Add(new PresetData(info.Name, obj));
												Debug.Log("Saved " + info.Name + "setting");
											}

										if (field.FieldType.ToString().Contains("Int"))
										{
											int obj;
											obj = (int)field.GetValue(this);

											Rider_Save_List.Add(new PresetData(info.Name, obj));
											Debug.Log("Saved " + info.Name + "setting");
										}


									}

								}

							}
						}

						SaveLoadFrostyPManagersettings getsavescript = new SaveLoadFrostyPManagersettings();
						getsavescript.Save(userpresetname, Rider_Save_List, PresetDirectory);


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




						if (file.Name.Contains(".FrostyRiderPreset"))
						{




							if (GUILayout.Button(file.Name.Remove(file.Name.Length - 18, 18)))
							{
								MemberInfo[] members = this.GetType().GetMembers();
								SaveLoadFrostyPManagersettings loader = new SaveLoadFrostyPManagersettings();
								List<PresetData> loadedlist = loader.Load(file.FullName);



								///////// for each public float or int in here, try to match its name with a loaded preset name, if so, set /////////////////////////////////////////////////////////////////////
								foreach (MemberInfo member in members)
								{
									foreach (PresetData preset in loadedlist)
									{
										if (member.Name.CompareTo(preset.nameofsetting) == 0)
										{
											FieldInfo field = typeof(RiderPhysics).GetField(member.Name);
											if (field.FieldType.ToString().Contains("Single"))
											{



												float value = preset.valuefloat;
												field.SetValue(this, value);

											}
											else if (field.FieldType.ToString().Contains("Int"))
											{



												int value = preset.valueint;
												field.SetValue(this, value);

											}
											else
											{
												Debug.Log("Failed to update a Rider setting in FrostyPGameMananger after matching it with a loaded setting");
											}



										}
									}

								}
								///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


								userpresetname = file.Name.Remove(file.Name.Length - 18, 18);
								lastpresetselected = file.Name.Remove(file.Name.Length - 18, 18);


							}
						}
					}


					////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////   LOAD GAME  ///////////////////////////////////////////////////////////////////////////////////
					GUILayout.Space(100);

				}






				GUILayout.Space(30);

				if (GUILayout.Button("Default gravity to -12"))
				{
					Gravityvalue.y = -12f;
				}
				GUILayout.Space(5);
				GUILayout.BeginHorizontal();
				GUILayout.Label("Gravity = " + Gravityvalue.y, Labelstyle);
				Gravityvalue.y = GUILayout.HorizontalSlider(Gravityvalue.y, -1, -25);
				GUILayout.EndHorizontal();
				GUILayout.Space(30);

				GUILayout.BeginHorizontal();
				GUILayout.Label("Max Pedal Force : " + pedalingmanager.maxPedalVel.ToString(), Labelstyle);
				MaxPedalV = Mathf.RoundToInt(GUILayout.HorizontalSlider(MaxPedalV, 5, 50));
				GUILayout.EndHorizontal();

				GUILayout.Space(30);
				GUILayout.BeginHorizontal();
				opennollies = GUILayout.Toggle(opennollies, "Nollie Forces");
				openhops = GUILayout.Toggle(openhops, "Hop Forces");
				openmannysensitivity = GUILayout.Toggle(openmannysensitivity, "Manny/Nosey Sensitivity");
				GUILayout.EndHorizontal();


				if (opennollies)
				{
					openhops = false;
					openmannysensitivity = false;
					GUILayout.Space(10);

					NollieScroll = GUILayout.BeginScrollView(NollieScroll);

					GUILayout.Label("Standard Nollie Min Force" + groundedsettings.nollieMinForce.ToString());
					nollieMin = GUILayout.HorizontalSlider(nollieMin, 0.5f, 5);
					GUILayout.Label("Standard Nollie Max Force" + groundedsettings.nollieMaxForce.ToString());
					nollieMax = GUILayout.HorizontalSlider(nollieMax, 0.5f, 5);
					GUILayout.Space(30);
					GUILayout.Label("Nosey pop Max Force" + Noseysettings.nollieMaxForce.ToString());
					NoseyNollieMax = GUILayout.HorizontalSlider(NoseyNollieMax, 0.5f, 5);
					GUILayout.Label("Nosey pop Min Force" + Noseysettings.nollieMinForce.ToString());
					NoseyNollieMin = GUILayout.HorizontalSlider(NoseyNollieMin, 0.5f, 5);
					GUILayout.Space(30);
					GUILayout.Label("Smith Nollie Max Force" + Smithsettings.nollieMaxForce.ToString());
					SmithNollieMax = GUILayout.HorizontalSlider(SmithNollieMax, 0.5f, 5);
					GUILayout.Label("Smith Nollie Min Force" + Smithsettings.nollieMinForce.ToString());
					SmithNollieMin = GUILayout.HorizontalSlider(SmithNollieMin, 0.5f, 5);
					GUILayout.Space(30);
					GUILayout.Label("Feeble Nollie Max Force" + Feeblesettings.nollieMaxForce.ToString());
					FeebleNollieMax = GUILayout.HorizontalSlider(FeebleNollieMax, 0.5f, 5);
					GUILayout.Label("Feeble Nollie Min Force" + Feeblesettings.nollieMinForce.ToString());
					FeebleNollieMin = GUILayout.HorizontalSlider(FeebleNollieMin, 0.5f, 5);
					GUILayout.Space(30);
					GUILayout.Label("Tooth Nollie Max Force" + Toothsettings.nollieMaxForce.ToString());
					ToothNollieMax = GUILayout.HorizontalSlider(ToothNollieMax, 0.5f, 5);
					GUILayout.Label("Tooth Nollie Min Force" + Toothsettings.nollieMinForce.ToString());
					ToothNollieMin = GUILayout.HorizontalSlider(ToothNollieMin, 0.5f, 5);
					GUILayout.Space(30);
					GUILayout.Label("Crank Nollie Max Force" + Cranksettings.nollieMaxForce.ToString());
					CrankNollieMax = GUILayout.HorizontalSlider(CrankNollieMax, 0.5f, 5);
					GUILayout.Label("Crank Nollie Min Force" + Cranksettings.nollieMinForce.ToString());
					CrankNollieMin = GUILayout.HorizontalSlider(CrankNollieMin, 0.5f,5);
					GUILayout.Space(30);
					GUILayout.Label("Crook Nollie Max Force" + Crooksettings.nollieMaxForce.ToString());
					CrookNollieMax = GUILayout.HorizontalSlider(CrookNollieMax, 0.5f, 5);
					GUILayout.Label("Crook Nollie Min Force" + Crooksettings.nollieMinForce.ToString());
					CrookNollieMin = GUILayout.HorizontalSlider(CrookNollieMin, 0.5f, 5);
					GUILayout.Space(30);
					GUILayout.Label("LipTrick Nollie Max Force" + LipTrickhopsettings.nollieMaxForce.ToString());
					LipTrickNollieMax = GUILayout.HorizontalSlider(LipTrickNollieMax, 0.5f, 5);
					GUILayout.Label("LipTrick Nollie Min Force" + LipTrickhopsettings.nollieMinForce.ToString());
					LipTrickNollieMin = GUILayout.HorizontalSlider(LipTrickNollieMin, 0.5f, 5);
					GUILayout.Space(30);
					GUILayout.Label("Luce-E Nollie Max Force" + lucesettings.nollieMaxForce.ToString());
					LuceNollieMax = GUILayout.HorizontalSlider(LuceNollieMax, 0.5f, 5);
					GUILayout.Label("Luce-E Nollie Min Force" + lucesettings.nollieMinForce.ToString());
					LuceNollieMin = GUILayout.HorizontalSlider(LuceNollieMin, 0.5f, 5);
					GUILayout.Space(30);
					GUILayout.Label("Un Luce-E Nollie Max Force" + unlucesettings.nollieMaxForce.ToString());
					Unluc_eNollieMax = GUILayout.HorizontalSlider(Unluc_eNollieMax, 0.5f, 5);
					GUILayout.Label("Un Luce-E Nollie Min Force" + unlucesettings.nollieMinForce.ToString());
					Unluc_eNollieMin = GUILayout.HorizontalSlider(Unluc_eNollieMin, 0.5f, 5);
					GUILayout.Space(30);
					GUILayout.Label("Pegs Nollie Max Force" + doublepegssettings.nollieMaxForce.ToString());
					PegsNollieMax = GUILayout.HorizontalSlider(PegsNollieMax, 0.5f, 5);
					GUILayout.Label("Pegs Nollie Min Force" + doublepegssettings.nollieMinForce.ToString());
					PegsNollieMin = GUILayout.HorizontalSlider(PegsNollieMin, 0.5f, 5);
					GUILayout.Space(30);
					GUILayout.Label("Pedal Feebs Nollie Max Force" + Pedalfeeblesettings.nollieMaxForce.ToString());
					PedalFeebsNollieMax = GUILayout.HorizontalSlider(PedalFeebsNollieMax, 0.5f, 5);
					GUILayout.Label("Pedal Feebs Nollie Min Force" + Pedalfeeblesettings.nollieMinForce.ToString());
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
					GUILayout.Label("Hop Min Force" + groundedsettings.jHopMinForce.ToString());
					hopMin = GUILayout.HorizontalSlider(hopMin, 0.5f, 10);
					GUILayout.Label("Hop Max Force" + groundedsettings.jHopMaxForce.ToString());
					hopMax = GUILayout.HorizontalSlider(hopMax, 0.5f, 10);

					GUILayout.Space(30);


					GUILayout.Label("Smith Hop Max Force" + Smithsettings.jHopMaxForce.ToString());
					SmithhopMax = GUILayout.HorizontalSlider(SmithhopMax, 0.5f, 10);
					GUILayout.Label("Smith Hop Min Force" + Smithsettings.jHopMinForce.ToString());
					SmithhopMin = GUILayout.HorizontalSlider(SmithhopMin, 0.5f, 10);
					GUILayout.Space(30);
					GUILayout.Label("Feeble Hop Max Force" + Feeblesettings.jHopMaxForce.ToString());
					FeeblehopMax = GUILayout.HorizontalSlider(FeeblehopMax, 0.5f, 10);
					GUILayout.Label("Feeble Hop Min Force" + Feeblesettings.jHopMinForce.ToString());
					FeeblehopMin = GUILayout.HorizontalSlider(FeeblehopMin, 0.5f, 10);
					GUILayout.Space(30);
					GUILayout.Label("Ice Hop Max Force" + Icesettings.jHopMaxForce.ToString());
					IcehopMax = GUILayout.HorizontalSlider(IcehopMax, 0.5f, 10);
					GUILayout.Label("Ice Hop Min Force" + Icesettings.jHopMinForce.ToString());
					IcehopMin = GUILayout.HorizontalSlider(IcehopMin, 0.5f, 10);

					GUILayout.Space(30);
					GUILayout.Label("Crank Hop Max Force" + Cranksettings.jHopMaxForce.ToString());
					CrankhopMax = GUILayout.HorizontalSlider(CrankhopMax, 0.5f, 10);
					GUILayout.Label("Crank Hop Min Force" + Cranksettings.jHopMinForce.ToString());
					CrankhopMin = GUILayout.HorizontalSlider(CrankhopMin, 0.5f, 10);
					GUILayout.Space(30);
					GUILayout.Label("Crook Hop Max Force" + Crooksettings.jHopMaxForce.ToString());
					CrookhopMax = GUILayout.HorizontalSlider(CrookhopMax, 0.5f, 10);
					GUILayout.Label("Crook Hop Min Force" + Crooksettings.jHopMinForce.ToString());
					CrookhopMin = GUILayout.HorizontalSlider(CrookhopMin, 0.5f, 10);
					GUILayout.Space(30);
					GUILayout.Label("LipTrick Hop Max Force" + LipTrickhopsettings.jHopMaxForce.ToString());
					LipTrickhopMax = GUILayout.HorizontalSlider(LipTrickhopMax, 0.5f, 10);
					GUILayout.Label("LipTrick Hop Min Force" + LipTrickhopsettings.jHopMinForce.ToString());
					LipTrickhopMin = GUILayout.HorizontalSlider(LipTrickhopMin, 0.5f, 10);
					GUILayout.Space(30);
					GUILayout.Label("Luce-E Hop Max Force" + lucesettings.jHopMaxForce.ToString());
					LucehopMax = GUILayout.HorizontalSlider(LucehopMax, 0.5f, 10);
					GUILayout.Label("Luce-E Hop Min Force" + lucesettings.jHopMinForce.ToString());
					LucehopMin = GUILayout.HorizontalSlider(LucehopMin, 0.5f, 10);
					GUILayout.Space(30);
					GUILayout.Label("Un Luce-E Hop Max Force" + unlucesettings.jHopMaxForce.ToString());
					Unluc_ehopMax = GUILayout.HorizontalSlider(Unluc_ehopMax, 0.5f, 10);
					GUILayout.Label("Un Luce-E Hop Min Force" + unlucesettings.jHopMinForce.ToString());
					Unluc_ehopMin = GUILayout.HorizontalSlider(Unluc_ehopMin, 0.5f, 10);
					GUILayout.Space(30);
					GUILayout.Label("Pegs Hop Max Force" + doublepegssettings.jHopMaxForce.ToString());
					PegshopMax = GUILayout.HorizontalSlider(PegshopMax, 0.5f, 10);
					GUILayout.Label("Pegs Hop Min Force" + doublepegssettings.jHopMinForce.ToString());
					PegshopMin = GUILayout.HorizontalSlider(PegshopMin, 0.5f, 10);
					GUILayout.Space(30);
					GUILayout.Label("Manny Hop Max Force" + Mannyhopsettings.jHopMaxForce.ToString());
					MannyhopMax = GUILayout.HorizontalSlider(MannyhopMax, 0.5f, 10);
					GUILayout.Label("Manny Hop Min Force" + Mannyhopsettings.jHopMinForce.ToString());
					MannyhopMin = GUILayout.HorizontalSlider(MannyhopMin, 0.5f, 10);
					GUILayout.Space(30);
					GUILayout.Label("Pedal Feebs Hop Max Force" + Pedalfeeblesettings.jHopMaxForce.ToString());
					PedalFeebshopMax = GUILayout.HorizontalSlider(PedalFeebshopMax, 0.5f, 10);
					GUILayout.Label("Pedal Feebs Hop Min Force" + Pedalfeeblesettings.jHopMinForce.ToString());
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
					MannysensitivityThresholdMax = GUILayout.HorizontalSlider(MannysensitivityThresholdMax, 0.01f, 0.5f);
					GUILayout.Space(30);
					//GUILayout.Label("Max Manny Angle " + mannystate.maxMannyAngle);
					//GUILayout.Space(10);
					//MaxMannyAngle = GUILayout.HorizontalSlider(MaxMannyAngle, 30f, 100f);
				}


				
				GUILayout.Space(10);

				GUILayout.EndArea();

			}
			catch (UnityException x)
			{
				Debug.Log("Rider Options error  : " + x);
			}



		}





	}
}
