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
        public GUISkin skin = (GUISkin)ScriptableObject.CreateInstance("GUISkin");
        public GUIStyle Generalstyle = new GUIStyle();
        GUIStyle MiniLiveRidersStyle = new GUIStyle();
        GUIStyle SyncWindowStyle1 = new GUIStyle();
        GUIStyle SyncWindowStyle2 = new GUIStyle();
        GUIStyle SyncWindowStyle3 = new GUIStyle();
        GUIStyle MessagesBigStyle = new GUIStyle();
        GUIStyle MessagesSmallStyle = new GUIStyle();
        GUIStyle MessagesTextStyle = new GUIStyle();
        GUIStyle CurrentMessagestyle;
        GUIStyle PlayeroptionsStyle = new GUIStyle();


        public static GUIStyle BoxStyle = new GUIStyle();

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
        public int ServerPendingRel;
        Vector2 BootObjectScroll;
        Vector2 BootPlayerScroll;




        // MiniGUI
        bool MiniLiveRiderstoggle;


        // live riders
        Vector2 liveridersscroll;
        Rect LiveRiderBox;

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
        public bool PlayerObjectsToggle = true;
        string PlayerObjectsLabel = "Turn Objects Off";
        public bool PlayerTagsToggle = true;
        string PlayerTagsLabel = "Turn Objects Off";



        // Update window
        float Versionofupdate;
        bool UpdateOpen;
        public bool UpdateDownloaded;




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




            RedTex = new Texture2D(20,10);
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
            Color newcolor2 = new Color(0, 0, 0, 1f);
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
            Color newcolor5 = new Color(1f, 1f, 1f, 0.4f);
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
                bf.Serialize(File.OpenWrite(Playersavepath + "PlayerData.FrostyPreset"), PlayerSavedata);

            }
            else if (File.Exists(Playersavepath + "PlayerData.FrostyPreset"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                PlayerSavedata = bf.Deserialize(File.OpenRead(Playersavepath + "PlayerData.FrostyPreset")) as PlayerSaveData;

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

                
                CamModes[cyclemodes]();

            }

            


            if (Connected && OnlineMenu && Input.GetKeyDown(KeyCode.A))
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
                Debug.Log("ingameui error: " + x);
            }

            

        }
     


        public void OnlineShow()
        {

            if (OfflineMenu)
            {
                ClientsOfflineMenu();
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

                for (int i = 0; i < r.Objects.Count; i++)
                {
                    if(r.Objects[i]._Gameobject != null)
                    {
                        GameManager.instance.DestroyObj(r.Objects[i]._Gameobject);
                    }
                }



                objs.Add(r.gameObject);
                

            }
            if (objs.Count > 0)
            {
                foreach(GameObject obj in objs)
                {
                   GameManager.instance.DestroyObj(obj);
                }
            }
            GameManager.Players.Clear();
           

            
           
            
            
            FileSyncing.WaitingRequests.Clear();
            FileSyncing.IncomingIndexes.Clear();
            FileSyncing.OutGoingIndexes.Clear();

            // Server learns of disconnection itself and tells everyone

        }
        

       
       
    

       public void ClientsOfflineMenu()
        {


            GUILayout.BeginArea(new Rect(new Vector2(Screen.width/3,Screen.height/4), new Vector2(Screen.width/3,Screen.height/2)),BoxStyle);
            GUILayout.Space(10);
            // setup stuff before connecting
            GUILayout.BeginHorizontal();
            GUILayout.Label("Username");
            Username = GUILayout.TextField(Username);
            GUILayout.EndHorizontal();
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Label("IP:");
            desiredIP = GUILayout.TextField(desiredIP);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("PORT:");
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
            GUILayout.Space(5);


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
                LocalPlayer.RiderTrackingSetup();



                

                if (LocalPlayer.RiderModelname == "Daryien")
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
            GUILayout.EndHorizontal();
            GUILayout.Space(40);

            RecentSavedServersMenu = GUILayout.Toggle(RecentSavedServersMenu, "Saved Servers");
            if (RecentSavedServersMenu)
            {
                GUILayout.Space(15);
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
                        bf.Serialize(File.OpenWrite(Playersavepath + "PlayerData.FrostyPreset"), PlayerSavedata);
                        
                    }
                }
                Nickname = GUILayout.TextField(Nickname);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);

                GUILayout.Label("Saved server list:");
                GUILayout.Space(10);
                if (PlayerSavedata != null && PlayerSavedata.savedservers != null)
                {
                    foreach(SavedServer s in PlayerSavedata.savedservers.ToArray())
                    {
                        GUILayout.BeginHorizontal();
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
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);
                    }
                }



                GUILayout.Space(35);
            }
            GUILayout.Space(40);
            if (GUILayout.Button("Close"))
            {
                FrostyPGamemanager.instance.MenuShowing = 0;
            }
            GUILayout.EndArea();
            
           
           
            
        }

        public void ClientsOnlineMenu()
        {

            GUILayout.Space(20);
            GUILayout.BeginArea(new Rect(new Vector2(Screen.width / 4, 50), new Vector2(Screen.width / 2, Screen.height / 20)));
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
                
                GUILayout.Label($"player: {cycleplayerslist[cyclecounter].username}  CamMode: {cyclemodes}");
                if(GUILayout.Button("End Spectate"))
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
                    GUILayout.Label($"Server Pending Reliable: {ServerPendingRel}");

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
                        GUILayout.Label($"{r.username}", Generalstyle);
                        GUILayout.Label($"Riding at: {r.CurrentMap}" ,Generalstyle);
                        GUILayout.Label($"As: {r.CurrentModelName}",Generalstyle);
                        GUILayout.Label($"Net FPS: {Mathf.RoundToInt(r.PlayersFrameRate)}",Generalstyle);
                        GUILayout.Label($"Rider2Rider Ping: {r.R2RPing} Ms",Generalstyle);
                            GUILayout.Space(10);
                            r.PlayerIsVisible = GUILayout.Toggle(r.PlayerIsVisible, "Toggle Player Visibilty");
                            if (r.PlayerIsVisible)
                            {
                                if (!r.RiderModel.activeInHierarchy)
                                {
                                    r.ChangePlayerVisibilty(true);
                                }
                                if (!r.BMX.activeInHierarchy)
                                {
                                    r.ChangePlayerVisibilty(true);
                                }




                            }
                            if (!r.PlayerIsVisible)
                            {
                                if (r.RiderModel.activeInHierarchy)
                                {
                                    r.ChangePlayerVisibilty(false);
                                }
                                if (r.BMX.activeInHierarchy)
                                {
                                    r.ChangePlayerVisibilty(false);
                                }




                            }
                            GUILayout.Space(10);
                            r.PlayerCollides = GUILayout.Toggle(r.PlayerCollides, "Player Collides");
                            r.ChangeCollideStatus(r.PlayerCollides);


                            GUILayout.Space(10);
                            r.PlayerTagVisible = GUILayout.Toggle(r.PlayerTagVisible, "Toggle Name Tag");
                        if (r.PlayerTagVisible)
                        {
                                if (!r.tm.gameObject.activeInHierarchy)
                                {
                                    r.ChangePlayerTagVisible(true);

                                }
                        }
                        if (!r.PlayerTagVisible)
                        {
                                if (r.tm.gameObject.activeInHierarchy)
                                {
                                    r.ChangePlayerTagVisible(false);
                                }
                        }

                        if (r.Objects.Count > 0)
                        {
                            GUILayout.Space(10);
                        r.PlayerObjectsVisible = GUILayout.Toggle(r.PlayerObjectsVisible, "Toggle player Objects");
                            if (r.PlayerObjectsVisible)
                            {

                                    r.ChangeObjectsVisible(true);

                                
                            }
                            if (!r.PlayerObjectsVisible)
                            {

                                    r.ChangeObjectsVisible(false);

                                
                            }


                            GUILayout.Space(10);
                            GUILayout.Label($"Objects:", Generalstyle);
                            GUILayout.Space(10);
                            Riderinfoscroll = GUILayout.BeginScrollView(Riderinfoscroll);

                           
                            foreach (NetGameObject n in r.Objects)
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
            try
            {
                Rect _box;
                if (MessagesToggle)
                {
                  _box = new Rect(new Vector2(Screen.width/3/2, Screen.height/3-40), new Vector2(Screen.width / 3 * 2, Screen.height / 3 * 2));
                    CurrentMessagestyle = MessagesBigStyle;
                    Messageslabel = "Minimise Messages";
                }
                else
                {
                  _box = new Rect(new Vector2(Screen.width / 6 * 5, Screen.height / 2), new Vector2(Screen.width / 6.5f, Screen.height / 8));
                    CurrentMessagestyle = MessagesSmallStyle;
                    Messageslabel = "Messages";
                }



            GUI.skin = skin;
            GUILayout.BeginArea(_box);


                GUILayout.BeginHorizontal();
            MessagesToggle = GUILayout.Toggle(MessagesToggle,Messageslabel,CurrentMessagestyle);

                GUILayout.Space(10);
                if (MessagesToggle)
                {
                   // MessagesBigStyle.alignment = TextAnchor.MiddleCenter;
                    try
                    {
                    Messagetosend = GUILayout.TextField(Messagetosend);

                    }
                    catch (UnityException x)
                    {

                    }
                    GUILayout.Space(5);
                    if (GUILayout.Button("Send",CurrentMessagestyle))
                    {
                        if (Messagetosend != null)
                        {
                            ClientSend.SendTextMessage(Messagetosend.ToString());
                            Messagetosend = "";

                        }
                    }
                    GUILayout.Space(20);
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(20);
                Messagesscroll = GUILayout.BeginScrollView(Messagesscroll);

            if (Messages.Count > 0)
            {
            foreach (TextMessage mess in Messages)
            {
                       
                if (mess.FromCode == 3)
                {
                 
                 GUILayout.Label(mess.Message, GameManager.Players[mess.FromConnection].style);
                }
                    else
                    {
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = MessageColour[mess.FromCode];
                    style.alignment = TextAnchor.MiddleCenter;
                    style.padding = new RectOffset(10, 10, 2, 2);
                    style.normal.background = GreyTex;
                    GUILayout.Label(mess.Message, style);
                    }

                       
                
            }


            }


            GUILayout.EndScrollView();
            GUILayout.EndArea();
           

            }
            catch(Exception e)
            {
                Debug.Log(e);
            }
        }


        /// <summary>
        /// Call IngameUI.instance.NewMessage(constants.?, new textmessage("message", messagecolour.?, 1(incoming messages from server are coded and handled in clienthandle)  ))     to display message
        /// </summary>
        /// <param name="_secs"></param>
        /// <param name="message"></param>
        public void NewMessage(int _secs, TextMessage message)
        {
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
          
                foreach(SendReceiveIndex InIndex in FileSyncing.IncomingIndexes)
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
                            ClientSend.RequestFile(InIndex.NameOfFile, InIndex.PacketNumbersStored);
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
            PlayerObjectsToggle = GUILayout.Toggle(PlayerObjectsToggle, PlayerObjectsLabel, PlayeroptionsStyle);
            if (PlayerObjectsToggle)
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
            if (!PlayerObjectsToggle)
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


           
            GUILayout.EndArea();

        }




        void SetupGuis()
        {
            // box style
            BoxStyle.padding = new RectOffset(10, 10, 5, 5);
            BoxStyle.normal.background = whiteTex;




            // sync window
            SyncWindowStyle1.normal.background = GreyTex;
            SyncWindowStyle1.normal.textColor = Color.black;
            SyncWindowStyle1.fontStyle = FontStyle.Bold;
            SyncWindowStyle1.padding = new RectOffset(10, 10, 0, 0);
            SyncWindowStyle1.alignment = TextAnchor.MiddleCenter;


            SyncWindowStyle2.normal.background = GreyTex;
            SyncWindowStyle2.hover.background = GreenTex;
            SyncWindowStyle2.onHover.background = RedTex;
            SyncWindowStyle2.normal.textColor = Color.black;
            SyncWindowStyle2.padding = new RectOffset(10, 10, 2, 2);
            SyncWindowStyle2.alignment = TextAnchor.MiddleCenter;


            SyncWindowStyle3.normal.background = whiteTex;
            SyncWindowStyle3.normal.textColor = Color.black;
            SyncWindowStyle3.padding = new RectOffset(10, 10, 0, 0);





            // messages
            MessagesBigStyle.normal.background = whiteTex;
            MessagesBigStyle.fontStyle = FontStyle.Bold;
            MessagesBigStyle.alignment = TextAnchor.MiddleCenter;
            MessagesBigStyle.onHover.background = RedTex;
            MessagesBigStyle.hover.background = GreenTex;
            MessagesBigStyle.fixedWidth = 400;
            MessagesBigStyle.padding = new RectOffset(0, 0, 5, 5);
            MessagesBigStyle.clipping = TextClipping.Clip;
            MessagesBigStyle.stretchWidth = false;



          



            MessagesSmallStyle.normal.background = whiteTex;
            MessagesSmallStyle.alignment = TextAnchor.UpperCenter;
            MessagesSmallStyle.hover.background = GreenTex;





            // player options
            PlayeroptionsStyle.normal.background = RedTex;
            PlayeroptionsStyle.hover.background = GreenTex;
            PlayeroptionsStyle.onHover.background = RedTex;
            PlayeroptionsStyle.onNormal.background = GreenTex;
            PlayeroptionsStyle.alignment = TextAnchor.MiddleCenter;
            PlayeroptionsStyle.fontStyle = FontStyle.Bold;












            // general
            Generalstyle.normal.background = BlackTex;
            Generalstyle.normal.textColor = Color.black;

            Generalstyle.alignment = TextAnchor.MiddleCenter;
            Generalstyle.fontStyle = FontStyle.Bold;
            Generalstyle.hover.background = RedTex;
            Generalstyle.onHover.background = RedTex;

            








            // skin
            skin.label.normal.textColor = Color.black;
            skin.label.fontSize = 15;
            skin.label.fontStyle = FontStyle.Bold;
            skin.label.alignment = TextAnchor.MiddleCenter;
            skin.label.normal.background = TransTex;



            skin.textField.alignment = TextAnchor.MiddleCenter;
            skin.textField.normal.textColor = Color.red;
            skin.textField.hover.textColor = Color.white;
            skin.textField.normal.background = Texture2D.whiteTexture;
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
            skin.verticalScrollbarThumb.alignment = TextAnchor.MiddleRight;
            skin.verticalScrollbar.alignment = TextAnchor.MiddleRight;
            skin.verticalScrollbar.normal.background = whiteTex;
            skin.verticalScrollbar.hover.background = GreenTex;
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



        }







        public void UpdateAvailable(float version)
        {
            FrostyPGamemanager.instance.OpenMenu = false;
            Versionofupdate = version;
            UpdateOpen = true;
        }

        void ShowUpdateWindow()
        {

            // track state of download
            bool downloading = false;
            int totalpackets = 0;
            int currentpackets = 0;
            foreach(SendReceiveIndex f in FileSyncing.IncomingIndexes)
            {
                if(f.NameOfFile == "FrostyP_Game_Manager.dll" | f.NameOfFile == "FrostyP_Game_Manager.pdb" | f.NameOfFile == "PIPE_Valve_Console_Client.dll" | f.NameOfFile == "PIPE_Valve_Console_Client.pdb")
                {
                    downloading = true;
                    totalpackets = totalpackets + f.TotalPacketsinFile;
                    currentpackets = currentpackets + f.PacketNumbersStored.Count;

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
                   
                    FileSyncing.RequestFileFromServer("FrostyP_Game_Manager.dll");
                    FileSyncing.RequestFileFromServer("FrostyP_Game_Manager.pdb");
                    FileSyncing.RequestFileFromServer("PIPE_Valve_Console_Client.dll");
                    FileSyncing.RequestFileFromServer("PIPE_Valve_Console_Client.pdb");
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
              GUILayout.Label($"{currentpackets} of {totalpackets} received");
            }

            // if finished
            if (UpdateDownloaded)
            {
                GUILayout.Label($"{Versionofupdate} Update Downloaded!");
              GUILayout.Label("Update files in Mods/FrostyPGameManager/Update/");
              GUILayout.Label("Once the Game shuts down you can move them up one directory into Mods/FrostyPGameManager/ and overwrite current version");

            }





            GUILayout.EndArea();
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