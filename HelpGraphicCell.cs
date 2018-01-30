using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess
{
    public class HelpGraphicCell : Button
    {
        public readonly int I, J;
        readonly MainForm Papa;  // container
        public readonly System.Drawing.Color OriginalColor;
        public System.Drawing.Icon Icon;


        public HelpGraphicCell(int i, int j, MainForm papaForm, System.Drawing.Icon Icon)
        {
            this.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Location = new System.Drawing.Point(j * 50 + 700, i * 50 + 25);
            this.Size = new System.Drawing.Size(50, 50);
            this.UseVisualStyleBackColor = true;
            this.Icon = Icon;

            this.OriginalColor = i % 2 == j % 2 ? System.Drawing.Color.Gray : System.Drawing.Color.WhiteSmoke;

            papaForm.Controls.Add(this);
            this.Click += new System.EventHandler(this.OnClick);

            this.BackColor = OriginalColor;

            I = i;
            J = j;
            Papa = papaForm;
        }

        private void OnClick(object sender, EventArgs e)
        {
            Papa.CheckMyColor(I, J);
        }
    }
}