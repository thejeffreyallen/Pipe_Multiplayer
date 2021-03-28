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

        public string FrameTexname = " ";
        public string ForkTexname = " ";
        public string SeatTexname = " ";
        public string BarTexName = " ";
        public string TireTexName = " ";
        public string TireNormalName = " ";



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


            FrameColour = new Vector3(0, 0, 0);
        FrameSmooth = 0;
        FrameTex = new byte[0];

        ForksColour = new Vector3(0, 0, 0);
        ForksSmooth = 0;
        ForksTex = new byte[0];

        BarsColour = new Vector3(0, 0, 0);
        BarsSmooth = 0;
        BarsTex = new byte[0];

        SeatColour = new Vector3(0, 0, 0);
        SeatSmooth = 0;
        SeatTex = new byte[0];

        FTireColour = new Vector3(0, 0, 0);
        RTireColour = new Vector3(0, 0, 0);
        FTireSideColour = new Vector3(0, 0, 0);
        RTireSideColour = new Vector3(0, 0, 0);
        TiresTex = new byte[0];
        TiresNormal = new byte[0];

        FrameTexname = "e";
        ForkTexname = "e";
        SeatTexname = "e";
        BarTexName = "e";
        TireTexName = "e";
        TireNormalName = "e";




    }



       public void GrabTextures()
        {
            try
            {
                
                if (CharacterModding.instance.FrameRen.material.mainTexture != null)
                {
           FrameTexname = CharacterModding.instance.FrameRen.material.mainTexture.name;
                }
                if (CharacterModding.instance.ForksRen.material.mainTexture != null)
                {
                    ForkTexname = CharacterModding.instance.ForksRen.material.mainTexture.name;
                }
                if (CharacterModding.instance.SeatRen.material.mainTexture != null)
                {
                    SeatTexname = CharacterModding.instance.SeatRen.material.mainTexture.name;
                }
                if (CharacterModding.instance.BarsRen.material.mainTexture != null)
                {
                    BarTexName = CharacterModding.instance.BarsRen.material.mainTexture.name;
                }
                if (CharacterModding.instance.FTireRen.material.mainTexture != null)
                {
                    TireTexName = CharacterModding.instance.FTireRen.material.mainTexture.name;  // needs normal too really
                }
            
            Debug.Log("Grabbed Bmx Textures");

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
