﻿using System;
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
            try
            {

            ThreadManager.ExecuteOnMainThread(() =>
            {
                Server.Connection.SendMessageToConnection(toclient, bytes, sendflag);
            });
            }
            catch (Exception x)
            {

                Console.WriteLine("Send to one error : " + x);
            }
        }
       
        private static void SendToAll(uint Exceptthis, byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {


          
            try
            {
            foreach (Player client in Server.Players.Values.ToList())
            {
                if(client.RiderID != Exceptthis)
                    {
                        ThreadManager.ExecuteOnMainThread(() =>
                        {

                            Server.Connection.SendMessageToConnection(client.RiderID, bytes, sendflag);
                            
                        });
                }
            }

            }
            catch (Exception x)
            {
                Console.WriteLine("Interupted while Sending to all" + x);
            }

        }

        private static void SendToAllReadyToRoll(uint Sendingrider, byte[] bytes, Valve.Sockets.SendFlags sendflag)
        {
            // if sending player exists
            if(Server.Players.TryGetValue(Sendingrider, out Player player))
            {
            try
            {
                foreach (Player Receivingplayer in Server.Players.Values.ToList())
                {
                    if (Receivingplayer.RiderID != Sendingrider && Receivingplayer.ReadytoRoll)
                    {
                            if (ReadyToRollChecker(player, Receivingplayer))
                            {

                               ThreadManager.ExecuteOnMainThread(() =>
                               {

                                  Server.Connection.SendMessageToConnection(Receivingplayer.RiderID, bytes, sendflag);
                               });


                            }

                    }
                }

            }
            catch (Exception x)
            {
                Console.WriteLine("Interupted while Sending to all" + x);
            }

            }


        }

        private static bool ReadyToRollChecker(Player sender,Player receiver)
        {
            if (receiver.SendDataOverrides.ContainsKey(sender.RiderID))
            {
                if (receiver.SendDataOverrides[sender.RiderID])
                {
                  return true;
                }
            }
            if(sender.MapName.ToLower() == receiver.MapName.ToLower())
            {
            return true;
            }
            else if(sender.MapName.ToLower().Contains(receiver.MapName.ToLower()))
            {
              return true;
            }
            else if (receiver.MapName.ToLower().Contains(sender.MapName.ToLower()))
            {
              return true;
            }
            else if(receiver.MapName.Replace("_"," ").ToLower() == sender.MapName.Replace("_", " ").ToLower())
            {
                return true;
            }
            else
            {
              return false;
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
                _packet.Write($"Connected to {Server.SERVERNAME} Version:{Server.VERSIONNUMBER}");
             


                Server.Connection.SendMessageToConnection(ClientID, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

           
        }

        public static void RequestAllParts(uint Clientid)
        {
            using(Packet _packet = new Packet((int)ServerPacket.RequestAllParts))
            {
                // no data needed
                SendtoOne(Clientid, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        public static void RequestFile(uint Clientid, string name, List<int> _packetsihave,string dir)
        {
            // send request to client
            using(Packet _packet = new Packet((int)ServerPacket.RequestFile))
            {
                
                _packet.Write(name);
                _packet.Write(dir);
                _packet.Write(_packetsihave.Count);
                for (int i = 0; i < _packetsihave.Count; i++)
                {
                    _packet.Write(_packetsihave[i]);
                }


                SendtoOne(Clientid, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

            // if index exists, add id to ReceivingFrom, else create request index
            foreach(SendReceiveIndex sr in ServerData.IncomingIndexes)
            {
                if(sr.NameOfFile.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "") == name.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", ""))
                {
                    bool playerlogged = false;
                    foreach(int play in sr.PlayersRequestedFrom)
                    {
                        if(play == Clientid)
                        {
                            playerlogged = true;
                        }
                        
                    }
                    if (!playerlogged)
                    {
                        sr.PlayersRequestedFrom.Add(Clientid);
                    }

                    return;
                }
            }

            SendReceiveIndex Index = new SendReceiveIndex(name);
            Index.PacketNumbersStored = _packetsihave;
            Index.PlayersRequestedFrom.Add(Clientid);
            ServerData.IncomingIndexes.Add(Index);

        }

        public static void FileStatus(uint _player, string name, int Status)
        {
            using (Packet _packet = new Packet((int)ServerPacket.FileStatus))
            {
                _packet.Write(name);
                _packet.Write(Status);

                SendtoOne(_player,_packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        /// <summary>
        /// Fires when an new player joins,sending them to everyone
        /// </summary>
        /// <param name="_toClient"></param>
        /// <param name="_player"></param>
        public static void SetupNewPlayer(uint _toClient, Player _player)
        {
            try
            {
            using (Packet _packet = new Packet((int)ServerPacket.SetupAPlayer))
            {
                _packet.Write(_player.RiderID);
                _packet.Write(_player.Username);
                _packet.Write(_player.RiderRootPosition);
                _packet.Write(_player.RiderRootRotation);
                _packet.Write(_player.Ridermodel);
                _packet.Write(_player.Ridermodelbundlename);
                _packet.Write(_player.MapName);

                if(_player.Ridermodel == "Daryien")
                {
                _packet.Write(_player.Gear.RiderTextures.Count);
                if (_player.Gear.RiderTextures.Count > 0)
                {
                    for (int i = 0; i < _player.Gear.RiderTextures.Count; i++)
                    {
                        _packet.Write(_player.Gear.RiderTextures[i].Nameoftexture);
                        _packet.Write(_player.Gear.RiderTextures[i].NameofparentGameObject);
                        _packet.Write(_player.Gear.RiderTextures[i].Matnum);
                        _packet.Write(_player.Gear.RiderTextures[i].directory);
                    }
                }

                    _packet.Write(_player.Gear.Capforward);
                }


                // garage
                _packet.Write(_player.Gear.Garagesave.Length);
                _packet.Write(_player.Gear.Garagesave);

                _packet.Write(_player.PlayerObjects.Count);
                if (_player.PlayerObjects.Count > 0)
                {
                    for (int i = 0; i < _player.PlayerObjects.Count; i++)
                    {
                        _packet.Write(_player.PlayerObjects[i].NameofObject);
                        _packet.Write(_player.PlayerObjects[i].NameOfFile);
                        _packet.Write(_player.PlayerObjects[i].NameofAssetBundle);

                        _packet.Write(_player.PlayerObjects[i].Position);
                        _packet.Write(_player.PlayerObjects[i].Rotation);
                        _packet.Write(_player.PlayerObjects[i].Scale);
                        _packet.Write(_player.PlayerObjects[i].ObjectID);
                        _packet.Write(_player.PlayerObjects[i].Directory);
                    }
                }


                // Park
                _packet.Write(_player.PlayerObjects.Count);
                if (_player.PlayerObjects.Count > 0)
                {
                    for (int i = 0; i < _player.PlayerObjects.Count; i++)
                    {
                        _packet.Write(_player.PlayerObjects[i].NameofObject);
                        _packet.Write(_player.PlayerObjects[i].NameOfFile);
                        _packet.Write(_player.PlayerObjects[i].NameofAssetBundle);

                        _packet.Write(_player.PlayerObjects[i].Position);
                        _packet.Write(_player.PlayerObjects[i].Rotation);
                        _packet.Write(_player.PlayerObjects[i].Scale);
                        _packet.Write(_player.PlayerObjects[i].ObjectID);
                        _packet.Write(_player.PlayerObjects[i].Directory);
                    }
                }





                try
                {
                SendtoOne(_toClient, _packet.ToArray(),Valve.Sockets.SendFlags.Reliable);
                }
                catch (Exception x)
                {
                    Console.WriteLine("Failed To Send Setup Command  " + x);
                }


            }

            }
            catch (Exception x)
            {

                Console.WriteLine("Setup new Player error : " + x);
            }

        }

        /// <summary>
        /// Fires when a new player joins, sending everyone back to them in packs of 5 to ease the potential load of many players
        /// </summary>
        /// <param name="_toclient"></param>
        /// <param name="_players"></param>
        public static void SetupAllOnlinePlayers(uint _toclient, List<Player> _players)
        {
            try
            {

            List<Player> listof5 = new List<Player>();
            int count = 0;
            foreach(Player __player in _players.ToList())
            {
               
                if(__player.RiderID != _toclient)
                {

                listof5.Add(__player);
                count++;
                }
                if (listof5.Count == 5 | count == _players.Count -1 && listof5.Count>0)
                {
                    //Console.WriteLine($"Sending {listof5.Count} players out of {_players.Count} in Player list");
                    using (Packet _packet = new Packet((int)ServerPacket.SetupAllOnlinePlayers))
                    {
                        // amount of players in this bundle, for the last bundle or if less than 5 are on
                        _packet.Write(listof5.Count);
                        foreach (Player _player in listof5.ToList())
                        {
                            // general
                            _packet.Write(_player.RiderID);
                            _packet.Write(_player.Username);
                            _packet.Write(_player.RiderRootPosition);
                            _packet.Write(_player.RiderRootRotation);
                            _packet.Write(_player.Ridermodel);
                            _packet.Write(_player.Ridermodelbundlename);
                            _packet.Write(_player.MapName);

                            // daryien
                            if(_player.Ridermodel == "Daryien")
                            {
                            _packet.Write(_player.Gear.RiderTextures.Count);
                            if (_player.Gear.RiderTextures.Count > 0)
                            {
                                for (int i = 0; i < _player.Gear.RiderTextures.Count; i++)
                                {
                                    _packet.Write(_player.Gear.RiderTextures[i].Nameoftexture);
                                    _packet.Write(_player.Gear.RiderTextures[i].NameofparentGameObject);
                                    _packet.Write(_player.Gear.RiderTextures[i].Matnum);
                                    _packet.Write(_player.Gear.RiderTextures[i].directory);
                                    }
                            }

                                _packet.Write(_player.Gear.Capforward);
                            }

                            // garage
                            _packet.Write(_player.Gear.Garagesave.Length);
                            _packet.Write(_player.Gear.Garagesave);


                            // Park
                            _packet.Write(_player.PlayerObjects.Count);
                            if (_player.PlayerObjects.Count > 0)
                            {
                                for (int i = 0; i < _player.PlayerObjects.Count; i++)
                                {
                                    _packet.Write(_player.PlayerObjects[i].NameofObject);
                                    _packet.Write(_player.PlayerObjects[i].NameOfFile);
                                    _packet.Write(_player.PlayerObjects[i].NameofAssetBundle);

                                    _packet.Write(_player.PlayerObjects[i].Position);
                                    _packet.Write(_player.PlayerObjects[i].Rotation);
                                    _packet.Write(_player.PlayerObjects[i].Scale);
                                    _packet.Write(_player.PlayerObjects[i].ObjectID);
                                    _packet.Write(_player.PlayerObjects[i].Directory);
                                }
                            }
                          


                        }

                        try
                        {
                            SendtoOne(_toclient, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                            listof5.Clear();
                            Console.WriteLine("Player bundle sent");
                        }
                        catch (Exception x)
                        {
                        Console.WriteLine($"Failed To Send Player bundle: Players: {listof5.Count} in list, total to send: {_players}: Error {x}");

                        }

                    }

                
                }

            }

                
            }
            catch (Exception x)
            {
                Console.WriteLine($"Setup all online players error : {x}");
            }





        }

        /// <summary>
        /// Sends to all players except this one
        /// </summary>
        /// <param name="_aboutplayer"></param>
        /// <param name="count"></param>
        /// <param name="pos"></param>
        /// <param name="rot"></param>
        public static void SendATransformUpdate(uint _aboutplayer, Packet inpacket, long _serverstamp, int _ping)
        {
            using (Packet _Packet = new Packet((int)ServerPacket.SendTransformUpdate))
            {
                _Packet.Write(_ping);
                _Packet.Write(_serverstamp);
                _Packet.Write(_aboutplayer);
                _Packet.Write(inpacket.ToArray().Length);
                _Packet.Write(inpacket.ToArray());



                try
                {
                    SendToAllReadyToRoll(_aboutplayer, _Packet.ToArray(), Valve.Sockets.SendFlags.Unreliable);
                }
                catch (Exception x)
                {
                    Console.WriteLine("Failed transform relay, player left?  : " + x);
                }



            }
        }

        /// <summary>
        /// send connection int that disconnected to all remaining connections
        /// </summary>
        /// <param name="ClientThatDisconnected"></param>
        public static void DisconnectTellAll(uint ClientThatDisconnected)
        {
            try
            {
            if(Server.Players[ClientThatDisconnected]!= null)
            {
            using (Packet _packet = new Packet((int)ServerPacket.DisconnectedPlayer))
            {
                _packet.Write(ClientThatDisconnected);
                try
                {
                    SendToAll(ClientThatDisconnected, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                }
                catch (Exception X)
                {
                    Console.WriteLine("Failed Disconnect tell all, player just left? : " + X);
                }
            }

            }

            }
            catch (Exception x)
            {
                Console.WriteLine($"No players informed");
            }



        }

        public static void SendAudioUpdate(uint _from, byte[] Relayedpacket,Valve.Sockets.SendFlags flag)
        {
           
                try
                {
                  SendToAllReadyToRoll(_from, Relayedpacket, flag);
                }
                catch (Exception x)
                {
                    Console.WriteLine("Failed Audio relay, player left?  :" + x);
                }
            
            
        }

        public static void SendTextMessageFromPlayerToAll(uint _fromplayer, string _message)
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
                Console.WriteLine("Failed send tex to all, player left? :    " + x);
            }

        }

        public static void SendTextFromServerToOne(uint _to, string message)
        {
            using (Packet _packet = new Packet((int)ServerPacket.SendText))
            {
                _packet.Write((uint)0);
                _packet.Write(message);
                _packet.Write(4);
                SendtoOne(_to, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        public static void SendFileSegment(FileSegment segment)
        {
            
                        using (Packet _packet = new Packet((int)ServerPacket.SendFileSegment))
                        {
                            _packet.Write(segment.NameofFile);
                            _packet.Write(segment.this_segment_num);
                            _packet.Write(segment.segment_count);
                            _packet.Write(segment.segment.Length);
                            _packet.Write(segment.segment);
                            _packet.Write(segment.FileByteCount);
                            _packet.Write(segment.path);

                            SendtoOne(segment.client, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
                           Console.WriteLine($"Sending {segment.NameofFile} to {segment.client}: Packet {segment.this_segment_num} of {segment.segment_count}");
                        }
                    

        }

        public static void SendGearUpdate(uint __toplayer, Packet _packet)
        {
            try
            {
            SendToAll(__toplayer, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
            catch (Exception x)
            {
                Console.WriteLine("Gear update error : " + x);
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

       public static void DisconnectPlayer(string msg, uint _to)
        {
            using (Packet _packet = new Packet((int)ServerPacket.Disconnectyou))
            {
                _packet.Write(msg);
              
                SendtoOne(_to, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }

        public static void SpawnAnObject(uint to,uint _owner,NetGameObject _netobj)
        {
            using (Packet _packet = new Packet((int)ServerPacket.SpawnNewObjectSend))
            {

                _packet.Write(_netobj.NameofObject);
                _packet.Write(_netobj.NameOfFile);
                _packet.Write(_netobj.NameofAssetBundle);

                _packet.Write(_netobj.Position);
                _packet.Write(_netobj.Rotation);
                _packet.Write(_netobj.Scale);
                _packet.Write(_netobj.ObjectID);
                _packet.Write(_netobj.Directory);

                _packet.Write(_owner);


                SendtoOne(to, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        public static void DestroyAnObjectToAllButOwner(uint _ownerID, int ObjectID)
        {
            using (Packet _packet = new Packet((int)ServerPacket.DestroyAnObject))
            {
                _packet.Write(_ownerID);
                _packet.Write(ObjectID);

                SendToAllReadyToRoll(_ownerID, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);

            }
        }

        public static void MoveAnObject(uint _ownerId, NetGameObject _netobj)
        {
            using (Packet _packet = new Packet((int)ServerPacket.MoveAnObject))
            {

                _packet.Write(_netobj.Position);
                _packet.Write(_netobj.Rotation);
                _packet.Write(_netobj.Scale);
                _packet.Write(_netobj.ObjectID);

                _packet.Write(_ownerId);

                Console.WriteLine("Object update");
                SendToAllReadyToRoll(_ownerId, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }

        }

        public static void Update(uint Conn,List<string> updatefiles)
        {
            using(Packet _packet = new Packet((int)ServerPacket.Update))
            {
                _packet.Write(Server.VERSIONNUMBER);
                _packet.Write(updatefiles.Count);
                for (int i = 0; i < updatefiles.Count; i++)
                {
                    _packet.Write(updatefiles[i]);
                }
                

                SendtoOne(Conn, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        public static void InviteToSpawn(uint from,uint goingto, Vector3 pos, Vector3 rot)
        {
            using(Packet _packet = new Packet((int)ServerPacket.InviteToSpawn))
            {
                _packet.Write(from);
                _packet.Write(pos);
                _packet.Write(rot);

                SendtoOne(goingto, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        public static void LoginGood(uint _to)
        {
            using(Packet _packet = new Packet((int)ServerPacket.LoginGood))
            {

                SendtoOne(_to, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }



        }

        public static void StreamAdminInfo(uint to, AdminDataStream stream)
        {
            using(Packet _packet = new Packet((int)ServerPacket.AdminStream))
            {
                // Total Reliable messages pending
                _packet.Write(stream.bytesinpersec);
                _packet.Write(stream.Bytesoutpersec);
                _packet.Write(stream.inindexes);
                _packet.Write(stream.outindexes);
                _packet.Write(stream.PendingRel);
                _packet.Write(stream.PendingUnrel);
                _packet.Write(stream.Playercount);

                SendtoOne(to, _packet.ToArray(), Valve.Sockets.SendFlags.Reliable);
            }
        }

        #endregion




    }
}
