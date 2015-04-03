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
        PuppetService puppet = new PuppetService();
        Client client = null;

        public PuppetUI()
        {
            InitializeComponent();
        }

        private void worker_submit_Click(object sender, EventArgs e)
        {
            WorkerMetadata workerMetadata = createMetadataObjFromCommand(txt_workerCreate.Text);
            puppet.createWorker(workerMetadata);
        }

        private WorkerMetadata createMetadataObjFromCommand(string command)
        {
            String[] splits = command.Split(Constants.SPACE_CHAR);
            WorkerMetadata workerMetadata = new WorkerMetadata();
            workerMetadata.WorkerId = Convert.ToInt16(splits[1]);
            workerMetadata.PuppetRUL = splits[2];
            workerMetadata.ServiceURL = splits[3];

            if(splits.Length==5 && splits[4]!=string.Empty)
            workerMetadata.EntryURL = splits[4];
            return workerMetadata;

        }

        private void init_Click(object sender, EventArgs e)
        {
            puppet.initPuppet(txt_puppetId.Text);
            init.Enabled = false;
        }

        private void client_submit_Click(object sender, EventArgs e)
        {
            if (client == null)
            {
                client = new Client();
                client.initClient();
            }
            
            String[] pairs = txt_clientCreate.Text.Split(Constants.SPACE_CHAR);
            client.submitTask(pairs[1], pairs[2], pairs[3],Convert.ToInt16( pairs[4]), pairs[5]);
        }
    }
}
