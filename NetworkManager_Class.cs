using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;



namespace FrostyP_PIPE_MultiPlayer
{
    public class NetworkManager_Class : NetworkManager
    {
        // Server callbacks
        public override void OnServerConnect(NetworkConnection conn)
        {
            Debug.Log("A client connected to the server: " + conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            NetworkServer.DestroyPlayersForConnection(conn);
            if (conn.lastError != NetworkError.Ok)
            {
                if (LogFilter.logError) { Debug.LogError("ServerDisconnected due to error: " + conn.lastError); }
            }
            Debug.Log("A client disconnected from the server: " + conn);
        }
        public override void OnServerReady(NetworkConnection conn)
        {
            NetworkServer.SetClientReady(conn);
            Debug.Log("Client is set to the ready state (ready to receive state updates): " + conn);
        }










        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
           

            var player = (GameObject)GameObject.Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
           
            
            
         // this is what actually spawns the prefab and links to a connection and controller id, i think controller id is just gonna be 0=isplayer or -1=notlocal

            NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
            Debug.Log("Client has requested to get his player added to the game");
        }
















        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            if (player.gameObject != null)
                NetworkServer.Destroy(player.gameObject);
        }

        public override void OnServerError(NetworkConnection conn, int errorCode)
        {
            Debug.Log("Server network error occurred: " + (NetworkError)errorCode);
        }

        public override void OnStartHost()
        {
            Debug.Log("Host has started");
        }

        public override void OnStartServer()
        {
            Debug.Log("Server has started");
        }

        public override void OnStopServer()
        {
            Debug.Log("Server has stopped");
        }

        public override void OnStopHost()
        {
            Debug.Log("Host has stopped");
        }



        // Client callbacks
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);
            Debug.Log("Connected successfully to server, now to set up other stuff for the client...");

            // on client connect, delete exisitng offline rider and instantiate new Playerprefab, which is the same but autonetworked?
            //Destroy(UnityEngine.GameObject.Find("BMXS player components"));

            // potentially enter a lobby at this point? only OnServerAddConnection left to process


            // think this limits to one player per client as it always offers controller 0, no big deal
            ClientScene.AddPlayer(conn,0);
         }




        public override void OnClientDisconnect(NetworkConnection conn)
        {
            StopClient();
            if (conn.lastError != NetworkError.Ok)
            {
                if (LogFilter.logError) { Debug.LogError("ClientDisconnected due to error: " + conn.lastError); }
            }
            Debug.Log("Client disconnected from server: " + conn);
        }

        public override void OnClientError(NetworkConnection conn, int errorCode)
        {
            Debug.Log("Client network error occurred: " + (NetworkError)errorCode);
        }

        public override void OnClientNotReady(NetworkConnection conn)
        {
            Debug.Log("Server has set client to be not-ready (stop getting state updates)");
        }

        public override void OnStartClient(NetworkClient client)
        {
            Debug.Log("Client has started");
        }

        public override void OnStopClient()
        {
            Debug.Log("Client has stopped");
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            base.OnClientSceneChanged(conn);
            Debug.Log("Server triggered scene change and we've done the same, do any extra work here for the client...");
        }
    }
}