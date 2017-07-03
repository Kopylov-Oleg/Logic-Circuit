using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Logic_Circuit
{
    public partial class ChangeNameForm : Form
    {
        public ChangeNameForm(string name)
        {
            InitializeComponent();
            textBox1.Text = name;
            this.name = name;
        }

        public string name;

        private void savenamebutton_Click(object sender, EventArgs e)
        {
            textBox1.Text = textBox1.Text.Replace(" ", string.Empty);
            if (textBox1.Text != "")
                name = textBox1.Text;
            //name = (textBox1.Text == null) ? "" : textBox1.Text;
            this.Close();
        }

    }
}
