
using UnityEngine.Networking;
using UnityEngine;



namespace FrostyP_PIPE_MultiPlayer
{
    public class FrostyNetworkManager : MonoBehaviour
    {
        GameObject ManagerObject;
        NetworkManager_Class netmanager;
        NetworkManagerHUD hud;
      

        GameObject player;
        AssetBundle bundle;

        









        void Start()
        {
           
            // get playerPrefab
             bundle = AssetBundle.LoadFromFile(Application.dataPath + "/FrostyMultiPlayerAssets");
             player = bundle.LoadAsset("Player") as GameObject;
             player.AddComponent<Player>();
            player.GetComponent<NetworkTransform>().sendInterval = 0.003f;


            NetworkTransformChild[] arrayofchildren = player.GetComponents<NetworkTransformChild>();
            foreach(NetworkTransformChild child in arrayofchildren)
            {
                child.sendInterval = 0.003f;
                
            }


           








           















            // make new manager object and add custom networkmanager
            ManagerObject = new GameObject();
           netmanager = ManagerObject.AddComponent<NetworkManager_Class>();
           
            // add auto Hud for now and turn auto create off so custom manager can override if it wants
           hud = ManagerObject.AddComponent<NetworkManagerHUD>();
            netmanager.autoCreatePlayer = false;


;
            



            netmanager.networkAddress = "192.168.1.140";
            netmanager.playerPrefab = player;
            //netmanager.playerSpawnMethod = PlayerSpawnMethod.RoundRobin;   two modes, choose form list of positons at random, or use them in order
            
           


            

           



        }

        void Update()
        {
           
         

        }






        void OnGUI()
        {
           
            


        }







    }
}
