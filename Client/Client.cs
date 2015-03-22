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
        private string outputFilePah;

        static void Main(string[] args) {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Client),
                "Client",
                WellKnownObjectMode.Singleton);
          
           new Client().splitFile(@"C:\Users\ashansa\Documents\tmp\inputt.txt", 4);

            Console.ReadLine();
           
        }

        public void submitTask(string inputFile, int splits, string outputDir, string mapperFunctionFile)
        {
            this.inputFilePath = inputFile;
            this.outputFilePah = outputDir;
        }

        /**/
        public WorkerTaskMetadata receiveTaskRequest(FileSplitMetadata splitMetadata)
        {
            /*retrieve split metadata and set workermetadata for part file*/
            string mapperName = "Mapper";
            String inputCode = "E:\\Semester2-Chathuri\\Middleware\\project\\MapperTransfer\\MapperTransfer\\LibMapper\\bin\\Debug\\LibMapper.dll";
            byte[] code = File.ReadAllBytes(inputCode);
            WorkerTaskMetadata workerMetadata = new WorkerTaskMetadata();
            workerMetadata.MapperClassName = mapperName;
            workerMetadata.Code = code;
            return workerMetadata;
        }


        public Boolean receiveCompletedTask(TaskResult taskResult)
        {
            return true;
           
        }

        public void receiveWorkCompleteStatus()
        {
            //TODO combine the result files
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
    }
}