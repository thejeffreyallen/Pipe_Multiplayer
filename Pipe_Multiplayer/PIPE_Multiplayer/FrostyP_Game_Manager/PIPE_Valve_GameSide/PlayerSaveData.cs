using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{

    [Serializable]
    class PlayerSaveData
    {

        public string Username;




        public PlayerSaveData(string _username)
        {

            Username = _username;
        }

    }
}
