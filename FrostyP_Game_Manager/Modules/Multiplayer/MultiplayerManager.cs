using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Text;
using UnityEngine.Networking;
using System.Linq;

namespace FrostyP_Game_Manager
{
    public class MultiplayerManager : FrostyModule
    {
        public static MultiplayerManager instance;
        static RiptideManager Riptide;
        /// <summary>
        /// RemotePlayers Reference Daryien
        /// </summary>
        public static GameObject DaryienClone;
        /// <summary>
        /// RemotePlayers Reference BMX
        /// </summary>
        public static GameObject BmxClone;
        /// <summary>
        /// Master BMXS Player Components
        /// </summary>
        public static GameObject BMXSPlayer;
        public static PlayerSaveData PlayerSavedata;

        public static int Ping;
        public static int PendingReliable;
        public static int KBoutpersec;
        public static int KBinpersec;
        public static int ActiveFragments;
        public static int TransformDump;
        public static int AudioDump;

        public static string MycurrentLevel = "Unknown";
        public uint MyPlayerId;
        public bool firstMap = true;
        public static Dictionary<ushort, RemotePlayer> Players;
        public static int Playercount { 
            get
            { 
                return Players.Count;
            } 
        }
        public static List<ushort> PlayerIds
        {
            get
            {
                return Players.Keys.ToList();
            }
        }

        static float KeepAlivetimer = 0;

        // FrostyMultiplayerAssets Bundle
        GameObject Prefab;
        public GameObject wheelcolliderobj;
        public AssetBundle FrostyAssets;
      
        // directories
        public static string Rootdir = Application.dataPath + "/FrostyPGameManager/";
        public static string TexturesDir = Rootdir + "Textures/";
        public static string MapsDir = Application.dataPath + "/CustomMaps/";
        public static string ParkAssetsDir = Rootdir + "ParkBuilder/Assetbundles/";
        public static string GarageDir = Application.dataPath + "/GarageContent/";
        public static string PlayerModelsDir = Application.dataPath + "/Custom Players/";
        public static string UpdateDir = Application.dataPath + "/FrostyPGameManager/Updates/";
        public static string TempDir = Rootdir + "Temp/";
        public static string Playersavedir = Application.dataPath + "/FrostyPGameManager/PlayerSaveData/";

        //patchaMapImporter
        GameObject patcha;
        PatchaMapImporter.PatchaMapImporter mapImporter;
        // Panoramic skybox shader
        public static Material PanoMat;
        // replaycam object
        public static GameObject SLRcam;

        public int ServersActive;
        public string ServerList = "No Servers Active";
        public static bool GarageEnabled;
        public static string GarageVersion;

        #region Riptide
        public static bool isConnected() => Riptide.isConnected();
        public static bool isConnecting() => Riptide.isConnecting();
        #endregion

        //initialize mapImporter
        public void Awake()
        {
            Buttontext = "Online";
            instance = this;
            patcha = new GameObject();
            patcha.AddComponent<PatchaMapImporter.PatchaMapImporter>();
            mapImporter = patcha.GetComponent<PatchaMapImporter.PatchaMapImporter>();
            PatchaMapImporter.PatchaMapImporter.changingmap.AddListener(GetLevelName);

            // make all directories
            string[] dirs = new string[]
            {
                Rootdir,
                TempDir,
                MapsDir + "DLLs/",
                PlayerModelsDir,
                UpdateDir,
                Playersavedir,

            };
            for (int i = 0; i < dirs.Length; i++)
            {

            if (!Directory.Exists(dirs[i]))
            {
                Directory.CreateDirectory(dirs[i]);
            }
            }
            
            // Setup Clone of daryien and bmx for multiplayer use
            BMXSPlayer = GameObject.Find("BMXS Player Components");
            SetupDaryienAndBMXBaseModels();

           
            FrostyAssets = AssetBundle.LoadFromFile(Application.dataPath + "/FrostyPGameManager/FrostyMultiPlayerAssets");
            Prefab = FrostyAssets.LoadAsset("PlayerPrefab") as GameObject;
            Prefab.AddComponent<RemotePlayer>();
            wheelcolliderobj = FrostyAssets.LoadAsset("WheelCollider") as GameObject;
            PanoMat = FrostyAssets.LoadAsset("Skybox") as Material;
            SLRcam = FrostyAssets.LoadAsset("SLRCam") as GameObject;

            Players = new Dictionary<ushort, RemotePlayer>();
            Riptide = new RiptideManager();
            if (GetConfig() == null) SaveConfig(new MultiplayerConfig());
            Pooler.Setup(GetConfig());

        }

        // Use this for initialization
        void Start()
        {
            try
            {
                GarageSetup();
            }
            catch(System.Exception x)
            {
                Debug.Log("Check garage version error : " + x);
            }
           
            Component.FindObjectsOfType<SessionMarker>()[0].OnSetAtMarker.AddListener(DontWipeOutPlayersOnReset);

            StartCoroutine(UpdatePublicServers());
        }

        void Update()
        {
            
            if (Input.GetKeyDown(KeyCode.B))
            {
                if (InGameUI.instance.Minigui)
                {
                    InGameUI.instance.Minigui = false;
                }
                if (InGameUI.instance.IsSpectating)
                {
                    InGameUI.instance.SpectateExit();
                }
                if (!MenuManager.instance.saveMenu.activeInHierarchy)
                {
                FrostyPGamemanager.instance.OpenMenu = MainManager.instance.isOpen;
                }
                else
                {
                    FrostyPGamemanager.instance.OpenMenu = false;
                }
                
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                if(!MainManager.instance.isOpen && !MenuManager.instance.saveMenu.activeInHierarchy)
                {
                    FrostyPGamemanager.instance.PopUpMessage("PipeWorks Map Importer \n not supported by Multiplayer");
                }
            }

        }

