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
        public int Playerno;


       
        GameObject Bikemain_Parent;

        Transform Bike;
        Transform BikeNet;

        Transform Barsoflocal;
        Transform barsNet;

        Transform CrankLocal;
        Transform CrankNet;

        Transform FrameLocal;
        Transform FrameNet;

        Transform FrontWheelLocal;
        Transform FrontwheelNet;

        Transform BackWheelLocal;
        Transform BackWheelNet;

        Transform pedalLlocal;
        Transform pedalLNet;

        Transform pedalRlocal;
        Transform pedalRNet;

        // rider

        Transform Daryienmain;
        Transform Upperlegleft;
        Transform upperlegright;
        Transform midlegleft;
        Transform midlegright;
        Transform footleft;
        Transform footright;
        Transform spine1;
        Transform spine2;
        Transform spine3;
        Transform leftshoulder;
        Transform rightshoulder;
        Transform leftarm;
        Transform rightarm;
        Transform leftforearm;
        Transform rightforearm;
        Transform lefthand;
        Transform righthand;
        Transform leftfingers;
        Transform rightfingers;
        Transform Hips;

        Transform DaryienMainNet;
        Transform UpperlegleftNET;
        Transform upperlegrightNET;
        Transform midlegleftNET;
        Transform midlegrightNET;
        Transform footleftNET;
        Transform footrightNET;
        Transform spine1NET;
        Transform spine2NET;
        Transform spine3NET;
        Transform leftshoulderNET;
        Transform rightshoulderNET;
        Transform leftarmNET;
        Transform rightarmNET;
        Transform leftforearmNET;
        Transform rightforearmNET;
        Transform lefthandNET;
        Transform righthandNET;
        Transform leftfingersNET;
        Transform rightfingersNET;
        Transform HipsNET;




        NetworkInstanceId localnetid;

       



        public override void OnStartLocalPlayer()
        {
         localnetid = GetComponent<NetworkIdentity>().netId;

           
            
            Bikemain_Parent = UnityEngine.GameObject.Find("BMX");
            Bike = UnityEngine.GameObject.Find("BMX:Bike_Joint").transform;
            BikeNet = this.gameObject.transform.GetChild(5);

            Barsoflocal = UnityEngine.GameObject.Find("BMX:Bars_Joint").transform;
            barsNet = this.gameObject.transform.GetChild(0);

            CrankLocal = UnityEngine.GameObject.Find("BMX:DriveTrain_Joint").transform;
            CrankNet = this.gameObject.transform.GetChild(1);

            FrameLocal = UnityEngine.GameObject.Find("BMX:Frame_Joint").transform;
            FrameNet = this.gameObject.transform.GetChild(2);

            FrontWheelLocal = UnityEngine.GameObject.Find("BMX:Wheel").transform;
            FrontwheelNet = this.gameObject.transform.GetChild(3);

            BackWheelLocal = UnityEngine.GameObject.Find("BMX:Wheel 1").transform;
            BackWheelNet = this.gameObject.transform.GetChild(4);

            pedalLlocal = UnityEngine.GameObject.Find("BMX:LeftPedal_Joint").transform;
            pedalLNet = this.gameObject.transform.GetChild(27);

            pedalRlocal = UnityEngine.GameObject.Find("BMX:RightPedal_Joint").transform;
            pedalRNet = this.gameObject.transform.GetChild(28);




            Daryienmain = UnityEngine.GameObject.Find("Daryien").transform;
            DaryienMainNet = this.gameObject.transform.GetChild(6);

            Upperlegleft = UnityEngine.GameObject.Find("mixamorig:LeftUpLeg").transform;
            UpperlegleftNET = this.gameObject.transform.GetChild(7);

            upperlegright = UnityEngine.GameObject.Find("mixamorig:RightUpLeg").transform;
            upperlegrightNET = this.gameObject.transform.GetChild(8);

            midlegleft = UnityEngine.GameObject.Find("mixamorig:LeftLeg").transform;
            midlegleftNET = this.gameObject.transform.GetChild(9);

            midlegright = UnityEngine.GameObject.Find("mixamorig:RightLeg").transform;
            midlegrightNET = this.gameObject.transform.GetChild(10);

            footleft = UnityEngine.GameObject.Find("mixamorig:LeftFoot").transform;
            footleftNET = this.gameObject.transform.GetChild(11);

            footright = UnityEngine.GameObject.Find("mixamorig:RightFoot").transform;
            footrightNET = this.gameObject.transform.GetChild(12);



           spine1 = UnityEngine.GameObject.Find("mixamorig:Spine").transform;
            spine1NET = this.gameObject.transform.GetChild(13);

            spine2 = UnityEngine.GameObject.Find("mixamorig:Spine1").transform;
            spine2NET = this.gameObject.transform.GetChild(14);

            spine3 = UnityEngine.GameObject.Find("mixamorig:Spine2").transform;
            spine3NET = this.gameObject.transform.GetChild(15);

            leftshoulder = UnityEngine.GameObject.Find("mixamorig:LeftShoulder").transform;
            leftshoulderNET = this.gameObject.transform.GetChild(16);

            rightshoulder = UnityEngine.GameObject.Find("mixamorig:RightShoulder").transform;
            rightshoulderNET = this.gameObject.transform.GetChild(17);

            leftarm = UnityEngine.GameObject.Find("mixamorig:LeftArm").transform;
            leftarmNET = this.gameObject.transform.GetChild(18);

            rightarm = UnityEngine.GameObject.Find("mixamorig:RightArm").transform;
            rightarmNET = this.gameObject.transform.GetChild(19);

            leftforearm = UnityEngine.GameObject.Find("mixamorig:LeftForeArm").transform;
            leftforearmNET = this.gameObject.transform.GetChild(20);

            rightforearm = UnityEngine.GameObject.Find("mixamorig:RightForeArm").transform;
            rightforearmNET = this.gameObject.transform.GetChild(21);

            lefthand = UnityEngine.GameObject.Find("mixamorig:LeftHand").transform;
            lefthandNET = this.gameObject.transform.GetChild(22);

            righthand = UnityEngine.GameObject.Find("mixamorig:RightHand").transform;
            righthandNET = this.gameObject.transform.GetChild(23);

            leftfingers = UnityEngine.GameObject.Find("mixamorig:LeftHandIndex1").transform;
            leftfingersNET = this.gameObject.transform.GetChild(24);

            rightfingers = UnityEngine.GameObject.Find("mixamorig:RightHandIndex1").transform;
            rightfingersNET = this.gameObject.transform.GetChild(25);

            Hips = UnityEngine.GameObject.Find("mixamorig:Hips").transform;
            HipsNET = this.gameObject.transform.GetChild(26);


        }









        
        void Start()
        {


            // if your not local and this object doesnt already have a remoteplayer script, add one then disable networkbehaviour, but keep autosynced transforms of the player prefab your attached to and identity, remoteplayer is a monobehaviour(local) and just takes from its gameobjects hierarchy of transforms
            if (!isLocalPlayer)
            {

                if (!this.gameObject.GetComponent<RemotePlayer>())
                {


                   
                    this.gameObject.AddComponent<RemotePlayer>();
                  
                    if (this.gameObject.GetComponent<RemotePlayer>())
                    {
                        this.enabled = false;

                    }

                    if (this.gameObject.GetComponent<Player>().enabled == false)
                    {
                        Debug.Log("turned off player component of no local player");
                    }


                }


            }


        }












        void Update()
        {
            if (Barsoflocal == null)
            {
                Debug.Log("Cant find my bike bars");
            }

            if (barsNet == null)
            {
                Debug.Log("Cant find bars net");
            }





            if (CrankLocal == null)
            {
                Debug.Log("Cant find my crank");
            }
            if (CrankNet == null)
            {
                Debug.Log("Cant find my crankNet");
            }





            if (FrameLocal == null)
            {
                Debug.Log("Cant find my Frame");
            }
            if (FrameNet == null)
            {
                Debug.Log("Cant find my FrameNet");
            }






            if (FrontWheelLocal == null)
            {
                Debug.Log("Cant find my frontwheel");
            }

            if (FrontwheelNet == null)
            {
                Debug.Log("Cant find my frontwheelnet");
            }






            if (BackWheelNet == null)
            {
                Debug.Log("Cant find my backwheelnet");
            }

            if (BackWheelLocal == null)
            {
                Debug.Log("Cant find my backwheel");
            }













            if (isLocalPlayer)
            {
                
                transform.position = Bikemain_Parent.transform.position;
                transform.rotation = Bikemain_Parent.transform.rotation;
                BikeNet.localRotation = Bike.localRotation;
                BikeNet.localPosition = Bike.localPosition;

                barsNet.localRotation = Barsoflocal.localRotation;

                CrankNet.localRotation = CrankLocal.localRotation;

                FrameNet.localRotation = FrameLocal.localRotation;
                FrameNet.localPosition = FrameLocal.localPosition;

                FrontwheelNet.localRotation = FrontWheelLocal.localRotation;
                BackWheelNet.localRotation = BackWheelLocal.localRotation;

                pedalLNet.localRotation = pedalLlocal.localRotation;
                pedalRNet.localRotation = pedalRlocal.localRotation;









                DaryienMainNet.position = Daryienmain.position;
                DaryienMainNet.rotation = Daryienmain.rotation;

                upperlegrightNET.localRotation = upperlegright.localRotation;
                upperlegrightNET.localPosition = upperlegright.localPosition;
                UpperlegleftNET.localRotation = Upperlegleft.localRotation;
                UpperlegleftNET.localPosition = Upperlegleft.localPosition;
                midlegleftNET.localRotation = midlegleft.localRotation;
                midlegleftNET.localPosition = midlegleft.localPosition;
                midlegrightNET.localRotation = midlegright.localRotation;
                midlegrightNET.localPosition = midlegright.localPosition;
                footleftNET.localRotation = footleft.localRotation;
                footleftNET.localPosition = footleft.localPosition;
                footrightNET.localPosition = footright.localPosition;
                footrightNET.localRotation = footright.localRotation;

                spine1NET.localRotation = spine1.localRotation;
                spine1NET.localPosition = spine1.localPosition;

                spine2NET.localPosition = spine2.localPosition;
                spine2NET.localRotation = spine2.localRotation;

                spine3NET.localPosition = spine3.localPosition;
                spine3NET.localRotation = spine3.localRotation;

                leftshoulderNET.localRotation = leftshoulder.localRotation;
                leftshoulderNET.localPosition = leftshoulder.localPosition;

                rightshoulderNET.localRotation = rightshoulder.localRotation;
                rightshoulderNET.localPosition = rightshoulder.localPosition;

                leftarmNET.localRotation = leftarm.localRotation;
                leftarmNET.localPosition = leftarm.localPosition;
                rightarmNET.localRotation = rightarm.localRotation;
                rightarmNET.localPosition = rightarm.localPosition;

                leftforearmNET.localPosition = leftforearm.localPosition;
                leftforearmNET.localRotation = leftforearm.localRotation;

                rightforearmNET.localPosition = rightforearm.localPosition;
                rightforearmNET.localRotation = rightforearm.localRotation;

                lefthandNET.localRotation = lefthand.localRotation;
                lefthandNET.localPosition = lefthand.localPosition;

                righthandNET.localPosition = righthand.localPosition;
                righthandNET.localRotation = righthand.localRotation;

                leftfingersNET.localRotation = leftfingers.localRotation;
                leftfingersNET.localPosition = leftfingers.localPosition;

                rightfingersNET.localPosition = rightfingers.localPosition;
                rightfingersNET.localRotation = rightfingers.localRotation;

                HipsNET.localRotation = Hips.localRotation;
                HipsNET.localPosition = Hips.localPosition;
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
