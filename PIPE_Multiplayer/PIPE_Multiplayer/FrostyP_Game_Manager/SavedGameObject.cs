using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrostyP_Game_Manager
{
    /// <summary>
    /// An object that makes up a saved spot, saveable data about a GameObject from an Assetbundle
    /// </summary>
   [Serializable]
    public class SavedGameObject
    {
       
        public string nameofGameObject;
        public float[] position;
        public float[] rotation;
        public float[] Scale;
        public string FileName;
        public string AssetBundleName;
        

        public SavedGameObject(string nameofobject, float[] pos, float[] rot, float[] _scale,string _Filename, string _AssetbundleName)
        {
            nameofGameObject = nameofobject;
            position = pos;
            rotation = rot;
            Scale = _scale;
            FileName = _Filename;
            AssetBundleName = _AssetbundleName;
        }


    }
}
