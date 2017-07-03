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
    public partial class SchemeNameForm : Form
    {
        public SchemeNameForm(char schemesymbol, string schemename)
        {
            InitializeComponent();
            textBox1.Text = schemename;
            textBox2.Text = schemesymbol.ToString();
            this.formschemesymbol = schemesymbol;
            this.formschemename = schemename;
        }

        public char formschemesymbol;
        public string formschemename;

        private void changenameschemebutton_Click(object sender, EventArgs e)
        {
            textBox1.Text = textBox1.Text.Replace(" ", string.Empty);
            if (textBox1.Text != "")
                formschemename = textBox1.Text;
            if (textBox2.Text != "" || textBox2.Text != " ")
                formschemesymbol = char.Parse(textBox2.Text);
            this.Close();
        }

    }
}
