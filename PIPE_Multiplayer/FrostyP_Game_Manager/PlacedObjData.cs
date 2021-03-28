using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrostyP_Game_Manager
{
[System.Serializable]
    public class PlacedObjData
    {
       
       public string name;
      public float[] position;
       public float[] rotation;
        

        public PlacedObjData(string nameofobject, float[] pos, float[] rot)
        {
            name = nameofobject;
            position = pos;
            rotation = rot;
        }


    }
}
