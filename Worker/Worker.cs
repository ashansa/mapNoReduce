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
        public Worker()
        {

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
            Worker worker = new Worker();
            RemotingServices.Marshal(worker, "Worker",
              typeof(Worker));

            Console.WriteLine("starting tasks");
            FileSplitMetadata splitMetadata = new FileSplitMetadata();
            splitMetadata.SplitId = 1;
            splitMetadata.StartPosition =4;
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
        }

        public void receiveStatus(Status status)
        {
        }

        #endregion
    }
}
