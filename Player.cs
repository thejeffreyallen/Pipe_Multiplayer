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


        GameObject otherbikemain;
        GameObject Bikemain;
        GameObject Barsoflocal;
        GameObject barsNet;
        GameObject otherbike_Bars;
        NetworkInstanceId localnetid;

        // syncvars are kept on the server, using proper commands (customattributes like [command], [clientrpc] ) etc can be edited by a client or a server but always get pushed to ALL clients. not sure if i want this
        [SyncVar] public Vector3 Otherbikepos;
        [SyncVar] public Quaternion Otherbikerot;



        public override void OnStartLocalPlayer()
        {
            localnetid = netId;
               
           
        }



        

        void Start()
        {
            
            

            Bikemain = UnityEngine.GameObject.Find("BMX");
            barsNet = UnityEngine.GameObject.Find("BarsNetObject");
            Barsoflocal = UnityEngine.GameObject.Find("BMX:Bars_Joint");

            if (barsNet == null)
            {
                Debug.Log("Cant find bars net");
            }

            if (otherbike_Bars == null)
            {
                Debug.Log("Cant find otherbike bars");
            }

            if (Barsoflocal == null)
            {
                Debug.Log("Cant find my bike bars");
            }




            if (!isLocalPlayer)
            {
                /// if your not the local player you must be representing one on another machine, so locally instantiate a bike on this machine to represent your player
                
                //otherbike.name = "player2";
                otherbikemain = GameObject.Instantiate(GameObject.Find("BMX")) as GameObject;
                

                UnityEngine.Transform[] objectsinotherbike = otherbikemain.GetComponentsInChildren<Transform>();
                if(objectsinotherbike == null)
                {
                    Debug.Log("Cant find objects in other bike");
                }


                foreach(Transform ob in objectsinotherbike)
                {
                    Debug.Log(" Found transform in other bike called  " + ob.name.ToString());
                    if (ob.name.Contains("BMX:Bars_Joint"))
                    {
                        otherbike_Bars = ob.gameObject;
                        Debug.Log("Found other bikes bars joint");
                    }
                   
                }
                if(otherbike_Bars == null)
                {
                    Debug.Log("Cant find otherbike bars");
                }








                // debug all bikes found
                float num = 1;
                GameObject[] objects = UnityEngine.GameObject.FindObjectsOfType<GameObject>();
                foreach (GameObject gameobj in objects)
                {
                    if (gameobj.name.Contains("BMXS Player"))
                    {
                        Debug.Log(gameobj.name.ToString() + "Found a bmx" + num.ToString());
                        num++;
                    }
                }

            }


        }

        










        void Update()
        {


            if (isLocalPlayer && hasAuthority)
            {
                transform.position = Bikemain.transform.position;
                transform.rotation = Bikemain.transform.rotation;
                barsNet.transform.rotation = Barsoflocal.transform.rotation;

            }



            if (!isLocalPlayer && !hasAuthority)
            {
                
                otherbikemain.transform.position = transform.position;
                otherbikemain.transform.rotation = transform.rotation;
                otherbike_Bars.transform.localRotation = barsNet.transform.localRotation;
            }







        }


        /*

          INFO ---- Custom attributes

         [Command] = Put above a method to allow a client to call it, but have the server run it. method name must start with Cmd
         [ClientRpc] = Makes method invoke on the client after being called by server - start with Rpc
         [Client] = Only able to run on a client
         [TargetRpc] = Command but for one specified client.  - start with Rpc
         [Server] = only able to run n a server


          e.g


         void Callfunctiononclient()
           { 
           CmdDostuffonserverforme();
           }

        [command]
        void CmdDostuffonserverforme(){

        }



        */
    }






}
