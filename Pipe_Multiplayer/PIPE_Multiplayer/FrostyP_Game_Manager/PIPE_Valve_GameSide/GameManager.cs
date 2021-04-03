using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace PIPE_Valve_Console_Client
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        private CharacterModding _charactermod;
        

        public LocalPlayer _localplayer;
        public string MycurrentLevel = "Unknown";
        public uint MyPlayerId;
        public static Dictionary<uint, RemotePlayer> Players;
        public static Dictionary<uint, List<Vector3>> PlayersColours;
        public static Dictionary<uint, List<float>> PlayersSmooths;
        public static Dictionary<uint, List<float>> PlayersMetals;
        public static Dictionary<uint, List<TextureInfo>> BikeTexinfos;
        public static Dictionary<uint, List<TextureInfo>> Bikenormalinfos;
        public static Dictionary<uint, List<TextureInfo>> RiderTexinfos;

        public List<string> Texturenames = new List<string>();
        public List<Vector3> Vecs = new List<Vector3>();
        public List<float> floats = new List<float>();


        GameObject Prefab;
        public GameObject wheelcolliderobj;

        public AssetBundle FrostyAssets;

        // found riders in Custom Players
        public string riderdirectory = Application.dataPath + "/Custom Players/";
        public string TexturesRootdir = Application.dataPath + "/FrostyPGameManager/Textures/";

        //patchaMapImporter
        GameObject patcha;
        PatchaMapImporter.PatchaMapImporter mapImporter;

        //initialize mapImporter
        public void Awake()
        {
            patcha = new GameObject();
            patcha.AddComponent<PatchaMapImporter.PatchaMapImporter>();
            mapImporter = patcha.GetComponent<PatchaMapImporter.PatchaMapImporter>();
        }

        // Use this for initialization
        void Start()
        {
            _charactermod = gameObject.GetComponent<CharacterModding>();
           
            Players = new Dictionary<uint, RemotePlayer>();
            PlayersColours = new Dictionary<uint, List<Vector3>>();
            PlayersSmooths = new Dictionary<uint, List<float>>();
            PlayersMetals = new Dictionary<uint, List<float>>();
            BikeTexinfos = new Dictionary<uint, List<TextureInfo>>();
            Bikenormalinfos = new Dictionary<uint, List<TextureInfo>>();
            RiderTexinfos = new Dictionary<uint, List<TextureInfo>>();
            FrostyAssets = AssetBundle.LoadFromFile(Application.dataPath + "/FrostyPGameManager/FrostyMultiPlayerAssets");
            Prefab = FrostyAssets.LoadAsset("PlayerPrefab") as GameObject;
            wheelcolliderobj = FrostyAssets.LoadAsset("WheelCollider") as GameObject;
            Prefab.AddComponent<RemotePlayer>();


            _localplayer = gameObject.GetComponent<LocalPlayer>();

            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("Instance already exists, destroying object!");
                Destroy(this);
            }

           
        }


        public void GetLevelName()
        {
            try
            {
                MycurrentLevel = string.IsNullOrEmpty(mapImporter.GetCurrentMapName()) ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().name : mapImporter.GetCurrentMapName(); // Use the actual map file name or, if on the unmodded maps, use the scene name
            }
            catch (UnityException)
            {
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Couldnt grab Current Scene name", (int)MessageColour.Server, 1));
            }
        }


        public void SpawnOnMyGame(uint _id, string _username, string currentmodel,string modelbundlename, Vector3 _position, Vector3 _rotation, List<TextureInfo> BmxInfos, List<Vector3> Bikecolours, List<float> bikesmooths,List<float> bikemetts, string Currentmap, List<TextureInfo> Riderinfos, List<TextureInfo> Bmxnormalinfos)
        {
            Debug.Log($"Spawning : {_username} as {currentmodel}, bundlename: {modelbundlename}, bmxinfos count: {BmxInfos.Count}, Riderinfos count: {Riderinfos.Count}, Id: {_id}, bmxnormalcount: {Bmxnormalinfos.Count}");
                    GameObject New = GameObject.Instantiate(Prefab);
                    RemotePlayer r = New.GetComponent<RemotePlayer>();
                    
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


                
            

                    DontDestroyOnLoad(New);
            Debug.Log("Spawn finished");

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
            List<TextureInfo> Texturenames = new List<TextureInfo>();
            List<Vector3> Vecs = new List<Vector3>();
            List<float> floats = new List<float>();
            List<float> BikeMetallics = new List<float>();
            Vecs.Add(BMXNetLoadout.instance.FrameColour);
            Vecs.Add(BMXNetLoadout.instance.ForksColour);
            Vecs.Add(BMXNetLoadout.instance.BarsColour);
            Vecs.Add(BMXNetLoadout.instance.SeatColour);
            Vecs.Add(BMXNetLoadout.instance.FTireColour);
            Vecs.Add(BMXNetLoadout.instance.FTireSideColour);
            Vecs.Add(BMXNetLoadout.instance.RTireColour);
            Vecs.Add(BMXNetLoadout.instance.RTireSideColour);

            floats.Add(BMXNetLoadout.instance.FrameSmooth);
            floats.Add(BMXNetLoadout.instance.ForksSmooth);
            floats.Add(BMXNetLoadout.instance.BarsSmooth);
            floats.Add(BMXNetLoadout.instance.SeatSmooth);
            BikeMetallics.Add(BMXNetLoadout.instance.FrameMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.ForksMetallic);
            BikeMetallics.Add(BMXNetLoadout.instance.BarsMetallic);

            Texturenames.Add(new TextureInfo(BMXNetLoadout.instance.FrameTexname, "Frame Mesh"));
            Texturenames.Add(new TextureInfo(BMXNetLoadout.instance.ForkTexname, "Forks Mesh"));
            Texturenames.Add(new TextureInfo(BMXNetLoadout.instance.BarTexName, "Bars Mesh"));
            Texturenames.Add(new TextureInfo(BMXNetLoadout.instance.TireTexName, "Tire Mesh"));
            Texturenames.Add(new TextureInfo(BMXNetLoadout.instance.TireNormalName, "Tire Normal"));
            Texturenames.Add(new TextureInfo(BMXNetLoadout.instance.SeatTexname, "Seat Mesh"));
            


            ClientSend.SendQuickBikeUpdate(Vecs, floats,BikeMetallics, Texturenames);
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









        void Update()
        {
           

           

        }
    }
}