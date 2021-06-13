using System.Collections.Generic;
using UnityEngine;
using System.IO;
using FrostyP_Game_Manager;

namespace PIPE_Valve_Console_Client
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        private CharacterModding _charactermod;
        

        public LocalPlayer _localplayer;
        public string MycurrentLevel = "Unknown";
        public uint MyPlayerId;
        public bool firstMap = true;
        public static Dictionary<uint, RemotePlayer> Players;
        public static Dictionary<uint, List<Vector3>> PlayersColours;
        public static Dictionary<uint, List<float>> PlayersSmooths;
        public static Dictionary<uint, List<float>> PlayersMetals;
        public static Dictionary<uint, List<TextureInfo>> BikeTexinfos;
        public static Dictionary<uint, List<TextureInfo>> Bikenormalinfos;
        public static Dictionary<uint, List<TextureInfo>> RiderTexinfos;

        public static Dictionary<uint, List<FrostyP_Game_Manager.NetGameObject>> PlayersObjects;

      

        // FrostyMultiplayerAssets Bundle
        GameObject Prefab;
        public GameObject wheelcolliderobj;
        public AssetBundle FrostyAssets;
        //public GameObject FrostyCanvas;
      



        // found riders in Custom Players
        public string riderdirectory = Application.dataPath + "/Custom Players/";
        public string TexturesRootdir = Application.dataPath + "/FrostyPGameManager/Textures/";

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


           
          
        }

        // Use this for initialization
        void Start()
        {
            _charactermod = gameObject.GetComponent<CharacterModding>();
           
            _localplayer = gameObject.GetComponent<LocalPlayer>();


            Players = new Dictionary<uint, RemotePlayer>();
            PlayersColours = new Dictionary<uint, List<Vector3>>();
            PlayersSmooths = new Dictionary<uint, List<float>>();
            PlayersMetals = new Dictionary<uint, List<float>>();
            BikeTexinfos = new Dictionary<uint, List<TextureInfo>>();
            Bikenormalinfos = new Dictionary<uint, List<TextureInfo>>();
            RiderTexinfos = new Dictionary<uint, List<TextureInfo>>();
            PlayersObjects = new Dictionary<uint, List<FrostyP_Game_Manager.NetGameObject>>();





            FrostyAssets = AssetBundle.LoadFromFile(Application.dataPath + "/FrostyPGameManager/FrostyMultiPlayerAssets");
            Prefab = FrostyAssets.LoadAsset("PlayerPrefab") as GameObject;
            Prefab.AddComponent<RemotePlayer>();
            wheelcolliderobj = FrostyAssets.LoadAsset("WheelCollider") as GameObject;
           // FrostyCanvas = Instantiate(FrostyAssets.LoadAsset("FrostyCanvas") as GameObject);
          
        }

        public Dictionary<uint, RemotePlayer> GetPlayers() {
            return Players;
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
                }
                firstMap = false;
            }
            catch (UnityException)
            {
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Couldnt grab Current Scene name", (int)MessageColour.Server, 1));
            }


        }


        public void SpawnRider(uint _id, string _username, string currentmodel,string modelbundlename, Vector3 _position, Vector3 _rotation, List<TextureInfo> BmxInfos, List<Vector3> Bikecolours, List<float> bikesmooths,List<float> bikemetts, string Currentmap, List<TextureInfo> Riderinfos, List<TextureInfo> Bmxnormalinfos)
        {
            Debug.Log($"Spawning : {_username} as {currentmodel}, bundlename: {modelbundlename}, bmxinfos count: {BmxInfos.Count}, Riderinfos count: {Riderinfos.Count}, Id: {_id}, bmxnormalcount: {Bmxnormalinfos.Count}");

            foreach(RemotePlayer player in Players.Values)
            {
               if(player.id == _id)
                {
                    // player already exists, clean out first
                    CleanUpOldPlayer(_id);

                }
            }
            



                    GameObject New = GameObject.Instantiate(Prefab);
                    RemotePlayer r = New.GetComponent<RemotePlayer>();
                    Players.Add(_id, r);
                    
                    r.CurrentModelName = currentmodel;
                    New.name = _username + _id.ToString();
                    r.Modelbundlename = modelbundlename;
                    r.id = _id;
                    r.username = _username;
                    r.CurrentMap = Currentmap;



                    PlayersColours.Add(_id, Bikecolours);
                    PlayersSmooths.Add(_id, bikesmooths);
                    BikeTexinfos.Add(_id,BmxInfos);
                    Bikenormalinfos.Add(_id, Bmxnormalinfos);
                    RiderTexinfos.Add(_id, Riderinfos);
                    PlayersMetals.Add(_id, bikemetts);
                    PlayersObjects.Add(_id, new List<NetGameObject>());

           

                    DontDestroyOnLoad(New);
                    Debug.Log("Spawn finished");

        }



        public void SpawnObject(uint OwnerID, NetGameObject _netobj)
        {
           // check object doesnt exist already using objects unique id

            if(PlayersObjects[OwnerID] != null)
            {
            foreach(NetGameObject n in PlayersObjects[OwnerID])
            {
                if(n.ObjectID == _netobj.ObjectID)
                {
                        InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"Duplicate spawn attempt {_netobj.NameofObject} from package {_netobj.NameOfFile} for {Players[OwnerID].username}, object wont be spawned", (int)MessageColour.Server, 0));
                        return;
                }
            }

            }

            // check if objects bundle is loaded on this machine, if not look for filename and load bundle, if not, tell user what they need
            
            foreach(AssetBundle A in AssetBundle.GetAllLoadedAssetBundles())
            {
                if(A.name == _netobj.NameofAssetBundle)
                {
                    GameObject _newobj = Instantiate(A.LoadAsset(_netobj.NameofObject)) as GameObject;
                    _newobj.transform.position = _netobj.Position;
                    _newobj.transform.eulerAngles = _netobj.Rotation;
                    
                    _netobj._Gameobject = _newobj;
                    _netobj.AssetBundle = A;
                    DontDestroyOnLoad(_newobj);
                    GameManager.PlayersObjects[OwnerID].Add(_netobj);
                    return;
                }
            }

            foreach(FileInfo file in new DirectoryInfo(ParkBuilder.instance.AssetbundlesDirectory).GetFiles())
            {
                if(file.Name == _netobj.NameOfFile)
                {
                   AssetBundle newbundle = AssetBundle.LoadFromFile(file.FullName);
                    GameObject _newobj = Instantiate(newbundle.LoadAsset(_netobj.NameofObject)) as GameObject;

                    _newobj.transform.position = _netobj.Position;
                    _newobj.transform.eulerAngles = _netobj.Rotation;

                    _netobj._Gameobject = _newobj;
                    _netobj.AssetBundle = newbundle;
                    DontDestroyOnLoad(_newobj);
                    GameManager.PlayersObjects[OwnerID].Add(_netobj);
                    ParkBuilder.instance.bundlesloaded.Add(new BundleData(newbundle, file.Name));
                    return;
                }
            }

            // failed to find object, inform user what package is missing and clean up, resolve with server
            InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"Failed to spawn object {_netobj.NameofObject} from package {_netobj.NameOfFile} for {Players[OwnerID].username}, object wont be spawned", (int)MessageColour.Server, 0));


        }


        public void MoveObject(NetGameObject _netobj, Vector3 pos, Vector3 rot, Vector3 scale)
        {
            GameObject Obj = _netobj._Gameobject;

            Obj.transform.position = Vector3.MoveTowards(Obj.transform.position, pos, Vector3.Distance(Obj.transform.position, pos));
            Obj.transform.eulerAngles = Vector3.MoveTowards(Obj.transform.eulerAngles, rot, Vector3.Distance(Obj.transform.eulerAngles, rot));
            Obj.transform.localScale = Vector3.MoveTowards(Obj.transform.localScale, scale, Vector3.Distance(Obj.transform.localScale, scale));

        }


        /// <summary>
        /// Sent Once to server, immediatley after receiving Welcome, the server receiving this is packet is what triggers a sendtoall of your data and sendtoyou of everyones data
        /// </summary>
        public void SendAllParts()
        {
            List<TextureInfo> TexnamesBike = new List<TextureInfo>();
            List<TextureInfo> TexnormalsBike = new List<TextureInfo>();
            List<Vector3> Bikecolours = new List<Vector3>();
            List<float> BikeSmooths = new List<float>();
            List<float> BikeMetallics = new List<float>();
            Bikecolours.Add(BMXNetLoadout.instance.FrameColour);
            Bikecolours.Add(BMXNetLoadout.instance.ForksColour);
            Bikecolours.Add(BMXNetLoadout.instance.BarsColour);
            Bikecolours.Add(BMXNetLoadout.instance.SeatColour);
            Bikecolours.Add(BMXNetLoadout.instance.FTireColour);
            Bikecolours.Add(BMXNetLoadout.instance.FTireSideColour);
            Bikecolours.Add(BMXNetLoadout.instance.RTireColour);
            Bikecolours.Add(BMXNetLoadout.instance.RTireSideColour);
            Bikecolours.Add(BMXNetLoadout.instance.StemColour);
            Bikecolours.Add(BMXNetLoadout.instance.FRimColour);
            Bikecolours.Add(BMXNetLoadout.instance.RRimColour);



            BikeSmooths.Add(BMXNetLoadout.instance.FrameSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.ForksSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.BarsSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.SeatSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.StemSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.FRimSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.RRimSmooth);

            BikeMetallics.Add(BMXNetLoadout.instance.FrameMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.ForksMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.BarsMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.StemMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.FRimMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.RRimMetallic);

            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.FrameTexname, "Frame Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.ForkTexname, "Forks Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.BarTexName, "Bars Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.TireTexName, "Tire Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.SeatTexname, "Seat Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.StemTexName, "Stem Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.FRimTexName, "Front Rim"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.RRimTexName, "Rear Rim"));

            
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.FrameNormalName, "Frame Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.ForksNormalName, "Forks Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.BarsNormalName, "Bars Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.StemNormalName, "Stem Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.SeatNormalName, "Seat Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.FRimNormalName, "FRim Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.RRimNormalName, "RRim Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.TireNormalName, "Tires Normal"));



            List<TextureInfo> TexnamesRider = new List<TextureInfo>();

            if(_localplayer.RiderModelname == "Daryien")
            {

            foreach (Renderer n in CharacterModding.instance.Rider_Materials.Values)
            {
                if (n.gameObject.name == "body_geo")
                {
                    TexnamesRider.Add(new TextureInfo(n.materials[0].mainTexture.name, "Daryien_Head"));
                    TexnamesRider.Add(new TextureInfo(n.materials[1].mainTexture.name, "Daryien_Body"));
                    TexnamesRider.Add(new TextureInfo(n.materials[2].mainTexture.name, "Daryien_HandsFeet"));

                }
                else
                {
                    TexnamesRider.Add(new TextureInfo(n.material.mainTexture.name, n.gameObject.name));

                }
            }
            foreach (TextureInfo i in TexnamesRider)
            {
                Debug.Log($"{i.Nameoftexture} image and {i.NameofparentGameObject} coupled");
            }
            }


            ClientSend.SendAllParts(Bikecolours,BikeSmooths, TexnamesBike,TexnamesRider,BikeMetallics, TexnormalsBike);

        }






      

        public void SendQuickBikeUpdate()
        {
            Debug.Log("Sending quick bike Update");
            List<TextureInfo> TexnamesBike = new List<TextureInfo>();
            List<TextureInfo> TexnormalsBike = new List<TextureInfo>();
            List<Vector3> Bikecolours = new List<Vector3>();
            List<float> BikeSmooths = new List<float>();
            List<float> BikeMetallics = new List<float>();
            Bikecolours.Add(BMXNetLoadout.instance.FrameColour);
            Bikecolours.Add(BMXNetLoadout.instance.ForksColour);
            Bikecolours.Add(BMXNetLoadout.instance.BarsColour);
            Bikecolours.Add(BMXNetLoadout.instance.SeatColour);
            Bikecolours.Add(BMXNetLoadout.instance.FTireColour);
            Bikecolours.Add(BMXNetLoadout.instance.FTireSideColour);
            Bikecolours.Add(BMXNetLoadout.instance.RTireColour);
            Bikecolours.Add(BMXNetLoadout.instance.RTireSideColour);
            Bikecolours.Add(BMXNetLoadout.instance.StemColour);
            Bikecolours.Add(BMXNetLoadout.instance.FRimColour);
            Bikecolours.Add(BMXNetLoadout.instance.RRimColour);



            BikeSmooths.Add(BMXNetLoadout.instance.FrameSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.ForksSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.BarsSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.SeatSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.StemSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.FRimSmooth);
            BikeSmooths.Add(BMXNetLoadout.instance.RRimSmooth);

            BikeMetallics.Add(BMXNetLoadout.instance.FrameMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.ForksMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.BarsMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.StemMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.FRimMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.RRimMetallic);

            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.FrameTexname, "Frame Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.ForkTexname, "Forks Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.BarTexName, "Bars Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.TireTexName, "Tire Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.SeatTexname, "Seat Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.StemTexName, "Stem Mesh"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.FRimTexName, "Front Rim"));
            TexnamesBike.Add(new TextureInfo(BMXNetLoadout.instance.RRimTexName, "Rear Rim"));


            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.FrameNormalName, "Frame Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.ForksNormalName, "Forks Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.BarsNormalName, "Bars Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.StemNormalName, "Stem Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.SeatNormalName, "Seat Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.FRimNormalName, "FRim Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.RRimNormalName, "RRim Normal"));
            TexnormalsBike.Add(new TextureInfo(BMXNetLoadout.instance.TireNormalName, "Tires Normal"));



            ClientSend.SendQuickBikeUpdate(Bikecolours, BikeSmooths,BikeMetallics, TexnamesBike,TexnormalsBike);
            Debug.Log("Sent quick bike update");
        }

        public void SendQuickRiderUpdate()
        {
           
            List<TextureInfo> texnames = new List<TextureInfo>();

            Debug.Log("Sending quick rider update");
           
            foreach(Renderer n in CharacterModding.instance.Rider_Materials.Values)
            {
                if(n.gameObject.name == "body_geo")
                {
                    texnames.Add(new TextureInfo(n.materials[0].mainTexture.name, "Daryien_Head"));
                    texnames.Add(new TextureInfo(n.materials[1].mainTexture.name, "Daryien_Body"));
                    texnames.Add(new TextureInfo(n.materials[2].mainTexture.name, "Daryien_HandsFeet"));
                
                }
                else
                {
                texnames.Add(new TextureInfo(n.material.mainTexture.name, n.gameObject.name));

                }
            }
            foreach(TextureInfo i in texnames)
            {
                 Debug.Log($"{i.Nameoftexture} image and {i.NameofparentGameObject} coupled");
            }

            ClientSend.SendQuickRiderUpdate(texnames);
            Debug.Log("quickriderupdate sent");
        }




        public void CleanUpOldPlayer(uint _id)
        {
            try
            {
                if(Players[_id].RiderModel != null)
                {
                    Destroy(Players[_id].RiderModel);
                }
                if (Players[_id].Audio != null)
                {
                    Destroy(Players[_id].Audio);
                }
                if (Players[_id].BMX != null)
                {
                    Destroy(Players[_id].BMX);
                }
                if (Players[_id].nameSign != null)
                {
                    Destroy(Players[_id].nameSign);
                }
                    Destroy(Players[_id].gameObject);

                if (PlayersColours[_id] != null)
                {
                    PlayersColours.Remove(_id);
                }
                if (PlayersSmooths[_id] != null)
                {
                   PlayersSmooths.Remove(_id);
                }
                if (PlayersMetals[_id] != null)
                {
                    PlayersMetals.Remove(_id);
                }
                if (RiderTexinfos[_id] != null)
                {
                    RiderTexinfos.Remove(_id);
                }
                if (BikeTexinfos[_id] != null)
                {
                    BikeTexinfos.Remove(_id);
                }
                if (Bikenormalinfos[_id] != null)
                {
                    Bikenormalinfos.Remove(_id);
                }
                if (PlayersObjects[_id].Count > 0)
                {
                    foreach (NetGameObject n in PlayersObjects[_id])
                    {
                        if (n._Gameobject != null)
                        {
                            Destroy(n._Gameobject);
                        }
                    }

                }
                if (PlayersObjects[_id] != null)
                {
                    PlayersObjects.Remove(_id);
                }

            }
            catch (System.Exception x)
            {

            }



        }
        


       
       
    }
}