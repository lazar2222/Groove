using Groove.Pipeline;
using Groove.Test;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Groove
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        Mixer m;
        Player p;
        Sine s;

        private void Main_Load(object sender, EventArgs e)
        {
            //signal chain

            s = new Sine();
            m = new Mixer();
            p = new Player(m);
            p.SetDevice(0);
            p.SetFormat(48000, 16, 2);
            p.Init();
            m.PopHInps(p);
            m.PopHOuts(p);
            m.MasterC.Setup(m);
            m.Channels.Add(new Mixer.Channel(64));
            m.Channels[0].Input = s;
            m.Channels[0].level = 1;
            m.Channels[0].mute = false;
            m.Channels[0].name = "Track1";
            m.Channels[0].Output = m.MasterC;
            m.Channels[0].Pan = 0;

        }

        private void button1_Click(object sender, EventArgs e)
        {
           p.ASIO.Play();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            p.ASIO.Stop();
            Thread.Sleep(500);
            p.w.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            m.MasterC.mute = !m.MasterC.mute;
        }

        private void trackBar2_ValueChanged(object sender, EventArgs e)
        {
            m.MasterC.level = (float)trackBar1.Value/100;
            
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            m.Channels[0].Pan = (float)trackBar2.Value/10;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            s.SetFreq(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text));
        }
    }
}
