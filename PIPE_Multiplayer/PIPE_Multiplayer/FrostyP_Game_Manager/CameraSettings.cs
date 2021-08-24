using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.IO;
using System.Reflection;

namespace FrostyP_Game_Manager
{
	public class CameraSettings : MonoBehaviour
	{
		public static CameraSettings instance;


		private PostProcessVolume volume;
		private Bloom bloom;
		private DepthOfField depthoffield;
		private ColorGrading ColorGrade;
		private Vignette Vignette;
		private Grain Grain;
		public float newbloomvalueCAMSETTING;
		public float newbloomintvalueCAMSETTING;
		public float focusdistanceCAMSETTING;
		public float apertureCAMSETTING;
		public float ContrastNewCAMSETTING;
		public float SaturationNewCAMSETTING;
		public float BrightnessNewCAMSETTING;
		public float temperatureNewCAMSETTING;
		public float CamFOVCAMSETTING;
		public float vignetteintensityCAMSETTING;
		public float VignetteRoundnessCAMSETTING;
		public float GrainIntensityCAMSETTING;
		public float GrainLightcontribCAMSETTING;
		public float GrainSizeCAMSETTING;
		public float HueShiftCAMSETTING;

		private float newbloomsaved;
		private float newbloomthreshsaved;
		private float Focusdistancesaved;
		private float aperturesaved;
		private float Contrastnewsaved;
		private float Saturationsaved;
		private float Brightnesssaved;
		private float temperaturesaved;
		private float Camfovsaved = 60;
		public float vignetteintensitysaved;
		public float VignetteRoundnesssaved;
		public float GrainIntensitysaved;
		public float GrainLightcontribsaved;
		public float GrainSizesaved;
		public float HueShiftSaved;



		string userpresetnameCAM = "Preset Name for CAM here..";


		string PresetDirectory = Application.dataPath + "/FrostyPGameManager/Presets/";
		string lastpresetselectedCAM = "Default";


		
		Vector2 SaveloadScroll;
		Vector2 pagescroll;

		GUIStyle boxstyle = new GUIStyle();


		void Start()
		{
			instance = this;


			Component.FindObjectsOfType<SessionMarker>()[0].OnSetAtMarker.AddListener(ResetFOV);

			newbloomthreshsaved = 1;
			newbloomsaved = 0.01f;
			Focusdistancesaved = 2;
			aperturesaved = 18;
			Contrastnewsaved = 0;
			Saturationsaved = 0;
			Brightnesssaved = 0;
			temperaturesaved = 0;
			vignetteintensitysaved = 0;
			VignetteRoundnesssaved = 0;
			GrainIntensitysaved = 0;
			GrainLightcontribsaved = 0;
			GrainSizesaved = 0;


			newbloomvalueCAMSETTING = 1f;
			newbloomintvalueCAMSETTING = 0.01f;
			focusdistanceCAMSETTING = 2.01f;
			apertureCAMSETTING = 16f;

			ContrastNewCAMSETTING = 10;
			SaturationNewCAMSETTING = 10;
			BrightnessNewCAMSETTING = 0;
			CamFOVCAMSETTING = 60;
			temperatureNewCAMSETTING = 0;
			HueShiftSaved = 0;

		}

		void Update()
        {
            if (MGInputManager.Back_Down())
            {
				ChangeFOVWithBackButton();
            }
        }
		

