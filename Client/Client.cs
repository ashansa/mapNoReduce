﻿using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Configuration;
using System.Collections;
using System.Text;
using System.Threading;

namespace PADIMapNoReduce
{

    public class Client : MarshalByRefObject, IClient
    {
        string url;
        string dllPath;
        private string inputFilePath;
        private string outputDir;
        IWorkerTracker contactingWorker;
        String mapperName;

        public void initClient()
        {
            this.url = ConfigurationManager.AppSettings[Constants.APPSETT_CLIENT_URL];
            String[] namePortPair= url.Split(Constants.COLON_STR)[2].Split(Constants.SEP_PIPE);
            TcpChannel channel = new TcpChannel(Convert.ToInt32(namePortPair[0]));
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this, namePortPair[1], typeof(Client));

           
        }

        public void submitTask(String entryUrl,string inputFile, string outputDir, int splits, string mapperFunctionName,string dllPath)
        {
            this.inputFilePath = inputFile;
            this.outputDir = outputDir;
            this.mapperName = mapperFunctionName;
            this.dllPath = dllPath;
            byte[] input = File.ReadAllBytes(inputFilePath);
            JobMetadata jobDetails = new JobMetadata(input.Length, splits, url);
            contactingWorker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entryUrl);
            contactingWorker.receiveJobRequest(jobDetails);//calling the job tracker
        }

        public WorkerTaskMetadata receiveTaskRequest(FileSplitMetadata splitMetadata)
        {
            /*we need to set the input file part in workerMetadata.chunk*/
            String inputCode = this.dllPath;
            byte[] code = File.ReadAllBytes(inputCode);
            string workChunk = getSplit(splitMetadata.StartPosition, splitMetadata.EndPosition);
            //string workChunk = "this is \r\n my nice little \r\n text file and \r\n it has 5 lines";
            WorkerTaskMetadata workerMetadata = new WorkerTaskMetadata(code, mapperName, workChunk);
            Console.WriteLine(Environment.CurrentDirectory);
            return workerMetadata;
        }


        public Boolean receiveCompletedTask(TaskResult taskResult)
        {
            /*we need to get the bytestream and then write to file*/
            try
            {
                File.WriteAllBytes(outputDir + Path.DirectorySeparatorChar + taskResult.SplitId + ".txt", taskResult.Result);
                return true;
            }
            catch (Exception ex)
            {
                return false;//what we do if fails
            }
        }

        public void receiveJobCompletedNotification()
        {
            //TODO: notify UI???
            ClientApp clientapp = new ClientApp();
            new Thread(delegate()
           {
               Application.Run(clientapp);
           }).Start();
            Thread.Sleep(2000);
              ClientApp.printMsg eve = new ClientApp.printMsg(clientapp.addMessage);
                try
                {
                    clientapp.Invoke(eve, new Object[] { "job completed" });
                    // form1.Invoke(new ChatApp.Form1.printMsg(form1.addMessage), new Object[] { msg });
                }
                catch (Exception ex)
                {
                    
                }
            }
            

        private string getSplit(long startByte, long endByte)
        {
            FileStream fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);

            //if the startByte is in the middle of line start from next line
            if (startByte != 0)
            {
                fs.Seek(startByte - 1, SeekOrigin.Current);
                int previous = fs.ReadByte();
                int i;
                while ((i = fs.ReadByte()) != '\n')
                {
                    startByte++;
                }
            }

            fs.Position = 0;
            byte[] buffer = new byte[(endByte - startByte) * 2];
            fs.Seek(startByte, SeekOrigin.Current);
            int size = fs.Read(buffer, 0, (int)(endByte- startByte));
            int c;
            int additional = 0;
            //if endByte is in the middle of line, read until the end of line
            while ((c = fs.ReadByte()) != -1)
            {
                if (c == '\n')
                {          
                    break;
                }
                else
                {
                   additional++;
                }
            }


            fs.Position = 0;
            fs.Seek(startByte, SeekOrigin.Current);

            byte[] target = new byte[size+additional];
            fs.Read(target, 0, (int)(endByte+additional  - startByte));
            fs.Close();
            //string split = new UnicodeEncoding().GetString(target);
            string split = System.Text.Encoding.UTF8.GetString(target);
            split = split.Trim();
           
            return split;
        }

        #region specific
        //to avoid expiring objects within 20 minutes
        public override object InitializeLifetimeService()
        {
            //return base.InitializeLifetimeService();
            return null;
        }
        #endregion
    }
}
