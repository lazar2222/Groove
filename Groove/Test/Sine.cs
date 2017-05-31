using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DipBase;
using NAudio.Wave.SampleProviders;

namespace Groove.Test
{
    class Sine : Input
    {
        SignalGenerator s = new SignalGenerator(48000, 1);

        public void Get(float[][] dis, float[][] inp)
        {
            s.Read(inp[0], 0, inp[0].Length);
            inp[0].CopyTo(inp[1],0);
        }

        public void SetFreq(double freq, double vel)
        {
            s.Frequency = freq;
        }
    }
}
