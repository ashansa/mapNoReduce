using Server.worker;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;

namespace PADIMapNoReduce
{
    /// <summary>
    /// Program class is a container for application entry point Main
    /// </summary>
    class Worker : MarshalByRefObject, IWorker,IJobTracker
    {
        Boolean isJobTracker;
        Boolean isMapSuspended;
        List<FileSplitMetadata> splitMetadataList = new List<FileSplitMetadata>();
        List<TaskResult> taskResultList = new List<TaskResult>();
        Thread splitProcessor;
        Thread resultSender;
        MapTask mapTask = new MapTask();

        public Worker()
        {
         
        }

        public Boolean IsJobTracker
        {
            get { return isJobTracker; }
            set { isJobTracker = value; }
        }
        /// <summary>
        /// Application entry point Main
        /// </summary>
        /// <param name="args">No required arguments</param>
        static void Main(string[] args)
        {
            /*Workers are listening*/
            TcpChannel channel = new TcpChannel(10001);
            ChannelServices.RegisterChannel(channel, true);
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Worker),
                "Worker",
                WellKnownObjectMode.Singleton);


            FileSplitMetadata splitMetadata = new FileSplitMetadata();
            splitMetadata.SplitId = 1;
            splitMetadata.StartPosition = 10;
            splitMetadata.EndPosition = 20;
            Worker worker = new Worker();
            worker.receiveTask(splitMetadata);
            worker.receiveTask(splitMetadata);
            worker.receiveTask(splitMetadata);
            worker.receiveTask(splitMetadata);
            worker.receiveTask(splitMetadata);
            Thread.Sleep(5000);
            worker.receiveTask(splitMetadata);
            worker.receiveTask(splitMetadata);
            worker.receiveTask(splitMetadata);
            worker.receiveTask(splitMetadata);
            worker.receiveTask(splitMetadata);

            worker.splitProcessor = new Thread(new ThreadStart(worker.processSplits));
            worker.splitProcessor.Start();

            worker.resultSender = new Thread(new ThreadStart(worker.sendResults));
            worker.resultSender.Start();

            Console.ReadLine();
        }

        #region Worker
        private void sendResults()
        {
            WorkerCommunicator workerTask = new WorkerCommunicator();
             TaskResult taskResult;
            while (true)
            {
                lock (taskResultList)
                {
                    if (taskResultList.Count == 0)
                    {
                        Monitor.Wait(taskResultList);
                        continue;
                    }

                   
                 taskResult = taskResultList[0];
                 taskResultList.RemoveAt(0);
                }
                workerTask.sendResultsToClient(taskResult);
            }
        }
    
        private void processSplits()
        {
            FileSplitMetadata fileSplitMetadata = null;
            while (true)
            {
                lock (splitMetadataList)
                {
                    if (splitMetadataList.Count > 0)
                    {
                        fileSplitMetadata = splitMetadataList[0];
                        splitMetadataList.RemoveAt(0);
                    }
                    else
                    {
                        Monitor.Wait(splitMetadataList);
                        continue;
                    }
                }
                WorkerCommunicator workerTask = new WorkerCommunicator();
                WorkerTaskMetadata workerTaskMetadata= workerTask.getTaskFromClient(fileSplitMetadata);

                mapTask.processMapTask(workerTaskMetadata,fileSplitMetadata);
               
                
                TaskResult taskResult=new TaskResult();
                taskResult.Result=mapTask.result;
                taskResult.SplitId=fileSplitMetadata.SplitId;
                //once done for each line

                addTaskToTaskList(taskResult);
            }
        }

        private void addTaskToTaskList(TaskResult taskResult)
        {
            lock (taskResultList)
            {
                taskResultList.Add(taskResult);
                Monitor.Pulse(taskResultList);
            }
        }

        #region IWorker implementation
        /* will be called by JobTracker*/
        /*upon call from JT, get split and file from client and start executing it*/
        public void receiveTask(FileSplitMetadata splitMetadata)
        {
            lock (splitMetadataList)
            {
                splitMetadataList.Add(splitMetadata);
                Monitor.Pulse(splitMetadataList);
            }
            //we don't block the job tracker as we execute task seperately     
        }


        public void checkHeartbeat()
        {

        }
 
        #endregion
        #endregion


        #region JobTracker
        public void receiveJobRequest(ClientMetadata clientMetadata)
        {
          //now he is a Job Tracker. Implement all job tracker functions upon this
        //Start channel with other workers as Job tracker
        }

        public void receiveHeartbeat()
        {
   
        }

        public void receiveStatus(String status)
        {
        }

        #endregion
    }
}
