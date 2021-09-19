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
using Valve.Sockets;
using System.Diagnostics;




namespace PIPE_Valve_Console_Client
{
    public class InGameUI : MonoBehaviour
    {
        public GUISkin skin = (GUISkin)ScriptableObject.CreateInstance("GUISkin");
        public GUIStyle Generalstyle = new GUIStyle();
        GUIStyle SyncWindowStyle1 = new GUIStyle();
        GUIStyle SyncWindowStyle2 = new GUIStyle();
        GUIStyle SyncWindowStyle3 = new GUIStyle();
        GUIStyle PlayeroptionsStyle = new GUIStyle();
        public GUIStyle MiniPanelStyle = new GUIStyle();
        GUIStyle MinipanelstyeImportant = new GUIStyle();
        public static GUIStyle BoxStyle = new GUIStyle();
        GUIStyle CurrentOverrideStyle;
        GUIStyle ButtonOnstyle = new GUIStyle();
        private PlayerSaveData PlayerSavedata;
        private string Playersavepath = Application.dataPath + "/FrostyPGameManager/PlayerSaveData/";
        public static InGameUI instance;
        public LocalPlayer LocalPlayer;
        //Spectate mode
        public GameObject SpecCamOBJ;
        public Camera Cam;
        GameObject Ridersmoothfollower;
        GameObject ControlObj;
        public GameObject Targetrider;
        public List<RemotePlayer> cycleplayerslist;
        public int cyclecounter;
        delegate void CamMode();
        Dictionary<int, CamMode> CamModes;
        public int cyclemodes = 0;
        Dictionary<int, string> CamModeDisplays;
        public bool ShowCamSettingsOverride;
        public bool SpectateShowGUI = true;
        // connection status
        public float Ping = 0;
        public float LastPing = 0;
        public float Outbytespersec = 0;
        public float InBytespersec = 0;
        public int Pendingreliable;
        public int Pendingunreliable;
        public ConnectionState connectionstate;
        public float connectionqualitylocal;
        public float connectionqualityremote;
        public int SendRate;
        // Admin mode
        string Adminpass = "Admin Password..";
        bool AdminOpen;
        public bool AdminLoggedin;
        bool bootplayeropen;
        string BantimeParse = "5"; // mins
        int _bantime = 5;
        bool BootObjectOpen;
        public int ServerPendingRel;
        public int ServerPendingUnRel;
        public int ServerPlayercount;
        public float serverbytesoutpersec;
        public float serverbytesinpersec;
        public int Serverinindexes;
        public int Serveroutindexes;

        Vector2 BootObjectScroll;
        Vector2 BootPlayerScroll;
        string Banword = "word to ban..";
        // MiniGUI
        Vector2 minguiscroll;
        // saved servers
        Vector2 savedserversscroll;
        // live riders
        Vector2 liveridersscroll;
        bool LiveRiderToggle;
        // Messages
        Vector2 Messagesscroll;
        bool MessagesToggle;
        string Messageslabel = "Messages";
        // Riderinfo
        Vector2 Riderinfoscroll;
        // FileSync
        /// <summary>
        /// give number, get displayable word for that FileType
        /// </summary>
        public Dictionary<int, string> FileTypeDisplayNames = new Dictionary<int, string>();
        Vector2 SyncScroll1;
        Vector2 SyncScroll2;
        int InPacketsReceived = 0;
        int InPacketsTotal = 0;
        int OutPacketsTotal = 0;
        int OutPacketsSent = 0;
        // playeroptions
        public bool PlayeroptionsOpen;
        public bool CollisionsToggle = true;
        string Collisionslabel = "Turn Collisions Off";
        public bool AllPlayerObjectsVisibleToggle = true;
        string PlayerObjectsLabel = "Turn Objects Off";
        public bool PlayerTagsToggle = true;
        string PlayerTagsLabel = "Turn Objects Off";
        public bool BottompanelToggle = true;
        string BottompanelLabel = "Turn Bottom Panel Off";
        Dictionary<int, string> connectionstatelabels = new Dictionary<int, string>();
        // bottom panel
        bool BottompanelOpen = true;
        public string currentGaragePreset = "None";
        // Update window
        float Versionofupdate;
        List<string> UpdateFiles;
        bool UpdateOpen;
        public bool UpdateDownloaded;
        public string Username = "Username...";
        public string Nickname = "Server 1";
        /// <summary>
        /// In Online mode
        /// </summary>
        public bool Connected;
        public bool OfflineMenu = true;
        public bool OnlineMenu;
        public bool Minigui;
        public bool SyncWindowOpen;
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
        public Texture2D RedTex;
        public Texture2D BlackTex;
        public Texture2D GreyTex;
        public Texture2D GreenTex;
        public Texture2D whiteTex;
        public Texture2D TransTex;
        // players that are on my level but are toggled off
        public List<uint> ToggledOffPlayersByMe = new List<uint>();

        float movespeed;
        float rotatespeed;

        public Stopwatch publicisewatch = new Stopwatch();
        public List<PublicServer> Publicservers = new List<PublicServer>();
        public bool UpdatingServerlist;
        Vector2 publicserverscroll;
        Vector2 savedserverscroll;


        private void Awake()
        {
           

            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                UnityEngine.Debug.Log("IngameUI already exists, destroying old InGameUI now");
                Destroy(this);
            }

            CamModes = new Dictionary<int, CamMode>
            {
                {0,SpectateAutoFollowFreeMove },
                {1,SpectateTripodAutoLookAt },
                {2,SpectateTripodFullManual },

            };
            CamModeDisplays = new Dictionary<int, string>
            {
                {0,"Auto-Follow / Free Move" },
                {1,"Tripod/Auto Look At : LT for Variable Zoom   " },
                {2,"Tripod / Full Manual : LB for Rotation   " },

            };



            connectionstatelabels = new Dictionary<int, string>()
            {
                {3, "Connected" },
                {4, "Server Closed" },
                {1, "Connecting.." },
                {0, "Offline" },
                {5, "Local Problem" },


            };







            RedTex = new Texture2D(20,10);
            Color[] colorarray = RedTex.GetPixels();
            Color newcolor = new Color(0.5f, 0, 0, 1);
            for (var i = 0; i < colorarray.Length; ++i)
            {
                colorarray[i] = newcolor;
            }

            RedTex.SetPixels(colorarray);

            RedTex.Apply();

            BlackTex = new Texture2D(2, 2);
            Color[] colorarray2 = BlackTex.GetPixels();
            Color newcolor2 = new Color(0, 0, 0, 1f);
            for (var i = 0; i < colorarray2.Length; ++i)
            {
                colorarray2[i] = newcolor2;
            }

            BlackTex.SetPixels(colorarray2);

            BlackTex.Apply();

            GreyTex = new Texture2D(2, 2);
            Color[] colorarray3 = GreyTex.GetPixels();
            Color newcolor3 = new Color(0.3f, 0.3f, 0.3f, 1);
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
            Color newcolor5 = new Color(1f, 1f, 1f, 0.7f);
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





        }

