using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio;
using NAudio.Wave;
using System.Windows;

namespace Groove.Pipeline
{
    class Player
    {
        public  AsioOut ASIO;
        public float[,] inpbuf;
        public float[,] outbuf;
        int selected_device;
        WaveFormat format;
        Mixer m;
        IntPtr[] inp;
        IntPtr[] ou;
        int spb;

        public Player(Mixer m)
        {
            this.m = m;
        }

        public void SetFormat(int sr, int bd, int chn)
        {
            format = new WaveFormat(sr, bd, chn);
        }

        public int[] GetFormat()
        {
            int[] a = { format.SampleRate, format.BitsPerSample, format.Channels };
            return a;
        }

        public void Init()
        {
            ASIO = new AsioOut(selected_device);
            ASIO.InitRecordAndPlayback(new BufferedWaveProvider(format), format.Channels, format.SampleRate);
            ASIO.AudioAvailable += ASIO_AudioAvailable;
        }

        internal void SetDevice(int v)
        {
            selected_device = v;
        }

        private unsafe void ASIO_AudioAvailable(object sender, AsioAudioAvailableEventArgs e)
        {
            if (inpbuf == null) { inpbuf = new float[e.SamplesPerBuffer, e.InputBuffers.Length];
                                  outbuf = new float[e.SamplesPerBuffer, e.InputBuffers.Length];}
            if (e.AsioSampleType == NAudio.Wave.Asio.AsioSampleType.Int32LSB)
            {
                spb = e.SamplesPerBuffer;
                ou = e.OutputBuffers;
                inp = e.InputBuffers;

                for (int i = 0; i < spb; i++)
                {
                    for (int j = 0; j < inp.Length; j++)
                    {
                        inpbuf[i, j] = *((int*)inp[j] + i)/(float)int.MaxValue;
                    }
                    m.Mix(inpbuf, outbuf,spb,ou.Length);
                    //for (int j = 0; j < ou.Length; j++)
                    //{
                    //    outbuf[i, j] = inpbuf[i, j];
                    //}
                    for (int j = 0; j < ou.Length; j++)
                    {
                        *((int*)ou[j] + i) = (int)(outbuf[i,j]*(float)int.MaxValue);
                    }
                }
                e.WrittenToOutputBuffers = true;
            }
        }

    }
}
