using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace PIPE_Valve_Online_Server
{
    static class ServerData
    {





        public static List<SendReceiveIndex> OutgoingIndexes = new List<SendReceiveIndex>();
        public static List<SendReceiveIndex> IncomingIndexes = new List<SendReceiveIndex>();
        static Dictionary<int, string> FileTypeDirectory = new Dictionary<int, string>();
        public static List<FileSegment> UpdateSegments = new List<FileSegment>();

        public static string Rootdir = "Game Data/";
        public static string TexturesDir = Rootdir + "Textures/";
        public static string MapsDir = Rootdir + "Maps/";
        public static string ParkAssetsDir = Rootdir + "ParkAssets/";
        public static string BikeMeshesDir = Rootdir + "GarageMeshes/";
        public static string PlayerModelsDir = Rootdir + "PlayerModels/";
        static string TempDir = Rootdir + "Temp/";
        public static string UpdateDir = Rootdir + "Update/";
        
        
        
        public static List<string> BannedWords = new List<string>();
        public static List<string> BanMessageAlternates = new List<string>();

        public static string AdminPassword = "DaveMirra";

        
      




       static int received = 0;


        /// <summary>
        /// Load servers saved data
        /// </summary>
        public static void LoadData()
        {
            BannedWords = new List<string>
            {
                   {"gay"},
                   {"homo"},
                    {"queer"},
                     {"cunt"},
                      {"nigga"},
                       {"paki"},
                        {"bitch"},
                         {"niga" },
                          {"nga" },
                          {"ngga" },
                           {"btch" },
                            {"fuck" },
                             {"fck" },
                             {"prick"},

            };

            BanMessageAlternates = new List<string>
            {
                {"I admire you all" },
                {"You guys are Awesome" },
                {"Lets Session!" },
               
            };




            if (!Directory.Exists(Rootdir))
            {
                Directory.CreateDirectory(Rootdir);
            }
            if (!Directory.Exists(TexturesDir))
            {
                Directory.CreateDirectory(TexturesDir);
            }
            if (!Directory.Exists(MapsDir))
            {
                Directory.CreateDirectory(MapsDir);
            }
            if (!Directory.Exists(ParkAssetsDir))
            {
                Directory.CreateDirectory(ParkAssetsDir);
            }
            if (!Directory.Exists(BikeMeshesDir))
            {
                Directory.CreateDirectory(BikeMeshesDir);
            }
            if (!Directory.Exists(PlayerModelsDir))
            {
                Directory.CreateDirectory(PlayerModelsDir);
            }
            if (!Directory.Exists(TempDir))
            {
                Directory.CreateDirectory(TempDir);
            }
            if (!Directory.Exists(UpdateDir))
            {
                Directory.CreateDirectory(UpdateDir);
            }


            FileTypeDirectory = new Dictionary<int, string>
            {
                {1,TexturesDir },
                {2,MapsDir },
                {3,PlayerModelsDir },
                {4,BikeMeshesDir },
                {5,ParkAssetsDir },
                {6,UpdateDir },
            };
            

        }


        public static void BanPlayer(string _username, string IP, uint connid, int mins)
        {
            DateTime Time_of_release = DateTime.Now.AddMinutes(mins);

            Server.BanProfiles.Add(new BanProfile(IP, _username, connid, Time_of_release));


        }


        public static void FileCheckAndSend(string FileName, int Filetype, List<int> _packetsowned, uint _from)
        {
            FileInfo _fileinfo = null;


            // Find Fileinfo
                if (FileName != "")
                {
                   
                    foreach (FileInfo file in new DirectoryInfo(FileTypeDirectory[Filetype]).GetFiles())
                    {
                        if (file.Name.ToLower() == FileName.ToLower())
                        {
                        _fileinfo = file;
                        Console.WriteLine($"Located {FileName}");
                        }
                    }
                    
                }

                // file not found
                if(_fileinfo == null)
                {
                ServerSend.SendTextFromServerToOne(_from, "Server doesn't have this file yet");
                return;
                }


            FileStream Stream = _fileinfo.OpenRead();

            // get total packets and bytelength
            float PacketCount = 0;
            long length = Stream.Length;
            long mod = length % 400000;
            if (mod > 0)
            {
                PacketCount = length / 400000 + 1;
            }
            if (mod == 0)
            {
                PacketCount = length / 400000;
            }
            if (length < 400000)
            {
                PacketCount = 1;
            }


            SendReceiveIndex NewSend = new SendReceiveIndex(FileName, Filetype,(int)PacketCount);
            NewSend.ByteLength = length;
            NewSend.PlayerTosendTo = _from;
            NewSend.isSending = true;
            NewSend.Fileinfo = _fileinfo;

            Stream.Close();

            OutgoingIndexes.Add(NewSend);
            Console.WriteLine($"Sending {FileName} to {_from}");

        }



        public static void FileSaver(byte[] bytes, string name, int SegsTotal, int SegNo, uint _player, int _filetype, long Totalbytes)
        {

            // if no Temp file exists create one 
            if (!File.Exists(TempDir + name + ".temp"))
            {
                TempFile Temp = new TempFile();
                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.OpenWrite(TempDir + name + ".temp");

                
                Temp.ByteLengthOfFile = Totalbytes;
                Temp.FileType = _filetype;

                bf.Serialize(stream,Temp);
                stream.Close();

            }



            // grab Receive index and FileStream
            SendReceiveIndex InIndex = null;
            foreach (SendReceiveIndex r in IncomingIndexes)
            {
                if (r.NameOfFile == name)
                {
                    InIndex = r;
                }
            }
            FileStream _f = File.OpenWrite(TempDir + name);

            if (!InIndex.IsReceiving)
            {
                InIndex.IsReceiving = true;
            }
            InIndex.ByteLength = Totalbytes;
            InIndex.TotalPacketsinFile = SegsTotal;

            // determine where to write data
            int Startpos = (SegNo - 1) * 400000;

            // Write packet to its location in file
            _f.Seek(Startpos, SeekOrigin.Begin);
            _f.Write(bytes, 0, bytes.Length);
            _f.Close();


            // update SendReceive index
            InIndex.PacketNumbersStored.Add(SegNo);


            // Do Save or just update temp file
            if (InIndex.PacketNumbersStored.Count == InIndex.TotalPacketsinFile)
            {


                if (!Directory.Exists(FileTypeDirectory[_filetype])) Directory.CreateDirectory(FileTypeDirectory[_filetype]);
                File.Move(TempDir + name, FileTypeDirectory[_filetype] + InIndex.NameOfFile);
                File.Delete(TempDir + name + ".temp");
                
                // Tell Anyone that i've asked to send this file that i have it now ,if still online
                foreach(uint id in InIndex.PlayersRequestedFrom)
                {
                    if(Server.Players[id] != null)
                    {
                      ServerSend.FileStatus(id,name, _filetype, (int)FileStatus.Received);

                    }
                }

                IncomingIndexes.Remove(InIndex);
                Console.WriteLine($"Saved {name} to {FileTypeDirectory[_filetype]}");



            }
            else
            {
                // update temp file
                FileStream stream = File.OpenRead(TempDir + name + ".temp");
                BinaryFormatter bf = new BinaryFormatter();
                TempFile _temp = bf.Deserialize(stream) as TempFile;
                stream.Close();
                _temp.PacketNumbersStored.Add(SegNo);
                stream = File.OpenWrite(TempDir + name + ".temp");
                bf.Serialize(stream, _temp);
                stream.Close();

            }






        }


        public static void FileCheckAndRequest(string Filename, uint _fromclient, int _filetype)
        {
           
                    bool found = false;
                if (Filename.ToLower() != "e" && Filename != "" && Filename != " ")
                {

                     // find file
                    foreach (FileInfo file in new DirectoryInfo(FileTypeDirectory[_filetype]).GetFiles())
                    {
                        if (file.Name == Filename)
                        {
                            found = true;
                           // Console.WriteLine($"Matched {s.Nameoftexture} to {file.Name}");
                        }
                    }
                     

                    // if not found
                    if (!found)
                    {
                         List<int> Packetsiown = new List<int>();
                         if(File.Exists(TempDir + Filename + ".temp"))
                         {
                              BinaryFormatter bf = new BinaryFormatter();
                              FileStream _f = File.OpenRead(TempDir + Filename + ".temp");
                              TempFile temp = bf.Deserialize(_f) as TempFile;
                              _f.Close();


                              foreach(int seg in temp.PacketNumbersStored)
                              {
                                Packetsiown.Add(seg);

                              }
                         }
                
                         ServerSend.RequestFile(_fromclient, Filename, _filetype,Packetsiown);
                        Console.WriteLine(Filename + $" requested from {Server.Players[_fromclient].Username}");
                    }



                }
            
        }


        



    }

    /// <summary>
    /// Used for storing a segment of a textures byte array until all have been received
    /// </summary>
     [Serializable]
    class FileSegment
        {

            public byte[] segment;
            public string NameofFile;
            public int segment_count;
            public int this_segment_num;
            public uint client;
            public int Filetype;
            public int senttimes;
            public long Bytecount;
            
        
            
            public FileSegment(byte[] _seg,string _name,int _segcount,int _thissegno, uint _player, int _filetype, long _bytecount)
            {
                segment = _seg;
                NameofFile = _name;
                segment_count = _segcount;
                this_segment_num = _thissegno;
                client = _player;
                Filetype = _filetype;
                Bytecount = _bytecount;
            }

        }


    /// <summary>
	/// Keep track of a texture in bytes and its file name
	/// </summary>
	class FileBytes
    {
        public byte[] bytes;
        public string Filename;

        public FileBytes(byte[] _bytes, string _texname)
        {
            Filename = _texname;
            bytes = _bytes;
        }

    }



    [Serializable]
        public class BanProfile
        {
           public string IP;
           public string Username;
           public uint ConnId;
           public DateTime Timeofbanrelease;


            public BanProfile(string ip, string username, uint conid, DateTime timeofrelease)
            {
                IP = ip;
                Username = username;
                ConnId = conid;
            Timeofbanrelease = timeofrelease;
            }


        }


    public enum FileType
    {
        Texture = 1,
        Map = 2,
        PlayerModel = 3,
        Mesh = 4,
        ParkAsset = 5,
        Update = 6,
    }


    public class SendReceiveIndex
    {
        public List<int> PacketNumbersStored = new List<int>();
        public string NameOfFile;
        public int Filetype;
        public int TotalPacketsinFile;
        public bool isSending;
        public bool IsReceiving;
        public long ByteLength;
        public FileInfo Fileinfo;
        public uint PlayerTosendTo;
        public List<uint> PlayersRequestedFrom = new List<uint>();


        public SendReceiveIndex(string _filename, int _filetype, int _totalpackets)
        {
            NameOfFile = _filename;
            Filetype = _filetype;
            TotalPacketsinFile = _totalpackets;
        }

        public SendReceiveIndex(string _filename, int _filetype)
        {
            NameOfFile = _filename;
            Filetype = _filetype;
        }



    }


    public enum FileStatus
    {
        Received = 1,
        Cancel = 2,

    }



}
