using System.Collections.Generic;
using System.Collections;
using FrostyPipeServer.ServerFiles;

namespace FrostyPipeServer.Models
{
    public class PlayersModel
    {

        public List<Player> Players;

        public PlayersModel(List<Player> players)
        {
            Players = players;
        }
    }
}
