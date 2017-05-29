using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DipBase;

namespace TriOsc
{
    public class TriOsc : Input, Instrument
    {
        public void Get(float[,] inp, int i, float[] stereo)
        {
            Get(stereo);
        }

        public AutoParams[] GerAutomationParams()
        {
            throw new NotImplementedException();
        }

        public void Get(float[] output)
        {
            throw new NotImplementedException();
        }

        public double GetAutomation(string name)
        {
            throw new NotImplementedException();
        }

        public PluginInfo GetPluginInfo()
        {
            throw new NotImplementedException();
        }

        public void SetAutomation(string name, double value)
        {
            throw new NotImplementedException();
        }

        public void SetNote(Note note)
        {
            throw new NotImplementedException();
        }

        public void SetParams(Note note)
        {
            throw new NotImplementedException();
        }
    }
}
