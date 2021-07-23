using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Valve.Sockets;
using System.IO;
using System.Collections;


namespace PIPE_Valve_Online_Server
{
    class FileSyncServer
    {


        public void Update()
        {
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

                   // add to send rack
                    FileSegment Segment = new FileSegment(Packet, Index.NameOfFile, Index.TotalPacketsinFile, PacketnoToSend, Index.PlayerTosendTo, Index.Filetype, Index.ByteLength,Index.Fileinfo.DirectoryName);
                      ServerSend.SendFileSegment(Segment);
                      AddedAPacket = true;
                    Index.PacketNumbersStored.Add(PacketnoToSend);

                    _stream.Close();


                }
            }





        }

       













    }
}
