using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPE_Valve_Online_Server
{
    public class PublicServer
    {
        public string Name, IP, Port;
        public int Id;

        public PublicServer(string name, string ip, string port, int id)
        {
            Name = name;
            IP = ip;
            Port = port;
            Id = id;
        }

    }
}
