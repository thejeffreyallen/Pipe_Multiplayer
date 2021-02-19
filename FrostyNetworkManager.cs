
using UnityEngine.Networking;
using UnityEngine;
using System.Collections.Generic;



namespace FrostyP_PIPE_MultiPlayer
{
    public class FrostyNetworkManager : MonoBehaviour
    {
        GameObject ManagerObject;
        NetworkManager_Class netmanager;
        NetworkManagerHUD hud;
       
      

        GameObject player;
        AssetBundle bundle;



        public List<string> Master_message_list;







        void Start()
        {
          


            // get playerPrefab, add local player script to it,
            // when local player is local, it just updates its prefab with all transform data, theres a transformchild for every moving part,
            // when localplayer detects it has made it to a client machine it fires up a remote player script on its own object and shuts itself down,
            // then remoteplayer taps into the playerprefab its attached to, creates a rider and plugs in transform data from its playerprefab, also naming the rider and bike with its own netid.value
             bundle = AssetBundle.LoadFromFile(Application.dataPath + "/FrostyMultiPlayerAssets");
             player = bundle.LoadAsset("Player") as GameObject;
             player.AddComponent<Player>();
             player.GetComponent<NetworkTransform>().sendInterval = 0.003f;


            // set the sync rate of every synced transformchild, set to compress and set interpolation values
            NetworkTransformChild[] arrayofchildren = player.GetComponents<NetworkTransformChild>();
            foreach(NetworkTransformChild child in arrayofchildren)
            {
                child.sendInterval = 0.003f;
                child.rotationSyncCompression = NetworkTransform.CompressionSyncMode.Low;
                child.interpolateMovement = 0.5f;
                child.interpolateRotation = 0.5f;
                
            }



            // make new manager object and add custom networkmanager
            ManagerObject = new GameObject();
           netmanager = ManagerObject.AddComponent<NetworkManager_Class>();
           
            // add auto Hud for now and turn auto create off so custom manager can override if it wants
           hud = ManagerObject.AddComponent<NetworkManagerHUD>();  
           netmanager.autoCreatePlayer = false;


            netmanager.networkAddress = "192.168.1.140";
            netmanager.playerPrefab = player;
            //netmanager.playerSpawnMethod = PlayerSpawnMethod.RoundRobin;   two modes, choose form list of positons at random, or use them in order
            
           

        }





    }
}
