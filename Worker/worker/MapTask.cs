using PADIMapNoReduce;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Server.worker
{
    public class MapTask
    {
        private Status status = new Status();
        int splitId;

        public int SplitId
        {
            get { return splitId; }
            set { splitId = value; }
        }
        Boolean isMapSuspended = false;

        public Boolean IsMapSuspended
        {
            get { return isMapSuspended; }
            set { isMapSuspended = value; }
        }

        public Status Status
        {
            get { return status; }
            set { status = value; }
        }
        List<KeyValuePair<string, string>> result;

        public bool runMapperForLine(byte[] code, string className, long lineNumber, String line)
        {
            Assembly assembly = Assembly.Load(code);
            // Walk through each type in the assembly looking for our class
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsClass == true)
                {
                    if (type.FullName.EndsWith("." + className))
                    {
                        // create an instance of the object
                        object ClassObj = Activator.CreateInstance(type);

                        // Dynamically Invoke the method
                        object[] args = new object[] { line };
                        IList resultObject = (IList)type.InvokeMember("Map",
                         BindingFlags.Default | BindingFlags.InvokeMethod,
                              null,
                              ClassObj,
                              args);
                        result.AddRange((IList<KeyValuePair<string, string>>)resultObject);

                        return true;
                    }
                }
            }
            throw (new System.Exception("could not invoke method"));
            return false;
        }

        internal TaskResult processMapTask(WorkerTaskMetadata workerTaskMetadata, FileSplitMetadata splitMetaData)
        {
            String chunk = workerTaskMetadata.Chunk;
            long lineNumber = splitMetaData.StartPosition;
            long linesProcessed = 0;
            string line;
            result = new List<KeyValuePair<string, string>>();
            using (StringReader reader = new System.IO.StringReader(chunk))
            {
                while (true)
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        if (!isMapSuspended)
                        {
                            runMapperForLine(workerTaskMetadata.Code, workerTaskMetadata.MapperClassName, lineNumber++, line);

                            linesProcessed++;
                            setTaskStatus(splitMetaData, linesProcessed);
                        }
                        else
                        {
                            //clear the results and wait for next map
                            result = new List<KeyValuePair<string, string>>();
                            isMapSuspended = false;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                Console.WriteLine("total sequences" + lineNumber);
                ////send complete status

                return createTaskResultBoject(splitMetaData.SplitId);
            }
        }

        private void setTaskStatus(FileSplitMetadata splitMetaData, long linesProcessed)
        {
            long totalLines = splitMetaData.EndPosition - splitMetaData.StartPosition;
            double percentage = 100 * (linesProcessed / (double)totalLines);
            int oldfactor = (int)status.PercentageCompleted / 10;
            int newfactor = (int)percentage / 10;

            status.PercentageCompleted = percentage;

            lock (status)
            {
                if (newfactor > oldfactor)//send for each 10% percentage
                {
                    Monitor.Pulse(status);
                }
            }

        }

        private TaskResult createTaskResultBoject(int splitId)
        {
            TaskResult taskResult = new TaskResult(getByteStreamOfResults(), splitId);
            return taskResult;
        }

        private byte[] getByteStreamOfResults()
        {
            StringBuilder output = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in result)
            {
                output.Append(pair.Key).Append(":").Append(pair.Value);
                output.Append("\r\n");
            }
            byte[] bytes = new byte[output.ToString().Length * sizeof(char)];
            System.Buffer.BlockCopy(output.ToString().ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
    }
}
