using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System;

namespace PIPE_Valve_Console_Client
{
    public class InGameUI : MonoBehaviour
    {
        public static InGameUI instance;
        public LocalPlayer _localplayer;


        public string Username = "Username...";
        
        public string Key;
        public string IV;
        /// <summary>
        /// In Online mode
        /// </summary>
        public bool Connected;
        public bool OfflineMenu;
        private bool OnlineMenu;
        private bool P2PMenu;
        public string desiredport = "7777";

        public int messagetimer;
        public List<TextMessage> Messages = new List<TextMessage>();
        public string Messagetosend = "Send a message to all...";
        Dictionary<int, Color> MessageColour;


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
            MessageColour = new Dictionary<int, Color>()
            {
                {(int)MessageCode.Me,Color.blue},
                {(int)MessageCode.Player,Color.black},
                {(int)MessageCode.System,Color.yellow},
                {(int)MessageCode.Server,Color.red},
            };
           _localplayer = gameObject.GetComponent<LocalPlayer>();
           

        }


        /// <summary>
        /// Master Call to connect
        /// </summary>
        public void ConnectToServer()
        {
           
            GameNetworking.instance.ConnectMaster();
            Connected = true;
           // DeleteMessageLoop();
        }


        /// <summary>
        /// shuts down and cleans up
        /// </summary>
        public void Disconnect()
        {
            GameNetworking.instance.DisconnectMaster();
           
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
            
          
            if (OfflineMenu && !OnlineMenu)
            {
                ClientsOfflineMenu();
            }
            if (!OfflineMenu && OnlineMenu)
            {
                ClientsOnlineMenu();
            }

           


        }
        

        
       


    

        void ClientsOfflineMenu()
        {
            
            GUILayout.Label("Client Mode");




            GUILayout.Space(10);
            // setup stuff before connecting
            Username = GUILayout.TextField(Username);
            GUILayout.Space(10);
            GameNetworking.instance.ip = GUILayout.TextField(GameNetworking.instance.ip);
            desiredport = GUILayout.TextField(desiredport);
            GameNetworking.instance.port = int.Parse(desiredport);
            GUILayout.Space(5);

            if (GUILayout.Button("Connect to Server"))
            {
                InGameUI.instance.NewMessage(Constants.SystemMessage, new TextMessage("Trying Setup..", 1, 0));
                // just detects if ridermodel has changed from daryien and if so realigns to be tracking new rig
                _localplayer.RiderTrackingSetup();
            // do Grabtextures to get list of materials main texture names, server will ask for them when it detects you are daryien
            _localplayer.GrabTextures();
                ConnectToServer();
                OnlineMenu = true;
                OfflineMenu = false;
               
            }


            P2PMenu = GUILayout.Toggle(P2PMenu, "P2P Menu");
            if (P2PMenu)
            {
                GUILayout.Label("P2P Menu");
                GUILayout.Space(10);

                GUILayout.Label("Generate Secure Key");

                GUILayout.Space(10);
            }

           
            GUILayout.Space(10);
            if (GUILayout.Button("Exit"))
            {
                OfflineMenu = false;
                //GetComponent<LocalPlayer>().enabled = false;
                //GetComponent<InGameUI>().enabled = false;
            }
            GUILayout.Space(20);
           
            
        }

        void ClientsOnlineMenu()
        {
            Messagetosend = GUILayout.TextField(Messagetosend.ToString());
           if( GUILayout.Button("Send"))
            {
                if(Messagetosend != null)
                {
                ClientSend.SendTextMessage(Messagetosend.ToString());
                    Messagetosend = "";

                }
            }
            GUILayout.Space(20);
            GUILayout.Label("Messages:");
            foreach(TextMessage mess in Messages)
            {
                var style = new GUIStyle(GUI.skin.label);
                style.normal.textColor = MessageColour[mess.FromCode];
                GUILayout.Label(mess.Message,style);
            }


            if (GUILayout.Button("Disconnect"))
            {
                OnlineMenu = false;
                OfflineMenu = true;
                Disconnect();
                Connected = false;
            }
            GUILayout.Space(20);
        }


     
        /*
        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new System.Exception("plainText");
            if (Key == null || Key.Length <= 0)
                throw new System.Exception("Key");
            if (IV == null || IV.Length <= 0)
                throw new System.Exception("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        */


        /// <summary>
        /// Incoming messages get different colors
        /// </summary>
        enum MessageCode
        {
            System = 1,
            Me = 2,
            Player = 3,
            Server = 4,
        }







        public void NewMessage(int _time, TextMessage message)
        {
            StartCoroutine(MessageEnum(_time, message));
        }
        private IEnumerator MessageEnum(int _time, TextMessage message)
        {
            InGameUI.instance.Messages.Add(message);
            yield return new WaitForSeconds(_time);
            InGameUI.instance.Messages.Remove(message);
            yield return null;
        }

    }
}