using PADIMapNoReduce;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Puppet_Master
{
    public partial class PuppetUI : Form
    {

        Utils utils = new Utils();
        string scriptPath = null;
        string[] scriptCommands = null;

        public PuppetUI()
        {
            try
            {
                utils.initPuppet();
                InitializeComponent();
            }
            catch (Exception ex)
            {
              
            }
        }

     /*   private void worker_submit_Click(object sender, EventArgs e)
        {
            utils.executeCommand(txt_workerCreate.Text);          
        }


        private void client_submit_Click(object sender, EventArgs e)
        {
            utils.executeCommand(txt_clientCreate.Text);
        }*/

   

        private void button1_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(() => utils.executeCommand(txt_command.Text));
            thread.Start();
            
        }

        // browse file button
        private void button2_Click(object sender, EventArgs e)
        {
            if (openScriptFileDialog.ShowDialog() == DialogResult.OK)
            {
                this.label_filePath.Text = openScriptFileDialog.FileName;
                scriptPath = openScriptFileDialog.FileName;
            }
        }
        //btn_executeScript button
        private void button3_Click(object sender, EventArgs e)
        {
            if (scriptPath == null)
            {
                label_filePath.Text = "Please select the file with script commands";
            }
            else
            {
                scriptCommands = File.ReadAllLines(scriptPath);
                Thread thread = new Thread(() => utils.executeScript(scriptCommands));
                thread.Start();
            }
        }

        private void btn_Status_Click(object sender, EventArgs e)
        {
            utils.executeCommand("status");
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            utils.executeCommand(txt_slow_worker.Text);
        }
    }
}
