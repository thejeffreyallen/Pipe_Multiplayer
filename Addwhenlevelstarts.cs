using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace FrostyP_PIPE_MultiPlayer
{
    class Addwhenlevelstarts : MonoBehaviour
    {

        GameObject networkobj;

        void Update()
        {

            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.Contains(""))
            {
                if (networkobj == null)
                {
                    networkobj = new GameObject("NetworkingObj");
                }
                if (!networkobj.GetComponent<FrostyNetworkManager>())
                {
                networkobj.AddComponent<FrostyNetworkManager>();

                }


            }
           


        }



    }
}
