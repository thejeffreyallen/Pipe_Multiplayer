using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace FrostyP_Game_Manager
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
            if(File.Exists(MultiplayerManager.TempDir + FileName + ".json"))
            {
               

                TempFile Temp = TinyJson.JSONParser.FromJson<TempFile>(File.ReadAllText(MultiplayerManager.TempDir + FileName + ".json"));
                foreach(int seg in Temp.PacketNumbersStored)
                {
                    PacketsIalreadyHave.Add(seg);
                }
                
            }

            RipTideOutgoing.RequestFile(FileName, PacketsIalreadyHave,path);

            bool found = false;
            foreach (SendReceiveIndex sr in IncomingIndexes)
            {
                if (sr.NameOfFile == FileName)
                {
                    found = true;
                    if (sr.IsReceiving)
                    {
                    InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Already Downloading", FrostyUIColor.Server, TextMessage.Textmessagemode.system));
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
                   
                    string UnicodeFilename = _di.Name;



                        if (UnicodeFilename.ToLower() == index.NameOfFile.ToLower())
                        {
                            _file = _di;
                        }
                    
                }


                if (_file == null)
                {
                    InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage("Couldnt find file to upload", FrostyUIColor.Server, TextMessage.Textmessagemode.system));
                    return;
                }
               


                // Populate SendReceiveIndex with Data it needs
                
                        FileStream Stream = _file.OpenRead();

                        // get total packets and bytelength
                        float Divider = 0;
                        long length = Stream.Length;



                if(Stream.Length > 400000000)
                {
                    InGameUI.instance.NewMessage(10, new TextMessage("File too large", FrostyUIColor.System, TextMessage.Textmessagemode.system));
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
           

            // sort directory
            int gdata = path.ToLower().LastIndexOf("game data");
            string mypath = path.Remove(0, gdata + 10) + "/";

            // if filesize too big, satisfy request to end receive
            if(Totalbytes > 400000000)
            {
                RipTideOutgoing.FileStatus(name, 1);
                return;
            }

            if (!Directory.Exists(MultiplayerManager.TempDir + mypath)) Directory.CreateDirectory(MultiplayerManager.TempDir + mypath);
            if (!Directory.Exists(Application.dataPath + "/" + mypath)) Directory.CreateDirectory(Application.dataPath + "/" + mypath);

            FileInfo finfo = MultiplayerManager.instance.FileNameMatcher(new DirectoryInfo(Application.dataPath + "/" + mypath).GetFiles(), name);
            FileInfo tempfinfo = MultiplayerManager.instance.FileNameMatcher(new DirectoryInfo(MultiplayerManager.TempDir + mypath).GetFiles(), name + ".json");


            // if no Temp file exists create one 
            if (tempfinfo == null)
            {
                TempFile Temp = new TempFile();
                Temp.ByteLengthOfFile = Totalbytes;
                string jsonsave = TinyJson.JSONWriter.ToJson(Temp);
                File.WriteAllText(MultiplayerManager.TempDir + mypath + name + ".temp",jsonsave);

            }
            else
            {
                name = tempfinfo.Name.Replace(".temp","");
            }

            // grab Receive index and FileStream
            SendReceiveIndex InIndex = null;
                foreach(SendReceiveIndex r in IncomingIndexes)
                {

                  if (r.NameOfFile.Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower() == name.Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower())
                  {
                    InIndex = r;
                  }
                }
                if(InIndex == null)
                {
                Debug.Log($"Unassigned file receive: {name}");
                return;
                }
                if (InIndex.PacketNumbersStored.Contains(SegNo))
                {
                return;
                }


                FileStream _f = File.OpenWrite(MultiplayerManager.TempDir + mypath + name);
                
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
                string fullpath = Application.dataPath + "/" + mypath;
                Debug.Log($"Saving {name} with path: {fullpath}");

                string savename = name.Replace("(1)", "").Replace("(2)", "").Replace("(3)", "");


                if (!Directory.Exists(fullpath)) Directory.CreateDirectory(fullpath);
                if(!File.Exists(fullpath + savename))
                {
                File.Move(MultiplayerManager.TempDir + mypath + name, fullpath + savename);
                }
                else
                {
                    Debug.Log($"dumping file, duplicate found {fullpath + savename}");
                }
                File.Delete(MultiplayerManager.TempDir + mypath + name + ".json");
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Saved {name} to {fullpath}", FrostyUIColor.System, TextMessage.Textmessagemode.system));

                IncomingIndexes.Remove(InIndex);





                RipTideOutgoing.FileStatus(name,(int)SyncStatus.Received);
                
                 


                // if any waiting request exist, have them process again now the file exists
                List<Waitingrequest> todelete = new List<Waitingrequest>();
                if(WaitingRequests.ToArray().Length > 0)
                {
                    foreach(Waitingrequest r in WaitingRequests)
                    {
                        
                       
                        if(r.Nameofasset.Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower() == name.Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower())
                        {


                            if(!MultiplayerManager.Players.TryGetValue(r.player, out RemotePlayer player))
                            {
                                return;
                            }


                            if(r.filetype == (int)FileTypeByNum.Texture)
                            {
                            MultiplayerManager.Players[r.player].UpdateDaryien();
                            }

                            if(r.filetype == (int)FileTypeByNum.PlayerModel)
                            {
                                
                                MultiplayerManager.Players[r.player].UpdateModel();
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
                                                    MultiplayerManager.instance.DontDestroy(_newobj);
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
                                                    MultiplayerManager.instance.DontDestroy(_newobj);

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
                        for (int i = 0; i < todelete.ToArray().Length; i++)
                        {

                            WaitingRequests.Remove(todelete[i]);
                        }
                        todelete.Clear();
                    }
               
            }
            else
            {
                // update temp file
                TempFile _temp = TinyJson.JSONParser.FromJson<TempFile>(File.ReadAllText(MultiplayerManager.TempDir + mypath + name + ".json"));
                _temp.PacketNumbersStored.Add(SegNo);
                string jsonsave = TinyJson.JSONWriter.ToJson(_temp);
                File.WriteAllText(MultiplayerManager.TempDir + mypath + name + ".json",jsonsave);
            }


            InIndex.IsReceiving = false;




        }

        public static void CheckForMap(string name, string user)
        {
            bool got = false;

            if(name.ToLower().Contains("pipe") | name.ToLower().Contains("chuck") | name.ToLower().Contains("tutorialsetup"))
            {
                return;
            }
            
            foreach (FileInfo _map in new DirectoryInfo(MultiplayerManager.MapsDir).GetFiles())
            {
               
                if (_map.Name.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower() == name.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower())
                {
                    got = true;
                    
                }
            }
            if (!got)
            {
                
                foreach(SendReceiveIndex s in IncomingIndexes)
                {
                    if(s.NameOfFile.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower() == name.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower())
                    {
                        return;
                    }
                   
                }
                InGameUI.instance.NewMessage(Constants.ServerMessageTime, new TextMessage($"{user} is at {name.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower()} that you dont have, request left in syncwindow", FrostyUIColor.System, TextMessage.Textmessagemode.system));
                IncomingIndexes.Add(new SendReceiveIndex(name.Replace("_", " ").Replace("(1)", "").Replace("(2)", "").Replace("(3)", "").ToLower(), new List<int>(),MultiplayerManager.MapsDir));
            }


        }

        public static bool CheckForFile(string nameoffile,string directory)
        {

            int pdata = directory.ToLower().LastIndexOf("pipe_data");
            string path = Application.dataPath + directory.Remove(0, pdata + 9);
            
            FileInfo info = MultiplayerManager.instance.FileNameMatcher(new DirectoryInfo(path).GetFiles(),nameoffile);
            if (info != null)
            {
                return true;
            }
            else
            {
              return false;
            }

        }

       public static void AddToRequestable(int Filetype, string nameoffile, ushort player, string directory)
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
        public static void AddToRequestable(int Filetype, string nameoffile, int objectid, ushort player)
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
                IncomingIndexes.Add(new SendReceiveIndex(nameoffile, Filetype,MultiplayerManager.ParkAssetsDir));
            }

            WaitingRequests.Add(new Waitingrequest(player, nameoffile, Filetype, objectid));


        }



    }

    public enum SyncStatus
    {
        Received = 1,
        Cancel = 2,

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
       public ushort player;
        public string Nameofasset;
        public int filetype;
        public int ObjectID;

        public Waitingrequest(ushort _id,string name_of_asset, int _filetype)
        {
            player = _id;
            Nameofasset = name_of_asset;
            filetype = _filetype;
        }
        public Waitingrequest(ushort _id, string name_of_asset, int _filetype, int objectid)
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


    

}
