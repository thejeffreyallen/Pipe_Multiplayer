using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using FrostyP_Game_Manager;


namespace PIPE_Valve_Console_Client
{
    public static class FileSyncing
    {
       
        public static List<SendReceiveIndex> IncomingIndexes = new List<SendReceiveIndex>();
        public static List<SendReceiveIndex> OutGoingIndexes = new List<SendReceiveIndex>();
        public static List<Waitingrequest> WaitingRequests = new List<Waitingrequest>();
        


        public static void RequestFileFromServer(string FileName,string path)
        {
            int lastslash = FileName.LastIndexOf("/");
            if(lastslash != -1)
            {
                FileName = FileName.Remove(0, lastslash + 1);
            }

            if(FileName.ToLower() == "e" | FileName == "" | FileName == " ")
            {
                return;
            }

           


            // send list of any packet numbers i already have in a .temp file
            List<int> PacketsIalreadyHave = new List<int>();
            if(File.Exists(GameManager.TempDir + FileName + ".temp"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.OpenRead(GameManager.TempDir + FileName + ".temp");

                TempFile Temp = bf.Deserialize(stream) as TempFile;
                stream.Close();

                foreach(int seg in Temp.PacketNumbersStored)
                {
                    PacketsIalreadyHave.Add(seg);
                }
                
            }

            ClientSend.RequestFile(FileName, PacketsIalreadyHave,path);

            bool found = false;
            foreach (SendReceiveIndex sr in IncomingIndexes)
            {
                if (sr.NameOfFile == FileName)
                {
                    found = true;
                    if (sr.IsReceiving)
                    {
                    InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Already Downloading", (int)MessageColourByNum.Server, 1));
                    return;

                    }
                    else
                    {
                        sr.PacketNumbersStored = PacketsIalreadyHave;
                        
                    }
                }
            }

            if (!found)
            {
                IncomingIndexes.Add(new SendReceiveIndex(FileName,PacketsIalreadyHave,path));

            }

           


        }

        // called by syncwindow, drops packets into outgoingtexturesegments
        public static void SendFileToServer(SendReceiveIndex index)
        {
            Debug.Log($"Sending {index.NameOfFile} to Server..");
            FileInfo _file = null;


            try
            {

              
                foreach (FileInfo _di in new DirectoryInfo(index.Directory).GetFiles(index.NameOfFile,SearchOption.TopDirectoryOnly))
                {
                   
                    string UnicodeFilename = GameManager.ConvertToUnicode(_di.Name);



                        if (UnicodeFilename.ToLower() == index.NameOfFile.ToLower())
                        {
                            _file = _di;
                        }
                    
                }


                if (_file == null)
                {
                    InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Couldnt find file to upload", (int)MessageColourByNum.Server, 1));
                    return;
                }
               


                // Populate SendReceiveIndex with Data it needs
                
                        FileStream Stream = _file.OpenRead();

                        // get total packets and bytelength
                        float Divider = 0;
                        long length = Stream.Length;



                if(Stream.Length > 400000000)
                {
                    InGameUI.instance.NewMessage(10, new TextMessage("File too large", (int)MessageColourByNum.System, 1));
                    OutGoingIndexes.Remove(index);

                    return;
                }


                        long mod = length % 400000;
                        if (mod > 0)
                        {
                            Divider = length / 400000 + 1;
                        }
                        if (mod == 0)
                        {
                            Divider = length / 400000;
                        }
                        if (length < 400000)
                        {
                            Divider = 1;
                        }




                        // give Fileinfo and mark as Sending
                        index.ByteLength = length;
                        index.Fileinfo = _file;
                        index.TotalPacketsinFile = (int)Divider;
                        index.IsSending = true;

                        Stream.Close();
                        Debug.Log($"matched " + index.NameOfFile + $" MB: { (float)length/1000000}");
                        Debug.Log($"Total packets: {Divider}");
                    


            }
            catch (Exception x)
            {
                index.IsSending = false;
                Debug.Log("Error reading file: " + x);
            }
            




        }

        public static void FileReceive(byte[] bytes, string name, int segmentcount, int SegNo, long Totalbytes, string path)
        {
            // sort filename
            int lastslash = name.LastIndexOf("/");
            if (lastslash != -1)
            {
                name = name.Remove(0, lastslash + 1);
            }
            string inputString = name;
            string UnicodeName = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(inputString));

            // sort directory
            int gdata = path.ToLower().LastIndexOf("game data");
            string mypath = path.Remove(0, gdata + 9) + "/";



            if (!Directory.Exists(GameManager.TempDir + mypath)) Directory.CreateDirectory(GameManager.TempDir + mypath);
            if (!Directory.Exists(Application.dataPath + mypath)) Directory.CreateDirectory(Application.dataPath + mypath);

            // if no Temp file exists create one 
            if (!File.Exists(GameManager.TempDir + mypath + UnicodeName + ".temp"))
            {
                TempFile Temp = new TempFile();
                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.OpenWrite(GameManager.TempDir + mypath + UnicodeName + ".temp");

                
                Temp.ByteLengthOfFile = Totalbytes;

                bf.Serialize(stream, Temp);
                stream.Close();

            }

            // grab Receive index and FileStream
            SendReceiveIndex InIndex = null;
                foreach(SendReceiveIndex r in IncomingIndexes)
                {

                string fileinputString = r.NameOfFile;
                string UnicodeFilename = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(fileinputString));



                if (UnicodeFilename.ToLower() == UnicodeName.ToLower())
                {
                    InIndex = r;
                }
                }
                FileStream _f = File.OpenWrite(GameManager.TempDir + mypath + name);
                
