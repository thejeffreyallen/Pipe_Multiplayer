using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrostyP_Game_Manager
{
    public class MultiplayerConfig
    {
        public string HubAPIPassword = "PIPE_BMX_Multiplayer_FROSTYP";
        public string GetUrl = "https://pipe-bmx-api.herokuapp.com/servers";
        public string RequestserverUrl = "https://pipe-bmx-api.herokuapp.com/request/";
        public int TransformPool;
        public int AudioPool;

        public MultiplayerConfig()
        {
            HubAPIPassword = "PIPE_BMX_Multiplayer_FROSTYP";
            GetUrl = "https://pipe-bmx-api.herokuapp.com/servers";
            RequestserverUrl = "https://pipe-bmx-api.herokuapp.com/request/";
            TransformPool = 30;
            AudioPool = 30;
        }

    }
}
