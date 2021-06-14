using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System;
using FrostyP_Game_Manager;
using UnityEngine.Rendering.PostProcessing;
using System.Runtime.Serialization.Formatters.Binary;




namespace PIPE_Valve_Console_Client
{
    public class InGameUI : MonoBehaviour
    {

        private PlayerSaveData PlayerSavedata;
        private string Playersavepath = Application.dataPath + "/FrostyPGameManager/PlayerSaveData/";

        public static InGameUI instance;
        public LocalPlayer _localplayer;

        //Spectate mode
        public GameObject SpecCamOBJ;
        public Camera Cam;
        GameObject Ridersmoothfollower;
        GameObject ControlObj;
        public GameObject Targetrider;
        uint currentspecid;
        public List<RemotePlayer> cycleplayerslist;
        public int cyclecounter;
        delegate void CamMode();
        Dictionary<int, CamMode> CamModes;
        public int cyclemodes = 0;


        // connection status
        public float Ping = 0;
        public int SendBytesPersec = 0;
        public float Outbytespersec = 0;
        public float InBytespersec = 0;
        public int Pendingreliable;
        public int Pendingunreliable;



        // Admin mode
        string Adminpass = "Admin Password..";
        bool AdminOpen;
        public bool AdminLoggedin;
        bool bootplayeropen;
        string BantimeParse = "5"; // mins
        int _bantime = 5;
        bool BootObjectOpen;


        // MiniGUI
        bool MiniLiveRiderstoggle;
        GUIStyle MiniLiveRidersStyle;


        // live riders
        Vector2 liveridersscroll;
        Rect LiveRiderBox;

        // Messages
        Vector2 Messagesscroll;

        // Riderinfo
        Vector2 Riderinfoscroll;
        bool TogglePlayerObjects = true;
        bool TogglePlayerTag = true;



        public GUISkin skin = (GUISkin)ScriptableObject.CreateInstance("GUISkin");
        public GUIStyle Generalstyle = new GUIStyle();

        public string Username = "Username...";
        public string Nickname = "Server 1";

        public string Key;
        public string IV;


        /// <summary>
        /// In Online mode
        /// </summary>
        public bool Connected;
        public bool OfflineMenu = true;
        public bool OnlineMenu;
        public bool Minigui;
        public bool RecentSavedServersMenu;
        public string desiredport = "7777";
        public string desiredIP = "127.0.0.1";
        public bool IsSpectating;
        public bool RiderInfoMenuOpen;
        public uint IdofRidertoshow = 0;

        public int messagetimer;
        public List<TextMessage> Messages = new List<TextMessage>();
        public string Messagetosend = "Send a message to all...";
        Dictionary<int, Color> MessageColour;

        
        Texture2D RedTex;
        Texture2D BlackTex;
        Texture2D GreyTex;
        Texture2D GreenTex;
        Texture2D whiteTex;
        Texture2D TransTex;


       

        /// <summary>
        /// just checks that theres only one instance and this is it
        /// </summary>
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("IngameUI already exists, destroying old InGameUI now");
                Destroy(this);
            }

