using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FrostyP_Game_Manager
{

	public class FrostyPGamemanager : MonoBehaviour
	{
		public static FrostyPGamemanager instance;
		public ParkBuilder _parkbuilder;

		
		float mouseXlast;
		float mouseYlast;
		float mouseX;
		float mouseY;
		float RectposX = 30;
		float RectposY = 30;
		string userpresetname = "Preset Name for Forces here..";
		string userpresetnameCAM = "Preset Name for CAM here..";
		bool consoleon = false;
		bool camerasettingsopen = false;
		bool Onlinesettingsopen = false;
		bool TextureSettingsopen = false;
		bool BMXSettingsopen = false;
		bool ForcesEditBool = false;
		bool loadpresetsbool;
		bool SaveloadCampresets;
		bool opennollies;
		bool openhops;
		bool openmannysensitivity;
		string PresetDirectory = Application.dataPath + "/FrostyPGameManager/Presets/";
		string lastpresetselected = "Default";
		string lastpresetselectedCAM = "Default";
		Texture2D RedTex;
		Texture2D BlackTex;
		Texture2D GreyTex;
		Texture2D GreenTex;
		Texture2D whiteTex;
		Texture2D TransTex;
		public GUISkin skin = (GUISkin)ScriptableObject.CreateInstance("GUISkin");
		public GUIStyle Generalstyle = new GUIStyle();
		public GameObject Onlineobj;
		
		FileInfo[] Files;
		


		// Camera Settings
		private PostProcessVolume volume;
		private Bloom bloom;
		private DepthOfField depthoffield;
		private ColorGrading ColorGrade;
		public float newbloomvalueCAMSETTING;
		public float newbloomintvalueCAMSETTING;
		public float focusdistanceCAMSETTING;
		public float apertureCAMSETTING;
		public float ContrastNewCAMSETTING;
		public float SaturationNewCAMSETTING;
		public float BrightnessNewCAMSETTING;
		public float temperatureNewCAMSETTING;
		public float CamFOVCAMSETTING;

		private float newbloomsaved;
		private float newbloomthreshsaved;
		private float Focusdistancesaved;
		private float aperturesaved;
		private float Contrastnewsaved;
		private float Saturationsaved;
		private float Brightnesssaved;
		private float temperaturesaved;
		private float Camfovsaved = 60;





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

		// online


		// menu paramters
		public bool OpenMenu;
		Vector2 scrollPosition;


		private VehicleManager manager = null;
		private DrivableVehicle currentvehicle = null;
		private SessionMarker marker;
		private CameraAngleChanger camanglecomponent;
		

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


			// Park Builder stuff

			_parkbuilder = gameObject.GetComponent<ParkBuilder>();



			DirectoryInfo Presetdirectoryinfo = new DirectoryInfo(PresetDirectory);

			if (Presetdirectoryinfo.Exists)
			{
				Files = Presetdirectoryinfo.GetFiles();
			}
			else
			{
				Presetdirectoryinfo.Create();
			}

			camanglecomponent = UnityEngine.Component.FindObjectOfType<CameraAngleChanger>();
			mannynoseyController = UnityEngine.Component.FindObjectOfType<MannyNoseyPopInControl>();


			newbloomthreshsaved = 1;
			newbloomsaved = 0.01f;
			Focusdistancesaved = 2;
			aperturesaved = 18;
			Contrastnewsaved = 0;
			Saturationsaved = 0;
			Brightnesssaved = 0;
			temperaturesaved = 0;


			newbloomvalueCAMSETTING = 1f;
			newbloomintvalueCAMSETTING = 0.01f;
			focusdistanceCAMSETTING = 2.01f;
			apertureCAMSETTING = 16f;

			ContrastNewCAMSETTING = 10;
			SaturationNewCAMSETTING = 10;
			BrightnessNewCAMSETTING = 0;
			CamFOVCAMSETTING = 60;
			temperatureNewCAMSETTING = 0;
			
			Gravityvalue = Physics.gravity;
			marker = UnityEngine.Component.FindObjectsOfType<SessionMarker>()[0];
			marker.OnSetAtMarker.AddListener(ResetFOV);

			arrayofhopsettings = UnityEngine.Component.FindObjectsOfType<StateHopSetting>();
			pedalingmanager = UnityEngine.Component.FindObjectOfType<Pedaling>();



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


			GreenTex = new Texture2D(20,10);
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
			Color newcolor5 = new Color(1f,1f, 1f, 0.3f);
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

			OpenMenu = true;
		}


		private void Update()
		{
			// if storm level loaded, hand over to storm inbuilt Manager: probs incorporate Storm components into here and remove need for seperate mod but level will then require this, still, better for updating. Installer.exe?
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains("Storm"))
            {
				OpenMenu = false;
				marker.OnSetAtMarker.RemoveListener(ResetFOV);
				this.enabled = false;
				
			}
            else
            {
				this.enabled = true;
            }
           




			/// toggle menu with D
			if (Input.GetKeyDown(KeyCode.G))
			{
				OpenMenu = !OpenMenu;
			}



		}










	 void ResetFOV()
        {
			StartCoroutine(Wait());

        }


		IEnumerator Wait()
        {

			yield return new WaitForSeconds(1f);
			float time = 0f;
			float transitionTime = 1f;
			Camera Cam = Camera.current;
			float resetfov = Cam.fieldOfView;
			while (time < transitionTime)
			{
				yield return new WaitForEndOfFrame();
				Cam.fieldOfView = Mathf.Lerp(resetfov, CamFOVCAMSETTING, time / transitionTime);
				time += Time.deltaTime;
			}
			Cam.fieldOfView = CamFOVCAMSETTING;
			StopCoroutine(Wait());

		}










		public void OnGUI()
		{



			if (OpenMenu)
			{


			    if (mannystate == null)
                {
			       mannystate = UnityEngine.Component.FindObjectOfType<BMXS_PStates.MannyState>();

                }
				if (mannynoseyController == null)
				{
					mannynoseyController = UnityEngine.Component.FindObjectOfType<MannyNoseyPopInControl>();
				}
				if (marker == null)
				{
					marker = UnityEngine.Component.FindObjectsOfType<SessionMarker>()[0];
					marker.OnSetAtMarker.AddListener(ResetFOV);
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
				currentvehicle = manager.currentVehicle;


				Physics.gravity = Gravityvalue;



				//if found all hop settings, go through them and assign them based on keyword in name
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



					mannystate.SetMaxMannyAngle(MaxMannyAngle,true);

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



				






				//Camera
				///////////////////////////////////////////////////////


				// fetches camera settings and updates if GUI is open
				volume = Camera.current.GetComponent<PostProcessVolume>();



				volume.profile.TryGetSettings(out bloom);


				volume.profile.TryGetSettings(out depthoffield);
				if (depthoffield == null)
				{
					volume.profile.AddSettings<DepthOfField>();
				}


				volume.profile.TryGetSettings(out ColorGrade);
				if (ColorGrade == null)
				{
					volume.profile.AddSettings<ColorGrading>();
				}


				bloom.threshold.overrideState = true;


				depthoffield.enabled.value = true;
				depthoffield.focusDistance.overrideState = true;
				depthoffield.aperture.overrideState = true;


				ColorGrade.enabled.value = true;
				ColorGrade.contrast.overrideState = true;
				ColorGrade.saturation.overrideState = true;
				ColorGrade.brightness.overrideState = true;
				ColorGrade.temperature.overrideState = true;



					bloom.threshold.value = newbloomvalueCAMSETTING;
					bloom.intensity.value = newbloomintvalueCAMSETTING;
					depthoffield.focusDistance.value = focusdistanceCAMSETTING;
					depthoffield.aperture.value = apertureCAMSETTING;
					ColorGrade.contrast.value = ContrastNewCAMSETTING;
					ColorGrade.saturation.value = SaturationNewCAMSETTING;
					ColorGrade.brightness.value = BrightnessNewCAMSETTING;
					ColorGrade.temperature.value = temperatureNewCAMSETTING;
					Camera.current.fieldOfView = CamFOVCAMSETTING;




				////////////////////////////////////////////////



				
				mouseX = Input.mousePosition.x;
				mouseY = Input.mousePosition.y;
				
                if (Input.GetMouseButton(1))
                {
					float FinalX = mouseX - Screen.width/8;
					float finalY = Screen.height - mouseY - Screen.height/5;
					GUILayout.Label("mouse x,y" + FinalX + finalY, Generalstyle);
					
					if (FinalX < Screen.width - 100 && FinalX > 10) 
					{
						RectposX = FinalX;
					}
					if (finalY < Screen.height -100 && finalY > 10)
					{
						RectposY = finalY;
					}
                }






				Rect box = new Rect(RectposX, RectposY, Screen.width / 4,Screen.height/1.5f);

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

				

				//GUILayout.BeginArea(box, Generalstyle);
				scrollPosition = GUILayout.BeginScrollView(scrollPosition, Generalstyle);
				
					
				GUILayout.Space(20);


				
				GUILayout.Label("PIPE Manager", Generalstyle);
				GUILayout.Space(10);
				GUILayout.Label("Use G to toggle this menu ", Generalstyle);
				GUILayout.Label("Uses PIPE_Data/FrostyPGameManager/", Generalstyle);
				///// CONSOLE: when its attached in FrostyPgamemanager.Main it can be toggled with this, when toggled on it records to Desktop/Your_logs, spacebar to see it recording
				consoleon = GUILayout.Toggle(consoleon, "Toggle Debugger on, SpaceBar to see, Streams to file on desktop");
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

				GUILayout.Space(20);

				// parkbuilder tab
				if(GUILayout.Button("Park Builder"))
                {
					GetComponent<ParkBuilder>().Open(PIPE_Valve_Console_Client.GameManager.instance._localplayer.Rider_Root.transform.position);
					OpenMenu = false;
                }

				// Texture tab
				TextureSettingsopen = GUILayout.Toggle(TextureSettingsopen, "Rider Setup");
                if (TextureSettingsopen)
                {
					GUILayout.Space(20);
					GUILayout.Label("Rider Clothing:", Generalstyle);
					PIPE_Valve_Console_Client.CharacterModding.instance.RiderTexturesOpen();
					GUILayout.Space(20);
				}
				BMXSettingsopen = GUILayout.Toggle(BMXSettingsopen, "BMX Setup");
                if (BMXSettingsopen)
                {
					GUILayout.Space(20);
					GUILayout.Label("BMX Settings:", Generalstyle);
					PIPE_Valve_Console_Client.CharacterModding.instance.BMXSetupOpen();
					GUILayout.Space(20);
				}


				// online tab
				Onlinesettingsopen = GUILayout.Toggle(Onlinesettingsopen,"Online");
                if (Onlinesettingsopen)
                {

					if (PIPE_Valve_Console_Client.InGameUI.instance.OfflineMenu)
					{
						PIPE_Valve_Console_Client.InGameUI.instance.ClientsOfflineMenu();
					}
					if (PIPE_Valve_Console_Client.InGameUI.instance.OnlineMenu)
					{
					   PIPE_Valve_Console_Client.InGameUI.instance.ClientsOnlineMenu();
					}

				}

				// camera tab
				camerasettingsopen = GUILayout.Toggle(camerasettingsopen, "Camera Options");
				if (camerasettingsopen)
				{
					
					GUILayout.Space(10);
					GUILayout.Label(lastpresetselectedCAM + " Profile active");
					GUILayout.Space(20);
					if (GUILayout.Button("Default all Cam settings"))
                    {
						newbloomintvalueCAMSETTING = newbloomsaved;
						newbloomvalueCAMSETTING = newbloomthreshsaved;
						focusdistanceCAMSETTING = Focusdistancesaved;
						apertureCAMSETTING = aperturesaved;
						temperatureNewCAMSETTING = temperaturesaved;
						ContrastNewCAMSETTING = Contrastnewsaved;
						BrightnessNewCAMSETTING = Brightnesssaved;
						SaturationNewCAMSETTING = Saturationsaved;
						CamFOVCAMSETTING = Camfovsaved;
                    }

                    
					GUILayout.Label("Camera FOV = " + CamFOVCAMSETTING);
					CamFOVCAMSETTING = GUILayout.HorizontalSlider(CamFOVCAMSETTING, 40, 120);
					GUILayout.Label("Bloom Intensity = " + newbloomintvalueCAMSETTING);
					newbloomintvalueCAMSETTING = GUILayout.HorizontalSlider(newbloomintvalueCAMSETTING, 0, 1f);
					GUILayout.Label("Bloom Threshold = " + newbloomvalueCAMSETTING);
					newbloomvalueCAMSETTING = GUILayout.HorizontalSlider(newbloomvalueCAMSETTING, 0, 10);
					GUILayout.Label("Aperture (0-16) = " + apertureCAMSETTING);
					apertureCAMSETTING = GUILayout.HorizontalSlider(apertureCAMSETTING, 0, 18);
					GUILayout.Label("Focus Distance (0-15) = " + focusdistanceCAMSETTING);
					focusdistanceCAMSETTING = GUILayout.HorizontalSlider(focusdistanceCAMSETTING, 0, 10);
					GUILayout.Label("Contrast (0-75) = " + ContrastNewCAMSETTING);
					ContrastNewCAMSETTING = GUILayout.HorizontalSlider(ContrastNewCAMSETTING, 0, 75);
					GUILayout.Label("Saturation (-100 - 50) = " + SaturationNewCAMSETTING);
					SaturationNewCAMSETTING = GUILayout.HorizontalSlider(SaturationNewCAMSETTING, -100, 50);
					GUILayout.Label("Brightness (-30 - 30) = " + BrightnessNewCAMSETTING);
					BrightnessNewCAMSETTING = GUILayout.HorizontalSlider(BrightnessNewCAMSETTING, -50, 50);
					GUILayout.Label("Temperature (-70 - 70) = " + temperatureNewCAMSETTING);
					temperatureNewCAMSETTING = GUILayout.HorizontalSlider(temperatureNewCAMSETTING, -70, 70);
				GUILayout.Space(50);

					
					
					
					SaveloadCampresets = GUILayout.Toggle(SaveloadCampresets, "Save/Load Cam Presets");
					if (SaveloadCampresets)
					{
						//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////      SAVE GAME /////////////////////////////////////////////////////////
						GUILayout.Space(20);
						userpresetnameCAM = GUILayout.TextField(userpresetnameCAM);
						GUILayout.Space(10);
						if (GUILayout.Button("Save Current Cam Settings"))
						{

							
							///////// look through all public members in here, if its an int or float, add to list for saving
							List<PresetData> Camsavelist = new List<PresetData>();
							MemberInfo[] members = this.GetType().GetMembers();
							foreach (MemberInfo info in members)
							{

								if (info.MemberType == MemberTypes.Field)
								{
									FieldInfo field = typeof(FrostyPGamemanager).GetField(info.Name);

									if (field.FieldType.ToString().Contains("Single") | field.FieldType.ToString().Contains("Int"))
									{
										if (info.Name.Contains("CAMSETTING"))
										{


											if (field.FieldType.ToString().Contains("Single"))
											{
												float obj;
												obj = (float)field.GetValue(this);

												Camsavelist.Add(new PresetData(info.Name, obj));
												Debug.Log("Saved " + info.Name + "setting");
											}

											if (field.FieldType.ToString().Contains("Int"))
											{
												int obj;
												obj = (int)field.GetValue(this);

												Camsavelist.Add(new PresetData(info.Name, obj));
												Debug.Log("Saved " + info.Name + "setting");
											}


										}

									}

								}
							}

							SaveLoadFrostyPManagersettings getsavescript = new SaveLoadFrostyPManagersettings();
							getsavescript.Save(userpresetnameCAM, Camsavelist, PresetDirectory);


						}
						GUILayout.Space(10);

						//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////      SAVE GAME ///////////////////////////////////////////////////////////////////////////////////////////////////////


						GUILayout.Space(30);
						GUILayout.Label("Saved Presets:", Generalstyle);
						GUILayout.Space(10);

						////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////   LOAD GAME  ///////////////////////////////////////////////////////////////////////////////////
						DirectoryInfo Directinfo = new DirectoryInfo(PresetDirectory);
						FileInfo[] camfilestoload = Directinfo.GetFiles();	
						//////////////////////////////////////////////// if file contains right extension, show button with name of file, on click, load presets from file /////////////////////////////////////////////////////////////////////////
						foreach (FileInfo file in camfilestoload)
						{
							



							if (file.Name.Contains(".FrostyCAMPreset"))
							{


								if (GUILayout.Button(file.Name.Remove(file.Name.Length - 16, 16)))
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
												FieldInfo field = typeof(FrostyPGamemanager).GetField(member.Name);
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
													Debug.Log("Failed to update a Cam setting in FrostyPGameMananger after matching it with a loaded setting");
												}



											}
										}

									}
									///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


									userpresetnameCAM = file.Name.Remove(file.Name.Length - 16, 16);
									lastpresetselectedCAM = file.Name.Remove(file.Name.Length - 16, 16);


								}
							}
						}


						////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////   LOAD GAME  ///////////////////////////////////////////////////////////////////////////////////
						GUILayout.Space(100);
					}



				}

				// rider tab
				if (arrayofhopsettings != null)
				{
					


					ForcesEditBool = GUILayout.Toggle(ForcesEditBool,"Rider Options");

					if (ForcesEditBool)
					{
						

						GUILayout.Space(30);
						GUILayout.Label(lastpresetselected + " Profile active");
						

						GUILayout.Space(20);
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

						if (loadpresetsbool)
						{
							//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////      SAVE GAME /////////////////////////////////////////////////////////
							
							
							GUILayout.Space(20);
							userpresetname = GUILayout.TextField(userpresetname);
							GUILayout.Space(10);
							if (GUILayout.Button("Save Current Forces Settings"))
							{


								///////// look through all public members in here, if its an int or float, add to list for saving
								List<PresetData> Rider_Save_List = new List<PresetData>();
								MemberInfo[] members = this.GetType().GetMembers();
								foreach (MemberInfo info in members)
								{

									if (info.MemberType == MemberTypes.Field)
									{
										FieldInfo field = typeof(FrostyPGamemanager).GetField(info.Name);

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


							//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////      SAVE GAME ///////////////////////////////////////////////////////////////////////////////////////////////////////


							GUILayout.Space(30);
							GUILayout.Label("Saved Presets:", Generalstyle);
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
													FieldInfo field = typeof(FrostyPGamemanager).GetField(member.Name);
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
						GUILayout.Label("Rider Forces:");
						GUILayout.Space(20);
						GUILayout.Label("Gravity = " + Gravityvalue.y);
						Gravityvalue.y = GUILayout.HorizontalSlider(Gravityvalue.y, -1, -25);
						if (GUILayout.Button("Default gravity to -12"))
						{
							Gravityvalue.y = -12f;
						}
						GUILayout.Space(30);
						GUILayout.Label("Max Velocity from pedalling alone = " + pedalingmanager.maxPedalVel.ToString());
						MaxPedalV = Mathf.RoundToInt(GUILayout.HorizontalSlider(MaxPedalV, 5, 50));

						GUILayout.Space(30);
						opennollies = GUILayout.Toggle(opennollies, "Nollie Forces");


						openhops = GUILayout.Toggle(openhops, "Hop Forces");

						openmannysensitivity = GUILayout.Toggle(openmannysensitivity, "Manny/Nosey Sensitivity");
						if (opennollies)
						{
							GUILayout.Label("Standard Nollie Min Force" + groundedsettings.nollieMinForce.ToString());
							nollieMin = GUILayout.HorizontalSlider(nollieMin, 0.5f, 10);
							GUILayout.Label("Standard Nollie Max Force" + groundedsettings.nollieMaxForce.ToString());
							nollieMax = GUILayout.HorizontalSlider(nollieMax, 0.5f, 10);
							GUILayout.Space(30);
							GUILayout.Label("Nosey pop Max Force" + Noseysettings.nollieMaxForce.ToString());
							NoseyNollieMax = GUILayout.HorizontalSlider(NoseyNollieMax, 0.5f, 10);
							GUILayout.Label("Nosey pop Min Force" + Noseysettings.nollieMinForce.ToString());
							NoseyNollieMin = GUILayout.HorizontalSlider(NoseyNollieMin, 0.5f, 10);
							GUILayout.Space(30);
						GUILayout.Label("Smith Nollie Max Force" + Smithsettings.nollieMaxForce.ToString());
						SmithNollieMax = GUILayout.HorizontalSlider(SmithNollieMax, 0.5f, 10);
						GUILayout.Label("Smith Nollie Min Force" + Smithsettings.nollieMinForce.ToString());
						SmithNollieMin = GUILayout.HorizontalSlider(SmithNollieMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Feeble Nollie Max Force" + Feeblesettings.nollieMaxForce.ToString());
						FeebleNollieMax = GUILayout.HorizontalSlider(FeebleNollieMax, 0.5f, 10);
						GUILayout.Label("Feeble Nollie Min Force" + Feeblesettings.nollieMinForce.ToString());
						FeebleNollieMin = GUILayout.HorizontalSlider(FeebleNollieMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Tooth Nollie Max Force" + Toothsettings.nollieMaxForce.ToString());
						ToothNollieMax = GUILayout.HorizontalSlider(ToothNollieMax, 0.5f, 10);
						GUILayout.Label("Tooth Nollie Min Force" + Toothsettings.nollieMinForce.ToString());
						ToothNollieMin = GUILayout.HorizontalSlider(ToothNollieMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Crank Nollie Max Force" + Cranksettings.nollieMaxForce.ToString());
						CrankNollieMax = GUILayout.HorizontalSlider(CrankNollieMax, 0.5f, 10);
						GUILayout.Label("Crank Nollie Min Force" + Cranksettings.nollieMinForce.ToString());
						CrankNollieMin = GUILayout.HorizontalSlider(CrankNollieMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Crook Nollie Max Force" + Crooksettings.nollieMaxForce.ToString());
						CrookNollieMax = GUILayout.HorizontalSlider(CrookNollieMax, 0.5f, 10);
						GUILayout.Label("Crook Nollie Min Force" + Crooksettings.nollieMinForce.ToString());
						CrookNollieMin = GUILayout.HorizontalSlider(CrookNollieMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("LipTrick Nollie Max Force" + LipTrickhopsettings.nollieMaxForce.ToString());
						LipTrickNollieMax = GUILayout.HorizontalSlider(LipTrickNollieMax, 0.5f, 10);
						GUILayout.Label("LipTrick Nollie Min Force" + LipTrickhopsettings.nollieMinForce.ToString());
						LipTrickNollieMin = GUILayout.HorizontalSlider(LipTrickNollieMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Luce-E Nollie Max Force" + lucesettings.nollieMaxForce.ToString());
						LuceNollieMax = GUILayout.HorizontalSlider(LuceNollieMax, 0.5f, 10);
						GUILayout.Label("Luce-E Nollie Min Force" + lucesettings.nollieMinForce.ToString());
						LuceNollieMin = GUILayout.HorizontalSlider(LuceNollieMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Un Luce-E Nollie Max Force" + unlucesettings.nollieMaxForce.ToString());
						Unluc_eNollieMax = GUILayout.HorizontalSlider(Unluc_eNollieMax, 0.5f, 10);
						GUILayout.Label("Un Luce-E Nollie Min Force" + unlucesettings.nollieMinForce.ToString());
						Unluc_eNollieMin = GUILayout.HorizontalSlider(Unluc_eNollieMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Pegs Nollie Max Force" + doublepegssettings.nollieMaxForce.ToString());
						PegsNollieMax = GUILayout.HorizontalSlider(PegsNollieMax, 0.5f, 10);
						GUILayout.Label("Pegs Nollie Min Force" + doublepegssettings.nollieMinForce.ToString());
						PegsNollieMin = GUILayout.HorizontalSlider(PegsNollieMin, 0.5f, 10);
						GUILayout.Space(30);
						GUILayout.Label("Pedal Feebs Nollie Max Force" + Pedalfeeblesettings.nollieMaxForce.ToString());
						PedalFeebsNollieMax = GUILayout.HorizontalSlider(PedalFeebsNollieMax, 0.5f, 10);
						GUILayout.Label("Pedal Feebs Nollie Min Force" + Pedalfeeblesettings.nollieMinForce.ToString());
						PedalFeebsNollieMin = GUILayout.HorizontalSlider(PedalFeebsNollieMin, 0.5f, 10);
						GUILayout.Space(30);

						}

						if (openhops)
						{
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
						}

                        if (openmannysensitivity)
                        {
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
							GUILayout.Label("Max Manny Angle " + mannystate.maxMannyAngle);
							GUILayout.Space(10);
							MaxMannyAngle = GUILayout.HorizontalSlider(MaxMannyAngle, 30f, 100f);
						}



					}






















				}








				GUILayout.Space(20);
				if (GUILayout.Button("Exit"))
				{
					OpenMenu = false;
				}
				GUILayout.Space(20);
				GUILayout.EndScrollView();
				//GUILayout.EndArea();

			}









			mouseXlast = Input.mousePosition.x;
			mouseYlast = Input.mousePosition.y;

		}





	}
}







		






       