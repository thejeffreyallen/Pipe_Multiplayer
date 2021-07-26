using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FrostyP_Game_Manager;
using System.Reflection;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace PIPE_Valve_Console_Client
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        public static GameObject DaryienClone;
        public static GameObject BmxClone;
        public static GameObject BMXSPlayer;

        public string MycurrentLevel = "Unknown";
        public uint MyPlayerId;
        public bool firstMap = true;
        public static Dictionary<uint, RemotePlayer> Players;
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
        public static string TempDir = Rootdir + "Temp/";
        public static string UpdateDir = Application.dataPath.Replace("PIPE_Data","") + "Mods/FrostyP Game Manager/Updates/";
        

        //patchaMapImporter
        GameObject patcha;
        PatchaMapImporter.PatchaMapImporter mapImporter;

        //initialize mapImporter
        public void Awake()
        {
            instance = this;
            patcha = new GameObject();
            patcha.AddComponent<PatchaMapImporter.PatchaMapImporter>();
            mapImporter = patcha.GetComponent<PatchaMapImporter.PatchaMapImporter>();

            Directory.CreateDirectory(UpdateDir);


            BMXSPlayer = GameObject.Find("BMXS Player Components");
            SetupDaryienAndBMXBaseModels();
          
        }

        // Use this for initialization
        void Start()
        {
            
            Players = new Dictionary<uint, RemotePlayer>();
           
            if (!Directory.Exists(TempDir))
            {
                Directory.CreateDirectory(TempDir);
            }


            FrostyAssets = AssetBundle.LoadFromFile(Application.dataPath + "/FrostyPGameManager/FrostyMultiPlayerAssets");
            Prefab = FrostyAssets.LoadAsset("PlayerPrefab") as GameObject;
            Prefab.AddComponent<RemotePlayer>();
            wheelcolliderobj = FrostyAssets.LoadAsset("WheelCollider") as GameObject;
          
        }

        public Dictionary<uint, RemotePlayer> GetPlayers() {
            return Players;
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

            foreach (Transform t in BmxClone.GetComponentsInChildren<Transform>())
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
                MycurrentLevel = string.IsNullOrEmpty(mapImporter.GetCurrentMapName()) ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().name : mapImporter.GetCurrentMapName(); // Use the actual map file name or, if on the unmodded maps, use the scene name
                if (!firstMap)
                {
                    ClientSend.SendMapName(GameManager.instance.MycurrentLevel);
                    InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Sent Map name", 1, 1));
                    ChangingLevel(MycurrentLevel);

                }
                firstMap = false;
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


            // file checks
            FileSyncing.CheckForMap(Currentmap, _username);
            for (int i = 0; i < Gear.RiderTextures.Count; i++)
            {
                if (!FileSyncing.CheckForFile(Gear.RiderTextures[i].Nameoftexture))
                {
                    FileSyncing.AddToRequestable(1, Gear.RiderTextures[i].Nameoftexture,_id);
                }
            }

            if(currentmodel != "Daryien")
            {
              if (!FileSyncing.CheckForFile(currentmodel))
              {
                FileSyncing.AddToRequestable(3,currentmodel, _id);
              }

            }
            




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
                    _netobj._Gameobject.SetActive(_player.PlayerObjectsVisible);
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

                    ParkBuilder.instance.bundlesloaded.Add(new BundleData(newbundle, file.Name));
                    return;
                }
            }

            // failed to find object, inform user what package is missing and clean up, resolve with server
            InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"Failed to find {_netobj.NameofObject} from {_netobj.NameOfFile} for {Players[_netobj.OwnerID].username}", (int)MessageColourByNum.Server, 0));
            if (!FileSyncing.CheckForFile(_netobj.NameOfFile))
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
                            list.Add(new TextureInfo(r.materials[i].mainTexture.name, r.gameObject.name, false, i));
                        }

                    }
                    
                   
                }
               
                gear.RiderTextures = list;
                gear.isRiderUpdate = true;

            }
            else
            {
                // grab garage list, convert to byte[] and place in gear update, send garagesave aswell as forRidermodel bool (false to inidcate bike update)
                SaveList List = GarageDeserialize(PlayerPrefs.GetString("lastPreset"));
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


            return daryien;
         }
         public static GameObject GetNewBMX()
         {
            GameObject bmx = Instantiate(BmxClone);
            foreach (Transform t in bmx.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.SetActive(true);
            }

            return bmx;

         }
        public static GameObject GetPlayerModel(string modelname, string modelbundlename)
        {
            

            IEnumerable<AssetBundle> bundles = AssetBundle.GetAllLoadedAssetBundles();
            foreach (AssetBundle a in bundles)
            {
                if (a.name.ToLower().Contains(modelbundlename.ToLower()))
                {
                    return Instantiate(a.LoadAsset(modelname)) as GameObject;

                }

            }


            if (File.Exists(PlayerModelsDir + modelname))
            {

                AssetBundle b = AssetBundle.LoadFromFile(PlayerModelsDir + modelname);


                return Instantiate(b.LoadAsset(modelname) as GameObject);
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

        public static Texture2D GetTexture(string name)
        {
            Texture2D image = new Texture2D(2,2);

            foreach(FileInfo _file in new DirectoryInfo(Rootdir).GetFiles(name, SearchOption.AllDirectories))
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
            if (KeepAlivetimer < 2)
            {
                KeepAlivetimer = KeepAlivetimer + Time.deltaTime;
            }
            else if (KeepAlivetimer >= 2)
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



        // A non-Monobehaviour wants to use a Mono function
        public void DestroyObj(Object g)
        {
            Destroy(g);
        }
        public void DontDestroy(GameObject g)
        {
            DontDestroyOnLoad(g);
        }


    }




   


}