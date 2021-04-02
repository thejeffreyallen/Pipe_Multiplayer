using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace PIPE_Valve_Console_Client
{
    public static class GameData
    {
        /// <summary>
        /// stores and organises segments by their Texture name
        /// </summary>
        static Dictionary<int, TextureSegment> temporarybyteslist = new Dictionary<int, TextureSegment>();
        static List<string> names = new List<string>();


        public static List<Waitingrequest> requests = new List<Waitingrequest>();











        public static void SaveImage(byte[] bytes, string name, int segmentcount, int this_segment_no)
        {
            // store

            temporarybyteslist.Add(this_segment_no, new TextureSegment(bytes, name, segmentcount, this_segment_no));
            names.Add(name);


            // check if im the last segment of my group

            int counter = 0;
            foreach (TextureSegment t in temporarybyteslist.Values)
            {
                if (t.name_of_tex == name)
                {
                    counter++;
                }

            }




            if (counter == segmentcount)
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
                    //Console.WriteLine("Found all packets");
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

                //  Console.WriteLine($"Combined segments into {addupsegments} bytes");

                // File.Create(TexturesDir + name + ".png");
                if (!Directory.Exists(GameManager.instance.TexturesRootdir + "/Temp/")) Directory.CreateDirectory(GameManager.instance.TexturesRootdir + "/Temp/");
                File.WriteAllBytes(GameManager.instance.TexturesRootdir + "/Temp/"  + name, Recombined_Bytes);
                InGameUI.instance.NewMessage(Constants.SystemMessageTime, new TextMessage($"Saved {name} to Textures folder", (int)MessageColour.System, 1));


                temporarybyteslist.Clear();
                names.Clear();

                if(requests.Count > 0)
                {
                    List<Waitingrequest> todelete = new List<Waitingrequest>();
                    foreach(Waitingrequest r in requests)
                    {
                       
                        if(r.Nameofasset == name)
                        {
                            GameManager.Players[r.player].UpdateDaryien();
                            todelete.Add(r);
                        }
                    }
                    if (todelete.Count > 0)
                    {
                        for (int i = 0; i < todelete.Count; i++)
                        {

                            requests.Remove(todelete[i]);
                        }
                        todelete.Clear();
                    }
                }
               
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



        public TextureSegment(byte[] _seg, string _name, int _segcount, int _thissegno)
        {
            segment = _seg;
            name_of_tex = _name;
            segment_count = _segcount;
            this_segment_num = _thissegno;
        }

    }

   public class Waitingrequest
    {
       public uint player;
        public string username;
        public string Nameofasset;

        public Waitingrequest(uint _id, string _username,string name_of_asset)
        {
            player = _id;
            username = _username;
            Nameofasset = name_of_asset;
        }


    }

}
