using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PIPE_Multiplayer
{
    public class PublicServer
    {
        public string Name,averageping,playercount,maxplayers,mostpopmap,lastpost,version,tickrate;

        public PublicServer(string name,string avping,string playc, string mostpop, string ver, string maxplay, string lpost,string tick)
        {
            Name = name;
            averageping = avping;
            playercount = playc;
            maxplayers = maxplay;
            mostpopmap = mostpop;
            lastpost = lpost;
            version = ver;
            tickrate = tick;
           
        }

    }
}
