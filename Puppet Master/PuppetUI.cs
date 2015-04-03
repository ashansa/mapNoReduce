using PADIMapNoReduce;
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

        Utils utils = new Utils();

        public PuppetUI()
        {
            InitializeComponent();
        }

        private void worker_submit_Click(object sender, EventArgs e)
        {
            utils.executeCommand(txt_workerCreate.Text);          
        }


        private void init_Click(object sender, EventArgs e)
        {
            utils.initPuppet(txt_puppetId.Text);
            init.Enabled = false;
        }

        private void client_submit_Click(object sender, EventArgs e)
        {
            utils.executeCommand(txt_clientCreate.Text);
        }

        private void btn_View_Status_Click(object sender, EventArgs e)
        {
            //call the client, 
        }
    }
}
