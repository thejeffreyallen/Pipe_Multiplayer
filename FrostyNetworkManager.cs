
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

        NetworkTransformChild BarsChild;









        void Start()
        {
            // from working version i had
            // get playerPrefab
             bundle = AssetBundle.LoadFromFile(Application.dataPath + "/Networkunpack");
             player = bundle.LoadAsset("Player") as GameObject;
             player.AddComponent<Player>();

           BarsChild = player.GetComponent<NetworkTransformChild>();




            // for all registered NetworkChildren, go through BMXS_Player_comps and choose the transforms needed, then assign a NetworkChild from above
            // if i name the networkChild the same as part it needs to find, i could do a simple compare to match all?
           // NetworkTransformChild BmxAnch_Child = player.AddComponent<NetworkTransformChild>();
            
          
            
            /*
            GameObject[] gameobjs_BMXS_P = player.GetComponentsInChildren<GameObject>();
            foreach(GameObject obj in gameobjs_BMXS_P)
            {
                if (obj.name.Contains("BMXAnchor"))
                {
                    BmxAnch_Child.target = obj.transform;
                    BmxAnch_Child.sendInterval = 0.05f;
                }
            }

            */









            // register children to this master object, must be pointed to transform in children, seperate sync rate for eachz















            // make new manager object and add custom networkmanager
            ManagerObject = new GameObject();
           netmanager = ManagerObject.AddComponent<NetworkManager_Class>();
           
            // add auto Hud for now and turn auto create off so custom manager can override if it wants
           hud = ManagerObject.AddComponent<NetworkManagerHUD>();
            netmanager.autoCreatePlayer = false;


;
            



            netmanager.networkAddress = "192.168.1.140";
            netmanager.playerPrefab = player;
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
            



            if(UnityEngine.GameObject.Find("BarsNetObject") != null)
            {
             GameObject barsnet = UnityEngine.GameObject.Find("BarsNetObject");
                

                GUILayout.Label("Found bars transform =" + barsnet.name + barsnet.transform.position.ToString() + "<<<<< bars pos" + "  " + barsnet.transform.rotation.ToString() + " bars rot");
            }




            GUILayout.Label(BarsChild.target.name.ToString());


            /// confirm these syncvars are working somehow
            /*
            if(UnityEngine.Component.FindObjectsOfType<Player>() != null)
            {
                foreach(Player comp in UnityEngine.Component.FindObjectsOfType<Player>())
                {
                    GUILayout.Label(" Other bike pos" + comp.Otherbikepos.ToString() + " Other bike rot " + comp.Otherbikerot.ToString());
                }

            }
            */
            


        }







    }
}
