using PADIMapNoReduce;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Puppet_Master
{
    public class Utils
    {
        Client client = null;
        PuppetService puppet = new PuppetService();
        
        private void createWorker(string command)
        {
            String[] splits = command.Split(Constants.SPACE_CHAR);
            WorkerMetadata workerMetadata = new WorkerMetadata();
            workerMetadata.WorkerId = Convert.ToInt16(splits[1]);
            workerMetadata.PuppetRUL = splits[2];
            workerMetadata.ServiceURL = splits[3];

            if (splits.Length == 5 && splits[4] != string.Empty)
                workerMetadata.EntryURL = splits[4];
            puppet.createWorker(workerMetadata);
        }

        private void submitJobToClient(String command)
        {
            if (client == null)
            {
                client = new Client();
                client.initClient();
            }
            String[] pairs = command.Split(Constants.SPACE_CHAR);
            client.submitTask(pairs[1], pairs[2], pairs[3], Convert.ToInt16(pairs[4]), pairs[5]);
        }

        public void executeCommand(string command)
        {
            string keyword = command.Split(' ')[0].ToLower();
            switch (keyword)
            {
                case "submit":
                    submitJobToClient(command);
                    break;

                case "worker":
                    createWorker(command);
                    break;

                case "status":
                    callDisplayStatus();
                    break;
                default:
                    break;
                
            }
        }

        private void callDisplayStatus()
        {
            puppet.callPuppetsDisplayStatus();
        }


        internal void initPuppet(string puppetUrl)
        {
            String puppetsStr= ConfigurationManager.AppSettings[Constants.APPSETT_PUPPETS_URL].ToString();
            String[] puppets = puppetsStr.Split(';');
            for (int i = 1; i < puppets.Length; i++)//i==0 is myself
            {
                puppet.OtherPuppetUrls.Add(puppets[i]);
            }
            puppet.initPuppet(puppetUrl);//later we will change it go take from puppetStr
        }
    }
}
