using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Collections;
using System.IO;

namespace Frosty_Pipe_Server
{
    class Server
    {
        // Directories for Servers Stored Data
        public static string Rootdir = Assembly.GetExecutingAssembly().Location.Replace(".exe","") + "/Game Data/";
        public static string TexturesDir = Rootdir + "Textures/";

        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        /// <summary>Starts the server.</summary>
        /// <param name="_maxPlayers">The maximum players that can be connected simultaneously.</param>
        /// <param name="_port">The port to start the server on.</param>
        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting server...");
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on port {Port}.");
        }

        /// <summary>Handles new TCP connections.</summary>
        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");

            for (int i = 1; i <= MaxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    clients[i].tcp.Connect(_client);
                    return;
                }
            }

            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        /// <summary>Receives incoming UDP data.</summary>
        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    int _clientId = _packet.ReadInt();

                    if (_clientId == 0)
                    {
                        return;
                    }

                    if (clients[_clientId].udp.endPoint == null)
                    {
                        // If this is a new connection
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        // Ensures that the client is not being impersonated by another by sending a false clientID
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error receiving UDP data: {_ex}");
            }
        }

        /// <summary>Sends a packet to the specified endpoint via UDP.</summary>
        /// <param name="_clientEndPoint">The endpoint to send the packet to.</param>
        /// <param name="_packet">The packet to send.</param>
        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
                }
            }
            catch (Exception _ex)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
            }
        }

        /// <summary>Initializes all necessary server data.</summary>
        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.TransformInfo, ServerHandle.TransformsReceive },
                { (int)ClientPackets.ReceiveDaryienTexNames, ServerHandle.ReceiveDaryienTexNames }
                
            };
            Console.WriteLine("Initialized packets.");
        }


        // called by a severhandle when receiving names of files, gives list of clients file names and client id
        public static void CheckForTextureFiles(List<string> listoffilenames,int _fromclient) 
        {
            DirectoryInfo info = new DirectoryInfo(TexturesDir);
            FileInfo[] files = info.GetFiles();
            List<string> Unfound = new List<string>();

            foreach(string s in listoffilenames)
            {
             bool found = false;
                foreach(FileInfo file in files)
                {
                    if(file.Name == s)
                    {
                        found = true;
                        Console.WriteLine($"Matched {s} to {file.Name}");
                    }
                }
                if (!found)
                {
                    // server doesnt have it, request it
                    Unfound.Add(s);
                    Console.WriteLine(s + "Added to unfound list");
                }

            }
            // if any unfound, send list of required textures back to the client
            if(Unfound.Count > 0)
            {
                ServerSend.RequestTextures(_fromclient, Unfound);

            }
            else
            {
                Console.WriteLine("Got All Textures");
            }

        }

    }
}