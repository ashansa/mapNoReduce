using System;
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
        [STAThread]
        static void Main()
        {
            //Run UI
           // Application.Run(new ClientApp());
        }

        public ClientApp()
        {
            InitializeComponent();
            
        }

        private void btSubmit_Click(object sender, EventArgs e)
        {
            int splits = Int32.Parse(txSplits.Text);
            Client client = new Client();
            client.initClient();
            client.submitTask(txtContactWorker.Text, txInputPath.Text, txOutputPath.Text, Convert.ToInt32(txSplits.Text), txtMapperName.Text, txMapperPath.Text);
        }

        private void ClientApp_Load(object sender, EventArgs e)
        {

        }

        public void addMessage(String message)
        {
            txtComplete.Text = message;
        }
        public delegate void printMsg(string message);
    }
}
