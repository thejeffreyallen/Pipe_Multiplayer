namespace FrostyPipeServer.Config
{
    public class ServerConfig
    {
        public ushort Maxplayers;
        public ushort Port;
        public string Servername;
        public ushort TickrateMax;
        public ushort TickrateMin;
        public ushort StandbyTick;
        public string Adminpassword;
        public bool Publicise;
        public string POST;
        public string PUT;
        public string KEY;
        public string PortalUrl;
        public string IPCheck;
        public string GarageVersion;
        public bool DynamicTick;
        public ushort DynamicTickMultiplier;
        public int MessageMaxPayload;
        public ushort MessageMTU;
        public ushort HubPostTimeout;
        public ushort SystemTickrate;
        public string[] RandomSpawnMessages;


        public ServerConfig()
        {
                Maxplayers = 8;
                Port = 7777;
                TickrateMax = 60;
                TickrateMin = 30;
                Servername = "PIPE server";
                Adminpassword = "DaveMirraXXX";
                Publicise = false;
                POST = "https://pipe-bmx-api.herokuapp.com/post";
                PUT = "https://pipe-bmx-api.herokuapp.com/update";
                KEY = "PIPE_BMX_Multiplayer_FROSTYP";
                PortalUrl = "http://*:5000;https://*:5001";
                IPCheck = "http://checkip.dyndns.org/";
                GarageVersion = "2.1.0";
                DynamicTick = true;
                DynamicTickMultiplier = 10;
                StandbyTick = 1;
                MessageMaxPayload = 100000;
                MessageMTU = 1250;
                HubPostTimeout = 5;
                SystemTickrate = 1;
                RandomSpawnMessages = new List<string>()
                {
                    {" is on it" },
                    {" broke the bmx out" },
                    {" is about to drop hammers" },
                    {"'s pumped the tyres up" },
                    {"'s about" },
                    {"'s here" },
                    { " showed up" },
                    {" 's rollin" },
                    {"'s gonna send it" },
                    {"'s about to go off" },
                    {"'s warming up" },
                    {" shreds" },
                    {"'s on the way" },
                }.ToArray();

        }

    }
}
