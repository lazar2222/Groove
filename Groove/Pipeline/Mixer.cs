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
            MasterC = new Master(64);
            HOuts = new List<HardOut>();
            HInps = new List<HardInp>();
        }

        public void Mix(float[][] inp, float[][] ou, int bufsize, int channelcount)
        {
            for (int i = 0; i < Channels.Count; i++)
            {
                Channels[i].Process(inp, ou);
            }
            for (int i = 0; i < Buses.Count; i++)
            {
                Buses[i].Process(ou);
            }
            MasterC.Process(ou);
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
            float[][] Stereo;
            float[][] Stereo2;

            public Channel(int bufsize)
            {
                Sends = new Dictionary<Bus, float>();
                Plugins = new List<AudioEffect>();
                Stereo = new float[2][];
                Stereo2 = new float[2][];
                for (int i = 0; i < 2; i++)
                {
                    Stereo[i] = new float[bufsize];
                    Stereo2[i] = new float[bufsize];
                }
            }

            internal void Process(float[][] inp, float[][] ou)
            {
                Input.Get(inp, Stereo);
                if (preE)
                {
                    for (int i = 0; i < Plugins.Count(); i++)
                    {
                        Plugins[i].Get(Stereo);
                    }
                }
                if (preS)
                {
                    for (int n = 0; n < Sends.Count; n++)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < Stereo[0].Length / 2; j++)
                            {
                                Stereo2[i][j] = Stereo[i][j] * Sends.ElementAt(n).Value;
                            }
                        }
                        Sends.ElementAt(n).Key.Set(Stereo2, ou);
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < Stereo[0].Length; j++)
                    {
                        Stereo[i][j] *= level;
                        Stereo[i][j] *= level;
                        Stereo[i][j] *= 1 + Math.Min(-1 * Pan, 0);
                        Stereo[i][j] *= 1 + Math.Min(Pan, 0);
                    }
                }
                if (!preE)
                {
                    for (int i = 0; i < Plugins.Count(); i++)
                    {
                        Plugins[i].Get(Stereo);
                    }
                }
                if (!preS)
                {
                    for (int n = 0; n < Sends.Count; n++)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < Stereo[0].Length; j++)
                            {
                                Stereo2[i][j] = Stereo[i][j] * Sends.ElementAt(n).Value;
                            }
                        }
                        Sends.ElementAt(n).Key.Set(Stereo2, ou);
                    }
                }
                if (mute)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < Stereo[0].Length; j++)
                        {
                            Stereo[i][j] = 0;
                        }
                    }
                }
                Output.Set(Stereo, ou);

            }
        }

        public class Bus : BaseChannel, Output
        {
            public float Pan;
            public Dictionary<Bus, float> Sends;
            float[][] Stereo;
            float[][] Stereo2;

            public Bus(int bufsize)
            {
                Sends = new Dictionary<Bus, float>();
                Plugins = new List<AudioEffect>();
                Stereo = new float[2][];
                Stereo2 = new float[2][];
                for (int i = 0; i < 2; i++)
                {
                    Stereo[i] = new float[bufsize];
                    Stereo2[i] = new float[bufsize];
                }
            }

            public void Set(float[][] stereo, float[][] ou)
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < Stereo[0].Length; j++)
                    {
                        Stereo[i][j] = +stereo[i][j];
                    }
                }
            }

            internal void Process(float[][] ou)
            {
                if (preE)
                {
                    for (int i = 0; i < Plugins.Count(); i++)
                    {
                        Plugins[i].Get(Stereo);
                    }
                }
                if (preS)
                {
                    for (int n = 0; n < Sends.Count; n++)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < Stereo[0].Length; j++)
                            {
                                Stereo2[i][j] = Stereo[i][j] * Sends.ElementAt(n).Value;
                            }
                        }
                        Sends.ElementAt(n).Key.Set(Stereo2, ou);
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < Stereo[0].Length; j++)
                    {
                        Stereo[i][j] *= level;
                        Stereo[i][j] *= level;
                        Stereo[i][j] *= 1 + Math.Min(-1 * Pan, 0);
                        Stereo[i][j] *= 1 + Math.Min(Pan, 0);
                    }
                }
                if (!preE)
                {
                    for (int i = 0; i < Plugins.Count(); i++)
                    {
                        Plugins[i].Get(Stereo);
                    }
                }
                if (!preS)
                {
                    for (int n = 0; n < Sends.Count; n++)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < Stereo[0].Length; j++)
                            {
                                Stereo2[i][j] = Stereo[i][j] * Sends.ElementAt(n).Value;
                            }
                        }
                        Sends.ElementAt(n).Key.Set(Stereo2, ou);
                    }
                }
                if (mute)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < Stereo[0].Length; j++)
                        {
                            Stereo[i][j] = 0;
                        }
                    }
                }
                Output.Set(Stereo, ou);
            }
        }

        public class Master : BaseChannel, Output
        {
            public float[][] Stereo;

            public Master(int bufsize)
            {
                name = "Master";
                mute = false;
                level = 1;
                Plugins = new List<AudioEffect>();
                Stereo = new float[2][];
                for (int i = 0; i < 2; i++)
                {
                    Stereo[i] = new float[bufsize];
                }
            }

            public void Setup(Mixer m)
            {
                Output = m.HOuts[0];
            }

            public void Set(float[][] stereo, float[][] ou)
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < Stereo[0].Length; j++)
                    {
                        Stereo[i][j] = +stereo[i][j];
                    }
                }
            }

            internal void Process(float[][] ou)
            {
                if (preE)
                {
                    for (int i = 0; i < Plugins.Count(); i++)
                    {
                        Plugins[i].Get(Stereo);
                    }
                }
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < Stereo[0].Length; j++)
                    {
                        Stereo[i][j] *= level;
                        Stereo[i][j] *= level;
                    }
                }
                if (!preE)
                {
                    for (int i = 0; i < Plugins.Count(); i++)
                    {
                        Plugins[i].Get(Stereo);
                    }
                }
                if (mute)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0; j < Stereo[0].Length; j++)
                        {
                            Stereo[i][j] = 0;
                        }
                    }
                }
                Output.Set(Stereo, ou);
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

            public void Set(float[][] stereo, float[][] ou)
            {
                for (int j = 0; j < stereo[0].Length; j++)
                {
                    ou[LeftIndex][j] = stereo[0][j];
                    ou[RightIndex][j] = stereo[1][j];
                }
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

            public void Get(float[][] inp, float[][] stereo)
            {

                for (int j = 0; j < stereo[0].Length; j++)
                {
                    stereo[0][j] = inp[j][LeftIndex];
                    stereo[1][j] = inp[j][RightIndex];
                }
            }

        }

    }

}
