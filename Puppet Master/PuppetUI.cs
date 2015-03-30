using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Puppet_Master
{
    public partial class PuppetUI : Form
    {
        PuppetService puppet = new PuppetService();
        public PuppetUI()
        {
            InitializeComponent();
        }

        private void worker_submit_Click(object sender, EventArgs e)
        {

        }

        private void init_Click(object sender, EventArgs e)
        {
            puppet.initPuppet(txt_puppetId.Text);
            init.Enabled = false;
        }
    }
}
