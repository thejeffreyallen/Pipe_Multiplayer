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
        public static Dictionary<int, string> DirectoryByFileType = new Dictionary<int, string>();
        

        public static void LoadData()
        {
            DirectoryByFileType = new Dictionary<int, string>
            {
                {1,GameManager.TexturesDir},
                {2,GameManager.MapsDir },
                {3,GameManager.PlayerModelsDir },
                {4,GameManager.GarageDir },
                {5,GameManager.ParkAssetsDir },
                {6,GameManager.UpdateDir },
            };
        }



        public static void RequestFileFromServer(string FileName, int FileType)
        {
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

            ClientSend.RequestFile(FileName, FileType, PacketsIalreadyHave);

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
                        sr.IsReceiving = true;
                    }
                }
            }

            if (!found)
            {
                IncomingIndexes.Add(new SendReceiveIndex(FileName, FileType,PacketsIalreadyHave));

            }

           


        }


        // called by syncwindow, drops packets into outgoingtexturesegments
        public static void SendFileToServer(SendReceiveIndex index)
        {
            Debug.Log($"Sending {index.NameOfFile} to Server..");
            FileInfo _file = null;


            try
            {
                // directories to search
                List<DirectoryInfo> Tosearch = new List<DirectoryInfo>();
                Tosearch.Add(new DirectoryInfo(DirectoryByFileType[(int)index.Filetype]));
               
                foreach (DirectoryInfo di in new DirectoryInfo(DirectoryByFileType[(int)index.Filetype]).GetDirectories())
                {
                    Tosearch.Add(di);
                }


                // search for file
                foreach (DirectoryInfo _di in Tosearch)
                {
                    foreach (FileInfo fi in _di.GetFiles())
                    {
                        if (fi.Name.ToLower() == index.NameOfFile.ToLower())
                        {
                            _file = fi;
                        }
                    }
                }


                if (_file == null)
                {
                    InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Couldnt find file to upload", (int)MessageColourByNum.Server, 1));
                    return;
                }
               

                if (index.Filetype == 0)
                {
                    return;
                }



                // Populate SendReceiveIndex with Data it needs
                
                        FileStream Stream = _file.OpenRead();

                        // get total packets and bytelength
                        float Divider = 0;
                        long length = Stream.Length;
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

        public static void FileReceive(byte[] bytes, string name, int segmentcount, int SegNo, int _filetype, long Totalbytes)
        {

            // if no Temp file exists create one 
            if (!File.Exists(GameManager.TempDir + name + ".temp"))
            {
                TempFile Temp = new TempFile();
                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = File.OpenWrite(GameManager.TempDir + name + ".temp");

                
                Temp.ByteLengthOfFile = Totalbytes;
                Temp.FileType = _filetype;

                bf.Serialize(stream, Temp);
                stream.Close();

            }

            // grab Receive index and FileStream
            SendReceiveIndex InIndex = null;
                foreach(SendReceiveIndex r in IncomingIndexes)
                {

                if(r.NameOfFile == name)
                {
                    InIndex = r;
                }
                }
                FileStream _f = File.OpenWrite(GameManager.TempDir + name);
                
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


            // Do Save or just update temp file
            if (InIndex.PacketNumbersStored.Count == InIndex.TotalPacketsinFile)
            {

               
                if (!Directory.Exists(DirectoryByFileType[_filetype])) Directory.CreateDirectory(DirectoryByFileType[_filetype]);
                File.Move(GameManager.TempDir + name, DirectoryByFileType[_filetype] + InIndex.NameOfFile);
                File.Delete(GameManager.TempDir + name + ".temp");
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Saved {name} to {DirectoryByFileType[_filetype]}", (int)MessageColourByNum.System, 1));

                IncomingIndexes.Remove(InIndex);
               


               

                 ClientSend.FileStatus(name, _filetype,(int)FileStatus.Received);
                
                 if(_filetype == 6)
                 {
                    InGameUI.instance.UpdateDownloaded = true;
                 }


                // if any waiting request exist, have them process again now the file exists
                if(WaitingRequests.Count > 0)
                {
                    List<Waitingrequest> todelete = new List<Waitingrequest>();
                    foreach(Waitingrequest r in WaitingRequests)
                    {

                        if(GameManager.Players.TryGetValue(r.player,out RemotePlayer player))
                        {
                            todelete.Add(r);
                           
                        }

                       
                        if(r.Nameofasset == name)
                        {
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

                                                    ParkBuilder.instance.bundlesloaded.Add(new BundleData(newbundle, file.Name));
                                                    return;
                                                }
                                            }


                                        }
                                    }
                                }
                                
                            }





                            todelete.Add(r);
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
               
            }
            else
            {
                // update temp file
                FileStream stream = File.OpenRead(GameManager.TempDir + name + ".temp");
                BinaryFormatter bf = new BinaryFormatter();
                TempFile _temp = bf.Deserialize(stream) as TempFile;
                stream.Close();
                _temp.PacketNumbersStored.Add(SegNo);
                stream = File.OpenWrite(GameManager.TempDir + name + ".temp");
                bf.Serialize(stream, _temp);
                stream.Close();

            }







        }


        public static void CheckForMap(string name, string user)
        {
            bool got = false;

            if(name.ToLower().Contains("pipe") | name.ToLower().Contains("chuck"))
            {
                return;
            }

            foreach(FileInfo _map in new DirectoryInfo(GameManager.MapsDir).GetFiles())
            {
                if(_map.Name.ToLower() == name.ToLower())
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
                IncomingIndexes.Add(new SendReceiveIndex(name, (int)FileTypeByNum.Map));
            }


        }


        public static bool CheckForFile(int Filetype, string nameoffile)
        {
            
            FileInfo[] info = new DirectoryInfo(DirectoryByFileType[Filetype]).GetFiles();
            for (int i = 0; i < info.Length; i++)
            {
                if(info[i].Name.ToLower() == nameoffile.ToLower())
                {
                    return true;
                }

            }

            return false;

            


        }


       public static void AddToRequestable(int Filetype, string nameoffile, uint player)
       {
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
                IncomingIndexes.Add(new SendReceiveIndex(nameoffile, Filetype));
            }
            WaitingRequests.Add(new Waitingrequest(player, nameoffile, Filetype));
        }
        public static void AddToRequestable(int Filetype, string nameoffile, int objectid, uint player)
        {
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
                IncomingIndexes.Add(new SendReceiveIndex(nameoffile, Filetype));
            }

            WaitingRequests.Add(new Waitingrequest(player, nameoffile, Filetype, objectid));


        }
       



    }





    /// <summary>
    /// Used for storing a segment of a textures byte array until all have been received
    /// </summary>
    [Serializable]
    public class FileSegment
    {

        public byte[] segment;
        public string NameofFile;
        public int segment_count;
        public int this_segment_num;
        public int FileType;
        public long ByteCount;



        public FileSegment(byte[] _seg, string _name, int _segcount, int _thissegno, int _filetype, long _bytecount)
        {
            segment = _seg;
            NameofFile = _name;
            segment_count = _segcount;
            this_segment_num = _thissegno;
            FileType = _filetype;
            ByteCount = _bytecount;
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
        public int Filetype;
        public int TotalPacketsinFile;
        public FileInfo Fileinfo;
        public bool IsSending;
        public bool IsReceiving;
        public long ByteLength;


        public SendReceiveIndex(string _filename, int _filetype, int _totalpackets)
        {
            NameOfFile = _filename;
            Filetype = _filetype;
            TotalPacketsinFile = _totalpackets;
        }

        public SendReceiveIndex(string _filename, int _filetype, List<int> packetsnumbersowned)
        {
            NameOfFile = _filename;
            Filetype = _filetype;
            PacketNumbersStored = packetsnumbersowned;
        }
        public SendReceiveIndex(string _filename, int _filetype)
        {
            NameOfFile = _filename;
            Filetype = _filetype;
        }


    }



    public enum FileTypeByNum
    {
        Texture = 1,
        Map = 2,
        PlayerModel = 3,
        Mesh = 4,
        ParkAsset = 5,
        Update = 6,
    }


    public enum FileStatus
    {
        Received = 1,
        Cancel = 2,

    }
    

}
