using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrostyP_Game_Manager
{
    [Serializable]
   public class SavedSpot
    {
        public List<SavedGameObject> Objects;
        public string NameOfSpot;
        public string[] Creators;
        public string MapName;



        public SavedSpot(List<SavedGameObject> _objects,string _nameofSpot, string[] _creators, string _map)
        {
            Objects = _objects;
            NameOfSpot = _nameofSpot;
            Creators = _creators;
            MapName = _map;
        }




    }
}
