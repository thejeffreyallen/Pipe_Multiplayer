using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;

namespace FrostyP_Game_Manager
{
    public static class SaveLoad
    {
        // Camera
        public static void Save(CameraPreset preset, string directory)
        {
            string destination = directory;
            string json = TinyJson.JSONWriter.ToJson(preset);
            if (!Directory.Exists(directory)) {

                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(directory + preset.name + ".json", json);
            Debug.Log("Completed Saving Presets");
            return;

        }

        public static CameraPreset Load(string from)
        {
            if (File.Exists(from))
            {
                CameraPreset preset = TinyJson.JSONParser.FromJson<CameraPreset>(File.ReadAllText(from));
                if(preset != null) return preset;
            }
            return null;
        }

        // Physics
        public static void SavePhysics(PhysicsProfile profile)
        {
            string json = TinyJson.JSONWriter.ToJson(profile);  
           File.WriteAllText(RiderPhysics.instance.PresetDirectory + profile.name + ".json",json);
            Debug.Log($"Saved physics profile: {profile.name}");

        }

        public static PhysicsProfile LoadPhysics(FileInfo file)
        {
            PhysicsProfile profile = TinyJson.JSONParser.FromJson<PhysicsProfile>(File.ReadAllText(file.FullName));
            if (profile == null) return null;
            return profile;

        }

        public static void SaveActive(PhysicsProfile profile)
        {
            string json = TinyJson.JSONWriter.ToJson(profile);
            File.WriteAllText(RiderPhysics.instance.ActiveDir + profile.name + ".json", json);
            Debug.Log($"Saved Active profile: {profile.name}");


        }

    }
}
