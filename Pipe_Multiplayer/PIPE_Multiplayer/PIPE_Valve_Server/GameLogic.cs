using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PIPE_Valve_Online_Server
{
    class GameLogic
    {
       
        /// <summary>
        /// Add any functions here which need to fire at tick rate
        /// </summary>
        public static void Update()
        {
            try
            {
            // run any actions added by other threads, listen socket automatically adds incoming messages to be
            // processed at tick rate
            ThreadManager.UpdateMain();


            // FileSync
            bool AddedAPacket = false;
            foreach(SendReceiveIndex Index in ServerData.OutgoingIndexes.ToArray())
            {
                if (Index.isSending && Server.PendingReliableForConnection(Index.PlayerTosendTo) < 500 && !AddedAPacket)
                {

                    byte[] Packet = null;
                    FileStream _stream = Index.Fileinfo.OpenRead();



                    // determine packet number to send
                    int PacketnoToSend = 0;
                    bool foundapacketno = false;

                    for (int i = 1; i <= Index.TotalPacketsinFile; i++)
                    {
                        if (!foundapacketno)
                        {
                            bool gotit = false;
                            foreach (int t in Index.PacketNumbersStored)
                            {
                                if (t == i)
                                {
                                    gotit = true;
                                }
                            }

                            if (!gotit)
                            {
                                PacketnoToSend = i;
                                foundapacketno = true;

                            }

                        }


                    }
                    if (!foundapacketno)
                    {
                        Index.isSending = false;
                        return;
                    }







                    // position to read from
                    int PostoStartAt = 0;
                    PostoStartAt = (PacketnoToSend - 1) * 400000;




                    // make byte array 400,000 unless its the last packet
                    if (PacketnoToSend == Index.TotalPacketsinFile && Index.TotalPacketsinFile == 1)
                    {
                        Packet = new byte[_stream.Length];

                       
                    }
                    if (PacketnoToSend < Index.TotalPacketsinFile && Index.TotalPacketsinFile > 1)
                    {
                        Packet = new byte[400000];

                       
                    }
                    if (PacketnoToSend == Index.TotalPacketsinFile && Index.TotalPacketsinFile > 1)
                    {
                        Packet = new byte[_stream.Length - (400000 * (PacketnoToSend - 1))];

                        
                    }

                    if (Packet == null)
                    {
                       
                        return;
                    }

                    // Read bytes
                    _stream.Seek(PostoStartAt, SeekOrigin.Begin);
                    _stream.Read(Packet, 0, Packet.Length);

                    Console.WriteLine(Index.Fileinfo.FullName);
                    // add to send rack
                    FileSegment Segment = new FileSegment(Packet, Index.NameOfFile, Index.TotalPacketsinFile, PacketnoToSend,Index.PlayerTosendTo, Index.Filetype, Index.ByteLength, Index.Fileinfo.DirectoryName);
                    ServerSend.SendFileSegment(Segment);
                    AddedAPacket = true;
                    Index.PacketNumbersStored.Add(PacketnoToSend);

                    _stream.Close();


                }
            }


            // Admin
            foreach(Player pl in Server.Players.Values)
            {
                if(pl.AdminLoggedIn)
                {
                    if (pl.AdminStreamWatch.Elapsed.TotalSeconds > 5)
                    {
                        Server.GiveAdminStream(pl.RiderID);
                        pl.AdminStreamWatch.Reset();
                        pl.AdminStreamWatch.Start();
                    }

                }
            }


            }
            catch (Exception)
            {
               
            }
            

        }



    }
}
