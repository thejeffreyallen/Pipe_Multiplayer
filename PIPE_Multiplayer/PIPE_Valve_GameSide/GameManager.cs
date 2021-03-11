using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace PIPE_Valve_Console_Client
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        public LocalPlayer _localplayer;
        public int MyPlayerId;
        public static Dictionary<uint, RemotePlayer> Players;


        GameObject Prefab;
        public GameObject wheelcolliderobj;

        public AssetBundle FrostyAssets;

        // found riders in Custom Players
        AssetBundle[] Ridermodels;
        DirectoryInfo riderdirinfo;
        FileInfo[] riderfiles;
        string riderdirectory = Application.dataPath + "/Custom Players/";


        // Use this for initialization
        void Start()
        {
            Players = new Dictionary<uint, RemotePlayer>();
            FrostyAssets = AssetBundle.LoadFromFile(Application.dataPath + "/FrostyMultiPlayerAssets");
            Prefab = FrostyAssets.LoadAsset("PlayerPrefab") as GameObject;
            wheelcolliderobj = FrostyAssets.LoadAsset("WheelCollider") as GameObject;
            Prefab.AddComponent<RemotePlayer>();


            _localplayer = gameObject.GetComponent<LocalPlayer>();

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





        public void SpawnOnMyGame(uint _id, string _username, string currentmodel, Vector3 _position, Vector3 _rotation)
        {

            try
            {
                GameObject New = GameObject.Instantiate(Prefab);
                RemotePlayer r = New.GetComponent<RemotePlayer>();
                New.name = _username + _id.ToString();
                r.CurrentModelName = currentmodel;
                r.id = _id;
                r.username = _username;

                Players.Add(_id, r);


            }
            catch (UnityException c)
            {
                Debug.Log("SpawnonmyGame error : " + c);
            }





        }















        // Update is called once per frame
        void Update()
        {

        }
    }
}