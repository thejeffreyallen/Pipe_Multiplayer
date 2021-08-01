using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{

    [Serializable]
    public class PlayerSaveData
    {
        public string Username;
        public List<SavedServer> savedservers;



        public PlayerSaveData(string _username)
        {
            Username = _username;
        }

    }

    [Serializable]
    public class SavedServer
    {

        public string IP;
        public string PORT;
        public string Nickname;

        public SavedServer(string _ip, string _port, string _nickname)
        {
            IP = _ip;
            PORT = _port;
            Nickname = _nickname;
        }

    }


}