        void FixedUpdate()
        {
            if (!LocalPlayer.instance.SendStream)
            {
                KeepNetworkActive();
            }

        }

        public override void Open()
        {
            if (!InGameUI.instance.GethubDataWatch.IsRunning && !isConnected())
            {
                StartCoroutine(UpdatePublicServers());
                InGameUI.instance.GethubDataWatch.Start();

            }
            base.Open();
        }
        public override void Close()
        {

            base.Close();
        }
        public override void Show() => InGameUI.instance.Show();


        #region TheGarage
        /// <summary>
        ///  enables GarageEnabled if requirements are met
        /// </summary>
        void GarageSetup()
        {
            Debug.Log("Running garage Setup");
            string Garageinfopath = Assembly.GetExecutingAssembly().Location;
            int index1 = Garageinfopath.IndexOf("Mods") + 4;
            Garageinfopath = Garageinfopath.Remove(index1);

            //check if garage dir exists
            if(!Directory.Exists(Garageinfopath + "/TheGarage"))
            {
                Debug.Log("No Mods/TheGarage/ directory not found, Garage support will be disabled");
                GarageEnabled = false;
                GarageVersion = "null";
                return;
            }
            Garageinfopath = Garageinfopath + "/TheGarage/Info.json";
            Debug.Log("Garage path check :" + Garageinfopath);
            // check file exists
            if (!File.Exists(Garageinfopath))
            {
                Debug.Log("No Mods/TheGarage/info.json file not found, Garage support will be disabled");
                GarageEnabled = false;
                GarageVersion = "null";
                return;
            }


            try
            {
               Dictionary<string,string> Garageinfo = TinyJson.JSONParser.FromJson<Dictionary<string, string>>(File.ReadAllText(Garageinfopath, Encoding.UTF8));
                if (Garageinfo != null)
                {
                    foreach(string key in Garageinfo.Keys)
                    {
                        Debug.Log(key + " : " + Garageinfo[key]);
                    }
                    if (Garageinfo.ContainsKey("Version"))
                    {
                        GarageVersion = Garageinfo["Version"];
                        GarageEnabled = true;
                        Debug.Log("\nSuccessfully located Garage Version from TheGarage's info.json, version: " + GarageVersion + "\n");
                    }
                    else
                    {
                        GarageEnabled = false;
                        GarageVersion = "null";
                        Debug.Log("Garage Version not found in TheGarage/info.json, Garage support will be disabled");
                        return;
                    }
                    
                }
            }
            catch(ReflectionTypeLoadException x)
            {
                Debug.Log("Unknown error while setting up TheGarage, Garage support will be disabled\n");
                GarageEnabled = false;
                GarageVersion = "null";
                Debug.Log("garage error : " + x);
                return;
            }
            
            
           
        }
        // Edited piece of code from TheGarage to just give me latest GarageSave without disturbing it
        public SaveList GetMyGarageSavelist(string name)
        {
            try
            {
                Debug.Log("Starting load of: " + name + ".preset");
                XmlSerializer deserializer = new XmlSerializer(typeof(SaveList));
                TextReader reader = new StreamReader(Application.dataPath + "//GarageContent/GarageSaves/" + name + ".preset");
                object obj = deserializer.Deserialize(reader);
                reader.Close();
                return (SaveList)obj;
            }
            catch (System.Exception e)
            {
                Debug.Log("Error while reading from XML: " + e.Message);
                return null;
            }
        }
        public static void EnableGarageSupport(string serverver)
        {
            GarageEnabled = serverver == GarageVersion? true:false;

            if (GarageEnabled)
            {
                InGameUI.instance.NewMessage(10, new TextMessage($"Garage {GarageVersion} Support Enabled", FrostyUIColor.System, 0));
            }
            else
            {
                InGameUI.instance.NewMessage(10, new TextMessage($"Garage Support Disabled,\n your version:{GarageVersion}\n Server Version: {serverver} ", FrostyUIColor.System, 0));
            }
        }
        public static void DoGarageSetup(RemotePlayer player)
        {
            if (player.Gear.GarageSaveXML == "null") return;
            Debug.Log($"Garage Setup on {player.username}'s bike..");
           
            // cast back to savelist
            XmlSerializer deserializer = new XmlSerializer(typeof(SaveList));
            MemoryStream reader = new MemoryStream(Encoding.UTF8.GetBytes(player.Gear.GarageSaveXML));
            SaveList obj = deserializer.Deserialize(reader) as SaveList;
            reader.Close();
            RemoteLoadManager.instance.Load(player, obj); // Where the magic happens
            Debug.Log("Garage Setup complete");
        }

        #endregion