                 if (!InIndex.IsReceiving)
                 {
                   InIndex.IsReceiving = true;
                 }
                 InIndex.ByteLength = Totalbytes;
                 InIndex.TotalPacketsinFile = segmentcount;

            // determine where to write data
            int Startpos = (SegNo - 1) * 400000;

            _f.Seek(Startpos, SeekOrigin.Begin);
            _f.Write(bytes, 0, bytes.Length);
            _f.Close();



                 InIndex.PacketNumbersStored.Add(SegNo);
                 Debug.Log($"Received packet {SegNo} of {segmentcount} for {name}");

            // Do Save or just update temp file
            if (InIndex.PacketNumbersStored.Count == InIndex.TotalPacketsinFile)
            {
                Debug.Log($"Saving {name}");
                string fullpath = Application.dataPath + mypath;
                

               
                if (!Directory.Exists(fullpath)) Directory.CreateDirectory(fullpath);
                File.Move(GameManager.TempDir + mypath + UnicodeName, fullpath + UnicodeName);
                File.Delete(GameManager.TempDir + mypath + UnicodeName + ".temp");
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Saved {UnicodeName} to {fullpath}", (int)MessageColourByNum.System, 1));

                IncomingIndexes.Remove(InIndex);
               


               

                 ClientSend.FileStatus(UnicodeName,(int)FileStatus.Received);
                
                 


                // if any waiting request exist, have them process again now the file exists
                List<Waitingrequest> todelete = new List<Waitingrequest>();
                if(WaitingRequests.Count > 0)
                {
                    foreach(Waitingrequest r in WaitingRequests)
                    {
                        
                       
                        if(r.Nameofasset.ToLower() == name.ToLower())
                        {


                            if(!GameManager.Players.TryGetValue(r.player, out RemotePlayer player))
                            {
                                return;
                            }


                            if(r.filetype == (int)FileTypeByNum.Texture)
                            {
                            GameManager.Players[r.player].UpdateDaryien();
                            }

                            if(r.filetype == (int)FileTypeByNum.PlayerModel)
                            {
                                
                                GameManager.Players[r.player].UpdateModel();
                                Debug.Log($"Found waiting request for {r.Nameofasset} model");
                            }

                            if (r.filetype == (int)FileTypeByNum.ParkAsset)
                            {
                                if(r.ObjectID != 0)
                                {
                                    foreach(NetGameObject n in player.Objects)
                                    {
                                        if(r.ObjectID == n.ObjectID)
                                        {

                                            foreach (AssetBundle A in AssetBundle.GetAllLoadedAssetBundles())
                                            {
                                                if (A.name == n.NameofAssetBundle)
                                                {
                                                    GameObject _newobj = UnityEngine.GameObject.Instantiate(A.LoadAsset(n.NameofObject)) as GameObject;
                                                    _newobj.transform.position = n.Position;
                                                    _newobj.transform.eulerAngles = n.Rotation;

                                                    n._Gameobject = _newobj;
                                                    n.AssetBundle = A;
                                                    GameManager.instance.DontDestroy(_newobj);
                                                    return;
                                                }
                                            }

                                            foreach (FileInfo file in new DirectoryInfo(ParkBuilder.instance.AssetbundlesDirectory).GetFiles())
                                            {
                                                if (file.Name == n.NameOfFile)
                                                {
                                                    AssetBundle newbundle = AssetBundle.LoadFromFile(file.FullName);
                                                    GameObject _newobj = UnityEngine.GameObject.Instantiate(newbundle.LoadAsset(n.NameofObject)) as GameObject;

                                                    _newobj.transform.position = n.Position;
                                                    _newobj.transform.eulerAngles = n.Rotation;

                                                    n._Gameobject = _newobj;
                                                    n.AssetBundle = newbundle;
                                                    GameManager.instance.DontDestroy(_newobj);

                                                    ParkBuilder.instance.bundlesloaded.Add(new BundleData(newbundle, file.Name,file.DirectoryName));
                                                    return;
                                                }
                                            }


                                        }
                                    }
                                }
                                
                            }

                            if(r.filetype == (int)FileTypeByNum.Garage)
                            {
                                CustomMeshManager.instance.LoadFiles();
                                player.UpdateBMX();
                            }



                            todelete.Add(r);
                        }
                    }
                }
                    if (todelete.Count > 0)
                    {
                        for (int i = 0; i < todelete.Count; i++)
                        {

                            WaitingRequests.Remove(todelete[i]);
                        }
                        todelete.Clear();
                    }
               
            }
            else
            {
                // update temp file
                FileStream stream = File.OpenRead(GameManager.TempDir + mypath + UnicodeName + ".temp");
                BinaryFormatter bf = new BinaryFormatter();
                TempFile _temp = bf.Deserialize(stream) as TempFile;
                stream.Close();
                _temp.PacketNumbersStored.Add(SegNo);
                stream = File.OpenWrite(GameManager.TempDir + mypath + UnicodeName + ".temp");
                bf.Serialize(stream, _temp);
                stream.Close();
            }


