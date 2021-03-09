using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

namespace Frosty_Online_GameSide
{
    public class RemotePlayerAudio : MonoBehaviour
    {

        EventInstance e;
        GameObject RidingSounds;
        GameObject SoundFX;
        FmodSoundLauncher launcher;
        FmodRiserByVel riserbyvel;

        // Use this for initialization
        void Start()
        {
            RidingSounds = GameObject.Instantiate(UnityEngine.GameObject.Find("BMX_Sounds"));
            SoundFX = GameObject.Instantiate(UnityEngine.GameObject.Find("SOUND_FX"));
            launcher = SoundFX.GetComponentInChildren<FmodSoundLauncher>();

            riserbyvel = RidingSounds.GetComponentsInChildren<FmodRiserByVel>()[1];
            riserbyvel.myTransfrom = gameObject.GetComponent<RemotePlayer>().RiderModel.transform;
            riserbyvel.soundPath = "Grind/rail_slide_single";
            riserbyvel.volume = 1;
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown(KeyCode.S))
            {
               
                riserbyvel.SetVelocity(1);
                riserbyvel.SetParamter(0, 1);
                riserbyvel.SetAdditionalVel(1);
                riserbyvel.SetAdditionalVolume(1);
                riserbyvel.SetPitch(1);
                riserbyvel.RunUpdate();
            }

        }


        void OnGUI()
        {
            if (RidingSounds)
            {
                GUILayout.Label(RidingSounds.name);
            }

            if (riserbyvel)
            {
            GUILayout.Label(riserbyvel.name);
            }
        }
    }
}