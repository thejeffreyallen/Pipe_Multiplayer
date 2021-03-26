using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PIPE_Valve_Console_Client
{
   
   public class BMXNetLoadout : MonoBehaviour
    {
        public static BMXNetLoadout instance;


        public Vector3 FrameColour = new Vector3(0,0,0);
        public float FrameSmooth = 0;
        public byte[] FrameTex = new byte[0];

        public Vector3 ForksColour = new Vector3(0, 0, 0);
        public float ForksSmooth = 0;
        public byte[] ForksTex = new byte[0];

        public Vector3 BarsColour = new Vector3(0, 0, 0);
        public float BarsSmooth = 0;
        public byte[] BarsTex = new byte[0];

        public Vector3 SeatColour = new Vector3(0, 0, 0);
        public float SeatSmooth = 0;
        public byte[] SeatTex = new byte[0];

        public Vector3 FTireColour = new Vector3(0, 0, 0);
        public Vector3 RTireColour = new Vector3(0, 0, 0);
        public Vector3 FTireSideColour = new Vector3(0, 0, 0);
        public Vector3 RTireSideColour = new Vector3(0, 0, 0);
        public byte[] TiresTex = new byte[0];
        public byte[] TiresNormal = new byte[0];

        public string FrameTexname = "";
        public string ForkTexname = "";
        public string SeatTexname = "";
        public string BarTexName = "";
        public string TireTexName = "";
        public string TireNormalName = "";



        void Start()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("Net loadout already exists, destroying old netloadout now");
                Destroy(this);
            }




            // load previous
            CharacterModding.instance.LoadBmxSetup();


        }



       public void GrabTextures()
        {
            try
            {
                
                if (CharacterModding.instance.BMX_Materials["framemesh"].material.mainTexture != null)
                {
           FrameTexname = CharacterModding.instance.BMX_Materials["framemesh"].material.mainTexture.name;
                }
                if (CharacterModding.instance.BMX_Materials["Forks Mesh"].material.mainTexture != null)
                {
                    ForkTexname = CharacterModding.instance.BMX_Materials["Forks Mesh"].material.mainTexture.name;
                }
                if (CharacterModding.instance.BMX_Materials["Seat Mesh"].material.mainTexture != null)
                {
                    SeatTexname = CharacterModding.instance.BMX_Materials["Seat Mesh"].material.mainTexture.name;
                }
                if (CharacterModding.instance.BMX_Materials["Bars Mesh"].material.mainTexture != null)
                {
                    BarTexName = CharacterModding.instance.BMX_Materials["Bars Mesh"].material.mainTexture.name;
                }
                if (CharacterModding.instance.BMX_Materials["Tire Mesh Front"].material.mainTexture != null)
                {
                    TireTexName = CharacterModding.instance.BMX_Materials["Tire Mesh Front"].material.mainTexture.name;
                }
            //TireNormalName = CharacterModding.instance.BMX_Materials["Tire Mesh"].material.mainTexture.name;
            Debug.Log("Grabbed" + FrameTexname);

            }
            catch (UnityException x)
            {
                Debug.Log(x);
            }
        }


        void OnGUI()
        {
           // GUILayout.Label(FrameColour.x.ToString());
        }

    }
}
