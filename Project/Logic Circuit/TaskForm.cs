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
    public partial class TaskForm : Form
    {
        public TaskForm()
        {
            InitializeComponent();

            stask = "";
        }

        public string stask;
        public bool andisallowed;
        public bool orisallowed;
        public bool notisallowed;
        public bool zeroisallowed;
        public bool otherschemesareallowed;

        private void changenameschemebutton_Click(object sender, EventArgs e)
        {
            stask = textBox1.Text;

            andisallowed = checkBox1.Checked;
            orisallowed = checkBox2.Checked;
            notisallowed = checkBox3.Checked;
            zeroisallowed = checkBox4.Checked;
            otherschemesareallowed = checkBox5.Checked;

            this.Close();
        }

    }
}