        public static void ConnectMaster(string ip, string port)
        {
            InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Starting Setup..", FrostyUIColor.System, 0));
            Players.Clear();
            

            // detects model and aligns tracking transforms, other models parent to daryien's bone with added offsets so we need to track them instead of daryiens bones in that case
            LocalPlayer.instance.RiderTrackingSetup();


            if (PlayerSavedata != null)
            {
                PlayerSavedata.Username = InGameUI.instance.Username;
                string json = TinyJson.JSONWriter.ToJson(PlayerSavedata);
                File.WriteAllText(Playersavedir + "PlayerData.json", json);

            }

            // setup list of nettextures ready to be asked about their names etc after successful connection
            if (LocalPlayer.instance.RiderModelname == "Daryien")
            {
                if (CharacterModding.instance.GetNetTextures().Count > 0)
                {
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Verified Daryien Textures..", FrostyUIColor.System, TextMessage.Textmessagemode.system));
                }
            }

            // find inital level name
            instance.GetLevelName();
            

            RiptideManager.instance.ConnectMaster(ip, port);

        }
        public static void DisconnectMaster()
        {
            try
            {
                LocalPlayer.instance.SendStream = false;
                RiptideManager.instance.DisconnectMaster();
                List<GameObject> playerroots = new List<GameObject>();

                foreach (RemotePlayer r in Players.Values)
                {
                    r.MasterShutdown();
                    playerroots.Add(r.gameObject);
                }
                if (Players.Count > 0)
                {
                    Players.Clear();

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
                UnLoadOtherPlayerModels();
                PlayersInvisibleOnMyMap();
                ParkBuilder.instance.ResetLoadedBundles();
                InGameUI.instance.Disconnect();
                // Server learns of disconnection itself and tells everyone

            }
            catch (UnityException x)
            {
                UnityEngine.Debug.Log("MP Disconnect error: " + x);
            }
        }
        public static void PlayerJoinedServer()
        {

        }
        public static void PlayerDisconnect(ushort _id)
        {
            if (Players.TryGetValue(_id, out RemotePlayer player))
            {

                if (InGameUI.instance.IsSpectating)
                {
                    InGameUI.instance.PlayerLeft(_id);
                }

                player.MasterShutdown();

                PlacedObject objinsavelist = null;
                foreach (NetGameObject n in player.Objects)
                {
                    if (n._Gameobject != null)
                    {
                        DestroyObj(n._Gameobject);
                    }

                    foreach (PlacedObject p in ParkBuilder.instance.ObjectstoSave)
                    {
                        if (p.OwnerID == player.id && p.ObjectId == n.ObjectID)
                        {
                            objinsavelist = p;
                        }
                    }


                    if (objinsavelist != null)
                    {
                        ParkBuilder.instance.ObjectstoSave.Remove(objinsavelist);
                    }

                }

                Players.Remove(_id);
                DestroyObj(player.gameObject);
            }

            foreach (Waitingrequest w in FileSyncing.WaitingRequests.ToArray())
            {
                if (w.player == _id)
                {
                    FileSyncing.WaitingRequests.Remove(w);
                }
            }

            PlayersInvisibleOnMyMap();
        }

        // Setup clones of daryien and the bike, so that remote daryiens and bikes are never linked to our daryien or bike
        void SetupDaryienAndBMXBaseModels()
        {
            DaryienClone = Instantiate(UnityEngine.GameObject.Find("Daryien"));
            BmxClone = Instantiate(UnityEngine.GameObject.Find("BMX"));
            DontDestroyOnLoad(DaryienClone);
            DontDestroyOnLoad(BmxClone);

            foreach(Transform t in DaryienClone.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.GetComponent<Animation>())
                {
                    DestroyObj(t.gameObject.GetComponent<Animation>());
                }
                if (t.gameObject.GetComponent<BMXLimbTargetAdjust>())
                {
                   DestroyObj(t.gameObject.GetComponent<BMXLimbTargetAdjust>());

                }
                if (t.gameObject.GetComponent<SkeletonReferenceValue>())
                {
                    DestroyObj(t.gameObject.GetComponent<SkeletonReferenceValue>());
                }
                if (t.gameObject.GetComponent<Rigidbody>())
                {
                    t.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }
                if (t.gameObject.name.Contains("Trigger"))
                {
                    DestroyObj((Object)t.gameObject);
                }

              
                t.gameObject.SetActive(false);
                
            }

            foreach (Transform t in BmxClone.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.name.Contains("Target"))
                {
                    DestroyObj((Object)t.gameObject);
                }
                if (t.gameObject.name.Contains("Foot"))
                {
                    DestroyObj((Object)t.gameObject);
                }

                if (t.gameObject.GetComponent<Hub>())
                {
                   DestroyObj(t.gameObject.GetComponent<Hub>());
                }
                if (t.gameObject.GetComponent<Rigidbody>())
                {
                    t.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }

              
                t.gameObject.SetActive(false);
            }

        }
        
