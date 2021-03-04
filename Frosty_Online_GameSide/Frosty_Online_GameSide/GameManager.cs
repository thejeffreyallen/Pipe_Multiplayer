using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.IO;

namespace Frosty_Online_GameSide
{
    public class GameManager : MonoBehaviour
    {
        ConsoleLog log;


        public static GameManager instance;

        public static Dictionary<int,RemotePlayer> players = new Dictionary<int, RemotePlayer>();
        public static Dictionary<int, ItemSpawner> itemSpawners = new Dictionary<int, ItemSpawner>();

        
               // inserted empty object, gets around nulling out, one for each player, static in scene relaying data
       GameObject Prefab;
      public GameObject wheelcolliderobj;

      public AssetBundle FrostyAssets;

        // found riders in Custom Players
        AssetBundle[] Ridermodels;
        DirectoryInfo riderdirinfo;
        FileInfo[] riderfiles;
        string riderdirectory = Application.dataPath + "/Custom Players/";




        private void Start()
        {
            log = UnityEngine.GameObject.Find("Fnet").GetComponent<ConsoleLog>();

                FrostyAssets = AssetBundle.LoadFromFile(Application.dataPath + "/FrostyMultiPlayerAssets");
                Prefab = FrostyAssets.LoadAsset("PlayerPrefab") as GameObject;
                wheelcolliderobj = FrostyAssets.LoadAsset("WheelCollider") as GameObject;
                Prefab.AddComponent<RemotePlayer>();
           

            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.Log("Instance already exists, destroying object!");
                Destroy(this);
            }


          

        }




      
        public void SpawnOnMyGame(int _id, string _username, string currentmodel, Vector3 _position, Vector3 _rotation)
        {
            
            try
            {
                GameObject New = GameObject.Instantiate(Prefab);
                RemotePlayer r = New.GetComponent<RemotePlayer>();
                New.name = _username + _id;
                r.CurrentModelName = currentmodel;
                r.id = _id;
                r.username = _username;
              
                players.Add(_id, r);
               
                
            }
            catch(UnityException c)
            {
                Debug.Log("SpawnonmyGame error : " + c);
            }


          

            
        }

       


      


        
         


      private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                log.enabled = !log.enabled;
            }
        }

        
    }
}