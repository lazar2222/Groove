using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DipBase
{
    public class Note
    {
        public double freq;
        public int velocity;
        public int aftertoch;
        public double pitcbend;
    }

    public class AutoParams
    {
        public double min;
        public double max;
        public string name;
    }

    public class PluginInfo
    {
        public int type;
        public string name;
        public string version;

    }

    public enum PluginType
    {
        Instrument,
        AudioEffect,
        MidiEffect
    }

    public interface Instrument
    {
        PluginInfo GetPluginInfo();
        AutoParams[] GerAutomationParams();
        double GetAutomation(string name);
        void SetAutomation(string name, double value);
        void Get(float[] output);
        void SetNote(Note note);
        void SetParams(Note note);
    }

    public interface AudioEffect
    {
        PluginInfo GetPluginInfo();
        AutoParams[] GerAutomationParams();
        double GetAutomation(string name);
        void SetAutomation(string name, double value);
        void Get(float[] output);
    }

    public interface Input
    {
        void Get(float[,] inp, int i, float[] stereo);
    }
    public interface Output
    {
        void Set(float[] stereo);
        void Set(float[] stereo, float[,] ou, int i);
    }
}
