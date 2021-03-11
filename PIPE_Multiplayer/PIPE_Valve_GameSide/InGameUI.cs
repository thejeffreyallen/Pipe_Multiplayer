using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace PIPE_Valve_Console_Client
{
    public class InGameUI : MonoBehaviour
    {
        public static InGameUI instance;
        public LocalPlayer _localplayer;


        public string Username = "Username...";

        public bool Connected;
        public string desiredport = "7777";

        
        public List<string> Messages = new List<string>();




        /// <summary>
        /// just checks that theres only one instance and this is it
        /// </summary>
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


        /// <summary>
        /// Grabs anything that wont change
        /// </summary>
        private void Start()
        {
           _localplayer = gameObject.GetComponent<LocalPlayer>();

        }


        /// <summary>
        /// Master Call to connect
        /// </summary>
        public void ConnectToServer()
        {
            GameNetworking.instance.ConnectToServer();

        }


        /// <summary>
        /// shuts down and cleans up
        /// </summary>
        public void Disconnect()
        {
            GameNetworking.instance.DisconnectMaster();
            Connected = false;
            foreach (RemotePlayer r in GameManager.Players.Values)
            {
                Destroy(r.RiderModel);
                Destroy(r.BMX);
                Destroy(r.gameObject);

            }
            GameManager.Players.Clear();
            
           
            // Server learns of disconnection itself and tells everyone

        }
        

       
        void OnGUI()
        {
            if (!Connected)
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
            GameNetworking.instance.ip = GUILayout.TextField(GameNetworking.instance.ip);
            desiredport = GUILayout.TextField(desiredport);
            GameNetworking.instance.port = int.Parse(desiredport);
            GUILayout.Space(5);


            // when connect
            if (GUILayout.Button("Connect To Host"))
            {
                // just detects if ridermodel has changed from daryien and if so realigns to be tracking new rig
                GameManager.instance._localplayer.RiderTrackingSetup();

                // if model is daryien, do Grabtextures to get list of materials main texture names, server will ask for them when it detects you are daryien
                if (GameManager.instance._localplayer.RiderModelname == "Daryien")
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
           

            // GUILayout.Label(UnityEngine.Component.FindObjectsOfType<RemotePlayer>().Length.ToString() + " Players Out");
            GUILayout.Space(10);
            if (GUILayout.Button("Disconnect"))
            {
                Disconnect();
            }
            GUILayout.Space(20);
            // toggleable message window here

            foreach (string message in Messages)
            {
                
                GUILayout.Label(message);
            }

        }


       
    }
}