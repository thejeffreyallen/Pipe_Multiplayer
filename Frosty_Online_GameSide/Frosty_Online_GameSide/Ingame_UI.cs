using UnityEngine;

namespace Frosty_Online_GameSide
{
    class Ingame_UI : MonoBehaviour
    {
        public static Ingame_UI instance;
        public LocalPlayer _localplayer;


        public string Username = "Username...";
        
        public bool Connected;
        public bool Hosting;
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
               // _localplayer.InitialiseLocalRider();
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
            if(!Hosting && !Connected)
            {
                HomeScreen();
            }

            if (Hosting && !Connected)
            {
               HostsGUI();
            }

            if (!Hosting && Connected)
            {
                ClientsGUI();
            }
        }



        void HomeScreen()
        {

           Username = GUILayout.TextField(Username);
            GUILayout.Space(10);
            Client.instance.ip = GUILayout.TextField(Client.instance.ip);
            desiredport = GUILayout.TextField(desiredport);
            Client.instance.port = int.Parse(desiredport);
            GUILayout.Space(5);
            if (GUILayout.Button("Connect To Host"))
            {
                _localplayer.RiderTracking();
                ConnectToServer();
                Connected = true;

            }
           

        }


        void HostsGUI()
        {
            if (GUILayout.Button("Disconnect"))
            {
                Disconnect();
            }
        }

        void ClientsGUI()
        {
            GUILayout.Label(lastmsgfromServer);

           // GUILayout.Label(UnityEngine.Component.FindObjectsOfType<RemotePlayer>().Length.ToString() + " Players Out");

            if (GUILayout.Button("Disconnect"))
            {
                Disconnect();
            }
        }

        

    }
}
