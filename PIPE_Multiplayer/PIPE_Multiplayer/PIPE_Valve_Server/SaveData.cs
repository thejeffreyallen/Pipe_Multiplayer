using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIPE_Valve_Online_Server
{
    [Serializable]
    class SaveData
    {

        public List<string> Banwords;
        public List<BanProfile> Banprofiles;



        public SaveData(List<string> banwords, List<BanProfile> bans)
        {
            Banwords = banwords;
            Banprofiles = bans;
        }



    }
}