        /// <summary>
        /// listening to PatchaMapImporter.ChangingMap unityevent, fired by us changing level via patcha
        /// </summary>
        public void GetLevelName()
        {
            try
            {
                firstMap = string.IsNullOrEmpty(mapImporter.GetCurrentMapName());
                MycurrentLevel = string.IsNullOrEmpty(mapImporter.GetCurrentMapName()) ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().name : mapImporter.GetCurrentMapName(); // Use the actual map file name or, if on the unmodded maps, use the scene name
               
                MycurrentLevel = MycurrentLevel.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "");

                if (!firstMap && isConnected() && LocalPlayer.instance.SendStream)
                {
                    RipTideOutgoing.SendMapName(MycurrentLevel);
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Sent {MycurrentLevel} Map name", FrostyUIColor.System, TextMessage.Textmessagemode.system));
                    ChangingLevel(MycurrentLevel);

                }
                
            }
            catch (UnityException)
            {
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Couldnt grab Current Scene name", FrostyUIColor.Server, TextMessage.Textmessagemode.system));
            }


        }
        /// <summary>
        ///  im changing level, change all rider visibilty to mylevel == theirlevel;
        /// </summary>
        /// <param name="mylevel"></param>
        public void ChangingLevel(string mylevel)
        {
          foreach(RemotePlayer r in Players.Values)
          {
                r.ChangePlayerVisibilty(r.CurrentMap.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower() == mylevel.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower());
          }
            
        }
        /// <summary>
        /// Someones chaging level, change their visiblity to mylevel == theirlevel;
        /// </summary>
        /// <param name="Riderslevel"></param>
        /// <param name="from"></param>
        public void ChangingLevel(string Riderslevel, ushort from)
        {
            if(Players.TryGetValue(from,out RemotePlayer player))
            {
                player.ChangePlayerVisibilty(Riderslevel.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower() == MycurrentLevel.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower());
            }
        }
        public void SpawnRider(NewRiderPacket config)
        {
            Debug.Log($"Spawning : {config._username} as {config.currentmodel}, Id: {config._id}");

            // checki if exists already
            foreach(RemotePlayer player in Players.Values)
            {
               if(player.id == config._id)
               {
                    // player already exists, clean out first
                    CleanUpOldPlayer(config._id);

               }
            }
            

            GameObject NewRider = GameObject.Instantiate(Prefab);
            DontDestroyOnLoad(NewRider);
            RemotePlayer r = NewRider.GetComponent<RemotePlayer>();
            Players.Add(config._id, r);
            NewRider.AddComponent<RemotePartMaster>();
            NewRider.AddComponent<RemoteBrakesManager>();
            r.CurrentModelName = config.currentmodel;
            NewRider.name = config._username + config._id.ToString();
            r.Modelbundlename = config.modelbundlename;
            r.id = config._id;
            r.username = config._username;
            r.CurrentMap = config.Currentmap;
            r.Gear = config.Gear;
            r.Objects = config.Objects;
            r.StartupPos = config._position;
            r.StartupRot = config._rotation;


            // file checks
            FileSyncing.CheckForMap(config.Currentmap, config._username);
            if (config.Gear.RiderTextures.Count > 0)
            {
            for (int i = 0; i < config.Gear.RiderTextures.Count; i++)
            {
                if(config.Gear.RiderTextures[i].Nameoftexture.ToLower() != "stock" && config.Gear.RiderTextures[i].Nameoftexture.ToLower() != "e")
                {
                   
                    if (!FileSyncing.CheckForFile(config.Gear.RiderTextures[i].Nameoftexture, config.Gear.RiderTextures[i].Directory))
                    {
                    FileSyncing.AddToRequestable(1, config.Gear.RiderTextures[i].Nameoftexture.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", ""), config._id, config.Gear.RiderTextures[i].Directory);
                    }

                }
            }

            }

            if(config.currentmodel != "Daryien")
            {
              if (!FileSyncing.CheckForFile(config.currentmodel,PlayerModelsDir))
              {
                FileSyncing.AddToRequestable(3, config.currentmodel.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", ""), config._id,PlayerModelsDir);
              }

            }

            Debug.Log("Spawn finished");

        }
        public void SpawnObject(NetGameObject _netobj)
        {
            


            if(Players.TryGetValue(_netobj.OwnerID,out RemotePlayer _player) != false)
            {
           // check object doesnt exist already using objects unique id
            foreach(NetGameObject n in _player.Objects)
            {
                if(n.ObjectID == _netobj.ObjectID)
                {
                        InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"Duplicate spawn attempt {_netobj.NameofObject} from package {_netobj.NameOfFile} for {Players[_netobj.OwnerID].username}, object wont be spawned", FrostyUIColor.Server, 0));
                        return;
                }
            }

            // add to players objects whether the object is loadable or not
                    _player.Objects.Add(_netobj);


                GetObject(_netobj);
                if(_netobj._Gameobject != null)
                {
                    if (!InGameUI.instance.AllPlayerObjectsVisibleToggle)
                    {
                    _netobj._Gameobject.SetActive(false);
                    }
                    else
                    {
                    _netobj._Gameobject.SetActive(_player.PlayerObjectsVisible);
                    }
                    
                }


            }

        }
        public void GetObject(NetGameObject _netobj)
        {
            // check if objects bundle is loaded on this machine, if not look for filename and load bundle, if not, tell user what they need
            int pdata = _netobj.Directory.ToLower().LastIndexOf("pipe_data");
            string mydir = Application.dataPath + _netobj.Directory.Remove(0,pdata + 9) + "/";

            Debug.Log($"Looking for Park asset {_netobj.NameofObject} at path {mydir}");

            foreach (AssetBundle A in AssetBundle.GetAllLoadedAssetBundles())
            {
                if (A.name == _netobj.NameofAssetBundle)
                {
                    Debug.Log($"Loading Park asset from already loaded bundle");
                    GameObject _newobj = Instantiate(A.LoadAsset(_netobj.NameofObject)) as GameObject;
                    _newobj.transform.position = _netobj.Position;
                    _newobj.transform.eulerAngles = _netobj.Rotation;

                    _netobj._Gameobject = _newobj;
                    _netobj.AssetBundle = A;
                    DontDestroyOnLoad(_newobj);
                    return;
                }
            }

            FileInfo Finfo = instance.FileNameMatcher(new DirectoryInfo(mydir).GetFiles(), _netobj.NameOfFile);
            
                if (Finfo != null)
                {
                Debug.Log($"Loading new Park bundle: requested: {_netobj.NameOfFile}:: matched to {Finfo.Name} ");
                    AssetBundle newbundle = AssetBundle.LoadFromFile(Finfo.FullName);
                    GameObject _newobj = Instantiate(newbundle.LoadAsset(_netobj.NameofObject)) as GameObject;

                    _newobj.transform.position = _netobj.Position;
                    _newobj.transform.eulerAngles = _netobj.Rotation;

                    _netobj._Gameobject = _newobj;
                    _netobj.AssetBundle = newbundle;
                    DontDestroyOnLoad(_newobj);

                    ParkBuilder.instance.bundlesloaded.Add(new BundleData(newbundle, Finfo.Name,Finfo.DirectoryName));
                    return;
                }
            

            // failed to find object, inform user what package is missing and clean up, resolve with server
            InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"Failed to find {mydir + _netobj.NameOfFile} for {Players[_netobj.OwnerID].username}'s {_netobj.NameofObject}", FrostyUIColor.Server, 0));
            if (!FileSyncing.CheckForFile(_netobj.NameOfFile,ParkAssetsDir))
            {
                FileSyncing.AddToRequestable(5, _netobj.NameOfFile, _netobj.ObjectID,_netobj.OwnerID);
            }

        }
        public void MoveObject(NetGameObject _netobj, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject Obj = _netobj._Gameobject;

            Obj.transform.position = Vector3.MoveTowards(Obj.transform.position, pos, Vector3.Distance(Obj.transform.position, pos));
            Obj.transform.eulerAngles = Vector3.MoveTowards(Obj.transform.eulerAngles, rot, Vector3.Distance(Obj.transform.eulerAngles, rot));
            Obj.transform.localScale = Vector3.MoveTowards(Obj.transform.localScale, scale, Vector3.Distance(Obj.transform.localScale, scale));

        }
        public GearUpdate GetMyGear(bool fordaryien)
        {
            GearUpdate gear = new GearUpdate();
            if (fordaryien)
            {
                List<NetTexture> list = CharacterModding.instance.GetNetTextures();
                
                gear.RiderTextures = list;
                gear.isRiderUpdate = true;
                gear.Capisforward = CharacterModding.instance.CapisForward;

            }
            else
            {
                if (!GarageEnabled) return gear;

                try
                {
                    string preset = PlayerPrefs.GetString("lastPreset");
                    gear.Presetname = preset;  
                    InGameUI.instance.currentGaragePreset = preset;
                    gear.GarageSaveXML = File.ReadAllText(Application.dataPath + "//GarageContent/GarageSaves/" + preset + ".preset");
                    gear.isRiderUpdate = false;
                }
                catch (System.Exception)
                {
                    Debug.Log("xml save:" + gear.GarageSaveXML);
                    InGameUI.instance.NewMessage(6, new TextMessage("Error Loading Garage Save, if problem persists try checking that BikeWorkshop.dll in Mods/FrostyPGameManager matches your current build of TheGarage2", FrostyUIColor.Server, TextMessage.Textmessagemode.system));
                    Debug.Log("Error Loading Garage Save, if problem persists try checking that BikeWorkshop.dll in Mods/FrostyPGameManager matches your current build of TheGarage2");
                }
            }

            return gear;
        }
        public void SendAllParts()
        {
            GearUpdate FullGear = new GearUpdate();

            // if daryien, add rider gear
            if(LocalPlayer.instance.RiderModelname == "Daryien")
            {
            FullGear = GetMyGear(true);
            }
            if (GarageEnabled)
            {
                GearUpdate bikegear = GetMyGear(false);
                FullGear.GarageSaveXML = bikegear.GarageSaveXML;
                FullGear.Presetname = bikegear.Presetname;

            }
            Debug.Log("Sending parts");
            RipTideOutgoing.SendAllParts(FullGear);

        }
        public static void TogglePlayerComponents(bool active)
        {
            if(BMXSPlayer == null)
            {
                BMXSPlayer = GameObject.Find("BMXS Player Components");
            }

            foreach(Transform t in BMXSPlayer.GetComponentsInChildren<Transform>(true))
            {
                if(t.gameObject.name.Contains("BMX Template") | t.gameObject.name.Contains("MGInputManager") | t.gameObject.name.Contains("CharacterManager"))
                {
                 t.gameObject.SetActive(active);
                }
            }
            if (active)
            {
            BMXSPlayer.GetComponentInChildren<SessionMarker>(true).ResetPlayerAtMarker();
            }
        }
        public static GameObject GetNewDaryien()
        {
        GameObject Daryien = Instantiate(DaryienClone);
            foreach (Transform t in Daryien.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.GetComponent<Rigidbody>())
                {
                    t.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }
                t.gameObject.SetActive(true);

            }

            DontDestroyOnLoad(Daryien);

        return Daryien;
        }
        public static GameObject GetNewBMX()
        {
            GameObject bmx = Instantiate(BmxClone);
            foreach (Transform t in bmx.GetComponentsInChildren<Transform>(true))
            {
                if (t.gameObject.GetComponent<Rigidbody>())
                {
                    t.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }

                t.gameObject.SetActive(true);
            }

            DontDestroyOnLoad(bmx);
            return bmx;

        }
        public static GameObject GetPlayerModel(string modelname, string modelbundlename)
        {
            GameObject player = null;

            IEnumerable<AssetBundle> bundles = AssetBundle.GetAllLoadedAssetBundles();
            foreach (AssetBundle a in bundles)
            {
                if (a.name.ToLower().Contains(modelbundlename.ToLower()))
                {
                    player = Instantiate(a.LoadAsset(modelname)) as GameObject;
                    DontDestroyOnLoad(player);
                    return player;
                }

            }


            if (File.Exists(PlayerModelsDir + modelname))
            {

                AssetBundle b = AssetBundle.LoadFromFile(PlayerModelsDir + modelname);


                player = Instantiate(b.LoadAsset(modelname) as GameObject);
                DontDestroyOnLoad(player);
                return player;
            }
            else
            {
               
              return GetNewDaryien();
              
            }






        }
        public static Texture2D GetTexture(string name,string directory)
        {
            Texture2D image = new Texture2D(2,2);
            string path = "";
            int pdata = directory.ToLower().LastIndexOf("pipe_data");
            if(pdata != -1)
            {
            path = Application.dataPath + directory.Remove(0, pdata + 9);
            }

            FileInfo Finfo = instance.FileNameMatcher(new DirectoryInfo(path).GetFiles(), name);

            if(Finfo != null)
            {
                if(Finfo.Name == name)
                {
                   byte[] file = File.ReadAllBytes(Finfo.FullName);
                   ImageConversion.LoadImage(image,file);
                   image.name = name;

                }

            }
            
            




            return image;
        }
        public void TurnOnNetUpdates()
        {
            RipTideOutgoing.TurnMeOn();
            LocalPlayer.instance.SendStream = true;

            foreach (RemotePlayer player in Players.Values)
            {
                player.MasterActive = true;
            }

        }
        public void TurnOffNetUpdates()
        {
            RipTideOutgoing.TurnMeOff();
            LocalPlayer.instance.SendStream = false;
            foreach(RemotePlayer player in Players.Values)
            {
                player.MasterActive = false;
            }

        }
        public static void KeepNetworkActive()
        {
            // keep connection active while no rider is streaming
            if (KeepAlivetimer < 10)
            {
                KeepAlivetimer = KeepAlivetimer + Time.deltaTime;
            }
            else if (KeepAlivetimer >= 10)
            {
                RipTideOutgoing.KeepActive();
                KeepAlivetimer = 0;
            }

        }
        public void CleanUpOldPlayer(ushort _id)
        {
            try
            {
                bool t = false;
                if( Players.TryGetValue(_id,out RemotePlayer player))
                {
                    // delete objects
                    if (player.Objects.Count > 0)
                    {
                        for (int i = 0; i < player.Objects.Count; i++)
                        {
                            if(player.Objects[i]._Gameobject!= null)
                            {
                                DestroyObj((Object)player.Objects[i]._Gameobject);
                            }
                        }
                    }

                    if(player.RiderModel != null)
                    {
                        DestroyObj((Object)player.RiderModel);
                    }

                    if(player.BMX!= null)
                    {
                        DestroyObj((Object)player.BMX);
                    }
                    t = true;



                }
                if (t)
                {
                    Players.Remove(_id);
                    DestroyObj((Object)player.gameObject);

                }

               
               
            }
            catch (System.Exception x)
            {

            }



        }
        /// <summary>
        /// PI assumes it knows what bundles are loaded based on my use. symptom: choosing a model after any remoteplayer has loaded it causes failure Asset/bundle already loaded
        /// </summary>
        public static void UnLoadOtherPlayerModels()
        {
            List<AssetBundle> tounload = new List<AssetBundle>();

            // get loaded bundles
            IEnumerable<AssetBundle> loaded = AssetBundle.GetAllLoadedAssetBundles();
            foreach(AssetBundle a in loaded)
            {
                string bundleinput = a.name;
                string bundleascii = Encoding.ASCII.GetString(Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(Encoding.ASCII.EncodingName, new EncoderReplacementFallback(string.Empty), new DecoderExceptionFallback()), Encoding.UTF8.GetBytes(bundleinput)));
                bundleascii = bundleascii.Trim(Path.GetInvalidFileNameChars());
                bundleascii = bundleascii.Trim(Path.GetInvalidPathChars());

                if(bundleascii.ToLower() != LocalPlayer.instance.RiderModelBundleName.ToLower())
                {
                    FileInfo[] models = new DirectoryInfo(PlayerModelsDir).GetFiles();

                    for (int i = 0; i < models.Length; i++)
                    {
                        if (a.Contains(models[i].Name))
                        {
                            tounload.Add(a);
                        }
                    }


                }


            }

            if (tounload.Count > 0)
            {
                for (int i = 0; i < tounload.Count; i++)
                {
                    tounload[i].Unload(true);
                }
            }


        }
        public void JumpToPlayerMap(string name)
        {
            bool found = false;
            foreach(FileInfo map in new DirectoryInfo(MapsDir).GetFiles())
            {
                if(map.Name.ToLower() == name.ToLower())
                {
                    found = true;
                  mapImporter._mapLoader.Load(new PatchaMapImporter.Models.Map(map.Name));
                   
                }
                else if (map.Name.Replace("_"," ").ToLower() == name.Replace("_", " ").ToLower())
                {
                    found = true;
                    mapImporter._mapLoader.Load(new PatchaMapImporter.Models.Map(map.Name));

                }
                else if (map.Name.Replace("_", " ").ToLower().Contains(name.Replace("_", " ").ToLower()))
                {
                    found = true;
                    mapImporter._mapLoader.Load(new PatchaMapImporter.Models.Map(map.Name));
                }
                else if (name.Replace("_", " ").ToLower().Contains(map.Name.Replace("_", " ").ToLower()))
                {
                    found = true;
                    mapImporter._mapLoader.Load(new PatchaMapImporter.Models.Map(map.Name));
                }
            }

            if (!found)
            {
                InGameUI.instance.NewMessage(10, new TextMessage("Cant match that Map name, Check Patcha", FrostyUIColor.System, TextMessage.Textmessagemode.system));
            }


        }
        public int RidersOnMyMap()
        {
            int count = 0;
            foreach(RemotePlayer player in Players.Values)
            {
                if(player.CurrentMap.ToLower() == MycurrentLevel.ToLower())
                {
                    count++;
                }
                else if (player.CurrentMap.ToLower().Contains(MycurrentLevel.ToLower()))
                {
                    count++;
                }
                else if (MycurrentLevel.ToLower().Contains(player.CurrentMap.ToLower()))
                {
                    count++;
                }
                else if (MycurrentLevel.Replace("_"," ").ToLower().Contains(player.CurrentMap.Replace("_", " ").ToLower()))
                {
                    count++;
                }
            }
            return count;
        }
        public bool RiderOnMyMap(RemotePlayer player)
        {
           
            
                if (player.CurrentMap.ToLower() == MycurrentLevel.ToLower())
                {
                return true;
                }
                else if (player.CurrentMap.ToLower().Contains(MycurrentLevel.ToLower()))
                {
                return true;
                }
                else if (MycurrentLevel.ToLower().Contains(player.CurrentMap.ToLower()))
                {
                return true;
                }
                else if (MycurrentLevel.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower().Contains(player.CurrentMap.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower()))
                {
                return true;
                }

            return false;
        }
        public float GetAveragePing()
        {
            List<float> pings = new List<float>();
            foreach (RemotePlayer player in Players.Values)
            {
                pings.Add(player.R2RPing);
            }
            float averageping = 0;
            if (pings.Count > 0)
            {
                for (int i = 0; i < pings.Count; i++)
                {
                    averageping = averageping + pings[i];
                }

                averageping = averageping / pings.Count;
            }
            return averageping;
        }
        public static void PlayersInvisibleOnMyMap()
        {
            List<ushort> ridersToBeAwareOf = new List<ushort>();
            foreach(RemotePlayer r in Players.Values)
            {
                if(r.CurrentMap.ToLower() == MycurrentLevel.ToLower())
                {
                    if (!r.PlayerIsVisible)
                    {
                        ridersToBeAwareOf.Add(r.id);
                    }
                }
            }

            InGameUI.instance.ToggledOffPlayersByMe = ridersToBeAwareOf;

        }
        // A non-Monobehaviour wants to use a Mono function
        public static void DestroyObj(Object g)
        {
            Destroy(g);
        }
        public void DontDestroy(GameObject g)
        {
            DontDestroyOnLoad(g);
        }
        public void DontWipeOutPlayersOnReset()
        {
            if (FindObjectOfType<WalkingSetUp>())
            {
                Destroy(FindObjectOfType<WalkingSetUp>().gameObject);
            }
            if (isConnected() && LocalPlayer.instance.SendStream)
            {
             StartCoroutine(EnumDontWipeOutPlayersOnReset());
            }
        }
        public IEnumerator EnumDontWipeOutPlayersOnReset()
        {
            LocalPlayer.instance.SendStream = false;
            yield return new WaitForSeconds(0.5f);
            LocalPlayer.instance.SendStream = true;
           
        }
        public FileInfo FileNameMatcher(FileInfo[] myfiles, string filetomatch)
        {
            bool found = false;
            for (int i = 0; i < myfiles.Length; i++)
            {
                if (!found)
                {

                if(myfiles[i].Name.ToLower() == filetomatch.ToLower())
                {
                    found = true;
                    return myfiles[i];
                }
                else if(myfiles[i].Name.Replace("_"," ").ToLower() == filetomatch.Replace("_"," ").ToLower())
                {
                    found = true;
                    return myfiles[i];
                }
                else if(myfiles[i].Name.Replace("_", " ").Replace("(1)","").ToLower() == filetomatch.Replace("_", " ").Replace("(1)", "").ToLower())
                {
                    found = true;
                    return myfiles[i];
                }
                else if (myfiles[i].Name.Replace("_", " ").Replace("(2)", "").ToLower() == filetomatch.Replace("_", " ").Replace("(2)", "").ToLower())
                {
                    found = true;
                    return myfiles[i];
                }
                else if (myfiles[i].Name.Replace("_", " ").Replace("(3)", "").ToLower() == filetomatch.Replace("_", " ").Replace("(3)", "").ToLower())
                {
                    found = true;
                    return myfiles[i];
                }

                }
            }

            
             return null;
            

        }
        /// <summary>
        /// sort hub list of all servers
        /// </summary>
        /// <param name="rawlist"></param>
        public void SortServerListData(string rawlist)
        {
            // break strings apart, determine how many servers are live and build publicserver classes to store info for GUI to show
            List<PublicServer> liveservers = new List<PublicServer>();

            try
            {
                Debug.Log(rawlist);
                string[] serversfull = rawlist.Split('}');
                for (int i = 0; i < serversfull.Length; i++)
                {
                    // if string is not dud
                    if (serversfull[i].Length > 5)
                    {
                        Debug.Log("split server " + i + " : " + serversfull[i]);

                        string[] data = new string[8];
                      
                        // do 8 times
                        for (int _i = 0; _i < data.Length; _i++)
                        {
                            int ind = serversfull[i].IndexOf(":");
                            int comma = -1;
                            if (ind != -1) comma = serversfull[i].IndexOf(",",ind);
                            if(ind == -1 && comma == -1)
                            {
                                Debug.Log("Didnt find colon and comma");
                                Debug.Log(serversfull[i]);
                                break;
                               
                            }
                            else
                            {
                            data[_i] = serversfull[i].Remove(0, ind+1).Replace('"',' ').Replace('{',' ').Trim();
                               int incomma = data[_i].IndexOf(",");
                                if (incomma != -1)
                                {
                                    data[_i] = data[_i].Remove(incomma, data[_i].Length - incomma);
                                }
                                else
                                {
                                    data[_i] = data[_i].Replace('}', ' ').Replace(']', ' ').Trim();
                                }
                            Debug.Log($"Built data entry {_i}: {data[_i]}");
                            serversfull[i] = serversfull[i].Remove(0,comma + 1);
                                Debug.Log("Remaining data in this server log: " + serversfull[i]);
                            }


                        }

                         liveservers.Add(new PublicServer(data[0], data[1], data[2], data[3], data[4], data[5], data[6],data[7]));
                         Debug.Log("live server sorted");

                    }
                }

                // give list to GUI
            InGameUI.instance.Publicservers = liveservers;


            }
            catch (System.Exception x)
            {
                Debug.Log(x);
            }

        }
        /// <summary>
        /// sort server request content for a server
        /// </summary>
        /// <param name="rawlist"></param>
        public void ReceivedServerDetails(string rawlist)
        {
            Debug.Log("received");
            int ind = rawlist.ToLower().IndexOf("port");
            if(ind == -1)
            {
                Debug.Log("Error sorting server details");
                return;
            }
            string IP = rawlist.Remove(ind,rawlist.Length-ind).Trim();
            string port = rawlist.Remove(0,ind + 4).Trim();


            ConnectMaster(IP, port);
        }
        /// <summary>
        /// Do get request on timer and give raw data to SortServerList to be added to list of available servers
        /// </summary>
        /// <returns></returns>
        public static IEnumerator UpdatePublicServers()
        {
            MultiplayerConfig config = GetConfig();
            if (config == null) yield return null;
            InGameUI.instance.UpdatingServerlist = true;
            UnityEngine.Debug.Log("server list update");
            UnityWebRequest www = UnityWebRequest.Get(config.GetUrl);
            www.SetRequestHeader("apikey", config.HubAPIPassword);
            while (!www.isDone)
            {

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                UnityEngine.Debug.Log("server list error");
                yield return new WaitForSeconds(5);
                }
                else
                {
                       
                    yield return new WaitForEndOfFrame();

                    var serverlist = www.downloadHandler.text;
                    // give raw list and sort into seperate servers data
                    if(serverlist != null) instance.SortServerListData(serverlist);
                    InGameUI.instance.UpdatingServerlist = false;

                        
                }
                
               
            }

        }
        /// <summary>
        /// A public server was clicked, get details and send to SortServerRequest
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEnumerator RequestServerConnection(string name)
        {
            MultiplayerConfig config = GetConfig();
            if (config == null) yield return null;
            int count = 0;
            UnityEngine.Debug.Log("requesting Servers address..");
            UnityWebRequest www = UnityWebRequest.Get(config.RequestserverUrl + name);
            www.SetRequestHeader("apikey", config.HubAPIPassword);
            while (!www.isDone)
            {

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    UnityEngine.Debug.Log("error getting ip and port from hub");
                    count++;
                    if (count >= 3)
                    {
                        InGameUI.instance.RequestingConnection = false;
                        yield return null;
                    }

                    yield return new WaitForSeconds(5);

                }
                else
                {

                    yield return new WaitForEndOfFrame();
                    InGameUI.instance.RequestingConnection = false;
                    var serverlist = www.downloadHandler.text;
                    instance.ReceivedServerDetails(serverlist);
                   
                }


            }

        }
        public static MultiplayerConfig GetConfig()
        {
            if(File.Exists(Playersavedir + "MultiplayerConfig.json"))
            {
                MultiplayerConfig config = TinyJson.JSONParser.FromJson<MultiplayerConfig>(File.ReadAllText(Playersavedir + "MultiplayerConfig.json"));
                return config;
            }
            else
            {
                SaveConfig(new MultiplayerConfig());
                return null;
            }
        }
        public static void SaveConfig(MultiplayerConfig conf)
        {
            string json = TinyJson.JSONWriter.ToJson(conf);
            File.WriteAllText(Playersavedir + "MultiplayerConfig.json", json);
            
        }

        public struct NewRiderPacket
        {
            public ushort _id;
            public string _username;
            public string currentmodel;
            public string modelbundlename;
            public Vector3 _position;
            public Vector3 _rotation;
            public string Currentmap;
            public GearUpdate Gear;
            public List<NetGameObject> Objects;

        }

    }







}