using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Unity;
using UnityEngine.Networking;

namespace FrostyP_PIPE_MultiPlayer
{
   public class Player : NetworkBehaviour
    {

        
        GameObject otherbike;
        GameObject obj;
        [SyncVar] public Vector3 Otherbikepos;
        [SyncVar] public Quaternion Otherbikerot;



        public override void OnStartLocalPlayer()
        {
            
           // bmxs_player_comps = UnityEngine.GameObject.Find("BMXS Player Components");
          //  vehicleman = bmxs_player_comps.GetComponent<VehicleManager>();
           // playeraccess = bmxs_player_comps.GetComponent<PlayersAccessor>();
            //GetComponent<Material>().color = color;
           // Vector3 buffer = new Vector3(17, 40, -13);
           // transform.position = buffer;
            //transform.localScale = new Vector3(1, 1, 1);
            //transform.parent = Camera.current.transform;

           // GameObject BMXobj = UnityEngine.Component.FindObjectOfType<VehicleSkeleton>().gameObject;
            

        }





      void Start()
        {
            obj = UnityEngine.GameObject.Find("BMX");
            Otherbikepos = obj.transform.position;
            Otherbikerot = obj.transform.rotation;



            if (!isLocalPlayer)
            {
                
                otherbike = UnityEngine.GameObject.Find("BMX");

                
                otherbike.name = "player2";

                


                otherbike = GameObject.Instantiate(otherbike) as GameObject;

                //GameObject[] components = otherbike.GetComponentsInChildren<GameObject>(true);
                //foreach (GameObject comp in components)
                //{
                //    if (comp.name.Equals("SessionMarker") || comp.name.Equals("TargetingCamera") || comp.name.Equals("SettingsData") || comp.name.Equals("CameraTarget") || comp.name.Equals("UTILITY"))
                //    {
                //        Destroy(comp);

                //    }
                //}


                otherbike.transform.position = Otherbikepos;
                otherbike.transform.rotation = Otherbikerot;
                

                // debug all bikes found
                float num = 1;
                GameObject[] objects = UnityEngine.GameObject.FindObjectsOfType<GameObject>();
                foreach(GameObject gameobj in objects)
                {
                    if (gameobj.name.Contains("player2"))
                    {
                        Debug.Log(gameobj.name.ToString() + "Found a bmx" + num.ToString());
                        num++;
                    }
                }
                
            }

            
        }











        
        void Update()
        {


            if (isLocalPlayer)
            {
                CmdSaveThisPlayerData();

                transform.position = obj.transform.position;

            }

            
            
            if (!isLocalPlayer)
            {
               
                UpdateOtherPlayer();
            }
           

            




        }

        [Command]
        void CmdSaveThisPlayerData()
        {
            


                Otherbikepos = obj.transform.position;
                Otherbikerot = obj.transform.rotation;
            
        }




      
        void UpdateOtherPlayer()
        {
            if (Otherbikepos != null)
            {
                otherbike.transform.position = Otherbikepos;
            }
            if (Otherbikerot != null)
            {
            otherbike.transform.rotation = Otherbikerot;

            }



           
        }
        




    }
}
