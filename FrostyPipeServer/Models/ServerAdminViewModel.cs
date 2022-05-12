using FrostyPipeServer.ServerFiles;
using System.Diagnostics;
using System;
using System.Collections.Generic;
namespace FrostyPipeServer.Models
{
    public class ServerAdminViewModel
    {
        public int Port;
        public bool Publicise;
        public int Maxplayers;
        public int TickMax;
        public int Tickmin;
        public string Servername;
        public string POST;
        public string PUT;

    }
}