		public void Show()
		{

			if(Camera.current.gameObject != PIPE_Valve_Console_Client.CharacterModding.instance.Cam)
            {



			if (!Camera.current.gameObject.GetComponent<PostProcessVolume>())
			{
				volume = Camera.current.gameObject.AddComponent<PostProcessVolume>();
				Camera.current.gameObject.AddComponent<PostProcessLayer>().volumeLayer = LayerMask.NameToLayer("PostProcessing");
				volume.isGlobal = true;
				volume.sharedProfile = Camera.main.gameObject.GetComponent<PostProcessVolume>().sharedProfile;
				

			}
            else
            {
				volume = Camera.current.GetComponent<PostProcessVolume>();
			}


			// if a volume is found, check for settings and add if not, then do update of setting values
			if (volume != null)
			{
				
				
				if (!volume.profile.GetSetting<Bloom>())
				{
					bloom = volume.profile.AddSettings<Bloom>();
				}
				else
				{
					bloom = volume.profile.GetSetting<Bloom>();
				}

				if (!volume.profile.GetSetting<DepthOfField>())
				{
					depthoffield = volume.profile.AddSettings<DepthOfField>();
				}
				else
				{
					depthoffield = volume.profile.GetSetting<DepthOfField>();
				}

				if (!volume.profile.GetSetting<ColorGrading>())
				{
					ColorGrade = volume.profile.AddSettings<ColorGrading>();
				}
				else
				{
					ColorGrade = volume.profile.GetSetting<ColorGrading>();
				}

				if (!volume.profile.GetSetting<Vignette>())
				{
					volume.profile.AddSettings<Vignette>();
				}
				else
				{
					Vignette = volume.profile.GetSetting<Vignette>();
				}

				if (!volume.profile.GetSetting<Grain>())
				{
					Grain = volume.profile.AddSettings<Grain>();
				}
				else
				{
					Grain = volume.profile.GetSetting<Grain>();
				}


				// depth of field updating
				depthoffield.enabled.value = true;
				depthoffield.focusDistance.overrideState = true;
				depthoffield.aperture.overrideState = true;
				depthoffield.focusDistance.value = focusdistanceCAMSETTING;
				depthoffield.aperture.value = apertureCAMSETTING;

				// colorgrade updating
				ColorGrade.enabled.value = true;
				ColorGrade.active = true;
				ColorGrade.gradingMode.overrideState = true;
				ColorGrade.hueShift.overrideState = true;
				ColorGrade.contrast.overrideState = true;
				ColorGrade.saturation.overrideState = true;
				ColorGrade.brightness.overrideState = true;
				ColorGrade.temperature.overrideState = true;
				ColorGrade.gradingMode.value = GradingMode.LowDefinitionRange;
				ColorGrade.hueShift.value = HueShiftCAMSETTING;
				ColorGrade.contrast.value = ContrastNewCAMSETTING;
				ColorGrade.saturation.value = SaturationNewCAMSETTING;
				ColorGrade.brightness.value = BrightnessNewCAMSETTING;
				ColorGrade.temperature.value = temperatureNewCAMSETTING;

				// bloom updating
				bloom.active = true;
				bloom.enabled.value = true;
				bloom.threshold.overrideState = true;
				bloom.threshold.value = newbloomvalueCAMSETTING;
				bloom.intensity.overrideState = true;
				bloom.intensity.value = newbloomintvalueCAMSETTING;

				// vignette updating
				Vignette.active = true;
				Vignette.enabled.value = true;
				Vignette.intensity.overrideState = true;
				Vignette.intensity.value = vignetteintensityCAMSETTING;
				Vignette.roundness.overrideState = true;
				Vignette.roundness.value = VignetteRoundnessCAMSETTING;

				// Grain updating
				Grain.active = true;
				Grain.enabled.value = true;
				Grain.SetAllOverridesTo(true);
				Grain.intensity.value = GrainIntensityCAMSETTING;
				Grain.lumContrib.value = GrainLightcontribCAMSETTING;
				Grain.size.value = GrainSizeCAMSETTING;

			}

			// fov updating
			Camera.current.fieldOfView = CamFOVCAMSETTING;




            }



			////////////////////////////////////////////////

			GUILayout.BeginArea(new Rect(new Vector2(50, 100), new Vector2(Screen.width / 3, Screen.height - 150)), PIPE_Valve_Console_Client.InGameUI.BoxStyle);
			pagescroll = GUILayout.BeginScrollView(pagescroll);
			if (GUILayout.Button("Close Camera Setup"))
			{
				ReplayMode.instance.OpenCamSettings = false;

                if (PIPE_Valve_Console_Client.InGameUI.instance.IsSpectating)
                {
				PIPE_Valve_Console_Client.InGameUI.instance.ShowCamSettingsOverride = false;
				}
                else
                {
				FrostyPGamemanager.instance.MenuShowing = 0;
                }
			}
			GUILayout.Space(10);
			GUILayout.Label(lastpresetselectedCAM + " Profile active", PIPE_Valve_Console_Client.InGameUI.instance.Generalstyle);
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
				GrainIntensityCAMSETTING = GrainIntensitysaved;
				GrainLightcontribCAMSETTING = GrainLightcontribsaved;
				GrainSizeCAMSETTING = GrainSizesaved;
				vignetteintensityCAMSETTING = vignetteintensitysaved;
				VignetteRoundnessCAMSETTING = VignetteRoundnesssaved;
				HueShiftCAMSETTING = HueShiftSaved;
			}


