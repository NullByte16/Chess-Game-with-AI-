using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    public partial class ChooseDubbedPiece : Form
    {
        public ChooseDubbedPiece()
        {
            InitializeComponent();
        }

        public String chosen = null;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            chosen = "knight";
            this.Close();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            chosen = "queen";
            this.Close();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            chosen = "rook";
            this.Close();
        }
    }
}
