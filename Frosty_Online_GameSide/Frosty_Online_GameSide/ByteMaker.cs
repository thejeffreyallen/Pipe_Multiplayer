using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.IO;

namespace Frosty_Online_GameSide
{
    /// <summary>
    /// Takes in various Unity objects and returns byte arrays
    /// </summary>
    class ByteMaker
    {

        /// <summary>
        /// Returns Byte array from input of Texture, Casts to Texture2D and uses .GetRawData
        /// </summary>
        /// <param name="texturetomakebytes"></param>
        /// <returns></returns>
        public static byte[] Image(Texture2D texturetomakebytes)
        {
            byte[] bytes = texturetomakebytes.GetRawTextureData();
          


            return bytes;

            
        }


    }
}
