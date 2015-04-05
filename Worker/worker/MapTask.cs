﻿using PADIMapNoReduce;
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
        private List<Status> statusList = new List<Status>();//to send status updates to tracker
        int splitId;
        static Boolean requiredStatusSend = false;
        Status currentStatus = new Status();//to keep track of local current status

        public Status CurrentStatus
        {
            get { return currentStatus; }
            set { currentStatus = value; }
        }

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

        public List<Status> StatusList
        {
            get { return statusList; }
        }
    
        List<KeyValuePair<string, string>> result;

        public bool runMapperForLine(byte[] code, string className, String line)
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

        internal TaskResult processMapTask(WorkerTaskMetadata workerTaskMetadata, FileSplitMetadata splitMetaData,int workerId)
        {
            String chunk = workerTaskMetadata.Chunk;
            //long lineNumber = splitMetaData.StartPosition;
            long bytesProcessed = 0;
            long totalSize = chunk.Length * sizeof(Char);
            string line;
            result = new List<KeyValuePair<string, string>>();
            using (StringReader reader = new System.IO.StringReader(chunk))
            {
                while (true)
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        if (!IsMapSuspended)
                        {
                            runMapperForLine(workerTaskMetadata.Code, workerTaskMetadata.MapperClassName, line);
                            bytesProcessed += line.Length * sizeof(char)+(Environment.NewLine.Length*sizeof(Char));
                            setTaskStatus(splitMetaData,totalSize, bytesProcessed,workerId);
                        }
                        else
                        {
                            //clear the results and wait for next map
                            result = new List<KeyValuePair<string, string>>();
                            IsMapSuspended = false;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                //Console.WriteLine("total sequences" + lineNumber);
                ////send complete status

                return createTaskResultBoject(splitMetaData.SplitId);
            }
        }

        private void setTaskStatus(FileSplitMetadata splitMetaData,long totalSize, long bytesProcessed,int workerId)
        {
            double percentage = 100 * (bytesProcessed / (double)totalSize);
            Status statusToSet = new Status();
            int oldfactor = (int)currentStatus.PercentageCompleted / 10;
            int newfactor = (int)percentage / 10;

            statusToSet.PercentageCompleted = percentage;
            statusToSet.SplitId = splitMetaData.SplitId;
            statusToSet.NodeId = workerId;
            CurrentStatus = statusToSet;
            lock (StatusList)
            {
                if (newfactor > oldfactor)//send for each 10% percentage
                {
                    StatusList.Add(statusToSet);

                    if (StatusList.Count == 1)
                        Monitor.Pulse(StatusList);          
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
