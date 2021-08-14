using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

namespace PIPE_Valve_Console_Client
{
    /// <summary>
    /// Updated by tick rate on server thread calculated by system time
    /// </summary>
    class NetworkThread
    {
        
      
        /// <summary>
        /// fixed update function of sendtoserver thread, everything done by the secondary thread at tick rate is here
        /// </summary>
        public static void Update()
        {
           
            // Run Send on anything left by Unity Thread
            SendToServerThread.UpdateMain();
            
            // run receive and send Data Back to Unity Thread
            GameNetworking.instance.Run();

            //Do FileSyncing

            // if outgoing is not going to hit Valves buffer cap (files are sent in 400Kb segments which is many ValveMessages)

            if (FileSyncing.OutGoingIndexes.Count > 0)
            {

            if (GameNetworking.instance.GetPendingReliable() < 200)
            {
                
                // only add one each time
                bool AddedAPacket = false;

                try
                {
                // find the first index that IsSending
                foreach (SendReceiveIndex sr in FileSyncing.OutGoingIndexes)
                {
                    if (sr.IsSending && !AddedAPacket)
                    {

                    if(sr.PacketNumbersStored.Count == sr.TotalPacketsinFile && sr.TotalPacketsinFile !=0)
                    {
                        // done
                        sr.IsSending = false;
                    }

                        byte[] Packet = null;
                        FileStream _stream = sr.Fileinfo.OpenRead();



                        // determine packet number to send
                        int PacketnoToSend = 0;
                        bool foundapacketno = false;

                        for (int i = 1; i <= sr.TotalPacketsinFile; i++)
                        {
                            if (!foundapacketno)
                            {
                            bool gotit = false;
                            foreach(int t in sr.PacketNumbersStored)
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
                            SendToUnityThread.instance.ExecuteOnMainThread(() =>
                            {
                                UnityEngine.Debug.Log($"no packet no!");
                            });
                            sr.IsSending = false;
                            return;
                        }




                        
                       

                        // position to read from
                        int PostoStartAt = 0;
                            PostoStartAt = (PacketnoToSend - 1) * 400000;
                        
                        


                        // make byte array 400,000 unless its the last packet
                        if(PacketnoToSend == sr.TotalPacketsinFile && sr.TotalPacketsinFile == 1)
                        {
                            Packet = new byte[_stream.Length];

                            SendToUnityThread.instance.ExecuteOnMainThread(() =>
                            {
                                UnityEngine.Debug.Log($"made packet:{Packet.Length}: {PacketnoToSend} of {sr.TotalPacketsinFile}");
                            });
                        }
                        if (PacketnoToSend < sr.TotalPacketsinFile && sr.TotalPacketsinFile > 1)
                        { 
                            Packet = new byte[400000];

                            SendToUnityThread.instance.ExecuteOnMainThread(() =>
                            {
                                UnityEngine.Debug.Log($"made packet:{Packet.Length}: {PacketnoToSend} of {sr.TotalPacketsinFile}");
                            });
                        }
                        if(PacketnoToSend == sr.TotalPacketsinFile && sr.TotalPacketsinFile > 1)
                        {
                            Packet = new byte[_stream.Length - (400000 * (PacketnoToSend - 1))];

                            SendToUnityThread.instance.ExecuteOnMainThread(() =>
                            {
                                UnityEngine.Debug.Log($"made packet:{Packet.Length}: {PacketnoToSend} of {sr.TotalPacketsinFile}");
                            });
                        }

                        if(Packet == null)
                        {
                            SendToUnityThread.instance.ExecuteOnMainThread(() =>
                            {
                                UnityEngine.Debug.Log($"Packet null");
                            });
                            return;
                        }

                        // Read bytes
                        _stream.Seek(PostoStartAt, SeekOrigin.Begin);
                       _stream.Read(Packet, 0, Packet.Length);

                        // add to send rack
                        FileSegment Segment = new FileSegment(Packet,sr.NameOfFile, sr.TotalPacketsinFile, PacketnoToSend, sr.ByteLength,sr.Fileinfo.DirectoryName + "/");
                        ClientSend.SendFileSegment(Segment);
                        AddedAPacket = true;
                        sr.PacketNumbersStored.Add(PacketnoToSend);

                        _stream.Close();

                        SendToUnityThread.instance.ExecuteOnMainThread(() =>
                        {
                            UnityEngine.Debug.Log($"Sent Packet: {sr.NameOfFile}: {PacketnoToSend} of {sr.TotalPacketsinFile}");
                        });

                            if(sr.PacketNumbersStored.Count == sr.TotalPacketsinFile)
                            {
                                sr.IsSending = false;
                            }

                    }

                }

                }

                catch (Exception x )
                {
                    SendToUnityThread.instance.ExecuteOnMainThread(() =>
                    {
                        Debug.Log("Network thread Filesync issue: " + x);   
                    });
                }

            }





            }

           
            
        }


    }
}
