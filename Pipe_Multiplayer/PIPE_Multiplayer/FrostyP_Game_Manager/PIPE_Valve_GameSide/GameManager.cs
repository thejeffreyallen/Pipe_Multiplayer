using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.IO;
using FrostyP_Game_Manager;
using System.Reflection;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;


namespace PIPE_Valve_Console_Client
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
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

        public string MycurrentLevel = "Unknown";
        public uint MyPlayerId;
        public bool firstMap = true;
        public static Dictionary<uint, RemotePlayer> Players;
        static float KeepAlivetimer = 0;
        List<string> RandomMessageOnSpawn;

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

        GameObject lastwalkchar;

        //patchaMapImporter
        GameObject patcha;
        PatchaMapImporter.PatchaMapImporter mapImporter;

        // Panoramic skybox shader
        public static Material PanoMat;


        //initialize mapImporter
        public void Awake()
        {
           
            instance = this;
            patcha = new GameObject();
            patcha.AddComponent<PatchaMapImporter.PatchaMapImporter>();
            mapImporter = patcha.GetComponent<PatchaMapImporter.PatchaMapImporter>();


            // make all directories

            string[] dirs = new string[]
            {
                TempDir,
                MapsDir + "DLLs/",
                PlayerModelsDir,
                UpdateDir,
                GarageDir,

            };

            for (int i = 0; i < dirs.Length; i++)
            {

            if (!Directory.Exists(dirs[i]))
            {
                Directory.CreateDirectory(dirs[i]);
            }
            }
            
            BMXSPlayer = GameObject.Find("BMXS Player Components");
            SetupDaryienAndBMXBaseModels();

            RandomMessageOnSpawn = new List<string>()
            {
                {" is on it" },
                {" broke the bmx out" },
                {" is about to drop hammers" },
                {"'s pumped the tyres up" },
                {"'s about" },
                {"'s here" },
                { " showed up" },
                {" 's rollin" },
                {"'s gonna send it" },
                {"'s about to go off" },
                {"'s warming up" },
                {" shreds" },
                {"'s on the way" },


            };

            Players = new Dictionary<uint, RemotePlayer>();

        }

        // Use this for initialization
        void Start()
        {
           
            FrostyAssets = AssetBundle.LoadFromFile(Application.dataPath + "/FrostyPGameManager/FrostyMultiPlayerAssets");
            Prefab = FrostyAssets.LoadAsset("PlayerPrefab") as GameObject;
            Prefab.AddComponent<RemotePlayer>();
            wheelcolliderobj = FrostyAssets.LoadAsset("WheelCollider") as GameObject;
            PanoMat = FrostyAssets.LoadAsset("Skybox") as Material;
            Component.FindObjectsOfType<SessionMarker>()[0].OnSetAtMarker.AddListener(DontWipeOutPlayersOnReset);

          
        }

        void Update()
        {
            if (MGInputManager.Y_Down())
            {
                StartCoroutine(FixWalkBug());
            }
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

        }

        // make clones so theres always a reference model for each and its not our Original versions
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

        public void GetLevelName()
        {
            try
            {
                firstMap = string.IsNullOrEmpty(mapImporter.GetCurrentMapName());
                MycurrentLevel = string.IsNullOrEmpty(mapImporter.GetCurrentMapName()) ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().name : mapImporter.GetCurrentMapName(); // Use the actual map file name or, if on the unmodded maps, use the scene name
               
                MycurrentLevel = ConvertToUnicode(MycurrentLevel);
                

                if (!firstMap && InGameUI.instance.Connected && LocalPlayer.instance.ServerActive)
                {
                    ClientSend.SendMapName(MycurrentLevel);
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Sent {MycurrentLevel} Map name", 1, 1));
                    ChangingLevel(MycurrentLevel);

                }
                
            }
            catch (UnityException)
            {
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Couldnt grab Current Scene name", (int)MessageColourByNum.Server, 1));
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
                r.ChangePlayerVisibilty(r.CurrentMap.ToLower() == mylevel.ToLower());
          }
            
        }

        /// <summary>
        /// Someones chaging level, change their visiblity to mylevel == theirlevel;
        /// </summary>
        /// <param name="Riderslevel"></param>
        /// <param name="from"></param>
        public void ChangingLevel(string Riderslevel, uint from)
        {
            if(Players.TryGetValue(from,out RemotePlayer player))
            {
                player.ChangePlayerVisibilty(Riderslevel.ToLower() == MycurrentLevel.ToLower());
            }
        }

        public void SpawnRider(uint _id, string _username, string currentmodel,string modelbundlename, Vector3 _position, Vector3 _rotation, string Currentmap, GearUpdate Gear)
        {
            Debug.Log($"Spawning : {_username} as {currentmodel}, Id: {_id}");

            // checki if exists already
            foreach(RemotePlayer player in Players.Values)
            {
               if(player.id == _id)
               {
                    // player already exists, clean out first
                    CleanUpOldPlayer(_id);

               }
            }
            

                    GameObject NewRider = GameObject.Instantiate(Prefab);
                    DontDestroyOnLoad(NewRider);
                    RemotePlayer r = NewRider.GetComponent<RemotePlayer>();
                    Players.Add(_id, r);
                    NewRider.AddComponent<RemotePartMaster>();
                    NewRider.AddComponent<RemoteBrakesManager>();
                    r.CurrentModelName = currentmodel;
                    NewRider.name = _username + _id.ToString();
                    r.Modelbundlename = modelbundlename;
                    r.id = _id;
                    r.username = _username;
                    r.CurrentMap = Currentmap;
                    r.Gear = Gear;
                    r.Objects = new List<NetGameObject>();
                    r.StartupPos = _position;
                    r.StartupRot = _rotation;


            // file checks
            FileSyncing.CheckForMap(Currentmap, _username);
            for (int i = 0; i < Gear.RiderTextures.Count; i++)
            {
                if(Gear.RiderTextures[i].Nameoftexture != "stock" && Gear.RiderTextures[i].Nameoftexture != "e")
                {
                   
                    if (!FileSyncing.CheckForFile(Gear.RiderTextures[i].Nameoftexture, Gear.RiderTextures[i].Directory))
                    {
                    FileSyncing.AddToRequestable(1, Gear.RiderTextures[i].Nameoftexture,_id,Gear.RiderTextures[i].Directory);
                    }

                }
            }

            if(currentmodel != "Daryien")
            {
              if (!FileSyncing.CheckForFile(currentmodel,PlayerModelsDir))
              {
                FileSyncing.AddToRequestable(3,currentmodel, _id,PlayerModelsDir);
              }

            }



            InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage(_username + RandomMessageOnSpawn[Random.Range(0, RandomMessageOnSpawn.Count - 1)], (int)MessageColourByNum.Player, _id));

            Debug.Log("Spawn finished");

        }

        public void SpawnObject(NetGameObject _netobj)
        {
            RemotePlayer _player;


            if(Players.TryGetValue(_netobj.OwnerID,out _player) != false)
            {
           // check object doesnt exist already using objects unique id
            foreach(NetGameObject n in _player.Objects)
            {
                if(n.ObjectID == _netobj.ObjectID)
                {
                        InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"Duplicate spawn attempt {_netobj.NameofObject} from package {_netobj.NameOfFile} for {Players[_netobj.OwnerID].username}, object wont be spawned", (int)MessageColourByNum.Server, 0));
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

            foreach (AssetBundle A in AssetBundle.GetAllLoadedAssetBundles())
            {
                if (A.name == _netobj.NameofAssetBundle)
                {
                    GameObject _newobj = Instantiate(A.LoadAsset(_netobj.NameofObject)) as GameObject;
                    _newobj.transform.position = _netobj.Position;
                    _newobj.transform.eulerAngles = _netobj.Rotation;

                    _netobj._Gameobject = _newobj;
                    _netobj.AssetBundle = A;
                    DontDestroyOnLoad(_newobj);
                    return;
                }
            }

            foreach (FileInfo file in new DirectoryInfo(ParkBuilder.instance.AssetbundlesDirectory).GetFiles())
            {
                if (file.Name == _netobj.NameOfFile)
                {
                    AssetBundle newbundle = AssetBundle.LoadFromFile(file.FullName);
                    GameObject _newobj = Instantiate(newbundle.LoadAsset(_netobj.NameofObject)) as GameObject;

                    _newobj.transform.position = _netobj.Position;
                    _newobj.transform.eulerAngles = _netobj.Rotation;

                    _netobj._Gameobject = _newobj;
                    _netobj.AssetBundle = newbundle;
                    DontDestroyOnLoad(_newobj);

                    ParkBuilder.instance.bundlesloaded.Add(new BundleData(newbundle, file.Name,file.DirectoryName));
                    return;
                }
            }

            // failed to find object, inform user what package is missing and clean up, resolve with server
            InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"Failed to find {_netobj.NameofObject} from {_netobj.NameOfFile} for {Players[_netobj.OwnerID].username}", (int)MessageColourByNum.Server, 0));
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

        public GearUpdate GetMyGear(bool forRidermodel)
        {
            GearUpdate gear = new GearUpdate();
            if (forRidermodel)
            {
                List<TextureInfo> list = new List<TextureInfo>();
                foreach(Renderer r in CharacterModding.instance.Rider_Materials.Values)
                {
                    for (int i = 0; i < r.materials.Length; i++)
                    {
                        if(r.materials[i].mainTexture != null)
                        {

                            // check there not the stock texs
                            byte[] bytes = null;
                            try
                            {
                              bytes = ImageConversion.EncodeToPNG((Texture2D)r.materials[i].mainTexture);
                            }
                            catch (System.Exception)
                            {

                            }

                            if(bytes != null)
                            {
                           
                             string Unicode = ConvertToUnicode(r.materials[i].mainTexture.name);
                                // find actual file

                                foreach(FileInfo file in new DirectoryInfo(TexturesDir).GetFiles("*.*", SearchOption.AllDirectories))
                                {
                                    if(file.Name == r.materials[i].mainTexture.name)
                                    {
                                      list.Add(new TextureInfo(Unicode, r.gameObject.name, false, i,file.DirectoryName));

                                    }
                                }




                            }
                            else
                            {
                                list.Add(new TextureInfo("stock", r.gameObject.name, false, i,"none"));
                            }

                        }
                        else
                        {
                            list.Add(new TextureInfo("e", r.gameObject.name, false, i,"none"));
                        }


                    }
                    
                }
               
                gear.RiderTextures = list;
                gear.isRiderUpdate = true;
                gear.Capisforward = CharacterModding.instance.CapisForward;

            }
            else
            {
                try
                {
               
                string preset = PlayerPrefs.GetString("lastPreset");
                InGameUI.instance.currentGaragePreset = preset;
                SaveList List = GarageDeserialize(preset);
                BinaryFormatter bf = new BinaryFormatter();
                byte[] bytes;
                using(var ms = new MemoryStream())
                {
                    bf.Serialize(ms, List);
                    bytes = ms.ToArray();
                    gear.GarageSave = bytes;
                }

                gear.isRiderUpdate = false;
               

                }
                catch (System.Exception)
                {
                    InGameUI.instance.NewMessage(6, new TextMessage("Error Loading Garage Save", (int)MessageColourByNum.Server, 1));
                   
                }
            }


            return gear;

        }

        // Edited piece of code from TheGarage to just give me latest GarageSave without disturbing it
        public SaveList GarageDeserialize(string name)
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
        
        public void SendAllParts()
        {
            GearUpdate FullGear = new GearUpdate();

            // if daryien, add rider gear
            if(LocalPlayer.instance.RiderModelname == "Daryien")
            {
            FullGear = GetMyGear(true);
            }

            FullGear.GarageSave = GetMyGear(false).GarageSave;

            Debug.Log("Sending parts");
            ClientSend.SendAllParts(FullGear);

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
            GameObject daryien = Instantiate(DaryienClone);

            // make sure meshes are active, pipeworks PI keeps daryien but turns off all his meshes, then tracks new models to daryiens bones
            foreach (Transform t in daryien.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.SetActive(true);
                if (t.gameObject.name == "Daryen_Hair_Matt")
                {
                    Destroy((Object)t.gameObject);
                }
                if (t.gameObject.GetComponent<Rigidbody>())
                {
                    t.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                }

            }


            daryien.SetActive(true);
            DontDestroyOnLoad(daryien);

            return daryien;
         }

         public static GameObject GetNewBMX()
         {
            GameObject bmx = Instantiate(BmxClone);
            foreach (Transform t in bmx.GetComponentsInChildren<Transform>(true))
            {
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

        public static void DoGarageSetup(RemotePlayer player, byte[] _savelist)
        {
            Debug.Log("Garage Setup on remote bike..");
            SaveList list;
            // cast back to savelist
            using (var ms = new MemoryStream(_savelist))
            {
              list = new BinaryFormatter().Deserialize(ms) as SaveList;
                
            }
            RemoteLoadManager.instance.Load(player, list); // Where the magic happens
            Debug.Log("Garage Setup complete");
        }

        public static Texture2D GetTexture(string name,string directory)
        {
            Texture2D image = new Texture2D(2,2);

            int pdata = directory.ToLower().LastIndexOf("pipe_data");
            string path = Application.dataPath + directory.Remove(0, pdata + 9);


            foreach (FileInfo _file in new DirectoryInfo(path).GetFiles(name, SearchOption.TopDirectoryOnly))
            {
                if(_file.Name == name)
                {
                   byte[] file = File.ReadAllBytes(_file.FullName);
                   ImageConversion.LoadImage(image,file);
                   image.name = name;

                }
            }




            return image;
        }

        public void TurnOnNetUpdates()
        {
            ClientSend.TurnMeOn();
            LocalPlayer.instance.ServerActive = true;

            foreach (RemotePlayer player in Players.Values)
            {
                player.MasterActive = true;
            }

        }
        public void TurnOffNetUpdates()
        {
            ClientSend.TurnMeOff();
            LocalPlayer.instance.ServerActive = false;
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
                ClientSend.KeepActive();
                KeepAlivetimer = 0;
            }

        }

        public void CleanUpOldPlayer(uint _id)
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
        public void UnLoadOtherPlayerModels()
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
            }

            if (!found)
            {
                InGameUI.instance.NewMessage(10, new TextMessage("Cant match that Map name, Check Patcha", (int)MessageColourByNum.System, 1));
            }


        }

        public int RidersOnMyMap()
        {
            int count = 0;
            foreach(RemotePlayer player in Players.Values)
            {
                if(player.CurrentMap == MycurrentLevel)
                {
                    count++;
                }
            }
            return count;
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

        public void UpdatePlayersOnMyLevelToggledOff()
        {
            List<uint> ridersToBeAwareOf = new List<uint>();
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

        public static string ConvertToUnicode(string text)
        {
            Encoding Uni = Encoding.Unicode;
            string outstring = Uni.GetString(Uni.GetBytes(text));


           outstring = outstring.Trim(Path.GetInvalidPathChars());
           outstring = outstring.Trim(Path.GetInvalidFileNameChars());
            return outstring;
        }

        // A non-Monobehaviour wants to use a Mono function
        public void DestroyObj(Object g)
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
            if (InGameUI.instance.Connected && LocalPlayer.instance.ServerActive)
            {
             StartCoroutine(EnumDontWipeOutPlayersOnReset());
            }
        }
        public IEnumerator EnumDontWipeOutPlayersOnReset()
        {
            LocalPlayer.instance.ServerActive = false;
            yield return new WaitForSeconds(0.5f);
            LocalPlayer.instance.ServerActive = true;
           
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

    }




   


}