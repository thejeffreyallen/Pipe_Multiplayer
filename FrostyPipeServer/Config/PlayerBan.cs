namespace FrostyPipeServer.Config
{
    public class PlayerBan
    {
        public string IP;
        public string Username;
        public ushort ConnId;
        public DateTime Timeofbanrelease;


        public PlayerBan(string ip, string username, ushort conid, DateTime timeofrelease)
        {
            IP = ip;
            Username = username;
            ConnId = conid;
            Timeofbanrelease = timeofrelease;
        }


        
    }
}
