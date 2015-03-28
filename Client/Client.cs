using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;


namespace PADIMapNoReduce
{

    public class Client : MarshalByRefObject, IClient
    {

        private string inputFilePath = "E:\\input\\chathuri.txt";
        private string outputDir = "E:\\input";

        public Client()
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this, "Client",
            typeof(Client));

            /* RemotingConfiguration.RegisterWellKnownServiceType(typeof(Client),
                 "Client", WellKnownObjectMode.Singleton);*/
        }

        public void submitTask(string inputFile, string outputDir, int splits, string mapperFunctionFile)
        {
            this.inputFilePath = inputFile;
            this.outputDir = outputDir;
            // get tracker service and send


            /////////////////
            //testing getSplit and receiveCompletedTask methods
            byte[] input = File.ReadAllBytes(inputFilePath);
            int length = input.Length;
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
            }

            //splitFile(inputFilePath, splits);

            //////////////////
        }

        public WorkerTaskMetadata receiveTaskRequest(FileSplitMetadata splitMetadata)
        {
            /*we need to set the input file part in workerMetadata.chunk*/
            string mapperName = "Mapper";
            String inputCode = @"..\..\..\LibMapper\bin\Debug\LibMapper.dll";
            byte[] code = File.ReadAllBytes(inputCode);
            //string workChunk = getSplit(splitMetadata.StartPosition, splitMetadata.EndPosition);
            string workChunk = "this is \r\n my nice little \r\n text file and \r\n it has 5 lines";
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

        private string getSplit(int startByte, int endByte)
        {
            /*using (BinaryReader b = new BinaryReader(File.Open(inputFilePath, FileMode.Open)))
            {
                /////TODO... check for line end
                b.BaseStream.Seek(startByte, SeekOrigin.Begin);
                byte[] byteSplit = b.ReadBytes(endByte - startByte);

                string split = System.Text.Encoding.UTF8.GetString(byteSplit, 0, byteSplit.Length);
                return split;
            }*/

            FileStream fs0 = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);

            //if the startByte is in the middle of line start from next line
            if (startByte != 0)
            {
                byte[] buf = new byte[2];
                fs0.Seek(startByte - 1, SeekOrigin.Current);
                int previous = fs0.ReadByte();
                //if previous is not 10 go forward to next line
                Console.WriteLine("byte before first byte--->" + previous);
                int i;
                while ((i = fs0.ReadByte()) != '\n')
                {
                    startByte++;
                }
            }
            fs0.Close();

            FileStream fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);

            byte[] buffer = new byte[(endByte - startByte) * 2];
            fs.Seek(startByte, SeekOrigin.Current);
            int size = fs.Read(buffer, 0, endByte - startByte);
            int c;

            //if endByte is in the middle of line, read until the end of line
            while ((c = fs.ReadByte()) != -1)
            {
                if (c == '\n')
                {
                    break;
                }
                else
                {
                    size++;
                    buffer[size] = (byte)c;
                }
            }

            fs.Close();
            string split = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
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
