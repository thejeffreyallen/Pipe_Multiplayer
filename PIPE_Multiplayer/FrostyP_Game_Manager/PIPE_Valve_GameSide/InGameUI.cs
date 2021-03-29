using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.IO;
using System.Threading;
using System;
using FrostyP_Game_Manager;
using UnityEngine.Rendering;



namespace PIPE_Valve_Console_Client
{
    public class InGameUI : MonoBehaviour
    {
        public static InGameUI instance;
        public LocalPlayer _localplayer;
       

        public GUISkin skin = (GUISkin)ScriptableObject.CreateInstance("GUISkin");
        public GUIStyle Generalstyle = new GUIStyle();

        public string Username = "Username...";
        
        public string Key;
        public string IV;
        /// <summary>
        /// In Online mode
        /// </summary>
        public bool Connected;
        public bool OfflineMenu = true;
        public bool OnlineMenu;
        public bool Minigui;
        public string desiredport = "7777";
        public string desiredIP = "127.0.0.1";

        public int messagetimer;
        public List<TextMessage> Messages = new List<TextMessage>();
        public string Messagetosend = "Send a message to all...";
        Dictionary<int, Color> MessageColour;

        Vector2 scrollPosition;
        Texture2D RedTex;
        Texture2D BlackTex;
        Texture2D GreyTex;
        Texture2D GreenTex;
        Texture2D whiteTex;
        Texture2D TransTex;

        


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
                Debug.Log("IngameUI already exists, destroying old InGameUI now");
                Destroy(this);
            }
        }


        /// <summary>
        /// Grabs anything that wont change
        /// </summary>
        public void Start()
        {
            

            RedTex = new Texture2D(Screen.width / 6, Screen.height / 4); ;
            Color[] colorarray = RedTex.GetPixels();
            Color newcolor = new Color(0.5f, 0, 0, 1);
            for (var i = 0; i < colorarray.Length; ++i)
            {
                colorarray[i] = newcolor;
            }

            RedTex.SetPixels(colorarray);

            RedTex.Apply();

            BlackTex = Texture2D.blackTexture;
            Color[] colorarray2 = BlackTex.GetPixels();
            Color newcolor2 = new Color(0, 0, 0, 0.4f);
            for (var i = 0; i < colorarray2.Length; ++i)
            {
                colorarray2[i] = newcolor2;
            }

            BlackTex.SetPixels(colorarray2);

            BlackTex.Apply();

            GreyTex = Texture2D.blackTexture;
            Color[] colorarray3 = GreyTex.GetPixels();
            Color newcolor3 = new Color(0.5f, 0.5f, 0.5f, 1);
            for (var i = 0; i < colorarray3.Length; ++i)
            {
                colorarray3[i] = newcolor3;
            }

            GreyTex.SetPixels(colorarray3);

            GreyTex.Apply();


            GreenTex = new Texture2D(20, 10);
            Color[] colorarray4 = GreenTex.GetPixels();
            Color newcolor4 = new Color(0.2f, 0.6f, 0.2f, 1f);
            for (var i = 0; i < colorarray4.Length; ++i)
            {
                colorarray4[i] = newcolor4;
            }

            GreenTex.SetPixels(colorarray4);

            GreenTex.Apply();



            whiteTex = new Texture2D(20, 10);
            Color[] colorarray5 = whiteTex.GetPixels();
            Color newcolor5 = new Color(1f, 1f, 1f, 0.3f);
            for (var i = 0; i < colorarray5.Length; ++i)
            {
                colorarray5[i] = newcolor5;
            }

            whiteTex.SetPixels(colorarray5);

            whiteTex.Apply();


            TransTex = new Texture2D(20, 10);
            Color[] colorarray6 = TransTex.GetPixels();
            Color newcolor6 = new Color(1f, 1f, 1f, 0f);
            for (var i = 0; i < colorarray6.Length; ++i)
            {
                colorarray6[i] = newcolor6;
            }

            TransTex.SetPixels(colorarray6);

            TransTex.Apply();

            Generalstyle.normal.background = whiteTex;
            Generalstyle.normal.textColor = Color.black;

            Generalstyle.alignment = TextAnchor.MiddleCenter;
            Generalstyle.fontStyle = FontStyle.Bold;
            //Generalstyle.fontSize = 16;
            //Generalstyle.border.left = 5;
            //Generalstyle.border.right = 5;
            //Generalstyle.margin.left = 5;
            //Generalstyle.margin.right = 5;

            skin.label.normal.textColor = Color.black;
            skin.label.fontSize = 15;
            skin.label.fontStyle = FontStyle.Bold;
            skin.label.alignment = TextAnchor.MiddleCenter;
            skin.label.normal.background = TransTex;



            skin.textField.alignment = TextAnchor.MiddleCenter;
            skin.textField.normal.textColor = Color.red;
            skin.textField.normal.background = GreyTex;
            skin.textField.focused.background = BlackTex;
            skin.textField.focused.textColor = Color.white;
            skin.textField.font = Font.CreateDynamicFontFromOSFont("Arial", 14);
            skin.textField.padding = new RectOffset(10, 10, 10, 10);



            skin.button.normal.textColor = Color.black;
            skin.button.alignment = TextAnchor.MiddleCenter;
            skin.button.normal.background = GreenTex;
            skin.button.onNormal.background = GreyTex;
            skin.button.onNormal.textColor = Color.red;
            skin.button.onHover.background = GreenTex;
            skin.button.hover.textColor = Color.green;
            skin.button.normal.background.wrapMode = TextureWrapMode.Clamp;
            skin.button.hover.background = GreyTex;



            skin.toggle.normal.textColor = Color.black;
            skin.toggle.alignment = TextAnchor.MiddleCenter;
            skin.toggle.normal.background = GreenTex;
            skin.toggle.onNormal.background = GreyTex;
            skin.toggle.onNormal.textColor = Color.black;
            skin.toggle.onHover.background = GreenTex;
            skin.toggle.hover.textColor = Color.green;
            skin.toggle.normal.background.wrapMode = TextureWrapMode.Clamp;
            skin.toggle.hover.background = GreyTex;


            skin.horizontalSlider.alignment = TextAnchor.MiddleCenter;
            skin.horizontalSlider.normal.textColor = Color.black;
            skin.horizontalSlider.normal.background = GreyTex;
            skin.horizontalSliderThumb.normal.background = GreenTex;
            skin.horizontalSliderThumb.normal.background.wrapMode = TextureWrapMode.Clamp;
            skin.horizontalSliderThumb.normal.textColor = Color.white;
            skin.horizontalSliderThumb.fixedWidth = 20;
            skin.horizontalSliderThumb.fixedHeight = 20;
            skin.horizontalSliderThumb.hover.background = BlackTex;

            skin.button.normal.textColor = Color.black;
            skin.button.alignment = TextAnchor.MiddleCenter;
            skin.scrollView.normal.background = GreenTex;
            skin.verticalScrollbarThumb.normal.background = GreenTex;
            skin.scrollView.alignment = TextAnchor.MiddleCenter;
            skin.scrollView.fixedWidth = Screen.width / 4;



            

            MessageColour = new Dictionary<int, Color>()
            {
                {(int)PIPE_Valve_Console_Client.MessageColour.Me,Color.blue},
                {(int)PIPE_Valve_Console_Client.MessageColour.Player,Color.black},
                {(int)PIPE_Valve_Console_Client.MessageColour.System,Color.green},
                {(int)PIPE_Valve_Console_Client.MessageColour.Server,Color.red},
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
          
        }


        /// <summary>
        /// shuts down and cleans up
        /// </summary>
        public void Disconnect()
        {
            Connected = false;
            List<GameObject> objs = new List<GameObject>();
            GameNetworking.instance.DisconnectMaster();
           
            foreach (RemotePlayer r in GameManager.Players.Values)
            {
                Destroy(r.RiderModel);
                Destroy(r.BMX);
                Destroy(r.Audio);
                objs.Add(r.gameObject);
                

            }
            if (objs.Count > 0)
            {
                foreach(GameObject obj in objs)
                {
                    Destroy(obj);
                }
            }
            GameManager.Players.Clear();
            GameManager.PlayersColours.Clear();
            GameManager.PlayersSmooths.Clear();
            GameManager.PlayersTexinfos.Clear();
           
            // Server learns of disconnection itself and tells everyone

        }
        

       
       
    

       public void ClientsOfflineMenu()
        {
            
            GUILayout.Label("Client Mode");




            GUILayout.Space(10);
            // setup stuff before connecting
            Username = GUILayout.TextField(Username);
            GUILayout.Space(10);
            desiredIP = GUILayout.TextField(GameNetworking.instance.ip);
            if(desiredIP != "")
            {
                GameNetworking.instance.ip = desiredIP;
            }
            desiredport = GUILayout.TextField(desiredport);
            if(desiredport != "")
            {
            GameNetworking.instance.port = int.Parse(desiredport);
            }
            GUILayout.Space(5);

            if (GUILayout.Button("Connect to Server"))
            {
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage("Trying Setup..", 1, 0));
                // just detects if ridermodel has changed from daryien and if so realigns to be tracking new rig
                _localplayer.RiderTrackingSetup();
                CharacterModding.instance.LoadBmxSetup();
                GameManager.instance.GetLevelName();
            // do Grabtextures to get list of materials main texture names, server will ask for them when it detects you are daryien
            _localplayer.GrabRiderTextures();
            BMXNetLoadout.instance.GrabTextures();
                ConnectToServer();
                OnlineMenu = true;
                OfflineMenu = false;

                
               
            }

            /*
            P2PMenu = GUILayout.Toggle(P2PMenu, "P2P Menu");
            if (P2PMenu)
            {
                GUILayout.Label("P2P Menu");
                GUILayout.Space(10);

                GUILayout.Label("Generate Secure Key");

                GUILayout.Space(10);
            }
            */
           
            GUILayout.Space(30);
           
            
           
           
            
        }

       public void ClientsOnlineMenu()
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Mini GUI"))
            {
                FrostyPGamemanager.instance.OpenMenu = false;
                Minigui = true;
            }
            if (GUILayout.Button("Send Map name"))
            {
                try
                {
                GameManager.instance.GetLevelName();
                ClientSend.SendMapName(GameManager.instance.MycurrentLevel);
                NewMessage(Constants.SystemMessageTime, new TextMessage("Sent Map name", 1, 1));
                }
                catch(UnityException x)
                {
                    Debug.Log(x);
                }
            }
            GUILayout.Space(10);

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
            GUILayout.Label("Live Rider list:", Generalstyle);
            foreach(RemotePlayer r in GameManager.Players.Values)
            {
                GUILayout.Label($"{r.username} as {r.CurrentModelName} at {r.CurrentMap}");
            }

            GUILayout.Space(20);
            GUILayout.Label("Messages:");
            
           // scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach(TextMessage mess in Messages)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = MessageColour[mess.FromCode];
                style.alignment = TextAnchor.MiddleCenter;
                style.padding = new RectOffset(10, 0, 0, 10);
                style.fontStyle = FontStyle.Bold;
                
                GUILayout.Label(mess.Message,style);
            }

           // GUILayout.EndScrollView();
            GUILayout.Space(50);
            if (GUILayout.Button("Disconnect"))
            {
                OnlineMenu = false;
                OfflineMenu = true;
                Disconnect();
                Connected = false;
            }
            GUILayout.Space(20);
        }


        void OnGUI()
        {
            if (Minigui)
            {
                MiniGUI();
            }

        }
     
        
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

        


       public void MiniGUI()
        {
            GUILayout.Space(50);
            if (GUILayout.Button("return"))
            {
                Minigui = false;
                OnlineMenu = true;
                OfflineMenu = false;
                FrostyPGamemanager.instance.OpenMenu = true;
               
               
            }
            GUILayout.Space(20);

            GUILayout.Label("Live Rider list:", Generalstyle);
            foreach (RemotePlayer r in GameManager.Players.Values)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.green;
                style.alignment = TextAnchor.MiddleCenter;
                style.padding = new RectOffset(10, 0, 0, 10);
                style.fontStyle = FontStyle.Bold;
                GUILayout.Label($"{r.username} as {r.CurrentModelName} at {r.CurrentMap}",style);
            }

            GUILayout.Space(20);
            GUILayout.Label("Messages:");

            // scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (TextMessage mess in Messages)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = MessageColour[mess.FromCode];
                style.alignment = TextAnchor.MiddleCenter;
                style.padding = new RectOffset(10, 0, 0, 10);
                style.fontStyle = FontStyle.Bold;

                GUILayout.Label(mess.Message, style);
            }

           

        }





        /// <summary>
        /// Call IngameUI.instance.NewMessage(constants.?, new textmessage("message", messagecolour.?, 1(incoming messages from server are coded and handled in clienthandle)  ))     to display message
        /// </summary>
        /// <param name="_time"></param>
        /// <param name="message"></param>
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

    /// <summary>
    /// Incoming messages get different colors
    /// </summary>
    public enum MessageColour
    {
        System = 1,
        Me = 2,
        Player = 3,
        Server = 4,
    }


}