            CamModes = new Dictionary<int, CamMode>
            {
                {0,SpectateAutoFollowFreeMove },
                {1,SpectateTripodAutoLookAt },
                {2,SpectateTripodFullManual },

            };

         
            

        }


        /// <summary>
        /// Grabs anything that wont change
        /// </summary>
        private void Start()
        {


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


            GreenTex = new Texture2D(20, 10);
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
            Color newcolor5 = new Color(1f, 1f, 1f, 0.3f);
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
            skin.textField.padding = new RectOffset(10, 10, 10, 10);



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
           // skin.scrollView.normal.background = GreenTex;
            skin.verticalScrollbarThumb.normal.background = GreenTex;
            skin.scrollView.alignment = TextAnchor.MiddleCenter;
            // skin.scrollView.fixedWidth = Screen.width / 4;



            MiniLiveRidersStyle = new GUIStyle();

            MiniLiveRidersStyle.alignment = TextAnchor.MiddleCenter;
            MiniLiveRidersStyle.fontStyle = FontStyle.Bold;
            MiniLiveRidersStyle.padding = new RectOffset(5, 5, 5, 5);
            MiniLiveRidersStyle.normal.background = GreenTex;
            MiniLiveRidersStyle.normal.textColor = Color.black;
            MiniLiveRidersStyle.onNormal.background = whiteTex;
            MiniLiveRidersStyle.onNormal.textColor = Color.green;
            MiniLiveRidersStyle.hover.background = GreyTex;
            MiniLiveRidersStyle.hover.textColor = Color.black;
            MiniLiveRidersStyle.onHover.background = GreyTex;







            MessageColour = new Dictionary<int, Color>()
            {
                {(int)PIPE_Valve_Console_Client.MessageColour.Me,Color.blue},
                {(int)PIPE_Valve_Console_Client.MessageColour.Player,Color.black},
                {(int)PIPE_Valve_Console_Client.MessageColour.System,Color.green},
                {(int)PIPE_Valve_Console_Client.MessageColour.Server,Color.red},
            };

            SpecCamOBJ = new GameObject();
            Cam = SpecCamOBJ.AddComponent<Camera>();
            DontDestroyOnLoad(SpecCamOBJ);
            SpecCamOBJ.SetActive(false);

            if (!Directory.Exists(Playersavepath))
            {
                Directory.CreateDirectory(Playersavepath);
            }
            if (!File.Exists(Playersavepath + "PlayerData.FrostyPreset"))
            {
                PlayerSavedata = new PlayerSaveData(Username);
                PlayerSavedata.savedservers = new List<SavedServer>();
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(File.OpenWrite(Playersavepath + "PlayerData.FrostyPreset"), PlayerSavedata);

            }
            else if (File.Exists(Playersavepath + "PlayerData.FrostyPreset"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                PlayerSavedata = bf.Deserialize(File.OpenRead(Playersavepath + "PlayerData.FrostyPreset")) as PlayerSaveData;

                Username = PlayerSavedata.Username;

            }

            _localplayer = gameObject.GetComponent<LocalPlayer>();
            cycleplayerslist = new List<RemotePlayer>();
            Ridersmoothfollower = new GameObject();
            DontDestroyOnLoad(Ridersmoothfollower);
            ControlObj = new GameObject();
            DontDestroyOnLoad(ControlObj);
            SpecCamOBJ = GameObject.Instantiate(UnityEngine.GameObject.Find("Main Camera"));
            Cam = SpecCamOBJ.GetComponent<Camera>();
            SpecCamOBJ.name = "SpecCam";
            SpecCamOBJ.SetActive(false);
            DontDestroyOnLoad(SpecCamOBJ);

        }


        void Update()
        {
            if (IsSpectating)
            {
                if (MGInputManager.RB_Down())
                {
                    if (cycleplayerslist.Count > 1)
                    {
                        if (cyclecounter == cycleplayerslist.Count -1)
                        {
                            cyclecounter = 0;
                        }
                        else
                        {
                            cyclecounter++;
                        }
                        Targetrider = cycleplayerslist[cyclecounter].RiderModel;

                    }
                }
                if (MGInputManager.LB_Down())
                {
                    if (cycleplayerslist.Count > 1)
                    {
                        if (cyclecounter == 0)
                        {
                            cyclecounter = cycleplayerslist.Count - 1;
                        }
                        else
                        {
                            cyclecounter--;
                        }
                        Targetrider = cycleplayerslist[cyclecounter].RiderModel;

                    }
                }


                Targetrider = cycleplayerslist[cyclecounter].RiderModel;

                if (MGInputManager.B_Down())
                {
                    if (cyclemodes == CamModes.Count - 1)
                    {
                        cyclemodes = 0;
                    }
                    else
                    {
                        cyclemodes++;
                    }
                }

                

            }

            if (IsSpectating)
            {
                CamModes[cyclemodes]();
            }



            if (Connected && OnlineMenu && Input.GetKeyDown(KeyCode.A))
            {
                AdminOpen = !AdminOpen;
            }
        }



        void OnGUI()

        {
            try
            {
            if (Minigui)
            {
                MiniGUI();
            }

            if(OnlineMenu && !Minigui && FrostyPGamemanager.instance.OpenMenu)
            {
                LiveRiders();
                MessagesShow();


            if (RiderInfoMenuOpen)
            {
                ShowRiderInfo();
            }

            }

            }
            catch (Exception x)
            {
                Debug.Log("ingameui error: " + x);
            }

        }
     




        /// <summary>
        /// Master Call to connect, also turns connected to true which some data sends have as their/one of their conditions
        /// </summary>
        public void ConnectToServer()
        {
           
            GameNetworking.instance.ConnectMaster();
            Connected = true;
          
        }


        /// <summary>
        /// Call to Shutdown, also cleans up all gamemanger data and remote riders in scene
        /// </summary>
        public void Disconnect()
        {
            Connected = false;
            GameManager.instance.firstMap = true;
            List<GameObject> objs = new List<GameObject>();
            GameNetworking.instance.DisconnectMaster();
           
            foreach (RemotePlayer r in GameManager.Players.Values)
            {
                if (r.RiderModel)
                {
                Destroy(r.RiderModel);
                }
                if (r.BMX)
                {
                Destroy(r.BMX);
                }
                if (r.Audio)
                {
                Destroy(r.Audio);
                }
                if (r.nameSign)
                {
                Destroy(r.nameSign);
                }
                objs.Add(r.gameObject);
                

            }
            if (objs.Count > 0)
            {
                foreach(GameObject obj in objs)
                {
                    Destroy(obj);
                }
            }
            GameManager.Players.Clear();
            GameManager.PlayersColours.Clear();
            GameManager.PlayersSmooths.Clear();
            GameManager.PlayersMetals.Clear();
            GameManager.BikeTexinfos.Clear();
            GameManager.Bikenormalinfos.Clear();
            GameManager.RiderTexinfos.Clear();

            
            foreach(List<NetGameObject> playerobjects in GameManager.PlayersObjects.Values)
            {
                foreach(NetGameObject n in playerobjects)
                {
                    if(n._Gameobject != null)
                    {
                        Destroy(n._Gameobject);
                    }
                }
            }
            GameManager.PlayersObjects.Clear();


            // Server learns of disconnection itself and tells everyone

        }
        

       
       
    

       public void ClientsOfflineMenu()
        {
           
            GUILayout.Label("Online Mode");

            GUILayout.Space(10);
            // setup stuff before connecting
            Username = GUILayout.TextField(Username);
            GUILayout.Space(30);
            RecentSavedServersMenu = GUILayout.Toggle(RecentSavedServersMenu, "Saved Servers");
            if (RecentSavedServersMenu)
            {
                GUILayout.Space(15);
                GUILayout.Label("Save curent Ip and port by nickname:");
                GUILayout.Space(5);
                Nickname = GUILayout.TextField(Nickname);
                if(GUILayout.Button($"Save current setup as {Nickname}"))
                {

                        if(PlayerSavedata.savedservers == null)
                        {
                            PlayerSavedata.savedservers = new List<SavedServer>();
                        }

                    if(PlayerSavedata != null)
                    {


                        PlayerSavedata.savedservers.Add(new SavedServer(desiredIP, desiredport, Nickname));
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(File.OpenWrite(Playersavepath + "PlayerData.FrostyPreset"), PlayerSavedata);
                        
                    }
                }
                GUILayout.Space(10);

                GUILayout.Label("Saved server list:");
                GUILayout.Space(10);
                if (PlayerSavedata != null && PlayerSavedata.savedservers != null)
                {
                    foreach(SavedServer s in PlayerSavedata.savedservers)
                    {
                        if (GUILayout.Button("Select :" + s.Nickname))
                        {
                            desiredIP = s.IP;
                            desiredport = s.PORT;
                        }
                        GUILayout.Space(5);
                        if (GUILayout.Button("Remove :" + s.Nickname))
                        {
                            PlayerSavedata.savedservers.Remove(s);
                            
                                
                                BinaryFormatter bf = new BinaryFormatter();
                                bf.Serialize(File.OpenWrite(Playersavepath + "PlayerData.FrostyPreset"), PlayerSavedata);


                            

                        }
                        GUILayout.Space(20);
                    }
                }



                GUILayout.Space(35);
            }
           

            desiredIP = GUILayout.TextField(desiredIP);
            if(desiredIP != "")
            {
                GameNetworking.instance.ip = desiredIP;
            }
            desiredport = GUILayout.TextField(desiredport);
            if(desiredport != "")
            {
                if (int.TryParse(desiredport, out int result) == true)
                {
                    GameNetworking.instance.port = result;
                }
            }
            GUILayout.Space(5);

            if (GUILayout.Button("Connect to Server"))
            {
                NewMessage(Constants.SystemMessageTime, new TextMessage("Trying Setup..", 1, 0));

                if(PlayerSavedata != null)
                {
                    PlayerSavedata.Username = Username;
                BinaryFormatter bf = new BinaryFormatter();
                    Stream _stream;
                    _stream = File.OpenWrite(Playersavepath + "PlayerData.FrostyPreset");

                bf.Serialize(_stream, PlayerSavedata);
                    _stream.Close();

                }



                // just detects if ridermodel has changed from daryien and if so realigns to be tracking new rig
                _localplayer.RiderTrackingSetup();
                if (CharacterModding.instance.LoadBmxSetup() == 0)
                {
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Couldn't load your bmx save, save a bmx", 4, 0));
                }
                else
                {
                    
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Loaded Bmx Save", 4, 0));
                }


                if(_localplayer.RiderModelname == "Daryien")
                {
                if (CharacterModding.instance.LoadRiderSetup() == 0)
                {
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Couldn't load your Rider, Select a texture for all rider parts and save for sync of Daryien", 4, 0));
                }
                else
                {
                    
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Loaded Daryien's Save", 4, 0));
                }

                }




                try
                {
                    GameManager.instance.GetLevelName();
                }
                catch (Exception x)
                {
                    Debug.Log("Cant find scene name  : " + x);
                }

               
                ConnectToServer();
                OnlineMenu = true;
                OfflineMenu = false;

                
               
            }



            if (GUILayout.Button("Connect to FrostyP"))
            {
                
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Trying FrostyP..", 1, 0));
                if (File.Exists(Playersavepath + "PlayerData.FrostyPreset"))
                {
                    PlayerSavedata = new PlayerSaveData(Username);
                    BinaryFormatter bf = new BinaryFormatter();
                    Stream _stream;
                    _stream = File.OpenWrite(Playersavepath + "PlayerData.FrostyPreset");

                    bf.Serialize(_stream, PlayerSavedata);
                    _stream.Close();

                }


                // detects if ridermodel has changed from daryien and if so re-aligns to be tracking new rig and updates modelname and bundlename
                _localplayer.RiderTrackingSetup();



                if (CharacterModding.instance.LoadBmxSetup() == 0)
                {
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Couldn't load your bmx save, save a bmx for instant sync", 4, 0));
                }
                else
                {
                    
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Loaded Bmx Save", 4, 0));
                }

                if (_localplayer.RiderModelname == "Daryien")
                {
                    if (CharacterModding.instance.LoadRiderSetup() == 0)
                    {
                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Couldn't load your Rider, Select a texture for all rider parts and save for sync of Daryien", 4, 0));
                    }
                    else
                    {

                        InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Loaded Daryien's Save", 4, 0));
                    }

                }



                try
                {
                    GameManager.instance.GetLevelName();
                }
                catch (Exception x)
                {
                    Debug.Log("Cant find scene name  : " + x);
                }
               
                GameNetworking.instance.ConnectFrosty();
                Connected = true;
                OnlineMenu = true;
                OfflineMenu = false;




            }



            GUILayout.Space(100);
           
            
           
           
            
        }




       public void ClientsOnlineMenu()
        {

            if (AdminOpen)
            {

                if (!AdminLoggedin)
                {
                Adminpass = GUILayout.TextField(Adminpass);

                if (GUILayout.Button("Login"))
                {
                    ClientSend.AdminModeOn(Adminpass);
                }

                }

                if (AdminLoggedin)
                {
                    bootplayeropen = GUILayout.Toggle(bootplayeropen, "Boot a Player");
                    if(bootplayeropen)
                    {
                        
                        GUILayout.Label("Bantime (mins)");
                        BantimeParse = GUILayout.TextField(BantimeParse);
                        if(int.TryParse(BantimeParse, out int bantime))
                        {
                            _bantime = bantime;
                        }

                        foreach (RemotePlayer r in GameManager.Players.Values)
                        {
                            GUIStyle style = new GUIStyle();
                            style.normal.textColor = Color.green;
                            style.fontSize = 12;
                            style.alignment = TextAnchor.MiddleCenter;
                            if(GUILayout.Button($"Boot {r.username}"))
                            {
                                ClientSend.SendBootPlayer(r.username, _bantime);
                            }
                           

                        }
                        GUILayout.Space(30);
                    }

                    BootObjectOpen = GUILayout.Toggle(BootObjectOpen, "Boot an Object");
                    if (BootObjectOpen)
                    {
                        foreach(RemotePlayer r in GameManager.Players.Values)
                        {
                            if(GameManager.PlayersObjects[r.id] != null)
                            {
                                if (GameManager.PlayersObjects[r.id].Count > 0)
                                {
                                    GUILayout.Space(10);
                                    GUILayout.Label($"{r.username} objects:");
                                    foreach(NetGameObject n in GameManager.PlayersObjects[r.id])
                                    {
                                        if(GUILayout.Button($"Boot {n.NameofObject}"))
                                        {
                                            ClientSend.AdminRemoveObject(r.id, n.ObjectID);
                                        }
                                    }

                                }
                            }
                            GUILayout.Space(10);
                        }
                    }


                    GUILayout.Space(30);
                }





            }

            
            

            GUILayout.Space(20);
            if (GUILayout.Button("Mini GUI"))
            {
                FrostyPGamemanager.instance.OpenMenu = false;
                Minigui = true;
            }
           
           
            GUILayout.Space(20);

            Messagetosend = GUILayout.TextField(Messagetosend.ToString());
            GUILayout.Space(5);
            if ( GUILayout.Button("Send"))
            {
                if(Messagetosend != null)
                {
                ClientSend.SendTextMessage(Messagetosend.ToString());
                    Messagetosend = "";

                }
            }
            GUILayout.Space(20);
            if (IsSpectating)
            {
                GUILayout.Space(10);
                GUILayout.Label($"player: {cyclecounter}  CamMode: {cyclemodes}");
                if(GUILayout.Button("End Spectate"))
                {
                    SpectateExit();
                }
               


            }
            else
            {
                if (GUILayout.Button("Enter Spectate Mode"))
                {
                    SpectateEnter();
                }
            }
            GUILayout.Space(20);
           
            if (GUILayout.Button("Disconnect"))
            {
                OnlineMenu = false;
                OfflineMenu = true;
                Disconnect();
                Connected = false;
            }
            GUILayout.Space(20);
           

           
        }



       public void MiniGUI()
        {
            GUILayout.Space(10);
            if (GUILayout.Button("return",MiniLiveRidersStyle))
            {
                Minigui = false;
                OnlineMenu = true;
                OfflineMenu = false;
                FrostyPGamemanager.instance.OpenMenu = true;
               
               
            }
            GUILayout.Space(20);

           // GUILayout.Label($"Ping: {Ping}");
           // GUILayout.Label($"Pending rel: {Pendingreliable}");
           // GUILayout.Label($"Pending unrel: {Pendingunreliable}");
           // GUILayout.Label($"Out per sec:  {Outbytespersec}");
            //GUILayout.Label($"in per sec:  {InBytespersec}");
            //GUILayout.Label($"Bytes out/s: {SendBytesPersec}");

            
            MiniLiveRiderstoggle = GUILayout.Toggle(MiniLiveRiderstoggle, " Live Riders", MiniLiveRidersStyle);

            if (MiniLiveRiderstoggle)
            {
               


                GUILayout.Space(20);
                foreach (RemotePlayer r in GameManager.Players.Values)
                {
                  GUIStyle Playernamestyle = new GUIStyle();

                    Playernamestyle.alignment = TextAnchor.MiddleCenter;
                    Playernamestyle.fontStyle = FontStyle.Bold;
                    Playernamestyle.padding = new RectOffset(5, 5, 5, 5);
                    Playernamestyle.normal.background = TransTex;
                    Playernamestyle.normal.textColor = r.tm.color;
                   

                    GUILayout.Label($"{r.username} is at {r.CurrentMap}",Playernamestyle);
                }

            GUILayout.Space(20);
            }
            GUILayout.Label("Messages:", Generalstyle);

            // scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (TextMessage mess in Messages)
            {
                GUIStyle style = new GUIStyle();
                if(mess.FromCode == 3)
                {
                    try
                    {
                       style.normal.textColor = GameManager.Players[mess.FromConnection].tm.color;
                    }
                    catch (Exception x)
                    {
                        style.normal.textColor = Color.black;
                        Debug.Log($"Assign text color to player color error : {x}");
                    }

                }
                else
                {
                    style.normal.textColor = MessageColour[mess.FromCode];
                }
                style.alignment = TextAnchor.MiddleCenter;
                style.padding = new RectOffset(10, 0, 0, 10);
                style.fontStyle = FontStyle.Bold;

                GUILayout.Label(mess.Message, style);
            }

           

        }

        public void ShowRiderInfo()
        {
            GUI.skin = skin;
            if(IdofRidertoshow != 0)
            {
                try
                {
                foreach(RemotePlayer r in GameManager.Players.Values)
                {
                    if(r.id == IdofRidertoshow)
                    {
                        GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 2 - 100, Screen.height / 18), new Vector2(400, 400)));
                        GUILayout.Label($"{GameManager.Players[IdofRidertoshow].username}", Generalstyle);
                        GUILayout.Label($"Riding at: {GameManager.Players[IdofRidertoshow].CurrentMap}" ,Generalstyle);
                        GUILayout.Label($"As: {GameManager.Players[IdofRidertoshow].CurrentModelName}",Generalstyle);
                        GUILayout.Label($"Net FPS: {Mathf.RoundToInt(r.PlayersFrameRate)}",Generalstyle);
                        GUILayout.Label($"Rider2Rider Ping: {r.R2RPing} Ms",Generalstyle);

                        GUILayout.Space(10);
                        TogglePlayerTag = GUILayout.Toggle(TogglePlayerTag, "Toggle Name Tag");
                        if (TogglePlayerTag)
                        {
                            r.tm.gameObject.SetActive(true);
                        }
                        if (!TogglePlayerTag)
                        {
                            r.tm.gameObject.SetActive(false);
                        }

                        if (GameManager.PlayersObjects[IdofRidertoshow].Count > 0)
                        {
                        TogglePlayerObjects = GUILayout.Toggle(TogglePlayerObjects,"Toggle player Objects");
                            GUILayout.Space(10);
                            if (TogglePlayerObjects)
                            {
                               
                                    foreach (NetGameObject n in GameManager.PlayersObjects[IdofRidertoshow])
                                    {
                                       if(n._Gameobject != null)
                                       {
                                          if (!n._Gameobject.activeSelf)
                                           {
                                            n._Gameobject.SetActive(true);
                                          }
                                       }
                                    }

                                
                            }
                            if (!TogglePlayerObjects)
                            {
                               
                                foreach (NetGameObject n in GameManager.PlayersObjects[IdofRidertoshow])
                                {
                                    if (n._Gameobject != null)
                                    {
                                        if (n._Gameobject.activeSelf)
                                        {
                                            n._Gameobject.SetActive(false);
                                        }
                                    }
                                }

                                
                            }


                            GUILayout.Space(10);
                            GUILayout.Label($"Objects:", Generalstyle);
                            GUILayout.Space(10);
                            Riderinfoscroll = GUILayout.BeginScrollView(Riderinfoscroll);

                           
                            foreach (NetGameObject n in GameManager.PlayersObjects[IdofRidertoshow])
                            {
                                if (GUILayout.Button($"Vote off {n.NameofObject}"))
                                {
                                    ClientSend.VoteToRemoveObject(n.ObjectID, IdofRidertoshow);
                                }
                            }

                            
                            GUILayout.EndScrollView();
                        }


                        GUILayout.Space(10);
                        if (GUILayout.Button("Close"))
                        {
                            RiderInfoMenuOpen = false;
                        }
                        GUILayout.EndArea();
                    }

                }

                }
                catch (Exception x)
                {
                    Debug.Log("Showriderinfo Error : " + x);
                }
              

            }

        }


        public void LiveRiders()
        {
            try
            {
            LiveRiderBox = new Rect(new Vector2(Screen.width / 6 * 5, Screen.height / 12), new Vector2(Screen.width / 6.5f, Screen.height/3));
            GUI.skin = skin;
            GUILayout.BeginArea(LiveRiderBox);
            GUILayout.Label("Live Rider list:", Generalstyle);
            liveridersscroll = GUILayout.BeginScrollView(liveridersscroll);
            if (GameManager.Players.Count > 0)
            {
            foreach (RemotePlayer r in GameManager.Players.Values)
            {
                try
                {
                GUIStyle Playernamestyle = new GUIStyle();

                Playernamestyle.alignment = TextAnchor.MiddleCenter;
                Playernamestyle.fontStyle = FontStyle.Bold;
                Playernamestyle.padding = new RectOffset(5, 5, 5, 5);
                Playernamestyle.normal.background = TransTex;
                        try
                        {
                            if(r.tm != null)
                            {
                              Playernamestyle.normal.textColor = r.tm.color;

                            }

                        }
                        catch (Exception x)
                        {
                            Playernamestyle.normal.textColor = Color.black;
                        }
                Playernamestyle.hover.textColor = Color.green;
                Playernamestyle.hover.background = whiteTex;

                if (GUILayout.Button($"{r.username}",Playernamestyle))
                {
                    IdofRidertoshow = r.id;
                    RiderInfoMenuOpen = true;
                }

                }
                catch (Exception x)
                {
                    Debug.Log("Live Rider issue : " + x);
                }

            }

            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();


            }
            catch (Exception x)
            {
                Debug.Log("Live Riders Show Error : " + x);
            }
            GUILayout.Space(50);
        }


        public void MessagesShow()
        {
            Rect box = new Rect(new Vector2(Screen.width / 6 * 5, Screen.height / 2), new Vector2(Screen.width / 6.5f, Screen.height / 8));
            GUI.skin = skin;
            GUILayout.BeginArea(box);
            GUILayout.Label("Messages:",Generalstyle);
            GUILayout.EndArea();

            Rect _box = new Rect(new Vector2(Screen.width / 2, Screen.height / 1.9f), new Vector2(Screen.width / 2, Screen.height / 3));
            GUILayout.BeginArea(_box);
            Messagesscroll = GUILayout.BeginScrollView(Messagesscroll);

            if (Messages.Count > 0)
            {
            foreach (TextMessage mess in Messages)
            {
                GUIStyle style = new GUIStyle();
                if (mess.FromCode == 3)
                {
                    try
                    {
                        style.normal.textColor = GameManager.Players[mess.FromConnection].tm.color;
                    }
                    catch (Exception x)
                    {
                        style.normal.textColor = Color.black;
                        Debug.Log($"Assign text color to player color error : {x}");
                    }

                }
                else
                {
                    style.normal.textColor = MessageColour[mess.FromCode];
                }
                style.alignment = TextAnchor.MiddleRight;
                style.padding = new RectOffset(10, 10, 10, 10);
                style.fontStyle = FontStyle.Bold;

                
                GUILayout.Label(mess.Message, style);
                
            }


            }


            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }


        /// <summary>
        /// Call IngameUI.instance.NewMessage(constants.?, new textmessage("message", messagecolour.?, 1(incoming messages from server are coded and handled in clienthandle)  ))     to display message
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="message"></param>
        public void NewMessage(int _time, TextMessage message)
        {
            StartCoroutine(MessageEnum(_time, message));
        }
        private IEnumerator MessageEnum(int _time, TextMessage message)
        {
            InGameUI.instance.Messages.Add(message);
            yield return new WaitForSeconds(_time);
            InGameUI.instance.Messages.Remove(message);
            yield return null;
        }


        /// <summary>
        /// Called by Disconnectme in ClientHandle, if the server rejects us it sends a command for us to initiate the disconnect, meaning the user can receive an online message before hand saying that they have lost connection with the reason sent from server
        /// </summary>
        public void Waittoend()
        {
            StartCoroutine(WaitthenEnd());
        }
        private IEnumerator WaitthenEnd()
        {
            yield return new WaitForSeconds(3);
           OnlineMenu = false;
            OfflineMenu = true;
            yield return null;
        }


        // --------------------------------------------------------------------------------------------  SPECTATE MODE ---------------------------------------------------------------------------------------------------
        public void SpectateEnter()
        {
           
            if (GameManager.Players.Count > 0)
            {

            bool got = false;
            foreach(RemotePlayer rem in GameManager.Players.Values)
            {
                if (!got)
                {
                    Targetrider = rem.RiderModel;
                    ControlObj.transform.position = rem.RiderModel.transform.position + (Vector3.back * 2) + (Vector3.up);
                    SpecCamOBJ.transform.position = rem.RiderModel.transform.position + (Vector3.back * 2) + (Vector3.up);
                    got = true;
                }
                cycleplayerslist.Add(rem);
            }



            // ControlObj.transform.parent = Ridersmoothfollower.transform;
                  if (got)
                  {
                    SpecCamOBJ.SetActive(true);
                    IsSpectating = true;
                  }


            }
           
        }

        public void SpectateAutoFollowFreeMove()
        {
            float speed = 15;
           

           SpecCamOBJ.transform.parent = Targetrider.transform;
            ControlObj.transform.position = Targetrider.transform.position + Targetrider.transform.TransformDirection(Vector3.up);
         
                SpecCamOBJ.transform.LookAt(ControlObj.transform);
                //ControlObj.transform.LookAt(Ridersmoothfollower.transform);
            
            if(MGInputManager.RStickX()> 0.15f | MGInputManager.RStickX() < -0.15f)
            {
                
                SpecCamOBJ.transform.RotateAround(Targetrider.transform.position, Vector3.up, -MGInputManager.RStickX() * Time.deltaTime * speed * 5);
            }
            if (MGInputManager.LStickY() > 0.15f)
            {

                Vector3 dir = -(SpecCamOBJ.transform.position - Targetrider.transform.position).normalized;
               SpecCamOBJ.gameObject.transform.position = Vector3.MoveTowards(SpecCamOBJ.transform.position, SpecCamOBJ.transform.position + dir, Time.deltaTime * 5);
            }
            if (MGInputManager.LStickY() < -0.15f)
            {
                Vector3 dir = (SpecCamOBJ.transform.position - Targetrider.transform.position).normalized;
                SpecCamOBJ.gameObject.transform.position = Vector3.MoveTowards(SpecCamOBJ.transform.position, SpecCamOBJ.transform.position + dir, Time.deltaTime * 5);
            }


            if (MGInputManager.RStickY() > 0.15f | MGInputManager.RStickY() < -0.15f)
            {
               SpecCamOBJ.gameObject.transform.RotateAround(Targetrider.transform.position, SpecCamOBJ.gameObject.transform.right, MGInputManager.RStickY() * Time.deltaTime * speed * 5);
            }


            
                
               


        }

        public void SpectateTripodAutoLookAt()
        {
            ControlObj.transform.parent = null;
            SpecCamOBJ.transform.parent = null;

            ControlObj.transform.position = Vector3.Lerp(ControlObj.transform.position, Targetrider.transform.position +  Vector3.up, Vector3.Distance(ControlObj.transform.position, Targetrider.transform.position + Vector3.up) * 4 * Time.deltaTime);
            Cam.transform.LookAt(ControlObj.transform);
            ControlObj.transform.LookAt(ControlObj.transform);
           

            float zoomspeed = 15f;
            float maxzoom = 1;
            float minzoom = 120;
            
            float X = 0;
            float Y = 0;
            float Z = 0;
            if (MGInputManager.RStickX() > 0.2f | MGInputManager.RStickX() < -0.2f)
            {
                X = MGInputManager.RStickX() * Time.deltaTime * 8;
            }
            if (MGInputManager.RStickY() > 0.2f | MGInputManager.RStickY() < -0.2f)
            {
                Y = MGInputManager.RStickY() * Time.deltaTime * 8;
            }


            if (MGInputManager.LStickY() > 0.2f && MGInputManager.LTrigger() < 0.5f )
            {
                Z = MGInputManager.LStickY() * Time.deltaTime * 10;
            }
            if(MGInputManager.LStickY() < -0.2f && MGInputManager.LTrigger() < 0.5f)
            {
                Z = MGInputManager.LStickY() * Time.deltaTime * 10;
            }

            // if LT down
            if(MGInputManager.LTrigger() > 0.5f)
            {

            // Hold LT then use LstickY to Zoom in and out
            if (MGInputManager.LStickY() > 0.2f)
            {
               
                Camera.current.fieldOfView = Camera.current.fieldOfView + (-MGInputManager.LStickY() * Time.deltaTime * zoomspeed);
            }
            if (MGInputManager.LStickY() < -0.2f)
            {
                Camera.current.fieldOfView = Camera.current.fieldOfView + (-MGInputManager.LStickY() * Time.deltaTime * zoomspeed);
            }

                // Focus distance
                if (MGInputManager.RStickY() > 0.18f)
                {
                  //  FrostyPGamemanager.instance.focusdistanceCAMSETTING++;
                  //  FrostyPGamemanager.instance.de

                }


            }
            

            Cam.gameObject.transform.Translate(X, Y, Z);

        }

        public void SpectateTripodFullManual()
        {
            if (Targetrider == null)
            {
                Targetrider = cycleplayerslist[0].RiderModel;

            }


            float X = 0;
            float Y = 0;
            float Z = 0;

            if (MGInputManager.RStickX() > 0.2f | MGInputManager.RStickX() < -0.2f)
            {
                X = MGInputManager.RStickX() * Time.deltaTime * 8;
            }
            if (MGInputManager.RStickY() > 0.2f | MGInputManager.RStickY() < -0.2f)
            {
                Z = MGInputManager.RStickY() * Time.deltaTime * 8;
            }
            if (MGInputManager.LStickY() > 0.2f| MGInputManager.LStickY() < -0.2f)
            {
                Y = MGInputManager.LStickY() * Time.deltaTime * 4;
            }


            if (MGInputManager.LStickX() > 0.2f | MGInputManager.LStickX() < -0.2f)
            {
                Cam.transform.Rotate(0, MGInputManager.LStickX() * 50 * Time.fixedDeltaTime, 0);
            }
            if (MGInputManager.LStickY() > 0.2f | MGInputManager.LStickY() < -0.2f)
            {
                Cam.transform.Rotate(-MGInputManager.LStickY() * 50 * Time.fixedDeltaTime,0, 0);
            }


            Cam.transform.Translate(X, 0, Z);

            if(MGInputManager.LTrigger() > 0.5f)
            {
                if (MGInputManager.LStickY() > 0.2f | MGInputManager.LStickY() < -0.2f)
                {
                    Cam.transform.Translate(0, MGInputManager.LStickY() * 18 * Time.fixedDeltaTime, 0);
                }
            }

        }





        public void SpectateExit()
        {
            if (cycleplayerslist.Count > 0)
            {
            cycleplayerslist.Clear();
            }
            cyclecounter = 0;
            IsSpectating = false;
            SpecCamOBJ.SetActive(false);
           
            Targetrider = null;

        }

        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        













    }

    /// <summary>
    /// Incoming messages get different colors
    /// </summary>
    public enum MessageColour
    {
        System = 1,
        Me = 2,
        Player = 3,
        Server = 4,
    }


}