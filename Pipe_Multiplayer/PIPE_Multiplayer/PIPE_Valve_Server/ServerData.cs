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
        

        /// <summary>
        /// stores and organises segments by their Texture name
        /// </summary>
        static Dictionary<int,TextureSegment> temporarybyteslist = new Dictionary<int, TextureSegment>();

        static List<string> IncomingTextureNames = new List<string>();

        public static string Rootdir = Assembly.GetExecutingAssembly().Location.Replace(".exe", "") + "/Game Data/";
        public static string TexturesDir = Rootdir + "Textures/";


        
        
        
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

            

        }


        public static void BanPlayer(string _username, string IP, uint connid, int mins)
        {
            DateTime Time_of_release = DateTime.Now.AddMinutes(mins);

            Server.BanProfiles.Add(new BanProfile(IP, _username, connid, Time_of_release));


        }





        public static void SaveATexture(byte[] bytes, string name, int segmentcount, int this_segment_no)
        {
            
           // store
                
                temporarybyteslist.Add(temporarybyteslist.Count, new TextureSegment(bytes, name, segmentcount, this_segment_no));
                IncomingTextureNames.Add(name);


            // check if im the last segment of my group

            int counter = 0;
            foreach(TextureSegment t in temporarybyteslist.Values)
            {
                if(t.name_of_tex == name)
                {
                    counter++;
                }

            }




            if(counter == segmentcount)
            {
                
               

                Dictionary<int, byte[]> grabbytes = new Dictionary<int, byte[]>();
                byte[] Recombined_Bytes;
                int addupsegments = 0;

                for (int i = 0; i < temporarybyteslist.Count; i++)
                {
                    if (temporarybyteslist[i].name_of_tex == name)
                    {
                        addupsegments = addupsegments + temporarybyteslist[i].segment.Length;
                        grabbytes.Add(temporarybyteslist[i].this_segment_num, temporarybyteslist[i].segment);

                    }
                }

                if (grabbytes.Count == segmentcount)
                {
                    Console.WriteLine("Found all packets");
                }


                Recombined_Bytes = new byte[addupsegments];
                int currentbyte = 0;
                for (int i = 0; i < grabbytes.Count; i++)
                {
                    for (int _i = 0; _i < grabbytes[i].Length; _i++)
                    {
                        Recombined_Bytes[currentbyte] = grabbytes[i][_i];
                        currentbyte++;
                    }


                }

                Console.WriteLine($"Combined segments into {addupsegments} bytes");

                // File.Create(TexturesDir + name + ".png");
                File.WriteAllBytes(TexturesDir + name, Recombined_Bytes);
                Console.WriteLine($"Saved {name}");

               
                temporarybyteslist.Clear();
                IncomingTextureNames.Clear();
            }
            

           

        }

       




    }

        /// <summary>
        /// Used for storing a segment of a textures byte array until all have been received
        /// </summary>
        class TextureSegment
        {

            public byte[] segment;
            public string name_of_tex;
            public int segment_count;
            public int this_segment_num;
            
        

            public TextureSegment(byte[] _seg,string _name,int _segcount,int _thissegno)
            {
                segment = _seg;
                name_of_tex = _name;
                segment_count = _segcount;
                this_segment_num = _thissegno;
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






}
