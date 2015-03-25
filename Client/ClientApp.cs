﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PADIMapNoReduce
{
    public partial class ClientApp : Form
    {
        public ClientApp()
        {
            InitializeComponent();
        }

        private void btSubmit_Click(object sender, EventArgs e)
        {
            int splits = Int32.Parse(txSplits.Text);
            new Client().submitTask(txInputPath.Text, txOutputPath.Text, splits, txMapper.Text);
        }
    }
}
