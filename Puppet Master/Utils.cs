using PADIMapNoReduce;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Puppet_Master
{
    public class Utils
    {
        Client client = null;
        PuppetService puppet = new PuppetService();

        public void executeCommand(string command)
        {
            string keyword = command.Split(' ')[0].ToLower();
            switch (keyword)
            {
                case "submit":
                    Console.WriteLine("job is going to be submitted");
                    submitJobToClient(command);
                    break;

                case "worker":
                   /* Thread thread = new Thread(() =>  createWorker(command));
                    thread.Start();*/
                    createWorker(command);
                    break;

                case "status":
                    callDisplayStatus();
                    break;

                case "sloww"://sloww 2 60
                    callWaitWorker(command);
                    break;

                case "wait":
                    waitPuppet(command);
                    break;

                case "freezew":
                    freezeWorker(command);
                    break;

                case "unfreezew":
                    unFreezeWorker(command);
                    break;

                case "freezec":
                    freezeTracker(command);
                    break;

                case "unfreezec":
                    unfreezeTracker(command);
                    break;

                default:
                    break;

            }
        }

        private void unfreezeTracker(string command)
        {
            string[] paramStr = command.Split(Constants.SPACE_CHAR);
            int trackerId = Convert.ToInt16(paramStr[1]);
            puppet.callRemoteUnfreezeTracker(trackerId);
        }

        private void freezeTracker(string command)
        {
            string[] paramStr = command.Split(Constants.SPACE_CHAR);
            int trackerId = Convert.ToInt16(paramStr[1]);
            puppet.callRemoteFreezeTracker(trackerId);
        }

        private void unFreezeWorker(string command)
        {
            string[] paramStr = command.Split(Constants.SPACE_CHAR);
            int workerId = Convert.ToInt16(paramStr[1]);
            puppet.callRemoteUnfreezeWorker(workerId);
        }

        private void freezeWorker(string command)
        {
            string[] paramStr = command.Split(Constants.SPACE_CHAR);
            int workerId = Convert.ToInt16(paramStr[1]);
            puppet.callRemoteFreezeWorker(workerId);
        }

        private void waitPuppet(string command)
        {
            string[] paramStr = command.Split(Constants.SPACE_CHAR);
            int delayTime= Convert.ToInt16(paramStr[1]);
            Thread.Sleep(delayTime*1000);
        }

        private void callWaitWorker(string command)
        {
            string[] paramStr = command.Split(Constants.SPACE_CHAR);
            int workerId = Convert.ToInt16(paramStr[1]);
            int delayInSeconds = Convert.ToInt32(paramStr[2]);
            Thread thread = new Thread(() => puppet.callRemoteWaitWorker(workerId, delayInSeconds));
            thread.Start();
        }

        public void executeScript(string[] scriptCommands)
        {
            foreach (string line in scriptCommands)
            {
                //ignoring lines starting with %  - comment lines
                if (!line.StartsWith("%"))
                {

                    executeCommand(line);
                }
            }
        }

        private void createWorker(string command)
        {
            String[] splits = command.Split(Constants.SPACE_CHAR);
            WorkerMetadata workerMetadata = new WorkerMetadata();
            workerMetadata.WorkerId = Convert.ToInt16(splits[1]);
            workerMetadata.PuppetRUL = splits[2];

            //Create worker-Id puppet map to use with wait, freeze, unfreeze etc
            puppet.WorkerPuppetMap.Add(workerMetadata.WorkerId, workerMetadata.PuppetRUL);
            puppet.UrlIdMap.Add(splits[3], workerMetadata.WorkerId);/* to fix issue of same puppet->multiple workers */

            if (splits.Length == 5 && splits[4] != string.Empty)
                workerMetadata.EntryURL = splits[4]+puppet.UrlIdMap[splits[4]];

            workerMetadata.ServiceURL = splits[3] + workerMetadata.WorkerId;

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

            //first param to fix issue of one puppet->multiple workers
            client.submitTask(pairs[1] + puppet.UrlIdMap[pairs[1]], pairs[2], pairs[3], Convert.ToInt16(pairs[4]), pairs[5], pairs[6]);
        }

        private void callDisplayStatus()
        {
            puppet.callPuppetsDisplayStatus();
        }


        internal void initPuppet()
        {
            String puppetsStr = ConfigurationManager.AppSettings[Constants.APPSETT_PUPPETS_URL].ToString();
            String[] puppets = puppetsStr.Split(';');
            for (int i = 0; i < puppets.Length; i++)//i==0 is myself
            {
                puppet.PuppetUrlList.Add(puppets[i]);
            }
            puppet.initPuppet(puppets[0]);//later we will change it go take from puppetStr
        }
    }
}