			GUILayout.Label("Camera FOV = " + CamFOVCAMSETTING , PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			CamFOVCAMSETTING = GUILayout.HorizontalSlider(CamFOVCAMSETTING, 0, 100);
			GUILayout.Label("Bloom Intensity = " + newbloomintvalueCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			newbloomintvalueCAMSETTING = GUILayout.HorizontalSlider(newbloomintvalueCAMSETTING, 0, 1f);
			GUILayout.Label("Bloom Threshold = " + newbloomvalueCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			newbloomvalueCAMSETTING = GUILayout.HorizontalSlider(newbloomvalueCAMSETTING, 0, 10);
			GUILayout.Label("Aperture (0-16) = " + apertureCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			apertureCAMSETTING = GUILayout.HorizontalSlider(apertureCAMSETTING, 0, 16);
			GUILayout.Label("Focus Distance (0-10) = " + focusdistanceCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			focusdistanceCAMSETTING = GUILayout.HorizontalSlider(focusdistanceCAMSETTING, 0, 10);
			GUILayout.Label("Contrast (0-75) = " + ContrastNewCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			ContrastNewCAMSETTING = GUILayout.HorizontalSlider(ContrastNewCAMSETTING, 0, 75);
			GUILayout.Label("Saturation (-100 - 50) = " + SaturationNewCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			SaturationNewCAMSETTING = GUILayout.HorizontalSlider(SaturationNewCAMSETTING, -100, 50);
			GUILayout.Label("Brightness (-50 - 50) = " + BrightnessNewCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			BrightnessNewCAMSETTING = GUILayout.HorizontalSlider(BrightnessNewCAMSETTING, -50, 50);
			GUILayout.Label("Temperature (-30 - 30) = " + temperatureNewCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			temperatureNewCAMSETTING = GUILayout.HorizontalSlider(temperatureNewCAMSETTING, -30, 30);

			GUILayout.Label("Vignette Intensity (0 - 1) = " + vignetteintensityCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			vignetteintensityCAMSETTING = GUILayout.HorizontalSlider(vignetteintensityCAMSETTING, 0, 1f);

			GUILayout.Label("Vignette Roundness (0 - 1) = " + VignetteRoundnessCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			VignetteRoundnessCAMSETTING = GUILayout.HorizontalSlider(VignetteRoundnessCAMSETTING, 0, 1);

			GUILayout.Label("Grain Intensity (0 - 1) = " + GrainIntensityCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			GrainIntensityCAMSETTING = GUILayout.HorizontalSlider(GrainIntensityCAMSETTING, 0, 1);

			GUILayout.Label("Grain Light contribution (0 - 1) = " + GrainLightcontribCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			GrainLightcontribCAMSETTING = GUILayout.HorizontalSlider(GrainLightcontribCAMSETTING, 0, 1);

			GUILayout.Label("Grain Size (0 - 1) = " + GrainSizeCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			GrainSizeCAMSETTING = GUILayout.HorizontalSlider(GrainSizeCAMSETTING, 0, 1);

			GUILayout.Label("Hue Shift (-50 - 50) = " + HueShiftCAMSETTING, PIPE_Valve_Console_Client.InGameUI.instance.MiniPanelStyle);
			HueShiftCAMSETTING = GUILayout.HorizontalSlider(HueShiftCAMSETTING, -50, 50);

			

			GUILayout.Space(50);

				//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////      SAVE GAME /////////////////////////////////////////////////////////
				GUILayout.Space(20);
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Save as"))
				{


					///////// look through all public members in here, if its an int or float, add to list for saving
					List<PresetData> Camsavelist = new List<PresetData>();
					MemberInfo[] members = this.GetType().GetMembers();
					foreach (MemberInfo info in members)
					{

						if (info.MemberType == MemberTypes.Field)
						{
							FieldInfo field = typeof(CameraSettings).GetField(info.Name);

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

					SaveLoad.Save(userpresetnameCAM, Camsavelist, PresetDirectory);


				}
				userpresetnameCAM = GUILayout.TextField(userpresetnameCAM);
			    GUILayout.EndHorizontal();
				GUILayout.Space(10);

				//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////      SAVE GAME ///////////////////////////////////////////////////////////////////////////////////////////////////////


				GUILayout.Space(30);
				GUILayout.Label("Saved Presets:", PIPE_Valve_Console_Client.InGameUI.instance.Generalstyle);
				GUILayout.Space(10);
				SaveloadScroll = GUILayout.BeginScrollView(SaveloadScroll,boxstyle);
				
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
							List<PresetData> loadedlist = SaveLoad.Load(file.FullName);



							///////// for each public float or int in here, try to match its name with a loaded preset name, if so, set /////////////////////////////////////////////////////////////////////
							foreach (MemberInfo member in members)
							{
								foreach (PresetData preset in loadedlist)
								{
									if (member.Name.CompareTo(preset.nameofsetting) == 0)
									{
										FieldInfo field = typeof(CameraSettings).GetField(member.Name);
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
					    GUILayout.Space(10);
				    }
				}


				////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////   LOAD GAME  ///////////////////////////////////////////////////////////////////////////////////
				GUILayout.Space(20);
				GUILayout.EndScrollView();

			GUILayout.EndScrollView();
			GUILayout.EndArea();




		}

		void ResetFOV()
		{
			
			StartCoroutine(WaitThenLerpFOV());

		}
		IEnumerator WaitThenLerpFOV()
		{
			yield return new WaitForSeconds(1f);
			float time = 0f;
			float transitionTime = 1f;
			Camera Cam = Camera.current;
			while (time < transitionTime)
			{
				yield return new WaitForEndOfFrame();
				Cam.fieldOfView = Mathf.Lerp(Cam.fieldOfView,CamFOVCAMSETTING, time / transitionTime);
				time += Time.deltaTime;
			}
			Cam.fieldOfView = CamFOVCAMSETTING;
			StopCoroutine(WaitThenLerpFOV());

		}

		// Reset CamFov Setting as user changes Cam position (first person, hybrid etc) to be the standard fov of that mode
		void ChangeFOVWithBackButton()
        {
			StartCoroutine(WaitThenChangeFOVWithBack());
        }
		IEnumerator WaitThenChangeFOVWithBack()
        {
			yield return new WaitForSeconds(0.7f);
			CamFOVCAMSETTING = Camera.current.fieldOfView;

        }

		void LoadLastUsedCam()
		{
			if (PlayerPrefs.HasKey("lastFrostyCam"))
			{
				string _preset = PlayerPrefs.GetString("lastFrostyCam");

				if (!string.IsNullOrEmpty(_preset))
				{
					FileInfo[] file = new DirectoryInfo(PresetDirectory).GetFiles();
					for (int i = 0; i < file.Length; i++)
					{
						if (file[i].Name.ToLower() == _preset.ToLower() + ".FrostyCAMPreset")
						{

							MemberInfo[] members = this.GetType().GetMembers();
							List<PresetData> loadedlist = SaveLoad.Load(file[i].FullName);



							///////// for each public float or int in here, try to match its name with a loaded preset name, if so, set /////////////////////////////////////////////////////////////////////
							foreach (MemberInfo member in members)
							{
								foreach (PresetData preset in loadedlist)
								{
									if (member.Name.CompareTo(preset.nameofsetting) == 0)
									{
										FieldInfo field = typeof(CameraSettings).GetField(member.Name);
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
						}
					}


				}


			}



		}

		void SetLastUsedCam()
        {
			
        }





	}
}
