using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace PIPE_Valve_Console_Client
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;
        private CharacterModding _charactermod;
        private BMXNetLoadout Netloadout;

        public LocalPlayer _localplayer;
        public uint MyPlayerId;
        public static Dictionary<uint, RemotePlayer> Players;
        public static Dictionary<uint, List<Vector3>> PlayersColours;
        public static Dictionary<uint, List<float>> PlayersSmooths;
        public static Dictionary<uint, List<TextureInfo>> PlayersTexinfos;


        GameObject Prefab;
        public GameObject wheelcolliderobj;

        public AssetBundle FrostyAssets;

        // found riders in Custom Players
        public string riderdirectory = Application.dataPath + "/Custom Players/";
        public string TexturesRootdir = Application.dataPath + "/FrostyPGameManager/Textures/";

        // Use this for initialization
        void Start()
        {
            _charactermod = gameObject.GetComponent<CharacterModding>();
            Netloadout = BMXNetLoadout.instance;
            Players = new Dictionary<uint, RemotePlayer>();
            PlayersColours = new Dictionary<uint, List<Vector3>>();
            PlayersSmooths = new Dictionary<uint, List<float>>();
            PlayersTexinfos = new Dictionary<uint, List<TextureInfo>>();
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





        public void SpawnOnMyGame(uint _id, string _username, string currentmodel,string modelbundlename, Vector3 _position, Vector3 _rotation, List<TextureInfo> infos, List<Vector3> colours, List<float> smooths)
        {
            // firstly transfer any data, ready for remote rider to pick up once created
            List<string> Unfound = new List<string>();
            List<TextureInfo> found = new List<TextureInfo>();
            DirectoryInfo dir = new DirectoryInfo(TexturesRootdir);
            DirectoryInfo[] subs = dir.GetDirectories();
            foreach (TextureInfo i in infos)
            {
                bool _found = false;
                foreach(DirectoryInfo d in subs)
                {
                    FileInfo[] files = d.GetFiles();
                    foreach(FileInfo f in files)
                    {
                        if(f.Name.Contains(i.Nameoftexture))
                        {
                            found.Add(i);
                            _found = true;
                        }
                       
                    }
                }
                    if (!_found)
                    {
                    Unfound.Add(i.Nameoftexture);
                    }

            }

            // for any unfound, put in a request to the server
            if (Unfound.Count != 0)
            {
               
                ClientSend.RequestTextures(Unfound);
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"Rider joined with {Unfound.Count} unknown Textures, requsted them from server", (int)MessageColour.Server, 1));

                // make a note of what were waiting for and who for
                foreach(string i in Unfound)
                {
                    GameData.requests.Add(new Waitingrequest(_id, _username, i));
                }
            }



            // for any found, store in players list of textureinfos for RemotePlayer to access
                if (found.Count != 0)
                {
                PlayersTexinfos.Add(_id, infos);
                
                }
           




            // finally intitialise a PlayerPrefab (empty object with RemotePlayer on that controls one rider that it creates)
                    GameObject New = GameObject.Instantiate(Prefab);
                    RemotePlayer r = New.GetComponent<RemotePlayer>();
                    Players.Add(_id, r);
                    New.name = _username + _id.ToString();
                    r.CurrentModelName = currentmodel;
                    r.Modelbundlename = modelbundlename;
                    r.id = _id;
                    r.username = _username;
                    r._texinfos = new List<TextureInfo>();
        }




        public void UpdateABikeData()
        {

        }



        /// <summary>
        /// Send bike data
        /// </summary>
        public void SendMyBikeToServer()
        {
           

            Debug.Log("Sending");
            List<string> Texturenames = new List<string>();
            List<Vector3> Vecs = new List<Vector3>();
            List<float> floats = new List<float>();
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

            Texturenames.Add(BMXNetLoadout.instance.FrameTexname);
            Texturenames.Add(BMXNetLoadout.instance.ForkTexname);
            Texturenames.Add(BMXNetLoadout.instance.BarTexName);
            Texturenames.Add(BMXNetLoadout.instance.TireTexName);
            Texturenames.Add(BMXNetLoadout.instance.TireNormalName);
            Texturenames.Add(BMXNetLoadout.instance.SeatTexname);



            ClientSend.SendBikeData(Vecs, floats, Texturenames);
            Debug.Log("Sent");
        }

        public void SendQuickBikeUpdate()
        {
            Debug.Log("Sending");
            List<string> Texturenames = new List<string>();
            List<Vector3> Vecs = new List<Vector3>();
            List<float> floats = new List<float>();
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

            Texturenames.Add(BMXNetLoadout.instance.FrameTexname);
            Texturenames.Add(BMXNetLoadout.instance.ForkTexname);
            Texturenames.Add(BMXNetLoadout.instance.BarTexName);
            Texturenames.Add(BMXNetLoadout.instance.TireTexName);
            Texturenames.Add(BMXNetLoadout.instance.TireNormalName);
            Texturenames.Add(BMXNetLoadout.instance.SeatTexname);



            ClientSend.SendQuickBikeUpdate(Vecs, floats, Texturenames);
            Debug.Log("Sent");
        }

        public void SendQuickRiderUpdate()
        {
            List<TextureInfo> texnames = new List<TextureInfo>();

            Debug.Log("Sending quick rider update");
            SkinnedMeshRenderer[] rends = _localplayer.Rider_Root.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach(SkinnedMeshRenderer n in rends)
            {
                texnames.Add(new TextureInfo(n.material.mainTexture.name, n.gameObject.name));
                Debug.Log(n.material.mainTexture.name + " material and " +  n.gameObject.name + " GO Added to quickriderupdate");
            }


            ClientSend.SendQuickRiderUpdate(texnames);

        }









        void Update()
        {
           

           

        }
    }
}