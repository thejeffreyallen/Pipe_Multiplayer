
using UnityEngine.Networking;
using UnityEngine;



namespace FrostyP_PIPE_MultiPlayer
{
    public class FrostyNetworkManager : MonoBehaviour
    {
        GameObject ManagerObject;
        NetworkManager_Class netmanager;
        NetworkManagerHUD hud;
        NetworkIdentity[] ids;
        GameObject[] foundplayers;

        GameObject prefab;
        AssetBundle bundle;

        void Start()
        {
           // get playerPrefab
            bundle = AssetBundle.LoadFromFile(Application.dataPath + "/Networkunpack");
            prefab = bundle.LoadAsset("Player") as GameObject;
            prefab.AddComponent<Player>();






            // make new manager object and add custom networkmanager
            ManagerObject = new GameObject();
           netmanager = ManagerObject.AddComponent<NetworkManager_Class>();
           
            // add auto Hud for now and turn auto create off so custom manager can override if it wants
           hud = ManagerObject.AddComponent<NetworkManagerHUD>();
            netmanager.autoCreatePlayer = false;


            NetworkIdentity id = prefab.GetComponent<NetworkIdentity>();
            id.localPlayerAuthority = true;

            NetworkTransform prefabtranform = prefab.GetComponent<NetworkTransform>();
            prefabtranform.sendInterval = 0.05f;
            //says sendRate in Unity, send Interval here, opposites



            netmanager.networkAddress = "192.168.16.203";
            netmanager.playerPrefab = prefab;
            //netmanager.playerSpawnMethod = PlayerSpawnMethod.RoundRobin;   two modes, choose form list of positons at rando, or use them in order
            
           


            

           



        }

        void Update()
        {
           
         

        }






        void OnGUI()
        {
            GUI.skin.label.normal.textColor = Color.black;
            GUI.skin.label.fontSize = 16;

            GUILayout.Space(200);



                GUILayout.Label( "    " + netmanager.numPlayers.ToString() + " Players live on my Server      " + "        " + UnityEngine.Object.FindObjectsOfType<NetworkIdentity>().Length.ToString() + "  network identities found" + "   ");
            



            if(UnityEngine.GameObject.Find("BMXAnchor") != null)
            {
             GameObject bike = UnityEngine.GameObject.Find("BMXAnchor");
                GUILayout.Label(bike.transform.position.ToString() + "<<<<< bikes pos" + "  " + bike.transform.rotation.ToString() + " Bikes rot");
            }



            /// confirm these syncvars are working somehow

            if(UnityEngine.Component.FindObjectsOfType<Player>() != null)
            {
                foreach(Player comp in UnityEngine.Component.FindObjectsOfType<Player>())
                {
                    GUILayout.Label(" Other bike pos" + comp.Otherbikepos.ToString() + " Other bike rot " + comp.Otherbikerot.ToString());
                }

            }

            


        }







    }
}
