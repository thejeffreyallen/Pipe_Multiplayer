using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Valve_Console_Client
{
    public class PublicServer
    {
        public string Name, IP, Port;

        public PublicServer(string name, string ip, string port)
        {
            Name = name;
            IP = ip;
            Port = port;
        }

    }
}