            InIndex.IsReceiving = false;




        }

        public static void CheckForMap(string name, string user)
        {
            bool got = false;

            if(name.ToLower().Contains("pipe") | name.ToLower().Contains("chuck"))
            {
                return;
            }
            
            foreach (FileInfo _map in new DirectoryInfo(GameManager.MapsDir).GetFiles())
            {
               
                string Unicode = GameManager.ConvertToUnicode(_map.Name);

                if (Unicode.ToLower() == name.ToLower())
                {
                    got = true;
                    
                }
            }
            if (!got)
            {
                
                foreach(SendReceiveIndex s in IncomingIndexes)
                {
                    if(s.NameOfFile == name)
                    {
                        return;
                    }
                   
                }
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"{user} is at {name} that you dont have, request left in syncwindow", (int)MessageColourByNum.System, 1));
                IncomingIndexes.Add(new SendReceiveIndex(name,new List<int>(),GameManager.MapsDir));
            }


        }

        public static bool CheckForFile(string nameoffile,string directory)
        {

            int pdata = directory.ToLower().LastIndexOf("pipe_data");
            string path = Application.dataPath + directory.Remove(0, pdata + 9);
            
            FileInfo[] info = new DirectoryInfo(path).GetFiles(nameoffile, SearchOption.TopDirectoryOnly);
            if (info.Length > 0)
            {
                return true;
            }

            return false;

        }

       public static void AddToRequestable(int Filetype, string nameoffile, uint player, string directory)
       {
            int lastslash = nameoffile.LastIndexOf("/");
            if(lastslash != -1)
            {
                nameoffile = nameoffile.Remove(0, lastslash + 1);
            }

            if (nameoffile == "" | nameoffile == "e" | nameoffile == " ")
            {
                return;
            }

            bool requested = false;
            foreach(SendReceiveIndex s in IncomingIndexes)
            {
                if(s.NameOfFile.ToLower() == nameoffile.ToLower())
                {
                    requested = true;
                }
            }
            if (!requested)
            {
                IncomingIndexes.Add(new SendReceiveIndex(nameoffile, Filetype,directory));
            }
            WaitingRequests.Add(new Waitingrequest(player, nameoffile, Filetype));
        }
        public static void AddToRequestable(int Filetype, string nameoffile, int objectid, uint player)
        {
            int lastslash = nameoffile.LastIndexOf("/");
            if (lastslash != -1)
            {
                nameoffile = nameoffile.Remove(0, lastslash + 1);
            }


            if (nameoffile == "" | nameoffile == "e" | nameoffile == " ")
            {
                return;
            }


            bool requested = false;
            foreach (SendReceiveIndex s in IncomingIndexes)
            {
                if (s.NameOfFile.ToLower() == nameoffile.ToLower())
                {
                    requested = true;
                }
            }
            if (!requested)
            {
                IncomingIndexes.Add(new SendReceiveIndex(nameoffile, Filetype,GameManager.ParkAssetsDir));
            }

            WaitingRequests.Add(new Waitingrequest(player, nameoffile, Filetype, objectid));


        }
       
    }





    /// <summary>
    /// Container for a segment of a file to be sent or just received
    /// </summary>
    [Serializable]
    public class FileSegment
    {

        public byte[] segment;
        public string NameofFile;
        public int segment_count;
        public int this_segment_num;
        public long ByteCount;
        public string path;



        public FileSegment(byte[] _seg, string _name, int _segcount, int _thissegno, long _bytecount, string _path)
        {
            segment = _seg;
            NameofFile = _name;
            segment_count = _segcount;
            this_segment_num = _thissegno;
            ByteCount = _bytecount;
            path = _path;
        }

    }

    /// <summary>
    /// if a texture/asset isnt found, the texture isnt changed/asset isnt loaded, but a waitingrequest is stored. On receive of a file, any waitingrequests matching the file trigger and I.E the riders UpdateDaryien fires again, this time finding the file
    /// </summary>
   public class Waitingrequest
    {
       public uint player;
        public string Nameofasset;
        public int filetype;
        public int ObjectID;

        public Waitingrequest(uint _id,string name_of_asset, int _filetype)
        {
            player = _id;
            Nameofasset = name_of_asset;
            filetype = _filetype;
        }
        public Waitingrequest(uint _id, string name_of_asset, int _filetype, int objectid)
        {
            player = _id;
            Nameofasset = name_of_asset;
            filetype = _filetype;
            ObjectID = objectid;
        }


    }


    public class SendReceiveIndex
    {
        public List<int> PacketNumbersStored = new List<int>();
        public string NameOfFile;
        public int TotalPacketsinFile;
        public FileInfo Fileinfo;
        public bool IsSending;
        public bool IsReceiving;
        public long ByteLength;
        public string Directory;


        public SendReceiveIndex(string _filename, int _totalpackets,string dir)
        {
            NameOfFile = _filename;
            TotalPacketsinFile = _totalpackets;
            Directory = dir;
        }

        public SendReceiveIndex(string _filename, List<int> packetsnumbersowned,string dir)
        {
            NameOfFile = _filename;
            PacketNumbersStored = packetsnumbersowned;
            Directory = dir;
        }
      

    }



    public enum FileTypeByNum
    {
        Texture = 1,
        Map = 2,
        PlayerModel = 3,
        Garage = 4,
        ParkAsset = 5,
        Update = 6,
    }


    public enum FileStatus
    {
        Received = 1,
        Cancel = 2,

    }
    

}
