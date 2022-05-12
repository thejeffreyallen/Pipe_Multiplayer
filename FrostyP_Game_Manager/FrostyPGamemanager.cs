using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FrostyP_Game_Manager
{

	public class FrostyPGamemanager : MonoBehaviour
	{
		public static FrostyPGamemanager instance;

		public GUIStyle style;
		Vector2 popupscroll;


		// Menu Controller
		public int MenuShowing;
		List<FrostyModule> Modules = new List<FrostyModule>();

		// GUI
		public GUIStyle Generalstyle = new GUIStyle();
		public GameObject Onlineobj;
		
		// menu paramters
		public bool OpenMenu;

		Queue<string> popups = new Queue<string>();
		bool PopupToshow;
		Popupmessage livepopup;
		GameObject lastwalkchar;

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

			
			int ModuleId = 1;
			foreach (FrostyModule module in GetComponents<FrostyModule>())
            {
				Modules.Add(module);
				module.MenuID = ModuleId;
				ModuleId++;
            }

			
			Generalstyle.normal.background = InGameUI.instance.whiteTex; 
			Generalstyle.normal.textColor = Color.black;

			Generalstyle.alignment = TextAnchor.MiddleCenter;
			Generalstyle.fontStyle = FontStyle.Bold;

			
			OpenMenu = true;
		}

		private void Update()
		{
			/// toggle menu with G
			if (Input.GetKeyDown(KeyCode.G))
			{
                if (!ReplayMode.instance.ReplayOpen)
                {
				  OpenMenu = !OpenMenu;
                }
                if (MainManager.instance.isOpen)
                {
					OpenMenu = false;
                }
				if (MultiplayerManager.isConnected() && InGameUI.instance.Minigui)
				{
					InGameUI.instance.Minigui = false;
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
			if (MGInputManager.Y_Down())
			{
				StartCoroutine(FixWalkBug());
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
				
					foreach (FrostyModule module in Modules)
					{
						if (GUILayout.Button(module.Buttontext, style))
						{
                            if (module.isOpen)
                            {
								module.Close();
                            }
                            else
                            {
								module.Open();
                            }
						}
					}

					GUILayout.Space(5);
					GUILayout.EndHorizontal();
				    GUILayout.EndArea();


                if (MenuShowing != 0)
                {
				 Modules[MenuShowing-1].Show();
                }

                if (PopupToshow)
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

		
		public void PopUpMessage(string message)
        {
			popups.Enqueue(message);
			PopupToshow = true;
        }

		void PopupShow()
        {
            if (PopupToshow)
            {
				if (livepopup == null)
				{
					DoneWithPopUp();
					return;
				}			
				GUILayout.BeginArea(new Rect(new Vector2(Screen.width/8*3,Screen.height/8*3), new Vector2(Screen.width/5,150)),InGameUI.BoxStyle);
				popupscroll = GUILayout.BeginScrollView(popupscroll);
				GUILayout.Label(livepopup.text);
				GUILayout.EndScrollView();
				GUILayout.EndArea();
				livepopup.time += Time.deltaTime;
				if(livepopup.time > 2)
                {
					DoneWithPopUp();
                }
            }
        }
		void DoneWithPopUp()
        {
            if (popups.Count > 0)
            {
				livepopup = new Popupmessage(popups.Dequeue());
            }
            else
            {
				PopupToshow = false;
				popups.TrimExcess();
            }

		}

		public IEnumerator FixWalkBug()
		{
			yield return new WaitForSeconds(0.2f);
			if (lastwalkchar)
			{
				Destroy(lastwalkchar);
			}
			if (FindObjectOfType<WalkingSetUp>())
			{
				lastwalkchar = Instantiate(FindObjectOfType<WalkingSetUp>().gameObject);
				lastwalkchar.SetActive(true);
				lastwalkchar.transform.parent = GameObject.Find("BMXS Player Components").transform;
			}

		}

		class Popupmessage
        {
			public string text;
			public float time;

			public Popupmessage(string _message)
            {
				text = _message;
				time = 0;
            }
        }
		

	}
}







		






       