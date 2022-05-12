using FrostyPipeServer.ServerFiles;
using UnityEngine;
using RiptideNetworking;

namespace FrostyPipeServer.RipTide
{
    public class OutgoingMessage
    {

        static void SendtoOne(ushort client,Message mess)
        {
            ThreadTransfer.GiveToSendThread(() => 
            {
                try
                {
                 Servermanager.server.Send(mess, client);
                }
                catch (Exception x)
                {
                    RiptideNetworking.Utils.RiptideLogger.Log(RiptideNetworking.Utils.LogType.warning, "Socket send error : " + x);
                    throw;
                }
            });
        }
        static void SendToAllReadyToRoll(ushort from, Message mess)
        {
            ThreadTransfer.GiveToSendThread(() =>
            {
                try
                {
                    lock (Servermanager.Players)
                    {
                    foreach (Player p in Servermanager.Players.Values)
                    {
                        if (p.ReadytoRoll && p.Uniqueid != from) // map matcher here
                        {
                            Servermanager.server.Send(mess, p.Uniqueid);
                        }
                    }

                    }
                }
                catch (Exception x)
                {
                    RiptideNetworking.Utils.RiptideLogger.Log(RiptideNetworking.Utils.LogType.warning, "Socket send error : " + x);
                    
                }

            });
        }
        static void SendtoAllGarageEnabled(ushort from, Message mess)
        {
            ThreadTransfer.GiveToSendThread(() =>
            {
                try
                {
                    lock (Servermanager.Players)
                    {
                        foreach (Player p in Servermanager.Players.Values)
                        {
                            if (p.GarageEnabled && p.Uniqueid != from)
                            {
                                Servermanager.server.Send(mess, p.Uniqueid);
                            }
                        }

                    }

                }
                catch (Exception x )
                {
                    RiptideNetworking.Utils.RiptideLogger.Log(RiptideNetworking.Utils.LogType.warning, "Socket send error : " + x);
                    throw;
                }

            });
        }
        static void SendToAll(ushort from, Message mess)
        {
            ThreadTransfer.GiveToSendThread(() =>
            {
                try
                {
                    lock (Servermanager.Players)
                    {
                        foreach (Player p in Servermanager.Players.Values)
                        {
                            if (p.Uniqueid != from)
                            {
                                Servermanager.server.Send(mess, p.Uniqueid);
                            }
                        }

                    }

                }
                catch (Exception x)
                {
                    RiptideNetworking.Utils.RiptideLogger.Log(RiptideNetworking.Utils.LogType.warning, "Socket send error : " + x);
                    throw;
                }
            });
        }


        #region Send Fuctions

        /// <summary>
        ///  send inital welcome on connection obtained to the server
        /// </summary>
        /// <param name="ClientID"></param>
        public static void Welcome(ushort ClientID)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.Welcome);
            
                _packet.AddString($"Connected to {Servermanager.config.Servername} \n Version:{Servermanager.VERSIONNUMBER}");
                _packet.AddString(Servermanager.config.GarageVersion);
                _packet.AddInt(Servermanager.config.TickrateMax);

