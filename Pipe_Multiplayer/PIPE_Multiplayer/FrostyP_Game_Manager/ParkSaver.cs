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
   public class ParkSaver
    {
        



        public List<PlacedObjData> GiveNamesAndTransformData(List<GameObject> listin)
        {
            List<PlacedObjData> savablelist = new List<PlacedObjData>();

            foreach(GameObject obj in listin)
            {
                float[] pos = new float[3];
                float[] rot = new float[3];

                pos[0] = obj.transform.position.x;
                pos[1] = obj.transform.position.y;
                pos[2] = obj.transform.position.z;

                rot[0] = obj.transform.eulerAngles.x;
                rot[1] = obj.transform.eulerAngles.y;
                rot[2] = obj.transform.eulerAngles.z;

                PlacedObjData datatosave = new PlacedObjData(obj.name, pos, rot);
                savablelist.Add(datatosave);
            }


            return savablelist;
        }



        public void Save(List<PlacedObjData> listin, string directory)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream;
            if (File.Exists(directory))
            {
               stream = File.OpenWrite(directory);
            }
            else
            {
               stream = File.Create(directory);
               
            }


            bf.Serialize(stream, listin);
            stream.Close();

        }

        public void LoadObjectSetup(string loaddir)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream;
            string AssetbundlesDirectory = Application.dataPath + "/FrostyPGameManager/ParkBuilder/Assetbundles";


            DirectoryInfo assetsinfo = new DirectoryInfo(AssetbundlesDirectory);
            FileInfo[] assetfiles = assetsinfo.GetFiles();





            stream = File.OpenRead(loaddir);
              List<PlacedObjData> loadedlist = bf.Deserialize(stream) as List<PlacedObjData>;

            foreach (PlacedObjData objdata in loadedlist)
            {
                GameObject newobj = GameObject.Instantiate(UnityEngine.GameObject.Find(objdata.name));
                newobj.transform.position = new Vector3(objdata.position[0], objdata.position[1], objdata.position[2]);
                newobj.transform.eulerAngles = new Vector3(objdata.rotation[0], objdata.rotation[1], objdata.rotation[2]);
            }





            /*
        foreach(FileInfo file in assetfiles)
        {
            string[] names;
            AssetBundle[] bundle = AssetBundle.;

            names = bundle.GetAllAssetNames();

            foreach(string name in names)
            {
                    if (name.Contains(objdata.name))
                    {


            }
        }

            */


        



            stream.Close();
        }







    }
}
