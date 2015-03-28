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
    class Worker : MarshalByRefObject, IWorkerTracker
    {
        public static string JOBTRACKER_URL;
        public static string CLIENT_URL;
        WorkerTask workerTask = new WorkerTask();
        int workerId;
        List<String> existingWorkerList = new List<string>();

        public int WorkerId
        {
            get { return workerId; }
            set { workerId = value; }
        }

        public Worker(int id)
        {
            this.workerId = id;
        }

        public List<String> ExistingWorkerList
        {
            get { return existingWorkerList; }
            set { existingWorkerList = value; }
        }

        /// <summary>
        /// Application entry point Main
        /// </summary>
        /// <param name="args">No required arguments</param>

        public static void Main()
        {
            /*Workers are listening*/
            TcpChannel channel = new TcpChannel(10001);
            ChannelServices.RegisterChannel(channel, true);
            Worker worker = new Worker(100);
            RemotingServices.Marshal(worker, "Worker",
              typeof(Worker));

            Console.WriteLine("starting tasks");
            FileSplitMetadata splitMetadata = new FileSplitMetadata(1, 4, 8, "clientURL");
            splitMetadata.SplitId = 1;
            splitMetadata.StartPosition = 4;
            splitMetadata.EndPosition = 8;

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


            //TODO:either make job tracker or task tracker tasks depending on status
            worker.startWorkerTasks();//start threads for Worker task
            //TODO: start tasks for jobtracker

            Console.ReadLine();
        }

        public void startWorkerTasks()
        {
            workerTask.startWorkerThreads();
        }

        public void stopWorkerTasks()
        {
            workerTask.stopWorkerThreads();
        }

        #region Worker
        #region IWorker implementation

        public void receiveTask(FileSplitMetadata splitMetadata)//job tracker will invoke this
        {
            CLIENT_URL = splitMetadata.ClientUrl;
            JOBTRACKER_URL = splitMetadata.JobTrackerUrl;
            workerTask.addSplitToSplitList(splitMetadata);

            //we don't block the job tracker as we execute task seperately     
        }


        public void checkHeartbeat()//job tracker will invoke this
        {

        }
        public bool suspendTask(int splitId)//job tracker will invoke this after certain slowness
        {
            return workerTask.suspendOrRemoveMapTask(splitId);
        }

        #endregion
        #endregion


        #region JobTracker
        public void receiveJobRequest(ClientMetadata clientMetadata)
        {
            //now he is a Job Tracker. Implement all job tracker functions upon this
            //Start channel with other workers as Job tracker


            //stop runing worker(WorkerTask) threads
            workerTask.stopWorkerThreads();

            //start jobtracker threads
        }

        public void receiveStatus(Status status)
        {
        }

        #endregion

        #region specific
        public override object InitializeLifetimeService()
        {
            //return base.InitializeLifetimeService();
            return null;
        }
        #endregion

        #region services exposed to puppet
        //will be called by puppet master

        public Boolean initWorker(WorkerMetadata workerMetadata)
        {
            WorkerCommunicator communicator = new WorkerCommunicator();
            List<String> existingWorkerList = new List<string>();
            if (workerMetadata.EntryURL == null || workerMetadata.EntryURL == String.Empty)//this is the first worker
            {
                existingWorkerList.Add(workerMetadata.ServiceURL);
            }
            else//connect to entryWorker and get urlList
            {
                existingWorkerList = communicator.getExistingWorkerURLList(workerMetadata.EntryURL);
                //notify all others about entry
                communicator.notifyExistingWorkers(workerMetadata.ServiceURL, existingWorkerList);
                existingWorkerList.Add(workerMetadata.ServiceURL);
            }

            //TODO: start worker
            //startWorker();
            return false;
        }

        public void addNewWorker(String newWorkerURL)
        {
            existingWorkerList.Add(newWorkerURL);
        }

        public List<String> getExistingWorkers()
        {
            return existingWorkerList;
        }
        #endregion
    }
}
