using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;



namespace PIPE_Valve_Online_Server
{

    /// <summary>
    /// Functions that format and send data
    /// </summary>
    class ServerSend
    {
       

        // These top three functions are used by the send functions, give connection number, bytes and specify a send mode from Valve.sockets.sendflags.
        private static void SendtoOne(uint toclient, byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                Server.server.SendMessageToConnection(toclient, bytes, sendflag);
            });
        }
        private static void SendToAll(byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            foreach(Player client in Server.Players.Values)
            {
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    Server.server.SendMessageToConnection(client.clientID, bytes, sendflag);
                });
            }
        }
        private static void SendToAll(uint Exceptthis, byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {


          
            try
            {
            foreach (Player client in Server.Players.Values)
            {
                if(client.clientID != Exceptthis)
                    {
                        ThreadManager.ExecuteOnMainThread(() =>
                        {

                            Server.server.SendMessageToConnection(client.clientID, bytes, sendflag);
                        });
                }
            }

            }
            catch (Exception x)
            {
                Console.WriteLine("Interupted while Sending to all" + x);
            }

        }




        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////







        #region Send Fuctions



        /// <summary>
        ///  send inital welcome on connection obtained to the server
        /// </summary>
        /// <param name="ClientID"></param>
        public static void Welcome(uint ClientID)
        {
            using (Packet _packet = new Packet((int)ServerPacket.Welcome))
            {
                _packet.Write("Welcome To the Server");
             


                Server.server.SendMessageToConnection(ClientID, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

           
        }




        /// <summary>
        /// Request a list of texture names from rider and bike to check server has them
        /// </summary>
        /// <param name="ClientId"></param>
        public static void RequestTexturenames(uint ClientId)
        {
            using(Packet _packet = new Packet((int)ServerPacket.RequestTexNames))
            {
                // giving packetcode is enough to trigger clienthandle

                SendtoOne(ClientId, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }


        public static void RequestBike(uint Clientid)
        {
            using(Packet _packet = new Packet((int)ServerPacket.RequestBike))
            {
                // no data needed
                SendtoOne(Clientid, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }



        /// <summary>
        /// Request a list of textures by name
        /// </summary>
        /// <param name="Clientid"></param>
        /// <param name="names"></param>
        public static void RequestTextures(uint Clientid, List<string> names)
        {
            using(Packet _packet = new Packet((int)ServerPacket.requestTextures))
            {
                _packet.Write(names.Count);

                foreach (string s in names)
                {
                    _packet.Write(s);
                }
                //SendtoOne(Clientid, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }






        /// <summary>
        /// Fires when an new player joins,sending them to everyone
        /// </summary>
        /// <param name="_toClient"></param>
        /// <param name="_player"></param>
        public static void SetupNewPlayer(uint _toClient, Player _player)
        {
           

            using (Packet _packet = new Packet((int)ServerPacket.SetupAPlayer))
            {
                _packet.Write(_player.clientID);
                _packet.Write(_player.Username);
                _packet.Write(_player.RiderPositions[0]);
                _packet.Write(_player.RiderRotations[0]);
                _packet.Write(_player.Ridermodel);
                _packet.Write(_player.Ridermodelbundlename);
                _packet.Write(_player.MapName);

                if (_player.Ridermodel == "Daryien")
                {
                _packet.Write(_player.RiderTextureInfoList.Count);
                foreach(TextureInfo t in _player.RiderTextureInfoList)
                {
                    _packet.Write(t.Nameoftexture);
                    _packet.Write(t.NameofparentGameObject);
                }

                }

               
                _packet.Write(_player.Loadout.FrameColour);
                _packet.Write(_player.Loadout.ForksColour);
                _packet.Write(_player.Loadout.BarsColour);
                _packet.Write(_player.Loadout.SeatColour);
                _packet.Write(_player.Loadout.FTireColour);
                _packet.Write(_player.Loadout.RTireColour);
                _packet.Write(_player.Loadout.FTireSideColour);
                _packet.Write(_player.Loadout.RTireSideColour);
                _packet.Write(_player.Loadout.FrameSmooth);
                _packet.Write(_player.Loadout.ForksSmooth);
                _packet.Write(_player.Loadout.SeatSmooth);
                _packet.Write(_player.Loadout.BarsSmooth);
                _packet.Write(_player.Loadout.FrameTexname);
                _packet.Write(_player.Loadout.ForkTexname);
                _packet.Write(_player.Loadout.BarTexName);
                _packet.Write(_player.Loadout.SeatTexname);
                _packet.Write(_player.Loadout.TireTexName);
                _packet.Write(_player.Loadout.TireNormalName);

                try
                {
                Server.server.SendMessageToConnection(_toClient, _packet.ToArray(), Valve.Sockets.SendFlags.NoNagle | Valve.Sockets.SendFlags.Reliable);
                }
                catch (Exception x)
                {
                    Console.WriteLine("Failed To Send Setup Command");
                }
            }
        }


        /// <summary>
        /// Fires when a new player joins, sending everyone back to them in packs of 5 to ease the potential load of many players
        /// </summary>
        /// <param name="_toclient"></param>
        /// <param name="_players"></param>
        public static void SetupAllOnlinePlayers(uint _toclient, List<Player> _players)
        {
            List<Player> listof5 = new List<Player>();
            
            for (int i = 0; i < _players.Count; i++)
            {
                if(_players[i].clientID != _toclient)
                {
                listof5.Add(_players[i]);

                }


                if (listof5.Count == 5 | i == _players.Count - 1 && listof5.Count>0)
                {
                    Console.WriteLine($"Sending {listof5.Count} players out of {_players.Count} in Player list");
                    using (Packet _packet = new Packet((int)ServerPacket.SetupAllOnlinePlayers))
                    {
                        // amount of players in this bundle, for the last bundle or if less than 5 are on
                        _packet.Write(listof5.Count);
                        foreach (Player _player in listof5)
                        {
                            _packet.Write(_player.clientID);
                            _packet.Write(_player.Username);
                            _packet.Write(_player.RiderPositions[0]);
                            _packet.Write(_player.RiderRotations[0]);
                            _packet.Write(_player.Ridermodel);
                            _packet.Write(_player.Ridermodelbundlename);
                            _packet.Write(_player.MapName);

                            if (_player.Ridermodel == "Daryien")
                            {
                                _packet.Write(_player.RiderTextureInfoList.Count);
                                foreach (TextureInfo t in _player.RiderTextureInfoList)
                                {
                                    _packet.Write(t.Nameoftexture);
                                    _packet.Write(t.NameofparentGameObject);
                                }

                            }


                            _packet.Write(_player.Loadout.FrameColour);
                            _packet.Write(_player.Loadout.ForksColour);
                            _packet.Write(_player.Loadout.BarsColour);
                            _packet.Write(_player.Loadout.SeatColour);
                            _packet.Write(_player.Loadout.FTireColour);
                            _packet.Write(_player.Loadout.RTireColour);
                            _packet.Write(_player.Loadout.FTireSideColour);
                            _packet.Write(_player.Loadout.RTireSideColour);
                            _packet.Write(_player.Loadout.FrameSmooth);
                            _packet.Write(_player.Loadout.ForksSmooth);
                            _packet.Write(_player.Loadout.SeatSmooth);
                            _packet.Write(_player.Loadout.BarsSmooth);
                            _packet.Write(_player.Loadout.FrameTexname);
                            _packet.Write(_player.Loadout.ForkTexname);
                            _packet.Write(_player.Loadout.BarTexName);
                            _packet.Write(_player.Loadout.SeatTexname);
                            _packet.Write(_player.Loadout.TireTexName);
                            _packet.Write(_player.Loadout.TireNormalName);

                        }

                    try
                    {
                       Server.server.SendMessageToConnection(_toclient, _packet.ToArray(), Valve.Sockets.SendFlags.NoNagle | Valve.Sockets.SendFlags.Reliable);
                            listof5.Clear();
                            Console.WriteLine("Player bundle sent");
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine($"Failed To Send Player bundle: Players: {listof5.Count} in list, total to send: {_players}");

                    }
                    }

                
                }



            }





        }




        /// <summary>
        /// Sends to all players except this one
        /// </summary>
        /// <param name="_aboutplayer"></param>
        /// <param name="count"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        public static void SendATransformUpdate(uint _aboutplayer,int count, Vector3[] pos, Vector3[] rot)
        {
            using (Packet _Packet = new Packet((int)ServerPacket.SendTransformUpdate))
            {
                _Packet.Write(_aboutplayer);
                _Packet.Write(count);

                for (int i = 0; i < count; i++)
                {
                    _Packet.Write(pos[i]);
                }

                for (int i = 0; i < count; i++)
                {
                    _Packet.Write(rot[i]);
                }


                try
                {
                SendToAll(_aboutplayer, _Packet.ToArray(), Valve.Sockets.SendFlags.Unreliable);
                }
                catch (Exception x)
                {
                    Console.WriteLine("Failed transform relay, player left?");
                }



            }
        }






        /// <summary>
        /// send connection int that disconnected to all remaining connections
        /// </summary>
        /// <param name="ClientThatDisconnected"></param>
        public static void DisconnectTellAll(uint ClientThatDisconnected)
        {
            Server.Players.Remove(ClientThatDisconnected);
            using (Packet _packet = new Packet((int)ServerPacket.DisconnectedPlayer))
            {
                _packet.Write(ClientThatDisconnected);
                try
                {
                SendToAll(ClientThatDisconnected, _packet.ToArray(),Valve.Sockets.SendFlags.Reliable);
                }
                catch (Exception X)
                {
                    Console.WriteLine("Failed Disconnect tell all, player just left?");
                }
            }
        }





        public static void SendAudioToAllPlayers(uint _from, byte[] lastupdate)
        {
            using(Packet _packet = new Packet((int)ServerPacket.SendAudioUpdate))
            {
                _packet.Write(_from);
                _packet.Write(lastupdate);
                try
                {
           SendToAll(_packet.ToArray(),Valve.Sockets.SendFlags.NoDelay | Valve.Sockets.SendFlags.Reliable);
                }
                catch (Exception x)
                {
                    Console.WriteLine("Failed Audio relay, player left?");
                }
            }
            
        }





        public static void SendTextMessageToAll(uint _fromplayer, string _message)
        {
            try
            {
            using (Packet _packet = new Packet((int)ServerPacket.SendText))
            {
                _packet.Write(_fromplayer);
                _packet.Write(_message);
                _packet.Write(3);
                SendToAll(_fromplayer, _packet.ToArray(),Valve.Sockets.SendFlags.Reliable);
            }

            using (Packet _packet = new Packet((int)ServerPacket.SendText))
            {
                _packet.Write(_fromplayer);
                _packet.Write(_message);
                _packet.Write(2);
                SendtoOne(_fromplayer, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

            }
            catch (Exception x)
            {
                Console.WriteLine("Failed send tex to all, player left?");
            }

        }


        public static void SendTextMessageToOne(uint _toplayer, string _message)
        {


        }



        public static void SendTextures(uint _toplayer, List<string> names)
        {
            List<byte[]> images = new List<byte[]>();

            // get all images
            List<TextureBytes> infos = Server.GiveTexturesFromDirectory(names);


            if (infos != null)
            {
                foreach (TextureBytes tex in infos)
                {
                    byte[] bytesofimage = tex.bytes;
                    int sizeofbytes = bytesofimage.Length;
                    int currentpos = 0;
                    int n = sizeofbytes / 10;
                    int a = (n / 10) * 10;
                    int b = a + 10;
                    int divider = 10;


                    for (int i = 0; i < divider; i++)
                    {
                        byte[] segment = new byte[sizeofbytes / divider];

                        for (int _i = 0; _i < bytesofimage.Length / divider; _i++)
                        {
                            segment[_i] = bytesofimage[currentpos];
                            currentpos++;

                        }

                        using (Packet _packet = new Packet((int)ServerPacket.SendTexturetoPlayer))
                        {
                            _packet.Write(i);
                            _packet.Write(divider);
                            _packet.Write(segment.Length);
                            _packet.Write(tex.Texname);
                            _packet.Write(segment);

                            //SendtoOne(_toplayer, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                           // Console.WriteLine($"Sending {n} to {_toplayer}: Packet {i} of {divider}");
                        }
                    }



                }

            }

            

        }



        public static void SendQuickBikeUpdate(uint __toplayer, Packet _packet)
        {
            try
            {
            SendToAll(__toplayer, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
            catch (Exception x)
            {
                Console.WriteLine("Quick bike update error : " + x);
            }
        }


        public static void SendQuickRiderUpdate(uint _toplayer, Packet _packet)
        {
            try
            {
                SendToAll(_toplayer, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
            catch (Exception x)
            {
                Console.WriteLine("Quick Rider update error : " + x);
            }
        }



        public static void SendMapName(uint _from,string name)
        {
            using(Packet _packet = new Packet((int)ServerPacket.SendMapName))
            {
                _packet.Write(name);
                _packet.Write(_from);
                SendToAll(_from, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }



        #endregion




    }
}
