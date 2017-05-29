using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DipBase;
using NAudio.Wave.SampleProviders;

namespace Groove.Pipeline
{
    class Mixer
    {
        public List<Channel> Channels;
        public List<Bus> Buses;
        public Master MasterC;
        public List<HardOut> HOuts;
        public List<HardInp> HInps;
        public Mixer()
        {
            Channels = new List<Channel>();
            Buses = new List<Bus>();
            MasterC = new Master();
            HOuts = new List<HardOut>();
            HInps = new List<HardInp>();
        }

        public void Mix(float[,] inp, float[,] ou, int bufsize, int channelcount)
        {
            for (int i = 0; i < bufsize; i++)
            {
                foreach (Channel C in Channels)
                {
                    C.Process(inp, ou, i);
                }
                foreach (Bus B in Buses)
                {
                    B.Process(ou, i);
                }
                MasterC.Process(ou, i);
            }
        }

        public void PopHOuts(Player p)
        {
            for (int i = 0; i < p.ASIO.DriverOutputChannelCount; i += 2)
            {
                HOuts.Add(new HardOut(p.ASIO.AsioOutputChannelName(i), i, i + 1));
            }
        }

        public void PopHInps(Player p)
        {
            for (int i = 0; i < p.ASIO.DriverInputChannelCount; i += 2)
            {
                HInps.Add(new HardInp(p.ASIO.AsioInputChannelName(i), i, i + 1));
            }
        }

        public abstract class BaseChannel
        {
            public String name;
            public float level;
            public bool mute;
            public bool preE;
            public bool preS;
            public Output Output;
            public List<AudioEffect> Plugins;
        }

        public class Channel : BaseChannel
        {
            public float Pan;
            public Dictionary<Bus, float> Sends;
            public Input Input;
            float[] Stereo;

            public Channel()
            {
                Sends = new Dictionary<Bus, float>();
                Plugins = new List<AudioEffect>();
                Stereo = new float[2];
            }

            internal void Process(float[,] inp, float[,] ou, int i)
            {
                Input.Get(inp, i, Stereo);
                if (preE)
                {
                    foreach (AudioEffect P in Plugins)
                    {
                        P.Get(Stereo);
                    }
                }
                if (preS)
                {
                    foreach (KeyValuePair<Bus, float> K in Sends)
                    {
                        Stereo[0] *= K.Value;
                        Stereo[1] *= K.Value;
                        K.Key.Set(Stereo);
                    }
                }
                Stereo[0] *= level;
                Stereo[1] *= level;
                Stereo[0] *= 1 + Math.Min(-1 * Pan, 0);
                Stereo[1] *= 1 + Math.Min(Pan, 0);
                if (!preE)
                {
                    foreach (AudioEffect P in Plugins)
                    {
                        P.Get(Stereo);
                    }
                }
                if (!preS)
                {
                    foreach (KeyValuePair<Bus, float> K in Sends)
                    {
                        Stereo[0] *= K.Value;
                        Stereo[1] *= K.Value;
                        K.Key.Set(Stereo);
                    }
                }
                if (mute) { Stereo[0] = 0; Stereo[1] = 0; }
                Output.Set(Stereo, ou, i);

            }
        }

        public class Bus : BaseChannel, Output
        {
            public float Pan;
            public Dictionary<Bus, float> Sends;
            public float[] Stereo;

            public Bus()
            {
                Sends = new Dictionary<Bus, float>();
                Plugins = new List<AudioEffect>();
            }

            public void Set(float[] stereo)
            {
                Stereo = stereo;
            }

            public void Set(float[] stereo, float[,] ou, int i)
            {
                Stereo = stereo;
            }

            internal void Process(float[,] ou, int i)
            {
                if (preE)
                {
                    foreach (AudioEffect P in Plugins)
                    {
                        P.Get(Stereo);
                    }
                }
                if (preS)
                {
                    foreach (KeyValuePair<Bus, float> K in Sends)
                    {
                        Stereo[0] *= K.Value;
                        Stereo[1] *= K.Value;
                        K.Key.Set(Stereo);
                    }
                }
                Stereo[0] *= level;
                Stereo[1] *= level;
                Stereo[0] *= 1 + Math.Min(-1 * Pan, 0);
                Stereo[1] *= 1 + Math.Min(Pan, 0);
                if (!preE)
                {
                    foreach (AudioEffect P in Plugins)
                    {
                        P.Get(Stereo);
                    }
                }
                if (!preS)
                {
                    foreach (KeyValuePair<Bus, float> K in Sends)
                    {
                        Stereo[0] *= K.Value;
                        Stereo[1] *= K.Value;
                        K.Key.Set(Stereo);
                    }
                }
                if (mute) { Stereo[0] = 0; Stereo[1] = 0; }
                Output.Set(Stereo, ou, i);
            }
        }

        public class Master : BaseChannel, Output
        {
            public float[] Stereo;

            public Master()
            {
                name = "Master";
                mute = false;
                level = 1;
                Plugins = new List<AudioEffect>();
            }

            public void Setup(Mixer m)
            {
                Output = m.HOuts[0];
            }

            public void Set(float[] stereo)
            {
                Stereo = stereo;
            }

            public void Set(float[] stereo, float[,] ou, int i)
            {
                Stereo = stereo;
            }

            internal void Process(float[,] ou, int i)
            {
                if (preE)
                {
                    foreach (AudioEffect P in Plugins)
                    {
                        P.Get(Stereo);
                    }
                }
                Stereo[0] *= level;
                Stereo[1] *= level;
                if (!preE)
                {
                    foreach (AudioEffect P in Plugins)
                    {
                        P.Get(Stereo);
                    }
                }
                if (mute) { Stereo[0] = 0; Stereo[1] = 0; }
                Output.Set(Stereo, ou, i);
            }
        }

        public class HardOut : Output
        {
            public HardOut(String Name, int LI, int RI)
            {
                this.Name = Name;
                LeftIndex = LI;
                RightIndex = RI;
            }

            public String Name;
            public int LeftIndex;
            public int RightIndex;

            public void Set(float[] stereo)
            {

            }

            public void Set(float[] stereo, float[,] ou, int i)
            {
                ou[i, LeftIndex] = stereo[0];
                ou[i, RightIndex] = stereo[1];
            }
        }

        public class HardInp : Input
        {
            public HardInp(String Name, int LI, int RI)
            {
                this.Name = Name;
                LeftIndex = LI;
                RightIndex = RI;
            }
            public String Name;
            public int LeftIndex;
            public int RightIndex;

            public void Get(float[,] inp, int i, float[] stereo)
            {
                stereo[0] = inp[i, LeftIndex];
                stereo[1] = inp[i, RightIndex];
            }

        }

    }

}
