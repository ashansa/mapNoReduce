using System.Runtime.Remoting.Channels.Tcp;
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

namespace PADIMapNoReduce
{

    public class Client : MarshalByRefObject, IClient
    {
        string url;
        private string inputFilePath;
        private string outputDir;
        IWorkerTracker contactingWorker;
        String mapperName;

       /* static void Main()
        {
            //call submit method directly
            Console.WriteLine("starting service");
            new Client(Constants.CLIENT_URL).submitTask(@"C:\Users\ashansa\Documents\tmp\input.txt", @"C:\Users\ashansa\Documents\tmp\out", 3, null);
            Console.ReadLine();
        }

        public Client(string url)
        {
            this.url = url;
            int clientPort = Int16.Parse(ConfigurationManager.AppSettings[Constants.APPSETT_CLIENT_URL]);
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] = clientPort;
            props["name"] = "client";
            props["timeout"] = 10000; // in milliseconds
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this, "Client", typeof(Client));

            contactingWorker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), "tcp://localhost:10001/Worker");
            // RemotingConfiguration.RegisterWellKnownServiceType(typeof(Client),
               //  "Client", WellKnownObjectMode.Singleton);
        }
    */

        public void initClient()
        {
            this.url = ConfigurationManager.AppSettings[Constants.APPSETT_CLIENT_URL];
            String[] namePortPair= url.Split(Constants.COLON_STR)[2].Split(Constants.SEP_PIPE);
            TcpChannel channel = new TcpChannel(Convert.ToInt32(namePortPair[0]));
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this, namePortPair[1], typeof(Client));

           
        }

        public void submitTask(String entryUrl,string inputFile, string outputDir, int splits, string mapperFunctionName)
        {
            this.inputFilePath = inputFile;
            this.outputDir = outputDir;
            this.mapperName = mapperFunctionName;
           
            byte[] input = File.ReadAllBytes(inputFilePath);
            JobMetadata jobDetails = new JobMetadata(input.Length, splits, url);
            contactingWorker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entryUrl);
            contactingWorker.receiveJobRequest(jobDetails);//calling the job tracker


           /* int length = input.Length;
            for (int i = 0; i < splits; i++)
            {
                int start = i * length / splits;
                int end = start + length / splits;
                //TODO give client url
                FileSplitMetadata metadata = new FileSplitMetadata(i, start, end, null);
                WorkerTaskMetadata workerData = receiveTaskRequest(metadata);
                Console.WriteLine("+====================================" + i);


                byte[] result = System.Text.Encoding.UTF8.GetBytes(workerData.Chunk);
                TaskResult taskResult = new TaskResult(result, i);
                receiveCompletedTask(taskResult);
            }*/
            //splitFile(inputFilePath, splits);

            //////////////////
        }

        public WorkerTaskMetadata receiveTaskRequest(FileSplitMetadata splitMetadata)
        {
            /*we need to set the input file part in workerMetadata.chunk*/
            String inputCode = ConfigurationManager.AppSettings[Constants.APPSET_DLL_PATH].ToString();
            byte[] code = File.ReadAllBytes(inputCode);
            string workChunk = getSplit(splitMetadata.StartPosition, splitMetadata.EndPosition);
            //string workChunk = "this is \r\n my nice little \r\n text file and \r\n it has 5 lines";
            WorkerTaskMetadata workerMetadata = new WorkerTaskMetadata(code, mapperName, workChunk);

            Console.WriteLine("split ===================> " + workerMetadata.Chunk);
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
        }

        private string getSplit(long startByte, long endByte)
        {
            FileStream fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);

            //if the startByte is in the middle of line start from next line
            if (startByte != 0)
            {
                fs.Seek(startByte - 1, SeekOrigin.Current);
                int previous = fs.ReadByte();
                //if previous is not 10 go forward to next line
                Console.WriteLine("byte before first byte--->" + previous);
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

            UnicodeEncoding unicode = new UnicodeEncoding();
            byte[] target = new byte[size+additional];
            fs.Read(target, 0, (int)(endByte+additional  - startByte));
            fs.Close();
            File.WriteAllBytes(outputDir + Path.DirectorySeparatorChar + "test.txt", target);
            string split = unicode.GetString(target);
            split = split.Trim();
           
            return split;
        }

        /* DECIDE WHETHER CLIENT DECIDE OR TRACKER INFORM
        public void receiveWorkCompleteStatus()
        {
        }*/


        /* NO NEED TO COMBINE 
           private void combineResults()
           {
               //////////////// temp setting output dir to test with others commented
               outputDir = @"C:\Users\ashansa\Documents\tmp\out";
               //combine results and clear output dir
               string[] resultFileParts = Directory.GetFiles(outputDir);

               Array.Sort(resultFileParts);
               foreach (string s in resultFileParts)
               {
                   Console.WriteLine(s);
               }
               Console.ReadLine();
           }*/

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
