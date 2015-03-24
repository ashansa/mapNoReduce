﻿using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;


namespace PADIMapNoReduce {

    public class Client :MarshalByRefObject, IClient {

        private string inputFilePath;
        private string outputDir;
        private const string tempDir = "tmp"; 

        static void Main(string[] args) {

          //  new Client().combineResults();
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Client),
                "Client",
                WellKnownObjectMode.Singleton);
          //  new Client().submitTask(@"C:\Users\ashansa\Documents\tmp\input.txt", 4, @"C:\Users\ashansa\Documents\tmp\out", null);
            Console.ReadLine();
        }

        public void submitTask(string inputFile, int splits, string outputDir, string mapperFunctionFile)
        {
            this.inputFilePath = inputFile;
            this.outputDir = outputDir;
            splitFile(inputFilePath, splits);

            //temp calling receive task here
            receiveCompletedTask(null, null);
        }

        public WorkerTaskMetadata receiveTaskRequest(FileSplitMetadata splitMetadata)
        {
            /*we need to set the input file part in workerMetadata.chunk*/
            string mapperName = "Mapper";
            String inputCode = "E:\\GIT\\PADI\\mapNoReduce\\LibMapper\\bin\\Debug\\LibMapper.dll";
            byte[] code = File.ReadAllBytes(inputCode);
            String strToProcess = "this is \r\n my nice string \r\n to process \r\n and it is too small";
            WorkerTaskMetadata workerMetadata = new WorkerTaskMetadata(code, mapperName, strToProcess);
            return workerMetadata;
        }


        public Boolean receiveCompletedTask(TaskResult taskResult)
        {
            /*we need to get the bytestream and then write to file*/
            try
            {
                File.WriteAllBytes("E:\\input\\chathuri-"+taskResult.SplitId+".txt",taskResult.Result);
                return true;
            }
            catch (Exception ex)
            {
                return false;//what we do if fails
            }
        }
        public void receiveCompletedTask(StreamReader resultStream, string splitName)
        {
            /////////////// temp place to result files
            string[] files = Directory.GetFiles(@"C:\Users\ashansa\Documents\tmp\");
            int count = 0;
            ////////////
            
            foreach (string file in files)
            {
                ///////////////// temp creating stream and split name 
                var sourceFile = new StreamReader(file);
                resultStream = sourceFile;
                count++;
                splitName = "file"+count+".txt";
                /////////////////

                string line;
                Directory.CreateDirectory(outputDir + Path.DirectorySeparatorChar + tempDir);
                var destinationFile = new StreamWriter(outputDir + Path.DirectorySeparatorChar + 
                    tempDir + Path.DirectorySeparatorChar + splitName);
                while ((line = resultStream.ReadLine()) != null)
                {
                    destinationFile.WriteLine(line);
                    destinationFile.Flush();
                }    
            }

            /////////////////// temp calling task completed method from here
            receiveWorkCompleteStatus();
        }

        public void receiveWorkCompleteStatus()
        {
            //TODO combine the result files
            combineResults();
        }

        private void splitFile(string sourceFilePath, int splits)
        {
            string fileWithoutExtension = Regex.Split(sourceFilePath, ".txt")[0];
            string destinationFileName = fileWithoutExtension + "-{0}To{1}.txt";
            int linesPerFile;
            int totalLineCount = File.ReadAllLines(sourceFilePath).Length;
            
            if (totalLineCount % splits == 0)
                linesPerFile = totalLineCount / splits;
            else
                linesPerFile = totalLineCount / splits + 1;

            using (var sourceFile = new StreamReader(sourceFilePath))
            {
                var fileCounter = 0;
                int currentReadingLine = 0;
                var destinationFile = new StreamWriter(
                    string.Format(destinationFileName, currentReadingLine +1 , currentReadingLine + linesPerFile ));

                try
                {
                    var lineCounter = 0;
                    string line;
                    int start = 0;
                   
                    while ((line = sourceFile.ReadLine()) != null)
                    {
                        currentReadingLine++;
                        if (lineCounter >= linesPerFile)
                        {
                            //starting a new file
                            lineCounter = 0;
                            fileCounter++;
                            start = currentReadingLine;

                            destinationFile.Dispose();
                            if(currentReadingLine == totalLineCount)
                                destinationFile = new StreamWriter(string.Format(destinationFileName, start, totalLineCount));
                            else
                                destinationFile = new StreamWriter(string.Format(destinationFileName, start, (start + linesPerFile - 1)));
                        }

                        destinationFile.WriteLine(line);
                        lineCounter++;
                    }
                }
                finally
                {
                    destinationFile.Dispose();
                }
            }
        }

        private void combineResults()
        {
            //////////////// temp setting output dir to test with others commented
            outputDir = @"C:\Users\ashansa\Documents\tmp\out";
            //combine results and clear output dir
            string[] resultFileParts = Directory.GetFiles(outputDir + Path.DirectorySeparatorChar + tempDir);

            Array.Sort(resultFileParts);
            foreach (string s in resultFileParts)
            {
                Console.WriteLine(s);
            }
            Console.ReadLine();
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
