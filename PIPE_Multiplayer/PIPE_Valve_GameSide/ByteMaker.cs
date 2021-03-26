using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PIPE_Valve_Console_Client
{
    class ByteMaker
    {

        /// <summary>
        /// Returns Byte array from input of Texture, Casts to Texture2D and uses .GetRawData
        /// </summary>
        /// <param name="texturetomakebytes"></param>
        /// <returns></returns>
        public static byte[] Image(Texture2D texturetomakebytes)
        {

            byte[] bytes = texturetomakebytes.EncodeToPNG();
            //InGameUI.instance.Messages.Add(bytes.Length.ToString() + " bytes in " + texturetomakebytes.name);


            return bytes;


        }
    }
}
