using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;


namespace PIPE_Valve_Online_Server
{
    static class ServerData
    {
        

        /// <summary>
        /// stores and organises segments by their Texture name
        /// </summary>
        static Dictionary<int,TextureSegment> temporarybyteslist = new Dictionary<int, TextureSegment>();

        static List<string> names = new List<string>();

        public static string Rootdir = Assembly.GetExecutingAssembly().Location.Replace(".exe", "") + "/Game Data/";
        public static string TexturesDir = Rootdir + "Textures/";
        /// <summary>
        /// username and associated IP
        /// </summary>
        public static Dictionary<string, string> BannedIps = new Dictionary<string, string>();

       static int received = 0;


        public static void LoadFiles()
        {

        }



        public static void SaveATexture(byte[] bytes, string name, int segmentcount, int this_segment_no)
        {
            
           // store
                
                temporarybyteslist.Add(temporarybyteslist.Count, new TextureSegment(bytes, name, segmentcount, this_segment_no));
                names.Add(name);


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
                names.Clear();
            }
            

           

        }

        // populate on startup

        // add to or remove via console




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


    }
}
