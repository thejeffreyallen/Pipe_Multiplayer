using System.Collections;
using UnityEngine;

namespace Frosty_Online_GameSide
{
    public class RemotePlayerAudio : MonoBehaviour
    {


        GameObject RidingSounds;

        // Use this for initialization
        void Start()
        {
            RidingSounds = GameObject.Instantiate(UnityEngine.GameObject.Find("BMX_Sounds"));

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}