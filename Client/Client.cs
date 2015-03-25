using System.Runtime.Remoting.Channels.Tcp;
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

        static void Main(string[] args) {

          //  new Client().combineResults();
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType( typeof(Client),
                "Client",WellKnownObjectMode.Singleton);
            new Client().submitTask(@"C:\Users\ashansa\Documents\tmp\input.txt",3, @"C:\Users\ashansa\Documents\tmp\out", null);
            Console.ReadLine();
        }

        public void submitTask(string inputFile, int splits, string outputDir, string mapperFunctionFile)
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
            String inputCode = @"C:\Users\ashansa\softwares\github\mapNoReduce\LibMapper\bin\Debug\LibMapper.dll";
            byte[] code = File.ReadAllBytes(inputCode);
            string workChunk = getSplit(splitMetadata.StartPosition, splitMetadata.EndPosition);
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

            FileStream fs2 = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);

            ////////tmp
            if (startByte != 0)
            {
                byte[] buf = new byte[2];
                fs2.Seek(startByte -1, SeekOrigin.Current);
                int previous =  fs2.ReadByte();
                //if previous is not 10 go forward to next line
                Console.WriteLine("byte before first byte--->" + previous);
                int i;
                while ((i = fs2.ReadByte()) != '\n')
                {
                    startByte++;
                }
            }
           
            ////////
            FileStream fs = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);

            byte[] buffer = new byte[(endByte - startByte) * 2];
            fs.Seek(startByte, SeekOrigin.Current);
           // int size = fs.Read(buffer, startByte, endByte - startByte);
            int size = fs.Read(buffer, 0, endByte - startByte);
            int c;

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

            //fs.Close();
            string split = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            return split;
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
                Directory.CreateDirectory(outputDir);
                var destinationFile = new StreamWriter(outputDir + Path.DirectorySeparatorChar + splitName);
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
            string[] resultFileParts = Directory.GetFiles(outputDir);

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