        private void Start()
        {

            SetupGuis();

           


            MessageColour = new Dictionary<int, Color>()
            {
                {(int)PIPE_Valve_Console_Client.MessageColourByNum.Me,Color.blue},
                {(int)PIPE_Valve_Console_Client.MessageColourByNum.Player,Color.black},
                {(int)PIPE_Valve_Console_Client.MessageColourByNum.System,Color.green},
                {(int)PIPE_Valve_Console_Client.MessageColourByNum.Server,Color.red},
            };


            FileTypeDisplayNames = new Dictionary<int, string>()
            {
                 {(int)FileTypeByNum.Texture,"Texture" },
                 {(int)FileTypeByNum.Map,"Map" },
                 {(int)FileTypeByNum.PlayerModel,"Player Model" },
                 {(int)FileTypeByNum.ParkAsset,"Park Asset" },
                 {(int)FileTypeByNum.Garage,"Garage mesh" },

            };


           
            if (!Directory.Exists(Playersavepath))
            {
                Directory.CreateDirectory(Playersavepath);
            }
            if (!File.Exists(Playersavepath + "PlayerData.FrostyPreset"))
            {
                PlayerSavedata = new PlayerSaveData(Username);
                PlayerSavedata.savedservers = new List<SavedServer>();
                BinaryFormatter bf = new BinaryFormatter();
                Stream stream = File.OpenWrite(Playersavepath + "PlayerData.FrostyPreset");
                bf.Serialize(stream, PlayerSavedata);
                stream.Close();

            }
            else if (File.Exists(Playersavepath + "PlayerData.FrostyPreset"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                Stream stream = File.OpenRead(Playersavepath + "PlayerData.FrostyPreset");
                PlayerSavedata = bf.Deserialize(stream) as PlayerSaveData;
                stream.Close();

                Username = PlayerSavedata.Username;
               

            }

            LocalPlayer = gameObject.GetComponent<LocalPlayer>();
            cycleplayerslist = new List<RemotePlayer>();
            Ridersmoothfollower = new GameObject();
            DontDestroyOnLoad(Ridersmoothfollower);
            ControlObj = new GameObject();
            DontDestroyOnLoad(ControlObj);
            SpecCamOBJ = GameObject.Instantiate(UnityEngine.GameObject.Find("Main Camera"));
            Cam = SpecCamOBJ.GetComponent<Camera>();
            SpecCamOBJ.name = "SpecCam";
            DontDestroyOnLoad(SpecCamOBJ);
            SpecCamOBJ.SetActive(false);

        }

        void Update()
        {
            if (IsSpectating)
            {
                if (Input.GetKeyDown(KeyCode.H))
                {
                    SpectateShowGUI = !SpectateShowGUI;
                }


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
                        ControlObj.transform.position = Targetrider.transform.position + (Vector3.back * 2) + (Vector3.up);
                        SpecCamOBJ.transform.position = Targetrider.transform.position + (Vector3.back * 2) + (Vector3.up);


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
                        ControlObj.transform.position = Targetrider.transform.position + (Vector3.back * 2) + (Vector3.up);
                        SpecCamOBJ.transform.position = Targetrider.transform.position + (Vector3.back * 2) + (Vector3.up);

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

                
                if (Connected)
                {
                    GameManager.KeepNetworkActive();
                }


            }

            


            if (Connected && OnlineMenu && Input.GetKeyDown(KeyCode.F12))
            {
                AdminOpen = !AdminOpen;
            }

            if (Input.GetKeyDown(KeyCode.Return) && Connected && OnlineMenu)
            {
                if (Messagetosend.Length > 0)
                {
                    ClientSend.SendTextMessage(Messagetosend.ToString());
                    Messagetosend = "";
                }
            }

           

        }

        void FixedUpdate()
        {
            if (IsSpectating && Connected)
            {
            CamModes[cyclemodes]();

            }


            if (!Connected)
            {
                if(FrostyPGamemanager.instance.MenuShowing == 5)
                {
                UpdateServerData();
                }
                else
                {
                    if (publicisewatch.IsRunning)
                    {
                        publicisewatch.Reset();
                    }
                }
            }
        }

        void OnGUI()
        {


            try
            {


                if (IsSpectating)
                {
                    if (SpectateShowGUI)
                    {
                     ShowSpectate();
                    if (ShowCamSettingsOverride)
                    {
                        CameraSettings.instance.Show();
                    }
                    }
                }
            
               if (Minigui)
               {
                MiniGUI();
               }

               if(OnlineMenu && !Minigui && FrostyPGamemanager.instance.OpenMenu && FrostyPGamemanager.instance.MenuShowing == 5)
               {
                LiveRiders();
                MessagesShow();


                  if (RiderInfoMenuOpen)
                  {
                    ShowRiderInfo();
                  }


                    if (SyncWindowOpen)
                    {
                        ShowSyncWindow();
                    }


                    if (PlayeroptionsOpen)
                    {
                        ShowPlayerOptions();
                    }

               }


                if (UpdateOpen)
                {
                    ShowUpdateWindow();
                }


            }
            catch (Exception x)
            {
                UnityEngine.Debug.Log("ingameui error: " + x);
            }

            

        }
     
        public void Show()
        {

            if (OfflineMenu)
            {
                ClientsOfflineMenu();
                ShowPublicServers();
            }
            if (OnlineMenu)
            {
                ClientsOnlineMenu();
            }
            if (Minigui)
            {
                MiniGUI();
            }
        }

        /// <summary>
        /// Master Call to connect, also turns connected to true which some data sends have as their/one of their conditions
        /// </summary>
        public void ConnectToServer(string ip, string port)
        {
           
            GameNetworking.instance.ConnectMaster(ip,port);
            Connected = true;
          
        }

        /// <summary>
        /// Call to Shutdown, also cleans up all gamemanger data and remote riders in scene
        /// </summary>
        public void Disconnect()
        {

            try
            {
            Connected = false;


            List<GameObject> playerroots = new List<GameObject>();
            GameNetworking.instance.DisconnectMaster();
           
            foreach (RemotePlayer r in GameManager.Players.Values)
            {
                r.MasterShutdown();
                playerroots.Add(r.gameObject);
                
            }
                if (GameManager.Players.Count > 0)
                {
                  GameManager.Players.Clear();

                }
            if (playerroots.Count > 0)
            {
                for (int i = 0; i < playerroots.Count; i++)
                {
                   Destroy(playerroots[i]);
                }
                
            }

            FileSyncing.WaitingRequests.Clear();
            FileSyncing.IncomingIndexes.Clear();
            FileSyncing.OutGoingIndexes.Clear();

            Resources.UnloadUnusedAssets();
            GameManager.instance.UnLoadOtherPlayerModels();
            GameManager.instance.UpdatePlayersOnMyLevelToggledOff();
            ParkBuilder.instance.ResetLoadedBundles();
            if (IsSpectating)
            {
                SpectateExit();
            }
            this.StopAllCoroutines();
            Messages.Clear();
            // Server learns of disconnection itself and tells everyone

            }
            catch (Exception x)
            {
                UnityEngine.Debug.Log("MP Disconnect error: " + x);
            }

        }
        
        public void ClientsOfflineMenu()
        {
           
            GUI.skin = skin;
            GUILayout.BeginArea(new Rect(new Vector2(Screen.width/3,Screen.height/4), new Vector2(Screen.width/3,Screen.height/2)),BoxStyle);
            GUILayout.Space(10);
            // setup stuff before connecting
            GUILayout.BeginHorizontal();
            GUILayout.Label("Username", GUILayout.MaxWidth(100));
            Username = GUILayout.TextField(Username);
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Label("IP:", GUILayout.MaxWidth(100));
            desiredIP = GUILayout.TextField(desiredIP);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("PORT:", GUILayout.MaxWidth(100));
            if (desiredIP != "")
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
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
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



                // detects model and aligns tracking transforms
                LocalPlayer.RiderTrackingSetup();
               


                if(LocalPlayer.RiderModelname == "Daryien")
                {
                if (CharacterModding.instance.LoadRiderSetup() == 0)
                {
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Couldn't load Daryien Save, Select a texture for all Daryien parts and save for sync of custom Daryien", 4, 0));
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
                    UnityEngine.Debug.Log("Cant find scene name  : " + x);
                }

               
                ConnectToServer(desiredIP,desiredport);
                OnlineMenu = true;
                OfflineMenu = false;

                
               
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
           
                GUILayout.Label("Saved servers:");
                GUILayout.BeginHorizontal();
                if(GUILayout.Button($"Save current setup as"))
                {

                    if(PlayerSavedata.savedservers == null)
                    {
                     PlayerSavedata.savedservers = new List<SavedServer>();
                    }

                    if(PlayerSavedata != null)
                    {
                     PlayerSavedata.savedservers.Add(new SavedServer(desiredIP, desiredport, Nickname));
                     BinaryFormatter bf = new BinaryFormatter();
                     Stream stream = File.OpenWrite(Playersavepath + "PlayerData.FrostyPreset");
                     bf.Serialize(stream, PlayerSavedata);
                     stream.Close();
                    }
                }
                Nickname = GUILayout.TextField(Nickname);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            savedserverscroll = GUILayout.BeginScrollView(savedserverscroll);
            GUILayout.Space(10);
                if (PlayerSavedata != null && PlayerSavedata.savedservers != null)
                {
                GUIStyle removestyle = new GUIStyle();
                removestyle.normal.background = RedTex;
                removestyle.hover.background = GreyTex;
                removestyle.alignment = TextAnchor.MiddleCenter;
                removestyle.normal.textColor = Color.white;
                removestyle.hover.textColor = Color.white;


                 savedserversscroll = GUILayout.BeginScrollView(savedserversscroll);
                    foreach(SavedServer s in PlayerSavedata.savedservers.ToArray())
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Select :" + s.Nickname))
                        {
                            desiredIP = s.IP;
                            desiredport = s.PORT;
                        }
                        GUILayout.Space(5);
                        if (GUILayout.Button("Remove :" + s.Nickname,removestyle))
                        {
                          PlayerSavedata.savedservers.Remove(s);
                          BinaryFormatter bf = new BinaryFormatter();
                          Stream stream = File.OpenWrite(Playersavepath + "PlayerData.FrostyPreset");
                          bf.Serialize(stream, PlayerSavedata);
                          stream.Close();
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);
                    }
                 GUILayout.EndScrollView();
                }


            GUILayout.EndScrollView();
            GUILayout.Space(40);
            if (GUILayout.Button("Close"))
            {
                FrostyPGamemanager.instance.MenuShowing = 0;
            }
            GUILayout.EndArea();
            
           
           
            
        }

        public void ShowPublicServers()
        {
            GUI.skin = skin;
            GUILayout.BeginArea(new Rect(new Vector2(Screen.width - (Screen.width/5) - 10,(Screen.height - (Screen.height/1.2f)) /2), new Vector2(Screen.width / 5, Screen.height / 1.2f)), BoxStyle);

            GUILayout.Space(30);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Public Servers:");
            if (GUILayout.Button("Refresh"))
            {
                StopCoroutine(GameManager.UpdatePublicServers());
                StartCoroutine(GameManager.UpdatePublicServers());
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginScrollView(publicserverscroll);
            if (UpdatingServerlist)
            {
                GUILayout.Label("Updating list...");
            }
            else
            {
                if (Publicservers.Count > 0)
                {
                    for (int i = 0; i < Publicservers.Count; i++)
                    {
                        ShowPublicServerButton(Publicservers[i]);
                    }
                }

            }
            GUILayout.EndScrollView();


            GUILayout.Space(10);
            GUILayout.EndArea();
        }


        public void ClientsOnlineMenu()
        {

            GUILayout.Space(20);
            GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 4, 5 + (Screen.height/50)), new Vector2(Screen.width / 2, Screen.height / 20)));
            GUILayout.BeginHorizontal();
            PlayeroptionsOpen = GUILayout.Toggle(PlayeroptionsOpen,"Options");
            GUILayout.Space(2);
            SyncWindowOpen = GUILayout.Toggle(SyncWindowOpen, "Sync Window");
            GUILayout.Space(2);
            if (GUILayout.Button("Mini GUI"))
            {
                FrostyPGamemanager.instance.OpenMenu = false;
                Minigui = true;
            }
            GUILayout.Space(2);
            if (IsSpectating)
            {
                if (GUILayout.Button("End Spectate"))
                {
                    SpectateExit();
                }
               
            }
            else
            {
                if (GUILayout.Button("Spectate"))
                {
                    SpectateEnter();
                }
            }
            GUILayout.Space(2);
            if (GUILayout.Button("Sync Bmx"))
            {
                ClientSend.SendGearUpdate(GameManager.instance.GetMyGear(false));
            }

