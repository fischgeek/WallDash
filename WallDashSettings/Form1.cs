﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WallDashSettings
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //var x = SharedFrameworkForms.InputBox.Show("Enter stuff", "A Title");
            //SharedFSharpForms.Forms.Picker
            WallDash.FSharp.Forms.MakeForm();
        }
    }
}
