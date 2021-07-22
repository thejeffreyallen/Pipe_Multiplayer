using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Reflection;

namespace FrostyP_Game_Manager
{
   public class ParkSaveLoad
    {
        


        public void SaveSpot(List<PlacedObject> listofobjects, string Filename, List<string> _CreatorsIn)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream;
            List<SavedGameObject> _objs = new List<SavedGameObject>();
            foreach(PlacedObject p in listofobjects)
            {
                float[] pos = new float[3];
                float[] rot = new float[3];
                float[] scale = new float[3];
                pos[0] = p.Object.transform.position.x;
                pos[1] = p.Object.transform.position.y;
                pos[2] = p.Object.transform.position.z;

                rot[0] = p.Object.transform.eulerAngles.x;
                rot[1] = p.Object.transform.eulerAngles.y;
                rot[2] = p.Object.transform.eulerAngles.z;

                scale[0] = p.Object.transform.localScale.x;
                scale[1] = p.Object.transform.localScale.y;
                scale[2] = p.Object.transform.localScale.z;

                SavedGameObject sgo = new SavedGameObject(p.Object.name.Replace("(Clone)",""), pos, rot, scale, p.BundleData.FileName, p.BundleData.Bundle.name);
                _objs.Add(sgo);
            }

            string[] Creatorlistout = new string[_CreatorsIn.Count];
            for (int i = 0; i < Creatorlistout.Length; i++)
            {
                Creatorlistout[i] = _CreatorsIn[i];
            }
            SavedSpot NewSpot = new SavedSpot(_objs, Filename, Creatorlistout, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);





            if (File.Exists(ParkBuilder.instance.ParksDirectory + Filename))
            {
               stream = File.OpenWrite(ParkBuilder.instance.ParksDirectory + Filename);
            }
            else
            {
               stream = File.Create(ParkBuilder.instance.ParksDirectory + Filename);
               
            }


            bf.Serialize(stream, NewSpot);
            stream.Close();
            Debug.Log("Saved Spot!");
        }

        public void LoadSpot(string _FullFilename)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream;
            string AssetbundlesDirectory = Application.dataPath + "/FrostyPGameManager/ParkBuilder/Assetbundles";

            // load savedspot
            try
            {
                stream = File.OpenRead(_FullFilename);
                SavedSpot _loadedspot = bf.Deserialize(stream) as SavedSpot;
                stream.Close();

                if(_loadedspot.Objects != null && _loadedspot.Objects.Count > 0)
                {
                    ParkBuilder.instance.LoadedSpotReceiveFromFile(_loadedspot);
                }

            }
            catch (Exception x)
            {
                Debug.Log("Load Spot Error : " + x);
            }



           

           
        }







    }
}