            GUILayout.Space(2);
            if (GUILayout.Button("Disconnect"))
            {
                OnlineMenu = false;
                OfflineMenu = true;
                Disconnect();
                Connected = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            if (AdminOpen)
            {
                GUILayout.BeginHorizontal();
                if (!AdminLoggedin)
                {
                    Adminpass = GUILayout.TextField(Adminpass);
                    GUILayout.Space(10);
                    if (GUILayout.Button("Login"))
                    {
                    ClientSend.AdminModeOn(Adminpass);
                    }

                }

                if (AdminLoggedin)
                {
                    if (GUILayout.Button("Log Out"))
                    {
                        ClientSend.AdminLogOut();
                        AdminLoggedin = false;
                        AdminOpen = false;
                    }
                    GUILayout.Space(10);

                    bootplayeropen = GUILayout.Toggle(bootplayeropen, "Boot a Player");

                    BootObjectOpen = GUILayout.Toggle(BootObjectOpen, "Boot an Object");

                    GUILayout.Space(30);
                    GUILayout.Label($"PRel: {ServerPendingRel} ");
                    GUILayout.Label($"PUnrel: {ServerPendingUnRel}");
                    GUILayout.Label($"out: {serverbytesoutpersec}");
                    GUILayout.Label($"in: {serverbytesinpersec}");
                    GUILayout.Label($"player count: {ServerPlayercount}");
                    Banword = GUILayout.TextField(Banword);
                    if(GUILayout.Button("Add word"))
                    {
                        ClientSend.AdminAlterBanWords(true, Banword);
                    }
                    if (GUILayout.Button("remove word"))
                    {
                        ClientSend.AdminAlterBanWords(false, Banword);
                    }


                }

                GUILayout.EndHorizontal();


            }

            GUILayout.EndArea();

                    if(bootplayeropen)
                    {
                    GUILayout.BeginArea(new Rect(new Vector2(Screen.width/4-5,200), new Vector2(Screen.width/4,Screen.height-300)),BoxStyle);
                    GUILayout.Label("Players");
                    GUILayout.Space(5);
                        GUILayout.Label("Bantime (mins)");
                        BantimeParse = GUILayout.TextField(BantimeParse);
                        if(int.TryParse(BantimeParse, out int bantime))
                        {
                            _bantime = bantime;
                        }
                     BootPlayerScroll = GUILayout.BeginScrollView(BootPlayerScroll);
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
                GUILayout.EndScrollView();
                    GUILayout.EndArea();
                        
                    }

                    if (BootObjectOpen)
                    {
                    GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 2 + 5, 200), new Vector2(Screen.width / 4, Screen.height - 300)),BoxStyle);
                    GUILayout.Label("Live Objects");
                    GUILayout.Space(10);
                    BootObjectScroll = GUILayout.BeginScrollView(BootObjectScroll);
                    GUILayout.Space(10);
                        foreach (RemotePlayer r in GameManager.Players.Values)
                        {
                          for (int i = 0; i < r.Objects.Count; i++)
                          {
                            if(r.Objects[i]._Gameobject != null)
                            {
                                
                                GUILayout.Space(10);
                                GUILayout.Label($"{r.username} objects:");
                                    
                                 if(GUILayout.Button($"Boot {r.Objects[i].NameofObject}"))
                                 {
                                   ClientSend.AdminRemoveObject(r.id, r.Objects[i].ObjectID);
                                 }
                                    

                                
                            }

                          }
                            GUILayout.Space(10);
                        }
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();
                    }

            if (BottompanelOpen)
            {
                ShowBottomPanel();
            }

        }

        /// <summary>
        /// Call IngameUI.instance.NewMessage(constants.?, new textmessage("message", messagecolour.?, 1(incoming messages from server are coded and handled in clienthandle)  ))     to display message
        /// </summary>
        /// <param name="_secs"></param>
        /// <param name="message"></param>
        public void NewMessage(int _secs, TextMessage message)
        {
            // if message is too long, add line breaks
            int length = MessagesToggle ? 80 : 35;


            if (message.Message.Length > length)
            {
                char[] chars = message.Message.ToCharArray();
                int counter = 0;
                for (int i = 0; i < chars.Length; i++)
                {
                    if(counter == length)
                    {
                       message.Message = message.Message.Insert(i, "\n");
                        counter = 0;
                    }
                    counter++;
                }
            }


            StartCoroutine(MessageEnum(_secs, message));
        }
        private IEnumerator MessageEnum(int _secs, TextMessage message)
        {
            InGameUI.instance.Messages.Add(message);
            yield return new WaitForSeconds(_secs);
            InGameUI.instance.Messages.Remove(message);
            yield return null;
        }

        /// <summary>
        /// Called by Disconnectme in ClientHandle, if the server rejects us it sends a command for us to initiate the disconnect, meaning the user can receive an online message before hand saying that they have lost connection with the reason sent from server
        /// </summary>
        public void ShutdownAfterMessageFromServer()
        {
            StartCoroutine(WaitthenEnd());
        }
        private IEnumerator WaitthenEnd()
        {
            yield return new WaitForSeconds(5);
           OnlineMenu = false;
            OfflineMenu = true;
            yield return null;
        }

        // --------------------------------------------------------------------------------------------  SPECTATE MODE ---------------------------------------------------------------------------------------------------
        public void SpectateEnter()
        {
            cycleplayerslist = new List<RemotePlayer>();
            if (GameManager.Players.Count > 0)
            {

            bool got = false;
            foreach(RemotePlayer rem in GameManager.Players.Values)
            {
                if (!got && rem.RiderModel)
                {
                        if (rem.PlayerIsVisible)
                        {
                          Targetrider = rem.RiderModel;
                          ControlObj.transform.position = rem.RiderModel.transform.position + (Vector3.back * 2) + (Vector3.up);
                          SpecCamOBJ.transform.position = rem.RiderModel.transform.position + (Vector3.back * 2) + (Vector3.up);
                          got = true;

                        }
                }
                cycleplayerslist.Add(rem);
            }



            // ControlObj.transform.parent = Ridersmoothfollower.transform;
                  if (got)
                  {
                    SpecCamOBJ.SetActive(true);
                    FrostyPGamemanager.instance.OpenMenu = false;
                    IsSpectating = true;
                    GameManager.TogglePlayerComponents(false);
                  }


            }
           
        }

        public void SpectateAutoFollowFreeMove()
        {
            movespeed = 10;
            rotatespeed = 50;
            float smoothtime = 1f * Time.fixedDeltaTime;
            float rotsmooth = 5 * Time.fixedDeltaTime;

         
            ControlObj.transform.parent = Targetrider.transform;

            Quaternion lookat = Quaternion.LookRotation((Targetrider.transform.position + (Vector3.up/2)) - SpecCamOBJ.transform.position);
            SpecCamOBJ.transform.rotation = Quaternion.Lerp(SpecCamOBJ.transform.rotation, lookat,rotsmooth);
            SpecCamOBJ.transform.position = Vector3.Lerp(SpecCamOBJ.transform.position, ControlObj.transform.position, Vector3.Distance(SpecCamOBJ.transform.position, ControlObj.transform.position) * smoothtime);
            ControlObj.transform.LookAt(Targetrider.transform);


            if(MGInputManager.LStickX()> 0.1f | MGInputManager.LStickX() < -0.1f | MGInputManager.LStickY() > 0.1f | MGInputManager.LStickY() < -0.1f | MGInputManager.RStickY() > 0.1f | MGInputManager.RStickY() < -0.1f)
            {

                ControlObj.transform.Translate(MGInputManager.LStickX() * Time.fixedDeltaTime * movespeed, MGInputManager.RStickY() * Time.fixedDeltaTime * movespeed, MGInputManager.LStickY() * Time.fixedDeltaTime * movespeed);


            }

            


        }

        public void SpectateTripodAutoLookAt()
        {
            ControlObj.transform.parent = null;
            SpecCamOBJ.transform.parent = null;

            ControlObj.transform.position = Vector3.Lerp(ControlObj.transform.position, Targetrider.transform.position +  Vector3.up, Vector3.Distance(ControlObj.transform.position, Targetrider.transform.position + Vector3.up) * 6 * Time.deltaTime);
            Cam.transform.LookAt(ControlObj.transform);
           

            float zoomspeed = 15f;
            
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
            ControlObj.transform.parent = null;
            SpecCamOBJ.transform.parent = null;

            if (Targetrider == null)
            {
                Targetrider = cycleplayerslist[0].RiderModel;

            }
            movespeed = 10;
            rotatespeed = 50;


            if (!MGInputManager.LB_Hold())
            {
                if (MGInputManager.LStickX() > 0.2f | MGInputManager.LStickX() < -0.2f | MGInputManager.LStickY() > 0.2f | MGInputManager.LStickY() < -0.2f | MGInputManager.RStickX() > 0.2f | MGInputManager.RStickX() < -0.2f | MGInputManager.RStickY() > 0.2f | MGInputManager.RStickY() < -0.2f)
                {
                    Cam.transform.Translate(MGInputManager.LStickX() * Time.fixedDeltaTime * movespeed, MGInputManager.RStickY() * Time.fixedDeltaTime * movespeed, MGInputManager.LStickY() * Time.fixedDeltaTime * movespeed);
                }

            }
            else
            {
                Cam.transform.Rotate(-MGInputManager.LStickY() * Time.fixedDeltaTime * rotatespeed, MGInputManager.LStickX() * Time.fixedDeltaTime * rotatespeed, -MGInputManager.RStickX() * Time.fixedDeltaTime * rotatespeed);
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
            FrostyPGamemanager.instance.OpenMenu = true;
            ControlObj.transform.parent = null;
            SpecCamOBJ.SetActive(false);
            GameManager.TogglePlayerComponents(true);

        }

        public void ShowSpectate()
        {
            GUI.skin = skin;
            GUILayout.BeginArea(new Rect(new Vector2((Screen.width - (Screen.width/1.2f)) / 2 , 5), new Vector2(Screen.width / 1.2f, 20)),MiniPanelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"H to Hide: B changes mode: LB/RB changes rider  ");
            GUILayout.Label($"CamMode: {CamModeDisplays[cyclemodes]}");
            GUILayout.Space(10);
            GUILayout.Label($"player: { cycleplayerslist[cyclecounter].username} ");
            GUILayout.Space(10);
            if (GUILayout.Button("Cam Settings"))
            {
                ShowCamSettingsOverride = !ShowCamSettingsOverride;
            }
            GUILayout.Space(10);
            if (GUILayout.Button("End Spectate"))
            {
                SpectateExit();
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


       public void MiniGUI()
        {
            GUI.skin = skin;
            GUILayout.BeginArea(new Rect(new Vector2(50, 0), new Vector2(Screen.width - 100, Screen.height / 55)),MiniPanelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("return",MinipanelstyeImportant))
            {
                Minigui = false;
                OnlineMenu = true;
                OfflineMenu = false;
                FrostyPGamemanager.instance.OpenMenu = true;
            }
            GUILayout.Label($"People Riding: {GameManager.Players.Count}",MiniPanelStyle);
            GUILayout.Label($"On this map: {GameManager.instance.RidersOnMyMap()}",MiniPanelStyle);
            GUILayout.Label($"Status: {connectionstatelabels[(int)connectionstate]}",MiniPanelStyle);
            GUILayout.Label($"Ping: {Ping / 1000} secs",MiniPanelStyle);
            GUILayout.Label($"Average delay: {GameManager.instance.GetAveragePing() / 1000} secs", MiniPanelStyle);
            GUILayout.Label($"Physics Profile: {RiderPhysics.instance.SelectedProfile}", MiniPanelStyle);
            if (ToggledOffPlayersByMe.Count > 0)
            {
            GUILayout.Label($"{ToggledOffPlayersByMe.Count} Invisible Players",MinipanelstyeImportant);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            // pop up message box
            if (Messages.Count > 0)
            {
            GUILayout.BeginArea(new Rect(new Vector2(50, Screen.height / 45), new Vector2(Screen.width/6,Screen.height/15)));
            minguiscroll = GUILayout.BeginScrollView(minguiscroll);
            foreach (TextMessage mess in Messages)
            {
                // from a player
                if(mess.FromCode == 3)
                {
                    try
                    {
                        if(GameManager.Players.TryGetValue(mess.FromConnection,out RemotePlayer player))
                        {
                        GUILayout.Label(mess.Message, player.style);
                        }

                    }
                    catch (Exception x)
                    {
                       
                      UnityEngine.Debug.Log($"Assign text style error, player left? : {x}");
                    }

                }
                else
                {
                    // system,server or you

                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = MessageColour[mess.FromCode];
                    style.alignment = TextAnchor.MiddleCenter;
                    style.padding = new RectOffset(2, 2, 2, 2);
                    style.normal.background = whiteTex;
                    GUILayout.Label(mess.Message, style);
                }
                

            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            }

        }

        public void ShowRiderInfo()
        {
            GUI.skin = skin;
            if(IdofRidertoshow != 0)
            {
                try
                {
                
                    if(GameManager.Players.TryGetValue(IdofRidertoshow,out RemotePlayer player))
                    {
                        GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 2 - 100, Screen.height / 10), new Vector2(400, 400)),BoxStyle);
                        GUILayout.Label($"{player.username}", Generalstyle);
                        GUILayout.Label($"Riding at: {player.CurrentMap}" ,Generalstyle);
                        GUILayout.Label($"As: {player.CurrentModelName}",Generalstyle);
                        GUILayout.Label($"Net FPS: {Mathf.RoundToInt(player.PlayersFrameRate)}",Generalstyle);
                        GUILayout.Label($"Rider2Rider Ping: {player.R2RPing} Ms",Generalstyle);
                        GUILayout.Label($"Position stack: {player.IncomingTransformUpdates.Count}");
                        GUILayout.Space(10);
                        player.PlayerIsVisible = GUILayout.Toggle(player.PlayerIsVisible, "Toggle Player Visibilty",PlayeroptionsStyle);
                            if (player.PlayerIsVisible)
                            {
                                if (!player.RiderModel.activeInHierarchy)
                                {
                                player.ChangePlayerVisibilty(true);
                                }
                                if (!player.BMX.activeInHierarchy)
                                {
                                player.ChangePlayerVisibilty(true);
                                }

                            }
                            if (!player.PlayerIsVisible)
                            {
                                if (player.RiderModel.activeInHierarchy)
                                {
                                player.ChangePlayerVisibilty(false);
                                }
                                if (player.BMX.activeInHierarchy)
                                {
                                player.ChangePlayerVisibilty(false);
                                }

                            }
                        GUILayout.Space(10);
                        
                        if (!GameManager.instance.RiderOnMyMap(player))
                        {
                            CurrentOverrideStyle = player.Override ? ButtonOnstyle : PlayeroptionsStyle;
                            if(GUILayout.Button($"Map sync Override {player.Override}",CurrentOverrideStyle))
                            {
                                player.Override = !player.Override;
                                ClientSend.OverrideAMapMatch(player.id, player.Override);
                            }
                        GUILayout.Space(10);
                        }
                        
                        player.PlayerCollides = GUILayout.Toggle(player.PlayerCollides, "Player Collides", PlayeroptionsStyle);
                        player.ChangeCollideStatus(player.PlayerCollides);


                        GUILayout.Space(10);
                        player.PlayerTagVisible = GUILayout.Toggle(player.PlayerTagVisible, "Toggle Name Tag", PlayeroptionsStyle);
                        if (player.PlayerTagVisible)
                        {
                                if (!player.tm.gameObject.activeInHierarchy)
                                {
                                player.ChangePlayerTagVisible(true);

                                }
                        }
                        if (!player.PlayerTagVisible)
                        {
                                if (player.tm.gameObject.activeInHierarchy)
                                {
                                player.ChangePlayerTagVisible(false);
                                }
                        }
                        GUILayout.Space(10);

                        // invite to spawn
                        if (GUILayout.Button("Invite to spawn",Generalstyle))
                        {
                            if(!GameManager.instance.RiderOnMyMap(player))
                            {
                                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Player not on your map", (int)MessageColourByNum.Server, 1));
                            }
                            else
                            {
                                SessionMarker marker = FindObjectOfType<SessionMarker>();
                                ClientSend.InviteToSpawn(player.id, marker.marker.position, marker.marker.eulerAngles);
                                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Sent spawn to {player.username}", (int)MessageColourByNum.System, 1));
                            }
                        }
                        GUILayout.Space(10);
                        if (GUILayout.Button("Jump to Map",Generalstyle))
                        {
                            GameManager.instance.JumpToPlayerMap(player.CurrentMap);
                        }

                        // accept spawn offer if one has been offered
                        if (player.InviteToSpawnLive)
                        {
                            GUILayout.Space(10);
                            if (GUILayout.Button("Spawn on rider"))
                            {
                              SessionMarker marker = FindObjectOfType<SessionMarker>();
                                marker.marker.position = player.spawnpos;
                                marker.marker.eulerAngles = player.spawnrot;

                                marker.ResetPlayerAtMarker();
                            }
                            GUILayout.Space(10);
                        }

                        if (player.Objects.Count > 0)
                        {
                            GUILayout.Space(10);
                            player.PlayerObjectsVisible = GUILayout.Toggle(player.PlayerObjectsVisible, "Toggle player Objects");
                            if (player.PlayerObjectsVisible)
                            {

                                player.ChangeObjectsVisible(true);

                                
                            }
                            if (!player.PlayerObjectsVisible)
                            {

                                player.ChangeObjectsVisible(false);

                                
                            }


                            GUILayout.Space(10);
                            GUILayout.Label($"Objects:", Generalstyle);
                            GUILayout.Space(10);
                            Riderinfoscroll = GUILayout.BeginScrollView(Riderinfoscroll);

                           
                            foreach (NetGameObject n in player.Objects)
                            {
                                if(n._Gameobject!= null)
                                {
                                if (GUILayout.Button($"Vote off {n.NameofObject}"))
                                {
                                    ClientSend.VoteToRemoveObject(n.ObjectID, IdofRidertoshow);
                                }

                                }
                            }

                            
                            GUILayout.EndScrollView();
                            GUILayout.Space(10);
                        }

                        GUILayout.Space(10);
                        if (GUILayout.Button("Close"))
                        {
                            IdofRidertoshow = 0;
                            RiderInfoMenuOpen = false;
                        }
                        GUILayout.EndArea();
                    }

                

                }
                catch (Exception x)
                {
                    UnityEngine.Debug.Log("Showriderinfo Error : " + x);
                }
              

            }

        }

        public void LiveRiders()
        {
            try
            {
                GUI.skin = skin;
                Rect _box;

                if (LiveRiderToggle)
                {
                    Dictionary<string, List<RemotePlayer>> PlayersAtMap = new Dictionary<string, List<RemotePlayer>>();
                    foreach (RemotePlayer player in GameManager.Players.Values)
                    {
                        bool foundmap = false;
                        foreach (string map in PlayersAtMap.Keys)
                        {
                            if (map.ToLower() == player.CurrentMap.ToLower())
                            {
                                foundmap = true;
                                bool imin = false;
                                foreach (RemotePlayer inlist in PlayersAtMap[map].ToArray())
                                {
                                    if (inlist.id == player.id)
                                    {
                                        imin = true;
                                    }


                                }
                                    if (!imin)
                                    {
                                        PlayersAtMap[map].Add(player);
                                    }

                            }
                        }
                        if (!foundmap)
                        {
                            List<RemotePlayer> list = new List<RemotePlayer>();
                            list.Add(player);
                            PlayersAtMap.Add(player.CurrentMap, list);
                        }

                    }
                    float averageping = GameManager.instance.GetAveragePing();
                    string mostpop = "none";
                    int mostpopridercount = 0;
                    foreach(string map in PlayersAtMap.Keys)
                    {
                        if (PlayersAtMap[map].Count > mostpopridercount)
                        {
                            mostpop = map;
                            mostpopridercount = PlayersAtMap[map].Count;
                            if (mostpop.ToLower().Contains("pipe_modhunt"))
                            {
                                mostpop = "Ride The Pipe Official level";
                            }
                            if (mostpop.ToLower().Contains("chuck"))
                            {
                                mostpop = "Community centre Official level";
                            }

                        }
                    }

                    GUIStyle title = new GUIStyle();
                    title.alignment = TextAnchor.MiddleLeft;
                    title.fontSize = 12;
                    title.fontStyle = FontStyle.Bold;
                    title.normal.textColor = Color.white;
                    title.margin = new RectOffset(0, 0, 0, 0);
                    title.padding = new RectOffset(0, 0, 0, 0);
                    title.wordWrap = true;
                    title.stretchWidth = true;
                   
                    GUIStyle Contentleft = new GUIStyle();
                    Contentleft.alignment = TextAnchor.MiddleLeft;
                    Contentleft.fontSize = 12;
                    Contentleft.fontStyle = FontStyle.Bold;
                    Contentleft.normal.textColor = Color.green;
                    Contentleft.hover.background = GreenTex;
                    Contentleft.hover.textColor = Color.white;
                    Contentleft.margin = new RectOffset(0, 0, 0, 0);
                    Contentleft.padding = new RectOffset(0, 0, 0, 0);
                    Contentleft.wordWrap = true;
                    Contentleft.stretchWidth = true;

                    GUIStyle Contentright = new GUIStyle();
                    Contentright.alignment = TextAnchor.MiddleRight;
                    Contentright.fontSize = 12;
                    Contentright.fontStyle = FontStyle.Bold;
                    Contentright.normal.textColor = Color.green;
                    Contentright.margin = new RectOffset(0, 0, 0, 0);
                    Contentright.padding = new RectOffset(0, 0, 0, 0);
                    Contentright.wordWrap = true;
                    Contentright.stretchWidth = true;


                    GUIStyle box = new GUIStyle();
                    box.alignment = TextAnchor.MiddleLeft;
                    box.normal.background = BlackTex;
                    box.padding = new RectOffset(5, 5, 5, 5);
                    box.wordWrap = true;

                    GUIStyle toggle = new GUIStyle();
                    toggle.onNormal.background = GreenTex;
                    toggle.onHover.background = RedTex;
                    toggle.onNormal.textColor = Color.black;
                    toggle.onHover.textColor = Color.white;


                    _box = new Rect(new Vector2(10, 100), new Vector2(Screen.width / 3.5f, Screen.height / 4*3f));
                    Messageslabel = "Rider Overview";
                    GUILayout.BeginArea(_box,box);
                    LiveRiderToggle = GUILayout.Toggle(LiveRiderToggle,Messageslabel, toggle);
                    GUILayout.Space(20);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{GameManager.Players.Count} ",Contentright, GUILayout.MaxWidth(30));
                    GUILayout.Label($" Online Riders", title, GUILayout.MaxWidth(80));
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Most popular map:", title, GUILayout.MaxWidth(140));
                   if(GUILayout.Button($"{mostpop}", Contentleft, GUILayout.MinWidth(50), GUILayout.MaxWidth(200)))
                   {
                        GameManager.instance.JumpToPlayerMap(mostpop);
                   }
                    GUILayout.Label($":", title, GUILayout.MaxWidth(10));
                    GUILayout.Label($"{mostpopridercount}",Contentleft, GUILayout.MaxWidth(30));
                    GUILayout.Label($" Riders there", title, GUILayout.MaxWidth(100));
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Average Rider to Rider delay: ", title, GUILayout.MaxWidth(200));
                    GUILayout.Label($"{averageping /1000}",Contentleft, GUILayout.MinWidth(10), GUILayout.MaxWidth(80));
                    GUILayout.Label($" Seconds", title, GUILayout.MinWidth(20), GUILayout.MaxWidth(60));
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                    GUILayout.Label("Sessions:",title);
                    GUILayout.Space(2);
                    liveridersscroll = GUILayout.BeginScrollView(liveridersscroll,SyncWindowStyle1);
                    foreach (string mapname in PlayersAtMap.Keys)
                    {
                        string maplabel = mapname;
                        if (maplabel.ToLower().Contains("pipe_modhunt"))
                        {
                            maplabel = "Ride The Pipe Official level";
                        }
                        if (maplabel.ToLower().Contains("chuck"))
                        {
                            maplabel = "Community centre Official level";
                        }

                        GUILayout.Space(5);
                        GUILayout.Label($"Session at {maplabel}:",title);
                        GUILayout.Space(5);
                        string names = "";
                        foreach(RemotePlayer player in PlayersAtMap[mapname])
                        {
                            if(names != "")
                            {
                            names = names + ", " + player.username;
                                if (names.Length > 400)
                                {
                                    names = names + "\n";
                                }
                            }
                            else
                            {
                                names = player.username;
                            }
                        }
                        names = names + " in session ";

                        GUILayout.Label(names,Contentleft);
                        GUILayout.Space(5);
                    }
                    GUILayout.EndScrollView();
                    GUILayout.EndArea();


                }
                else
                {
                    _box = new Rect(new Vector2(10, 100), new Vector2(Screen.width / 7f, Screen.height / 2));
                    
                    Messageslabel = "Riders";
                GUILayout.BeginArea(_box,BoxStyle);
                LiveRiderToggle = GUILayout.Toggle(LiveRiderToggle, Messageslabel);
                GUILayout.Space(10);
                liveridersscroll = GUILayout.BeginScrollView(liveridersscroll);
                    if (GameManager.Players.Count > 0)
                    {
                     foreach (RemotePlayer r in GameManager.Players.Values)
                     {
                     try
                     {
                        if (GUILayout.Button($"{r.username}",r.style))
                        {
                           if(IdofRidertoshow != r.id)
                           {
                             IdofRidertoshow = r.id;
                             RiderInfoMenuOpen = true;
                           }
                           else
                           {
                            IdofRidertoshow = 0;
                            RiderInfoMenuOpen = false;
                           }
                        }
                     }
                     catch (Exception x)
                     {
                       UnityEngine.Debug.Log("Live Rider issue : " + x);
                     }

                     }
                    }
                    else
                    {
                        GUILayout.Label("No Riders Online");
                    }
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                }


            }
            catch (Exception x)
            {
                UnityEngine.Debug.Log("Live Riders Show Error : " + x);
            }
            GUILayout.Space(50);
        }

        public void MessagesShow()
        {
            try
            {

                GUIStyle Toggle = new GUIStyle();
                Toggle.alignment = TextAnchor.MiddleCenter;
                Rect _box;

                GUIStyle scroll = new GUIStyle();
                scroll.padding = new RectOffset(5, 5, 5, 5);
                


                if (MessagesToggle)
                {
                  _box = new Rect(new Vector2(Screen.width/3, Screen.height/3-40), new Vector2(Screen.width / 3, Screen.height / 3 * 2));
                Toggle.normal.background = RedTex;
                Toggle.hover.background = whiteTex;
                Toggle.normal.textColor = Color.white;
                Toggle.hover.textColor = Color.black;
               
                    Toggle.fixedWidth = 100;
                    Toggle.fixedHeight = 35;
                    scroll.alignment = TextAnchor.MiddleLeft;
                    Messageslabel = "Minimise";
                }
                else
                {
                  _box = new Rect(new Vector2(Screen.width - (Screen.width/7) - 10,100), new Vector2(Screen.width / 7f, Screen.height / 2));
                    Toggle.normal.background = GreenTex;
                    Toggle.hover.background = whiteTex;
                    Toggle.normal.textColor = Color.white;
                    Toggle.hover.textColor = Color.black;
                    scroll.alignment = TextAnchor.MiddleCenter;
                    Messageslabel = "Messages";
                }


                GUIStyle send = new GUIStyle();
                send.normal.background = GreenTex;
                send.hover.background = whiteTex;
                send.normal.textColor = Color.white;
                send.hover.textColor = Color.black;
                send.fixedWidth = 100;
                send.fixedHeight = 35;
                send.alignment = TextAnchor.MiddleCenter;


                


                GUI.skin = skin;
            GUILayout.BeginArea(_box,BoxStyle);


                GUILayout.BeginHorizontal();
                if (GUILayout.Button(Messageslabel, Toggle))
                {
                    MessagesToggle = !MessagesToggle;
                    ChangeMessagesLength(MessagesToggle);
                }

                if (MessagesToggle)
                {
                    GUILayout.Space(5);
                    try
                    {
                    Messagetosend = GUILayout.TextArea(Messagetosend,200,GUILayout.MaxWidth(Screen.width/4));

                    }
                    catch (UnityException x)
                    {
                        UnityEngine.Debug.Log("Text area error : " + x);
                    }
                    GUILayout.Space(5);
                    if (GUILayout.Button("Send",send))
                    {
                        if (Messagetosend != null && Messagetosend != "")
                        {
                            ClientSend.SendTextMessage(Messagetosend.ToString());
                            Messagetosend = "";

                        }
                    }
                   
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                Messagesscroll = GUILayout.BeginScrollView(Messagesscroll,scroll);

            if (Messages.Count > 0)
            {
            foreach (TextMessage mess in Messages)
            {
                       
                if (mess.FromCode == 3)
                {
                 if(GameManager.Players.TryGetValue(mess.FromConnection,out RemotePlayer player))
                  {
                    player.style.alignment = scroll.alignment;
                    GUILayout.Label(mess.Message, player.style);

                  }
                }
                else
                {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = MessageColour[mess.FromCode];
                    style.alignment = scroll.alignment;
                    style.padding = new RectOffset(2, 2, 2, 2);
                    style.normal.background = TransTex;
                    GUILayout.Label(mess.Message, style);
                }

                       
                
            }

            }


            GUILayout.EndScrollView();
            GUILayout.EndArea();
           

            }
            catch(Exception e)
            {
                UnityEngine.Debug.Log(e);
            }
        }

        /// <summary>
        /// Replaces all new line markers to make text fit each message window
        /// </summary>
        /// <param name="MakeLonger"></param>
        void ChangeMessagesLength(bool MakeLonger)
        {
            int length = MakeLonger ? 80 : 35;


            foreach(TextMessage mess in Messages)
            {
                mess.Message = mess.Message.Replace("\n", "");

                if (mess.Message.Length > length)
                {
                    char[] chars = mess.Message.ToCharArray();
                    int counter = 0;
                    for (int i = 0; i < chars.Length; i++)
                    {
                        if (counter == length)
                        {
                            mess.Message = mess.Message.Insert(i, "\n");
                            counter = 0;
                        }
                        counter++;
                    }
                }


            }
        }

        public void ShowSyncWindow()
        {
            GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 4, 75), new Vector2(Screen.width/2, Screen.height - 150)),SyncWindowStyle1);
            GUILayout.BeginHorizontal();
            
               // GUILayout.Label("Download Packets: " + $"{InPacketsReceived} of {InPacketsTotal}",SyncWindowStyle1);
           // GUILayout.Label($"Upload Packets: {OutPacketsSent} of {OutPacketsTotal} ", SyncWindowStyle1);

            if (GUILayout.Button("Exit", SyncWindowStyle2))
            {
                SyncWindowOpen = false;
            }

            GUILayout.EndHorizontal();


            // Server Uploads
            GUILayout.Label("Server Requests:", SyncWindowStyle1);
            GUILayout.Space(5);
            SyncScroll1 = GUILayout.BeginScrollView(SyncScroll1, SyncWindowStyle3);
            GUILayout.Space(10);
                
                  
             foreach (SendReceiveIndex OutIndex in FileSyncing.OutGoingIndexes)
             {
                        
                    if (OutIndex.IsSending)
                    {
                      OutPacketsTotal = OutPacketsTotal + OutIndex.TotalPacketsinFile - OutIndex.PacketNumbersStored.Count;
                      OutPacketsSent = OutPacketsSent + OutIndex.PacketNumbersStored.Count;

                    if (GUILayout.Button($"Cancel {OutIndex.TotalPacketsinFile - OutIndex.PacketNumbersStored.Count} packets of {OutIndex.TotalPacketsinFile} for {OutIndex.NameOfFile}"))
                        {
                            ClientSend.FileStatus(OutIndex.NameOfFile, (int)FileStatus.Cancel);
                            OutIndex.IsSending = false;
                        }

                    }
                    else
                    {

                        if (GUILayout.Button($"{OutIndex.NameOfFile}", SyncWindowStyle2))
                        {
                          FileSyncing.SendFileToServer(OutIndex);
                        }

                    }
                    
             }


                    GUILayout.Space(5);
            GUILayout.EndScrollView();
            GUILayout.Space(15);




            // Downloads
                GUILayout.Label("Downloads:", SyncWindowStyle1);
            SyncScroll2 = GUILayout.BeginScrollView(SyncScroll2, SyncWindowStyle3);
            GUILayout.Space(10);
          
                foreach(SendReceiveIndex InIndex in FileSyncing.IncomingIndexes.ToArray())
                {
                        

                    InPacketsTotal = InPacketsTotal + InIndex.TotalPacketsinFile;
                    InPacketsReceived = InPacketsReceived + InIndex.PacketNumbersStored.Count;

                    if (InIndex.IsReceiving)
                    {
                        if (GUILayout.Button($"Cancel {InIndex.NameOfFile}: {InIndex.TotalPacketsinFile - InIndex.PacketNumbersStored.Count} Packets"))
                        {
                            ClientSend.FileStatus(InIndex.NameOfFile, (int)FileStatus.Cancel);
                            InIndex.IsReceiving = false;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button($" {InIndex.NameOfFile}: {InIndex.PacketNumbersStored.Count} Packets Stored"))
                        {
                        FileSyncing.RequestFileFromServer(InIndex.NameOfFile,InIndex.Directory);
                        }
                    }

                   
                    GUILayout.Space(5);
                }

                

            

            GUILayout.EndScrollView();
            GUILayout.Space(15);
            GUILayout.EndArea();

        }

        public void ShowPlayerOptions()
        {
            GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 8*3, Screen.height / 4), new Vector2(Screen.width / 4, Screen.height/5)), BoxStyle);
                if(GUILayout.Button("Player Options", SyncWindowStyle2))
                {
                PlayeroptionsOpen = false;
                }
            GUILayout.Space(15);

            // collisions
            CollisionsToggle = GUILayout.Toggle(CollisionsToggle, Collisionslabel,PlayeroptionsStyle);
            if (CollisionsToggle)
            {
                Collisionslabel = "Turn Off Collisions";

                foreach(RemotePlayer r in GameManager.Players.Values)
                {
                    if (!r.PlayerCollides)
                    {
                        r.ChangeCollideStatus(true);
                    }
                }

            }
            if (!CollisionsToggle)
            {
                Collisionslabel = "Turn On Collisions";

                foreach (RemotePlayer r in GameManager.Players.Values)
                {
                    if (r.PlayerCollides)
                    {
                      r.ChangeCollideStatus(false);
                    }
                }

            }

            GUILayout.Space(10);
            // player tags
            PlayerTagsToggle = GUILayout.Toggle(PlayerTagsToggle, PlayerTagsLabel, PlayeroptionsStyle);
            if (PlayerTagsToggle)
            {
                PlayerTagsLabel = "Turn Player Tags Off";
                foreach (RemotePlayer r in GameManager.Players.Values)
                {
                    if (r.PlayerTagVisible == false)
                    {
                        r.ChangePlayerTagVisible(true);
                    }
                }


            }
            if (!PlayerTagsToggle)
            {
                PlayerTagsLabel = "Turn Player Tags On";
                foreach (RemotePlayer r in GameManager.Players.Values)
                {
                    if(r.PlayerTagVisible == true)
                    {
                      r.ChangePlayerTagVisible(false);

                    }
                }


            }


            GUILayout.Space(10);
            // player objects
            AllPlayerObjectsVisibleToggle = GUILayout.Toggle(AllPlayerObjectsVisibleToggle, PlayerObjectsLabel, PlayeroptionsStyle);
            if (AllPlayerObjectsVisibleToggle)
            {
                PlayerObjectsLabel = "Turn Off All Rider Objects";
                foreach (RemotePlayer r in GameManager.Players.Values)
                {
                    if (r.PlayerObjectsVisible == false)
                    {
                        r.ChangePlayerTagVisible(true);
                    }
                }
            }
            if (!AllPlayerObjectsVisibleToggle)
            {
                PlayerObjectsLabel = "Turn On All Rider Objects";
                foreach (RemotePlayer r in GameManager.Players.Values)
                {
                    if (r.PlayerObjectsVisible == true)
                    {
                        r.ChangePlayerTagVisible(false);
                    }
                }

            }


            GUILayout.Space(10);
            // Bottom panel
            BottompanelToggle = GUILayout.Toggle(BottompanelToggle, BottompanelLabel, PlayeroptionsStyle);
            if (BottompanelToggle)
            {
                BottompanelOpen = true;
                BottompanelLabel = "Turn off Bottom Panel";
            }
            else 
            {
                BottompanelOpen = false;
                BottompanelLabel = "Turn on Bottom Panel";
            }

            GUILayout.EndArea();

        }

        public void ShowBottomPanel()
        {
            
            GUILayout.BeginArea(new Rect(new Vector2(50,Screen.height - (Screen.height/55)), new Vector2(Screen.width - 100, Screen.height/55)),MiniPanelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Connection State: {connectionstatelabels[(int)connectionstate]} ", MiniPanelStyle);
            GUILayout.Label($"Garage preset: {currentGaragePreset} ", MiniPanelStyle);
            GUILayout.Label($"Physics Profile: {RiderPhysics.instance.SelectedProfile}", MiniPanelStyle);
            GUILayout.Label($"Ping: {Ping/1000} secs",MiniPanelStyle);
            GUILayout.Label($"Average delay: {GameManager.instance.GetAveragePing() /1000} secs", MiniPanelStyle);
            GUILayout.Label($"KB out /s: {Outbytespersec / 1000} ", MiniPanelStyle);
            GUILayout.Label($"KB in /s: {InBytespersec / 1000} ", MiniPanelStyle);
            GUILayout.Label($"Pending Reliable: {Pendingreliable} ", MiniPanelStyle,GUILayout.MaxWidth(Screen.width / 9));
            GUILayout.Label($"Pending Unreliable: {Pendingunreliable} ", MiniPanelStyle, GUILayout.MaxWidth(Screen.width / 9));
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        void SetupGuis()
        {
            // box style
            BoxStyle.padding = new RectOffset(10, 10, 2, 2);
            BoxStyle.normal.background = BlackTex;
            BoxStyle.alignment = TextAnchor.UpperCenter;


            //Bottompanelstyle.fixedWidth = Screen.width / 9;
            MiniPanelStyle.alignment = TextAnchor.MiddleLeft;
            MiniPanelStyle.fontStyle = FontStyle.Bold;
            MiniPanelStyle.wordWrap = true;
            MiniPanelStyle.normal.background = BlackTex;
            MiniPanelStyle.normal.textColor = Color.white;
            MiniPanelStyle.padding = new RectOffset(5, 0, 2, 2);
            MiniPanelStyle.fontSize = 11;
            MiniPanelStyle.margin = new RectOffset(0, 0, 0, 0);

            MinipanelstyeImportant.alignment = TextAnchor.MiddleCenter;
            MinipanelstyeImportant.fontStyle = FontStyle.Bold;
            MinipanelstyeImportant.wordWrap = true;
            MinipanelstyeImportant.hover.background = GreenTex;
            MinipanelstyeImportant.normal.background = BlackTex;
            MinipanelstyeImportant.normal.textColor = Color.red;
            MinipanelstyeImportant.padding = new RectOffset(0, 0, 0, 5);
            MinipanelstyeImportant.fontSize = 11;
            MinipanelstyeImportant.margin = new RectOffset(0, 0, 0, 0);



            // sync window
            SyncWindowStyle1.normal.background = GreyTex;
            SyncWindowStyle1.normal.textColor = Color.white;
            SyncWindowStyle1.fontStyle = FontStyle.Bold;
            SyncWindowStyle1.padding = new RectOffset(10, 10, 0, 0);
            SyncWindowStyle1.alignment = TextAnchor.MiddleCenter;


            SyncWindowStyle2.normal.background = GreyTex;
            SyncWindowStyle2.hover.background = GreenTex;
            SyncWindowStyle2.onHover.background = RedTex;
            SyncWindowStyle2.normal.textColor = Color.white;
            SyncWindowStyle2.padding = new RectOffset(10, 10, 2, 2);
            SyncWindowStyle2.alignment = TextAnchor.MiddleCenter;


            SyncWindowStyle3.normal.background = whiteTex;
            SyncWindowStyle3.normal.textColor = Color.black;
            SyncWindowStyle3.padding = new RectOffset(10, 10, 0, 0);

            // player options
            PlayeroptionsStyle.normal.background = RedTex;
            PlayeroptionsStyle.hover.background = GreenTex;
            PlayeroptionsStyle.onHover.background = RedTex;
            PlayeroptionsStyle.onNormal.background = GreenTex;
            PlayeroptionsStyle.alignment = TextAnchor.MiddleCenter;
            PlayeroptionsStyle.fontStyle = FontStyle.Bold;


            ButtonOnstyle.normal.background = GreenTex;
            ButtonOnstyle.hover.background = GreenTex;
            ButtonOnstyle.onHover.background = RedTex;
            ButtonOnstyle.onNormal.background = GreenTex;
            ButtonOnstyle.alignment = TextAnchor.MiddleCenter;
            ButtonOnstyle.fontStyle = FontStyle.Bold;



            // general
            Generalstyle.normal.background = BlackTex;
            Generalstyle.normal.textColor = Color.white;

            Generalstyle.alignment = TextAnchor.MiddleCenter;
            Generalstyle.fontStyle = FontStyle.Bold;
            Generalstyle.hover.background = GreenTex;
            Generalstyle.hover.textColor = Color.black;
            Generalstyle.onHover.background = RedTex;

            // skin
            skin.label.normal.textColor = Color.white;
            skin.label.fontSize = 15;
            skin.label.fontStyle = FontStyle.Bold;
            skin.label.alignment = TextAnchor.MiddleCenter;
            skin.label.normal.background = BlackTex;



            skin.textField.alignment = TextAnchor.MiddleCenter;
            skin.textField.normal.textColor = Color.red;
            skin.textField.hover.textColor = Color.black;
            skin.textField.focused.textColor = Color.green;
            skin.textField.normal.background = whiteTex;
            skin.textField.active.background = whiteTex;
            skin.textField.onNormal.background = whiteTex;
            skin.textField.focused.background = whiteTex;
            skin.textField.onFocused.background = whiteTex;
            skin.textField.onActive.background = whiteTex;
            skin.textField.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            skin.textField.padding = new RectOffset(2, 2, 2, 2);


            skin.textArea.alignment = TextAnchor.MiddleCenter;
            skin.textArea.normal.textColor = Color.red;
            skin.textArea.hover.textColor = Color.black;
            skin.textArea.normal.background = whiteTex;
            skin.textArea.focused.background = whiteTex;
            skin.textArea.focused.textColor = Color.green;
            skin.textArea.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            skin.textArea.padding = new RectOffset(10, 10, 10, 10);
            skin.textArea.clipping = TextClipping.Clip;
            skin.textArea.wordWrap = true;


            skin.button.normal.textColor = Color.white;
            skin.button.alignment = TextAnchor.MiddleCenter;
            skin.button.normal.background = GreenTex;
            skin.button.onNormal.background = GreyTex;
            skin.button.onNormal.textColor = Color.red;
            skin.button.onHover.background = GreenTex;
            skin.button.hover.textColor = Color.black;
            skin.button.normal.background.wrapMode = TextureWrapMode.Clamp;
            skin.button.hover.background = whiteTex;
            skin.button.padding = new RectOffset(5,5,0,0);



            skin.toggle.normal.textColor = Color.white;
            skin.toggle.alignment = TextAnchor.MiddleCenter;
            skin.toggle.normal.background = GreenTex;
            skin.toggle.onNormal.background = GreyTex;
            skin.toggle.onNormal.textColor = Color.black;
            skin.toggle.onHover.background = GreenTex;
            skin.toggle.hover.textColor = Color.black;
            skin.toggle.hover.background = whiteTex;
            skin.toggle.padding = new RectOffset(0, 0, 0, 0);

            skin.horizontalSlider.alignment = TextAnchor.MiddleCenter;
            skin.horizontalSlider.normal.textColor = Color.black;
            skin.horizontalSlider.normal.background = GreyTex;
            skin.horizontalSliderThumb.normal.background = GreenTex;
            skin.horizontalSliderThumb.normal.background.wrapMode = TextureWrapMode.Clamp;
            skin.horizontalSliderThumb.normal.textColor = Color.white;
            skin.horizontalSliderThumb.fixedWidth = 20;
            skin.horizontalSliderThumb.fixedHeight = 20;
            skin.horizontalSliderThumb.hover.background = BlackTex;

           
            skin.verticalScrollbarThumb.normal.background = GreenTex;
            skin.verticalScrollbarThumb.alignment = TextAnchor.MiddleRight;
            skin.verticalScrollbar.alignment = TextAnchor.MiddleRight;
            skin.verticalScrollbar.normal.background = whiteTex;
            skin.verticalScrollbar.hover.background = GreenTex;

            
            skin.scrollView.padding = new RectOffset(5, 5, 5, 5);
            skin.scrollView.alignment = TextAnchor.UpperCenter;

            skin.verticalScrollbarThumb.normal.background =GreenTex;
            skin.verticalScrollbarThumb.hover.background = GreyTex;
            skin.verticalScrollbarThumb.fixedWidth = 14;
            skin.verticalScrollbarThumb.normal.background.wrapMode = TextureWrapMode.Clamp;
            skin.verticalScrollbar.alignment = TextAnchor.MiddleRight;
            skin.verticalScrollbarThumb.alignment = TextAnchor.MiddleRight;

          
            skin.verticalSliderThumb.normal.background = GreenTex;
            skin.verticalSliderThumb.hover.background = BlackTex;
            skin.verticalSliderThumb.fixedWidth = 14;
            skin.verticalSlider.alignment = TextAnchor.MiddleRight;
            skin.verticalSliderThumb.alignment = TextAnchor.MiddleRight;
           


        }

        public void ReceivedSpawnInvite(uint from, Vector3 pos, Vector3 rot)
        {
            if(GameManager.Players.TryGetValue(from,out RemotePlayer player))
            {
              NewMessage(Constants.ServerMessageTime, new TextMessage($"{player.username} offered his spawn", (int)MessageColourByNum.Player, from));
                player.spawnpos = pos;
                player.spawnrot = rot;

                // if an offer is live, shut it down
                if (player.InviteToSpawnLive)
                {
                    StopCoroutine(player.InvitedToSpawn());
                    player.InviteToSpawnLive = false;
                }

                // set new offer
                StartCoroutine(player.InvitedToSpawn());



            }


        }
        
        public void UpdateAvailable(float version,List<string> filesinUpdate)
        {
            FrostyPGamemanager.instance.OpenMenu = false;
            Versionofupdate = version;
            UpdateFiles = filesinUpdate;
            UpdateOpen = true;
        }

        void ShowUpdateWindow()
        {

            // track state of download
            bool downloading = false;
            int totalpackets = 0;
            int currentpackets = 0;
            int gotfiles = 0;
            foreach(SendReceiveIndex f in FileSyncing.IncomingIndexes)
            {
                
                    downloading = true;
                    totalpackets = totalpackets + f.TotalPacketsinFile;
                    currentpackets = currentpackets + f.PacketNumbersStored.Count;

                
            }

            if (Directory.Exists(GameManager.UpdateDir))
            {
                if(Directory.Exists(GameManager.UpdateDir + Versionofupdate + "/"))
                {
                    gotfiles = new DirectoryInfo(GameManager.UpdateDir + Versionofupdate + "/").GetFiles().Length;

                if (new DirectoryInfo(GameManager.UpdateDir + Versionofupdate + "/").GetFiles().Length == UpdateFiles.Count)
                {
                    UpdateDownloaded = true;
                }

                }
            }
           


            GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 3, Screen.height / 3), new Vector2(Screen.width/3, Screen.height/3)),SyncWindowStyle1);

            GUILayout.Space(50);
            GUILayout.Label($"Your Version of Frosty Manager: {GameNetworking.instance.VERSIONNUMBER}  doesnt match the Servers: {Versionofupdate}");
            GUILayout.BeginHorizontal();

            // if nothing is pressed
            if (!downloading && !UpdateDownloaded)
            {
            if(GUILayout.Button($"Grab {Versionofupdate} files?"))
            {
                   foreach(string file in UpdateFiles)
                   {
                    FileSyncing.RequestFileFromServer(file,"PIPE_Data/FrostyPGameManager/Updates/" + Versionofupdate + "/");
                   }
                   
            }

            }

            if (GUILayout.Button("Disconnect"))
            {
                InGameUI.instance.Disconnect();
                Connected = false;
                OnlineMenu = false;
                OfflineMenu = true;
                UpdateOpen = false;
                FrostyPGamemanager.instance.OpenMenu = true;
                
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            // if in progress
            if (downloading && !UpdateDownloaded)
            {
                GUILayout.Label($"Files: {gotfiles} of {UpdateFiles.Count}");
              GUILayout.Label($"{currentpackets} of {totalpackets} received");
            }

            // if finished
            if (UpdateDownloaded)
            {
                GUILayout.Label($"{Versionofupdate} Update Downloaded!");
              GUILayout.Label($"{Versionofupdate} files for Mods/FrostyPGameManager/ have been saved to PIPE_Data/FrostyPGameManager/Updates/{Versionofupdate}/");
              GUILayout.Label("Once the Game shuts down you can move them into Mods/FrostyPGameManager/ to overwrite current version of mod");

            }





            GUILayout.EndArea();
        }

        void ShowPublicServerButton(PublicServer server)
        {
            GUILayout.Space(5);
            if(GUILayout.Button(server.Name))
            {
                ConnectToServer(server.IP, server.Port);
                OnlineMenu = true;
                OfflineMenu = false;
            }
           
        }

        void UpdateServerData()
        {
            if (publicisewatch.Elapsed.TotalSeconds > 10)
            {
                StartCoroutine(GameManager.UpdatePublicServers());
                publicisewatch.Reset();
                publicisewatch.Start();
            }
            else if (!publicisewatch.IsRunning)
            {
                publicisewatch.Reset();
                publicisewatch.Start();
            }
        }

        


    }

    /// <summary>
    /// Incoming messages get different colors
    /// </summary>
    public enum MessageColourByNum
    {
        System = 1,
        Me = 2,
        Player = 3,
        Server = 4,
    }



}