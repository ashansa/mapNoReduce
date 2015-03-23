using PADIMapNoReduce;
using System;
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
        Boolean isMapSuspended;

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
        public IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

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
                        object resultObject = type.InvokeMember("Map",
                          BindingFlags.Default | BindingFlags.InvokeMethod,
                               null,
                               ClassObj,
                               args);
                        result.Concat((IList<KeyValuePair<string, string>>)resultObject);
                        // result= (IList<KeyValuePair<string, string>>)resultObject;
                        Console.WriteLine("Map call result was: ");
                        foreach (KeyValuePair<string, string> p in result)
                        {
                            Console.WriteLine("key: " + p.Key + ", value: " + p.Value);
                        }
                        return true;
                    }
                }
            }
            throw (new System.Exception("could not invoke method"));
            return true;
        }

        internal TaskResult processMapTask(WorkerTaskMetadata workerTaskMetadata, FileSplitMetadata splitMetaData)
        {
            String chunk = workerTaskMetadata.Chunk;
            long lineNumber =splitMetaData.StartPosition;
            long linesProcessed = 0;
            string line;
            using (StringReader reader = new System.IO.StringReader(chunk))
            {
                while (true)
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        if (!isMapSuspended)
                        {
                            runMapperForLine(workerTaskMetadata.Code, workerTaskMetadata.MapperClassName, lineNumber++, "this is test");
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
                return createTaskResultBoject(splitMetaData.SplitId);
            }
        }

        private void setTaskStatus(FileSplitMetadata splitMetaData,long linesProcessed)
        {
            long totalLines = splitMetaData.EndPosition - splitMetaData.StartPosition;
            double percentage = 100*(linesProcessed / (double)totalLines);
            int oldfactor = (int)status.PercentageCompleted/10;
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
            TaskResult taskResult = new TaskResult();
            taskResult.Result = result;
            taskResult.SplitId = splitId;
            return taskResult;
        }
    }
}
