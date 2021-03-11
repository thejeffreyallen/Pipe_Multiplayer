using UnityEngine;

namespace Frosty_Online_GameSide
{
    class Ingame_UI : MonoBehaviour
    {
        public static Ingame_UI instance;
        public LocalPlayer _localplayer;


        public string Username = "Username...";
        
        public bool Connected;
        public string desiredport = "7777";
        
        public string lastmsgfromServer = "Waiting for Server..";
        




        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("Client already exists, destroying old client now");
               Destroy(this);
            }
        }


        private void Start()
        {
            _localplayer = gameObject.GetComponent<LocalPlayer>();
               
        }



        public void ConnectToServer()
        {
            Client.instance.ConnectToServer();
        }

        public void Disconnect()
        {

            Connected = false;
            foreach(RemotePlayer r in GameManager.players.Values)
            {
                Destroy(r.RiderModel);
                Destroy(r.BMX);
                Destroy(r.gameObject);
                
            }
            GameManager.players.Clear();

            Client.instance.Disconnect();
            // Server learns of disconnection itself and tells everyone

        }



        void OnGUI()
        {
            if(!Connected)
            {
                HomeScreen();
            }


            if (Connected)
            {
                ClientsGUI();
            }
        }



        void HomeScreen()
        {
            // setup stuff before connecting
           Username = GUILayout.TextField(Username);
            GUILayout.Space(10);
            Client.instance.ip = GUILayout.TextField(Client.instance.ip);
            desiredport = GUILayout.TextField(desiredport);
            Client.instance.port = int.Parse(desiredport);
            GUILayout.Space(5);


            // when connect
            if (GUILayout.Button("Connect To Host"))
            {
                // just detects if ridermodel has changed from daryien and if so realigns to be tracking new rig
               GameManager.instance._localplayer.RiderTrackingSetup();

                // if model is daryien, do Grabtextures to get list of materials main texture names, server will ask for them when it detects you are daryien
                if(GameManager.instance._localplayer.RiderModelname == "Daryien")
                {
                    _localplayer.GrabTextures();
                }

                ConnectToServer();
                Connected = true;

            }
           

        }


       

        void ClientsGUI()
        {
            GUILayout.Label("Online");
            GUILayout.Label("Server: " + lastmsgfromServer);
           
            // GUILayout.Label(UnityEngine.Component.FindObjectsOfType<RemotePlayer>().Length.ToString() + " Players Out");
            GUILayout.Space(10);
            if (GUILayout.Button("Disconnect"))
            {
                Disconnect();
            }
        }

        

    }
}
