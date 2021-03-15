using System;
using System.Collections;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.Sockets;
using System.Threading;
using System.Net;
using UnityEngine;

namespace PIPE_Valve_Console_Client
{
    class GameNetworking : MonoBehaviour
    {
		public static GameNetworking instance;
		public bool running = false;

		// Encrypted key to share
		public string Key = "";
		

		// ip to connect to, default local
		public string ip = "127.0.0.1";
		public int port;

		public NetworkingUtils utils;
		public NetworkingSockets client;
		ConnectionState ConnState;
		
		//connection number for server, only ever one connection for client
		public uint connection = 0;

		StatusCallback status;

		// links functions in ServerHandles to Ints in ClientPackets, connection is read on a loop, if message exists an int (i) is read from the byte[], then the byte array is sent to serverhandle function[i]
		// a function that takes in a packet
		public delegate void PacketHandler(Packet _packet);
		// a function with its setup int key
		public static Dictionary<int, PacketHandler> packetHandlers;


		void Start()
        {
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				Debug.Log("Instance already exists, destroying object!");
				Destroy(this);
			}


			


			Debug.Log("Initialising..");

			// list of functions linked to incoming int codes
			packetHandlers = new Dictionary<int, PacketHandler>()
			{
				{ (int)ServerPacket.Welcome,ClientHandle.Welcome },
				{ (int)ServerPacket.ReceiveTransformUpdate,ClientHandle.PlayerPositionReceive },
				{ (int)ServerPacket.SetupAPlayer,ClientHandle.SetupPlayerReceive},
				{ (int)ServerPacket.RequestTexNames,ClientHandle.RequestforDaryienTexNamesReceive},
				{ (int)ServerPacket.requestTextures,ClientHandle.RequestForTextures},
				{ (int)ServerPacket.ReceiveTextureforPlayer,ClientHandle.ReceiveTexture},
				{ (int)ServerPacket.DisconnectedPlayer,ClientHandle.PlayerDisconnected},
				{ (int)ServerPacket.ReceiveAudioForPlayer,ClientHandle.ReceiveAudioForaPlayer}


			};





			

			

			Library.Initialize();
			Debug.Log("Setup Complete");

			
			
        }



		 public void ConnectToServer()
        {

			utils = new NetworkingUtils();
			client = new NetworkingSockets();


			utils.SetStatusCallback(status);

			Address address = new Address();
			address.SetAddress(ip,(ushort)port);
			connection = client.Connect(ref address);
			client.RunCallbacks();
			
			
			running = true;
			//MainThread = new Thread(Run);
			//MainThread.IsBackground = true;
			//MainThread.Start();



		}

		


		
		
		void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
				
				
				
			}

            if (Input.GetKeyDown(KeyCode.C))
            {
				GetComponent<ConsoleLog>().enabled = !GetComponent<ConsoleLog>().enabled;

			}
           
			
        }


		void FixedUpdate()
        {
            if (running)
            {
			Run();

            }
		}



		


		/// <summary>
		/// Receiving System loop for network
		/// </summary>
		private void Run()
        {

			status = (ref StatusInfo info) => {
				switch (info.connectionInfo.state)
				{
					case ConnectionState.None:
						break;

					case ConnectionState.Connected:
						Debug.Log("Client connected to server - ID: " + connection);
						break;

					case ConnectionState.ClosedByPeer:
					case ConnectionState.ProblemDetectedLocally:
						client.CloseConnection(connection);
						Debug.Log("Client disconnected from server");
						break;
				}
			};



#if VALVESOCKETS_SPAN
	MessageCallback message = (in NetworkingMessage netMessage) => {
		Debug.Log("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);
	};
#else
			const int maxMessages = 200;

			NetworkingMessage[] netMessages = new NetworkingMessage[maxMessages];
#endif
			

#if VALVESOCKETS_SPAN
		client.ReceiveMessagesOnConnection(connection, message, 20);
#else

			

				int netMessagesCount = client.ReceiveMessagesOnConnection(connection, netMessages, maxMessages);

				if (netMessagesCount > 0)
				{
					for (int i = 0; i < netMessagesCount; i++)
					{
						ref NetworkingMessage netMessage = ref netMessages[i];


						byte[] bytes = new byte[netMessage.length];
						netMessage.CopyTo(bytes);

						
							using (Packet _packet = new Packet(bytes))
							{
								int _packetId = _packet.ReadInt();
								packetHandlers[_packetId](_packet); // Call appropriate method to handle the packet
							}
						



						//Debug.Log("Message received from server - Channel ID: " + netMessage.channel + ", Data length: " + netMessage.length);

						netMessage.Destroy();
					}
				}

			

#endif
			
			

			
			
		
		}




		public void DisconnectMaster()
        {
			running = false;
			client.CloseConnection(connection);
			//Library.Deinitialize();
			//MainThread = null;
			utils = null;
			client = null;
			status = null;
			
        }

    }
}
