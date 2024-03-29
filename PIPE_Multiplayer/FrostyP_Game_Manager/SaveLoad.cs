﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FrostyP_Game_Manager
{
    public static class SaveLoad
    {

      
        // Cam Settings

       public static void Save(string users_name_for_preset, List<PresetData> list, string directory)
        {
            string destination = directory;
            
            FileStream file;






            for(int i = 0; i < list.Count; i++)
            {
            if(list[i].nameofsetting.Contains("CAMSETTING"))          
                {
            destination = directory + users_name_for_preset + ".FrostyCAMPreset";
            }


                else if (list[i].nameofsetting.Contains("Max") | list[i].nameofsetting.Contains("Min"))
                {

                    destination = directory + users_name_for_preset + ".FrostyRiderPreset";
                }

            }





            if (!Directory.Exists(directory)) {

                Directory.CreateDirectory(directory);
            }



            if (File.Exists(destination))
            {
                file = File.OpenWrite(destination);
            }
            else file = File.Create(destination);
            
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(file, list);
            file.Close();
            Debug.Log("Completed Saving Presets");
            return;

            


        }

        public static List<PresetData> Load(string destination)
        {
            List<PresetData> loadedlist;
            loadedlist = new List<PresetData>();
           
            
            FileStream file;
            if (File.Exists(destination)) file = File.OpenRead(destination);
            else
            {
                Debug.Log("Couldnt find file in there");
                return null;
            }


            BinaryFormatter bf = new BinaryFormatter();
            loadedlist = (List<PresetData>) bf.Deserialize(file);

            file.Close();


            return loadedlist;
        }


        // Rider Physics

        public static void SavePhysics(PhysicsProfile profile)
        {
            BinaryFormatter bf = new BinaryFormatter();
            Stream stream = File.OpenWrite(RiderPhysics.instance.PresetDirectory + profile.name + ".Physics");

            bf.Serialize(stream, profile);
            stream.Close();
            Debug.Log($"Saved physics profile: {profile.name}");


        }

        public static PhysicsProfile LoadPhysics(FileInfo file)
        {
            BinaryFormatter bf = new BinaryFormatter();
            Stream stream = File.OpenRead(file.FullName);
            PhysicsProfile profile = bf.Deserialize(stream) as PhysicsProfile;
            stream.Close();
            return profile;

        }

        public static void SaveActive(PhysicsProfile profile)
        {
            BinaryFormatter bf = new BinaryFormatter();
            Stream stream = File.OpenWrite(RiderPhysics.instance.LastLoadedDir + "Active.Physics");

            bf.Serialize(stream, profile);
            stream.Close();
            Debug.Log($"Saved Active profile: {profile.name}");


        }

       



    }
}