            SendtoOne(ClientID,_packet);
        }

        public static void RequestAllParts(ushort Clientid)
        {
            Console.WriteLine("Requesting parts");
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.RequestAllParts);
                // no data needed
                SendtoOne(Clientid, _packet);
            
        }

        public static void RequestFile(ushort Clientid, string name, List<int> _packetsihave, string dir)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.RequestFile);
         
                _packet.AddString(name);
                _packet.AddString(dir);
                _packet.AddInt(_packetsihave.Count);
                for (int i = 0; i < _packetsihave.Count; i++)
                {
                    _packet.AddInt(_packetsihave[i]);
                }


                SendtoOne(Clientid, _packet);
            

            // if index exists, add id to ReceivingFrom, else create request index
            foreach (SendReceiveIndex sr in ServerData.IncomingIndexes)
            {
                if (sr.NameOfFile.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "") == name.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", ""))
                {
                    bool playerlogged = false;
                    foreach (int play in sr.PlayersRequestedFrom)
                    {
                        if (play == Clientid)
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

        public static void FileStatus(ushort _player, string name, int Status)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.FileStatus);
            
                _packet.AddString(name);
                _packet.AddInt(Status);

                SendtoOne(_player, _packet);
            
        }

        /// <summary>
        /// Fires when an new player joins,sending them to everyone
        /// </summary>
        /// <param name="_toClient"></param>
        /// <param name="_player"></param>
        public static void NewPlayerArrived(ushort _toClient, Player _player)
        {
            try
            {
                Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.SetupAPlayer);
               
                    _packet.AddUShort(_player.Uniqueid);
                    _packet.AddString(_player.Username);
                    _packet.AddVector3(_player.RiderRootPosition);
                    _packet.AddVector3(_player.RiderRootRotation);
                    _packet.AddString(_player.Ridermodel);
                    _packet.AddString(_player.Ridermodelbundlename);
                    _packet.AddString(_player.MapName);

                    if (_player.Ridermodel == "Daryien")
                    {
                        _packet.AddInt(_player.Gear.RiderTextures.Count);
                        if (_player.Gear.RiderTextures.Count > 0)
                        {
                            for (int i = 0; i < _player.Gear.RiderTextures.Count; i++)
                            {
                                _packet.AddString(_player.Gear.RiderTextures[i].Nameoftexture);
                                _packet.AddString(_player.Gear.RiderTextures[i].NameofparentGameObject);
                                _packet.AddInt(_player.Gear.RiderTextures[i].Matnum);
                                _packet.AddString(_player.Gear.RiderTextures[i].directory);
                            }
                        }

                        _packet.AddBool(_player.Gear.Capforward);
                    }


                    // garage
                    _packet.AddString(_player.Gear.garagexml);
                    _packet.AddString(_player.Gear.presetname);

                    _packet.AddInt(_player.PlayerObjects.Count);
                    if (_player.PlayerObjects.Count > 0)
                    {
                        for (int i = 0; i < _player.PlayerObjects.Count; i++)
                        {
                            _packet.AddString(_player.PlayerObjects[i].NameofObject);
                            _packet.AddString(_player.PlayerObjects[i].NameOfFile);
                            _packet.AddString(_player.PlayerObjects[i].NameofAssetBundle);

                            _packet.AddVector3(_player.PlayerObjects[i].Position);
                            _packet.AddVector3(_player.PlayerObjects[i].Rotation);
                            _packet.AddVector3(_player.PlayerObjects[i].Scale);
                            _packet.AddInt(_player.PlayerObjects[i].ObjectID);
                            _packet.AddString(_player.PlayerObjects[i].Directory);
                        }
                    }


                    // Park
                    _packet.AddInt(_player.PlayerObjects.Count);
                    if (_player.PlayerObjects.Count > 0)
                    {
                        for (int i = 0; i < _player.PlayerObjects.Count; i++)
                        {
                            _packet.AddString(_player.PlayerObjects[i].NameofObject);
                            _packet.AddString(_player.PlayerObjects[i].NameOfFile);
                            _packet.AddString(_player.PlayerObjects[i].NameofAssetBundle);

                            _packet.AddVector3(_player.PlayerObjects[i].Position);
                            _packet.AddVector3(_player.PlayerObjects[i].Rotation);
                            _packet.AddVector3(_player.PlayerObjects[i].Scale);
                            _packet.AddInt(_player.PlayerObjects[i].ObjectID);
                            _packet.AddString(_player.PlayerObjects[i].Directory);
                        }
                    }


                    try
                    {
                        SendtoOne(_toClient, _packet);
                        SendTextFromServerToOne(_toClient, _player.Username + $"{Servermanager.config.RandomSpawnMessages[new System.Random().Next(Servermanager.config.RandomSpawnMessages.Length)]}");
                    }
                    catch (Exception x)
                    {
                        Console.WriteLine("Failed To Send Setup Command  " + x);
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
        public static void PlayersAlreadyOnline(ushort _toclient, List<Player> _players)
        {
            try
            {

                List<Player> listof5 = new List<Player>();
                int count = 0;
                foreach (Player player in _players)
                {

                    if (player.Uniqueid != _toclient)
                    {

                        listof5.Add(player);
                        count++;
                        if (listof5.Count == 5 | count == _players.Count - 1 && listof5.Count > 0)
                        {
                            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.SetupAllOnlinePlayers);
                            //Console.WriteLine($"Sending {listof5.Count} players out of {_players.Count} in Player list");
                        
                                // amount of players in this bundle, for the last bundle or if less than 5 are on
                                _packet.AddInt(listof5.Count);
                                foreach (Player _player in listof5.ToList())
                                {
                               
                                    _packet.AddUShort(_player.Uniqueid);
                                    _packet.AddString(_player.Username);
                                    _packet.AddVector3(_player.RiderRootPosition);
                                    _packet.AddVector3(_player.RiderRootRotation);
                                    _packet.AddString(_player.Ridermodel);
                                    _packet.AddString(_player.Ridermodelbundlename);
                                    _packet.AddString(_player.MapName);

                                    if (_player.Ridermodel == "Daryien")
                                    {
                                        _packet.AddInt(_player.Gear.RiderTextures.Count);
                                        if (_player.Gear.RiderTextures.Count > 0)
                                        {
                                            for (int i = 0; i < _player.Gear.RiderTextures.Count; i++)
                                            {
                                                _packet.AddString(_player.Gear.RiderTextures[i].Nameoftexture);
                                                _packet.AddString(_player.Gear.RiderTextures[i].NameofparentGameObject);
                                                _packet.AddInt(_player.Gear.RiderTextures[i].Matnum);
                                                _packet.AddString(_player.Gear.RiderTextures[i].directory);
                                            }
                                        }

                                        _packet.AddBool(_player.Gear.Capforward);
                                    }


                                    // garage
                                    _packet.AddString(_player.Gear.garagexml);
                                    _packet.AddString(_player.Gear.presetname);

                                    _packet.AddInt(_player.PlayerObjects.Count);
                                    if (_player.PlayerObjects.Count > 0)
                                    {
                                        for (int i = 0; i < _player.PlayerObjects.Count; i++)
                                        {
                                            _packet.AddString(_player.PlayerObjects[i].NameofObject);
                                            _packet.AddString(_player.PlayerObjects[i].NameOfFile);
                                            _packet.AddString(_player.PlayerObjects[i].NameofAssetBundle);

                                            _packet.AddVector3(_player.PlayerObjects[i].Position);
                                            _packet.AddVector3(_player.PlayerObjects[i].Rotation);
                                            _packet.AddVector3(_player.PlayerObjects[i].Scale);
                                            _packet.AddInt(_player.PlayerObjects[i].ObjectID);
                                            _packet.AddString(_player.PlayerObjects[i].Directory);
                                        }
                                    }


                                    // Park
                                    _packet.AddInt(_player.PlayerObjects.Count);
                                    if (_player.PlayerObjects.Count > 0)
                                    {
                                        for (int i = 0; i < _player.PlayerObjects.Count; i++)
                                        {
                                            _packet.AddString(_player.PlayerObjects[i].NameofObject);
                                            _packet.AddString(_player.PlayerObjects[i].NameOfFile);
                                            _packet.AddString(_player.PlayerObjects[i].NameofAssetBundle);

                                            _packet.AddVector3(_player.PlayerObjects[i].Position);
                                            _packet.AddVector3(_player.PlayerObjects[i].Rotation);
                                            _packet.AddVector3(_player.PlayerObjects[i].Scale);
                                            _packet.AddInt(_player.PlayerObjects[i].ObjectID);
                                            _packet.AddString(_player.PlayerObjects[i].Directory);
                                        }
                                    }




                                }

                                try
                                {
                                    SendtoOne(_toclient, _packet);
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
        public static void SendATransformUpdate(ushort _aboutplayer, Message _Packet, long _serverstamp, int _ping)
        {
            Message Relay = Message.Create(MessageSendMode.unreliable, ServerPacket.SendTransformUpdate);
                Relay.AddInt(_ping);
                Relay.AddLong(_serverstamp);
                Relay.AddUShort(_aboutplayer);
                byte[] data = new byte[_Packet.UnreadLength];
                Array.Copy(_Packet.Bytes, _Packet.CurrentReadPosition, data, 0, data.Length);
                Array.Copy(data, 0, Relay.Bytes, Relay.CurrentWritePosition, data.Length);
                Relay.ResetReadWritepos(0,Relay.CurrentWritePosition + data.Length);
               
                SendToAllReadyToRoll(_aboutplayer, Relay);
               
        }

        /// <summary>
        /// send connection int that disconnected to all remaining connections
        /// </summary>
        /// <param name="ClientThatDisconnected"></param>
        public static void DisconnectTellAll(ushort ClientThatDisconnected)
        {
            try
            {
                if (Servermanager.Players[ClientThatDisconnected] != null)
                {
                    Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.DisconnectedPlayer);
                    
                        _packet.AddUShort(ClientThatDisconnected);
                        try
                        {
                            SendToAll(ClientThatDisconnected, _packet);
                        }
                        catch (Exception X)
                        {
                            Console.WriteLine("Failed Disconnect tell all, player just left? : " + X);
                        }
                    

                }

            }
            catch (Exception x)
            {
                Console.WriteLine($"No players informed");
            }



        }

        public static void SendAudioUpdate(ushort _from, Message Relayedpacket)
        {

            try
            {
                SendToAllReadyToRoll(_from, Relayedpacket);
            }
            catch (Exception x)
            {
                Console.WriteLine("Failed Audio relay, player left?  :" + x);
            }


        }

        public static void SendTextMessageFromPlayerToAll(ushort _fromplayer, string _message)
        {
            try
            {
                Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.SendText);
                
                    _packet.AddUShort(2);
                    _packet.AddString(_message);
                    SendToAll(_fromplayer, _packet);
                    SendtoOne(_fromplayer, _packet);

            }
            catch (Exception x)
            {
                Console.WriteLine("Failed send tex to all, player left? :    " + x);
            }

        }

        public static void SendTextFromServerToOne(ushort _to, string message)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.SendText);

            _packet.AddUShort(0);
            _packet.AddString(message);
            SendtoOne(_to, _packet);
            
        }

        public static void SendFileSegment(FileSegment segment)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.SendFileSegment);
           
                _packet.AddString(segment.NameofFile);
                _packet.AddInt(segment.this_segment_num);
                _packet.AddInt(segment.segment_count);
                _packet.AddInt(segment.segment.Length);
                _packet.AddBytes(segment.segment);
                _packet.AddLong(segment.FileByteCount);
                _packet.AddString(segment.path);

                SendtoOne(segment.client, _packet);
                Console.WriteLine($"Sending {segment.NameofFile} to {segment.client}: Packet {segment.this_segment_num} of {segment.segment_count}");
            


        }

        public static void SendGearUpdate(ushort __toplayer, Message _packet)
        {
            try
            {
                SendtoAllGarageEnabled(__toplayer, _packet);
            }
            catch (Exception x)
            {
                Console.WriteLine("Gear update error : " + x);
            }
        }

        public static void SendMapName(ushort _from, string name)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.SendMapName);
            
                _packet.AddString(name);
                _packet.AddUShort(_from);
                SendToAll(_from, _packet);
            

        }

        public static void DisconnectPlayer(string msg, ushort _to)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.Disconnectyou);
            
                _packet.AddString(msg);

                SendtoOne(_to, _packet);
            

        }

        public static void SpawnAnObject(ushort to, ushort _owner, NetGameObject _netobj)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.SpawnNewObjectSend);
           

                _packet.AddString(_netobj.NameofObject);
                _packet.AddString(_netobj.NameOfFile);
                _packet.AddString(_netobj.NameofAssetBundle);

                _packet.AddVector3(_netobj.Position);
                _packet.AddVector3(_netobj.Rotation);
                _packet.AddVector3(_netobj.Scale);
                _packet.AddInt(_netobj.ObjectID);
                _packet.AddString(_netobj.Directory);

                _packet.AddUShort(_owner);


                SendtoOne(to, _packet);
            
        }

        public static void DestroyAnObjectToAllButOwner(ushort _ownerID, int ObjectID)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.DestroyAnObject);
            
                _packet.AddUShort(_ownerID);
                _packet.AddInt(ObjectID);

                SendToAllReadyToRoll(_ownerID, _packet);

            
        }

        public static void MoveAnObject(ushort _ownerId, NetGameObject _netobj)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.MoveAnObject);
            

                _packet.AddVector3(_netobj.Position);
                _packet.AddVector3(_netobj.Rotation);
                _packet.AddVector3(_netobj.Scale);
                _packet.AddInt(_netobj.ObjectID);

                _packet.AddUShort(_ownerId);

                Console.WriteLine("Object update");
                SendToAllReadyToRoll(_ownerId, _packet);
            

        }

        public static void Update(ushort Conn, List<string> updatefiles)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.Update);
            
                _packet.AddFloat(Servermanager.VERSIONNUMBER);
                _packet.AddInt(updatefiles.Count);
                for (int i = 0; i < updatefiles.Count; i++)
                {
                    _packet.AddString(updatefiles[i]);
                }


                SendtoOne(Conn, _packet);
            
        }

        public static void InviteToSpawn(ushort from, ushort goingto, Vector3 pos, Vector3 rot)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.InviteToSpawn);
            
                _packet.AddUShort(from);
                _packet.AddVector3(pos);
                _packet.AddVector3(rot);

                SendtoOne(goingto, _packet);
            
        }

        public static void LoginGood(ushort _to)
        {
            Message _packet = Message.Create(MessageSendMode.reliable, ServerPacket.LoginGood);
           
                SendtoOne(_to, _packet);
            



        }

        

        #endregion


    }
}
