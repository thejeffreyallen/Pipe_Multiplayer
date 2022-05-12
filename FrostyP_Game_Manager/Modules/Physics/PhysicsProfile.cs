using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrostyP_Game_Manager
{
    [Serializable]
    public class PhysicsProfile
    {
        public string name;
       public List<float> Values;


        public PhysicsProfile(List<float> values, string _name)
        {
            Values = values;
            name = _name;
        }


    }
}
