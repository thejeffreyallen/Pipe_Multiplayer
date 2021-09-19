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
        public static List<FileSegment> UpdateSegments = new List<FileSegment>();

        public static string Rootdir = "Game Data/";
        static string TempDir = Rootdir + "Temp/";
        public static string UpdateDir = Rootdir + $"FrostyPGameManager/Updates/{Server.VERSIONNUMBER}/";
        public static string ModelsDir = Rootdir + "Custom Players/";
        public static string MapsDir = Rootdir + "CustomMaps/";
        public static string ParkAssetsDir = Rootdir + "FrostyPGameManager/ParkBuilder/Assetbundles/";



        public static List<string> BannedWords = new List<string>();
        public static List<string> BanMessageAlternates = new List<string>();

        public static string AdminPassword = "DaveMirra";

        
      






        /// <summary>
        /// Load servers saved data
        /// </summary>
        public static void LoadData()
        {
            if (!Directory.Exists(Rootdir))
            {
                Directory.CreateDirectory(Rootdir);
            }
            if (!Directory.Exists(TempDir))
            {
                Directory.CreateDirectory(TempDir);
            }
            if (!Directory.Exists(UpdateDir))
            {
                Directory.CreateDirectory(UpdateDir);
            }


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

            if(File.Exists(Rootdir + "SaveData.Server"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                Stream stream = File.OpenRead(Rootdir + "SaveData.Server");
                SaveData data = bf.Deserialize(stream) as SaveData;
                BannedWords = data.Banwords;
                Server.BanProfiles = data.Banprofiles;
                stream.Close();

            }
            else
            {
                BinaryFormatter bf = new BinaryFormatter();
                SaveData data = new SaveData(BannedWords, Server.BanProfiles);
                Stream stream = File.OpenWrite(Rootdir + "SaveData.Server");
                bf.Serialize(stream,data);
                stream.Close();

            }

            if(new DirectoryInfo(UpdateDir).GetFiles().Length < 2)
            {
                Console.WriteLine($"There aren't any files in Game Data/FrostyPGameManager/Updates/{Server.VERSIONNUMBER}, consider adding all relevant Mod-Side files there for Auto update serving to clients PIPE_Data/FrostyPGameManager/Updates/{Server.VERSIONNUMBER}");
            }




           
        }

        public static void SaveServerData()
        {
            SaveData data = new SaveData(BannedWords, Server.BanProfiles);
            BinaryFormatter bf = new BinaryFormatter();
            Stream stream = File.OpenWrite(Rootdir + "SaveData.Server");
            bf.Serialize(stream, data);
            stream.Close();
        }

        public static void BanPlayer(string _username, string IP, uint connid, int mins)
        {
            DateTime Time_of_release = DateTime.Now.AddMinutes(mins);

            Server.BanProfiles.Add(new BanProfile(IP, _username, connid, Time_of_release));
            SaveServerData();

        }

        public static void AlterBanWords(uint admin,bool add,string word)
        {
            if (add)
            {
                bool found = false;
                for (int i = 0; i < BannedWords.Count; i++)
                {
                    if(BannedWords[i].ToLower() == word.ToLower())
                    {
                        found = true;
                    }

                }
                if (!found)
                {
                BannedWords.Add(word);
                SaveServerData();
                ServerSend.SendTextFromServerToOne(admin, $"Added {word}");

                }
                else
                {
                    ServerSend.SendTextFromServerToOne(admin, $"{word} already stored");
                }
            }
            else
            {
                for (int i = 0; i < BannedWords.Count; i++)
                {
                    if(BannedWords[i] == word)
                    {
                        BannedWords.RemoveAt(i);
                        SaveServerData();
                        ServerSend.SendTextFromServerToOne(admin, $"Removed {word}");
                    }

                }
                
            }
        }

        public static void FileCheckAndSend(string FileName, List<int> _packetsowned, uint _from, string dir)
        {
            FileInfo _fileinfo = null;

            int lastslash = FileName.LastIndexOf("/");
            if (lastslash != -1)
            {
                FileName = FileName.Remove(0, lastslash + 1);
            }
            // make dir
            int Dta = dir.ToLower().LastIndexOf("pipe_data");
            Console.WriteLine($"Dta: {Dta}, Dir: {dir}");
            string fulldir = Rootdir + dir.Remove(0, Dta + 10);




            // Find Fileinfo
            FileInfo finfo = FileNameMatcher(new DirectoryInfo(fulldir).GetFiles(), FileName);
                if (finfo != null)
                {
                  
                   _fileinfo = finfo;
                   Console.WriteLine($"Located {FileName}");
                        
                    
                    
                }

                // file not found
                if(_fileinfo == null)
                {
                ServerSend.SendTextFromServerToOne(_from, "Server doesn't have this file yet, ask owner to upload");
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


            SendReceiveIndex NewSend = new SendReceiveIndex(FileName,(int)PacketCount,dir);
            NewSend.PacketNumbersStored = _packetsowned;
            NewSend.ByteLength = length;
            NewSend.PlayerTosendTo = _from;
            NewSend.Fileinfo = _fileinfo;
            NewSend.isSending = true;

            Stream.Close();

            OutgoingIndexes.Add(NewSend);
           

        }

        public static void FileSaver(byte[] bytes, string name, int SegsTotal, int SegNo, uint _player, long Totalbytes, string path)
        {

                Console.WriteLine($"Receiving file: {name}, path: {path}, Segment: {SegNo}");
            try
            {
                if(Totalbytes > 400000000)
                {
                    if(Server.Players.TryGetValue(_player,out Player player))
                    {
                        ServerSend.FileStatus(_player, name, 1);
                        return;
                    }
                }
                string originname = name;
                // make sure filename is just filename
            int lastslash = name.LastIndexOf("/");
            if (lastslash != -1)
            {
                name = name.Remove(0, lastslash + 1);
            }
                // make our directory
                int pdata = path.ToLower().LastIndexOf("pipe_data");
                string _mypath = path.Remove(0, pdata + 10);


                if (!Directory.Exists(TempDir + _mypath)) Directory.CreateDirectory(TempDir + _mypath);
                if (!Directory.Exists(Rootdir + _mypath)) Directory.CreateDirectory(Rootdir + _mypath);

                FileInfo Finfo = FileNameMatcher(new DirectoryInfo(Rootdir + _mypath).GetFiles(), name);
                FileInfo TempFinfo = FileNameMatcher(new DirectoryInfo(TempDir + _mypath).GetFiles(), name + ".temp");

                if(Finfo!= null)
                {
                    Console.WriteLine($"incoming data for file already found in location: {Finfo.FullName} ");
                    if(Server.Players.TryGetValue(_player,out Player player))
                    {
                        ServerSend.FileStatus(_player, name,(int)FileStatus.Received);
                    }
                    return;
                }



                // if no Temp file exists create one 
                if (TempFinfo == null)
                {
                TempFile Temp = new TempFile();
                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.OpenWrite(TempDir + _mypath + name + ".temp");

                
                Temp.ByteLengthOfFile = Totalbytes;

                bf.Serialize(stream,Temp);
                stream.Close();

                }
                else
                {
                    name = TempFinfo.Name.Replace(".temp","");
                }
               


            // grab Receive index and FileStream
            SendReceiveIndex InIndex = null;
            foreach (SendReceiveIndex r in IncomingIndexes)
            {
                if (r.NameOfFile.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower() == name.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower())
                {
                    InIndex = r;
                    InIndex.NameOfFile = name;
                }
            }
                if(InIndex == null)
                {
                    Console.WriteLine($"File receive with no Index");
                    return;
                }



                if (InIndex.PacketNumbersStored.Contains(SegNo))
                {
                    return;
                }
            FileStream _f = File.OpenWrite(TempDir + _mypath + name);

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
               
               string FullPath = _mypath.Insert(0, Rootdir);
                Console.WriteLine("Saving file to path: " + FullPath);

                if (!Directory.Exists(FullPath)) Directory.CreateDirectory(FullPath);

                    // take out anything weird
                    string savename = name.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "");


                    if(!File.Exists(FullPath + savename))
                    {
                     File.Move(TempDir + _mypath + name, FullPath + savename);

                    }
                    else
                    {
                        Console.WriteLine("Dumped file, duplicate found");
                    }
                    File.Delete(TempDir + _mypath + name + ".temp");
                
                // Tell Anyone that i've asked to send this file that i have it now ,if still online
                foreach(uint id in InIndex.PlayersRequestedFrom)
                {
                    if(Server.Players.TryGetValue(id,out Player player))
                    {
                      ServerSend.FileStatus(id, originname, (int)FileStatus.Received);

                    }
                }

                IncomingIndexes.Remove(InIndex);
                Console.WriteLine($"Saved {savename} to {FullPath}");



            }
            else
            {
                // update temp file
                FileStream stream = File.OpenRead(TempDir + _mypath + name + ".temp");
                BinaryFormatter bf = new BinaryFormatter();
                TempFile _temp = bf.Deserialize(stream) as TempFile;
                stream.Close();
                _temp.PacketNumbersStored.Add(SegNo);
                stream = File.OpenWrite(TempDir + _mypath + name + ".temp");
                bf.Serialize(stream, _temp);
                stream.Close();

            }








            }
            catch (Exception x)
            {

                Console.WriteLine(x);
            }



        }


        public static void FileCheckAndRequest(string Filename, uint _fromclient,string dir)
        {

            try
            {


                bool found = false;
                if (Filename.ToLower() != "e" && Filename != "" && Filename != " " && Filename != "stock")
                {

                      int lastslash = Filename.LastIndexOf("/");
                   if (lastslash != -1)
                   {
                      Filename = Filename.Remove(0, lastslash + 1);
                   }

                   string name = Filename;
                   Filename = ConvertToUnicode(name);

                   int pdata = dir.ToLower().LastIndexOf("pipe_data");
                   string mydir = dir.Remove(0,pdata + 10);

                   Console.WriteLine($"Filecheck: Full path: {Rootdir + mydir} :: file: {Filename}");

                if (!Directory.Exists(Rootdir + mydir)) Directory.CreateDirectory(Rootdir + mydir);
                if (!Directory.Exists(TempDir + mydir)) Directory.CreateDirectory(TempDir + mydir);



                FileInfo Finfo = FileNameMatcher(new DirectoryInfo(Rootdir + mydir).GetFiles(), Filename);

                

                    // if not found
                    if (Finfo == null)
                    {
                        Console.WriteLine($"File not located, looking for temp data..");

                        List<int> Packetsiown = new List<int>();
                         if(File.Exists(TempDir + mydir + Filename + ".temp"))
                         {
                              BinaryFormatter bf = new BinaryFormatter();
                              FileStream _f = File.OpenRead(TempDir + mydir + Filename + ".temp");
                              TempFile temp = bf.Deserialize(_f) as TempFile;
                              _f.Close();


                              foreach(int seg in temp.PacketNumbersStored)
                              {
                                Packetsiown.Add(seg);

                              }
                              Console.WriteLine($"Temp data located: packets stored: {Packetsiown.Count}: byte length: {temp.ByteLengthOfFile}");
                         }

                         if(Server.Players.TryGetValue(_fromclient, out Player player))
                         {

                           ServerSend.RequestFile(_fromclient,Filename,Packetsiown,dir);
                           Console.WriteLine(Filename + $" requested from {player.Username}");

                         }
                
                    }



                }
            



            }
            catch (Exception)
            {

               
            }

        }


        public static SaveList DeserialiseGarage(byte[] save)
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                SaveList list;
                using (var ms = new MemoryStream(save))
                {
                    list = bf.Deserialize(ms) as SaveList;
                }
              
               
                return list;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message + " : " + e.StackTrace);
                return null;
            }

           
        }

        public static List<string> GiveUpdateFileNames()
        {
            List<string> files = new List<string>();
            foreach(FileInfo f in new DirectoryInfo(UpdateDir).GetFiles())
            {
                files.Add(f.Name);
            }



            return files;

        }

        public static string ConvertToUnicode(string text)
        {
            Encoding Uni = Encoding.Unicode;
            string outstring = Uni.GetString(Uni.GetBytes(text));


            outstring = outstring.Trim(Path.GetInvalidPathChars());
            outstring = outstring.Trim(Path.GetInvalidFileNameChars());
            return outstring;
        }

        public static FileInfo FileNameMatcher(FileInfo[] myfiles, string filetomatch)
        {
            bool found = false;
            for (int i = 0; i < myfiles.Length; i++)
            {
                if (!found)
                {

                    if (myfiles[i].Name.ToLower() == filetomatch.ToLower())
                    {
                        found = true;
                        return myfiles[i];
                    }
                    else if (myfiles[i].Name.Replace("_", " ").ToLower() == filetomatch.Replace("_", " ").ToLower())
                    {
                        found = true;
                        return myfiles[i];
                    }
                    else if (myfiles[i].Name.Replace("_", " ").Replace("(1)", "").ToLower() == filetomatch.Replace("_", " ").Replace("(1)", "").ToLower())
                    {
                        found = true;
                        return myfiles[i];
                    }
                    else if (myfiles[i].Name.Replace("_", " ").Replace("(2)", "").ToLower() == filetomatch.Replace("_", " ").Replace("(2)", "").ToLower())
                    {
                        found = true;
                        return myfiles[i];
                    }
                    else if (myfiles[i].Name.Replace("_", " ").Replace("(3)", "").ToLower() == filetomatch.Replace("_", " ").Replace("(3)", "").ToLower())
                    {
                        found = true;
                        return myfiles[i];
                    }

                }
            }


            return null;


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
            public long FileByteCount;
            public string path;
            
        
            
            public FileSegment(byte[] _seg,string _name,int _segcount,int _thissegno, uint _player, int _filetype, long _bytecount, string _path)
            {
                segment = _seg;
                NameofFile = _name;
                segment_count = _segcount;
                this_segment_num = _thissegno;
                client = _player;
                Filetype = _filetype;
                FileByteCount = _bytecount;
                path = _path;
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
        public string directory;


        public SendReceiveIndex(string _filename, int _totalpackets,string dir)
        {
            NameOfFile = _filename;
            TotalPacketsinFile = _totalpackets;
            directory = dir;
        }

        public SendReceiveIndex(string _filename)
        {
            NameOfFile = _filename;
        }



    }


    public enum FileStatus
    {
        Received = 1,
        Cancel = 2,

    }



}
