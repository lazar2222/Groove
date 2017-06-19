using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Groove
{
    public partial class PianoRoll : Form
    {
        int cells;
        int bars;

        public PianoRoll(int bars)
        {
            this.bars = bars;
            cells = bars * 4 * 16;
            InitializeComponent();
        }

        private void PianoRoll_Load(object sender, EventArgs e)
        {
            DataGridViewColumn c;
            for (int i = 0; i < cells; i++)
            {
                c = new DataGridViewTextBoxColumn();
                c.Width = 15;
                c.HeaderText = (Math.Truncate(i / 16d) + 1).ToString();
                dataGridView1.Columns.Add(c);
            }
            DataGridViewRow r;
            for (int i = 0; i < 108; i++)
            {
                r = new DataGridViewRow();
                r.HeaderCell.Value = NoteLib.NoteLib.LUT.ElementAt(i).Key.ToString() ;
                dataGridView1.Rows.Add(r);
            }
            trackBar1.Maximum = cells;
        }

        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            
        }
    }
}
