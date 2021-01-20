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

        // syncvars are kept on the server, using proper commands (customattributes like [command], [clientrpc] ) etc can be edited by a client or a server but always get pushed to ALL clients. not sure if i want this
        [SyncVar] public Vector3 Otherbikepos;
        [SyncVar] public Quaternion Otherbikerot;



        public override void OnStartLocalPlayer()
        {


        }





        void Start()
        {
            obj = UnityEngine.GameObject.Find("BMX");




            if (!isLocalPlayer)
            {
                /// if your not the local player you must be representing one on another machine, so locally instantiate a bike on this machine to represent your player
                
                //otherbike.name = "player2";
                otherbike = GameObject.Instantiate(GameObject.Find("BMX")) as GameObject;




             





                // debug all bikes found
                float num = 1;
                GameObject[] objects = UnityEngine.GameObject.FindObjectsOfType<GameObject>();
                foreach (GameObject gameobj in objects)
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

                //if local, move this empty playerprefab with obj, which is local mans bike
                transform.position = obj.transform.position;

            }



            if (!isLocalPlayer)
            {
                // if not local, move the other bike youve made to transform of this empty playerprefab, which is autosyncing is trans while its tracing an object back on local machine, messy
                otherbike.transform.position = transform.position;
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
