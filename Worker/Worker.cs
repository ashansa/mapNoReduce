using PADIMapNoReduce.Entities;
using Server.tracker;
using Server.worker;
using System;
using System.Collections;
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
   public class Worker : MarshalByRefObject, IWorkerTracker
    {
        public static string JOBTRACKER_URL;
        public static string CLIENT_URL;

        IClient client;
        //TODO: change this to get from puppet
        WorkerTask workerTask;
        TrackerTask trackerTask;
        int workerId;
        Dictionary<Int32,String> existingWorkerMap = new Dictionary<Int32,string>();
        String serviceUrl;
        bool isJobTracker;

        public bool IsJobTracker
        {
            get { return isJobTracker; }
            set { isJobTracker = value; }
        }

        public int WorkerId
        {
            get { return workerId; }
            set { workerId = value; }
        }

        public Worker(int id)
        {
            this.workerId = id;
        }

        public Dictionary<Int32,String> ExistingWorkerList
        {
            get { return existingWorkerMap; }
            set { existingWorkerMap = value; }
        }

        /// <summary>
        /// Application entry point Main
        /// </summary>
        /// <param name="args">No required arguments</param>

        public void startWorker()
        {
            /*Workers are listening*/
            String[] portNamePair = serviceUrl.Split(Constants.COLON_STR)[2].Split(Constants.SEP_PIPE);
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["port"] =Convert.ToInt32( portNamePair[0]);
            props["name"] = "worker";
           // props["timeout"] = 100000; // in milliseconds
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            RemotingServices.Marshal(this, portNamePair[1],typeof(Worker));

           /* Console.WriteLine("starting tasks");
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
*/
            
            //TODO: start tasks for jobtracker
            Common.Logger().LogInfo("Worker Started", string.Empty, string.Empty);
            Console.WriteLine("worker started");
            
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
            Console.WriteLine("worker recieved the job details");
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
        public void receiveJobRequest(JobMetadata jobMetadata)
        {
            Common.Logger().LogInfo("Job Request received", string.Empty, string.Empty);
            //now he is a Job Tracker. Implement all job tracker functions upon this
            //Start channel with other workers as Job tracker

            JOBTRACKER_URL = this.serviceUrl;//I set my own as job tracker url
            isJobTracker = true;
            //stop runing worker(WorkerTask) threads
            workerTask.stopWorkerThreads();

            CLIENT_URL = jobMetadata.ClientUrl;

            client = (IClient)Activator.GetObject(typeof(IClient), CLIENT_URL);

            //start jobtracker threads
            //remove jobtrackers url from workermap
            Dictionary<int, String> workerTasksWithoutTracker = existingWorkerMap;
            workerTasksWithoutTracker.Remove(WorkerId);
            TrackerDetails trackerDetails = new TrackerDetails(CLIENT_URL, workerTasksWithoutTracker);
            trackerTask = new TrackerTask(trackerDetails);

            trackerTask.splitJob(jobMetadata);
                
        }

        public void receiveStatus(Status status)
        {
            Console.WriteLine("result received " + status.PercentageCompleted + "%");
            Common.Logger().LogInfo("Results received " + status.PercentageCompleted + "%", string.Empty, string.Empty);
            trackerTask.updateStatus(status);
        }

        /*will call by worker when job completed   */
        public void taskCompleted(int nodeId,int splitId)
        {
            if (trackerTask.TrackerDetails.FileSplitData.Count == trackerTask.TrackerDetails.resultSentToClientSplits.Count)
            {
                client.receiveJobCompletedNotification();
            }
        }

        /*will call by worker when result has sent to client*/
        public void resultSentToClient(int nodeId, int splitId)
        {
            //add split to completed
            trackerTask.resultSentToClient(nodeId, splitId);

            //stop tracker threads

            //FIXME  !!!!!!!!!!!!! AM I CORRECT?
            //resume worker threads, should happen when all results are sent
            if (trackerTask.TrackerDetails.FileSplitData.Count == trackerTask.TrackerDetails.resultSentToClientSplits.Count)
            {
                workerTask = new WorkerTask(nodeId);
                workerTask.startWorkerThreads();
            }
        }

        #endregion

        #region services exposed to puppet
        //will be called by puppet master

        public Boolean initWorker(WorkerMetadata workerMetadata)
        {
            this.serviceUrl = workerMetadata.ServiceURL;
            startWorker();
            WorkerCommunicator communicator = new WorkerCommunicator();
            this.WorkerId = workerMetadata.WorkerId;
            workerTask = new WorkerTask(WorkerId);
            if (workerMetadata.EntryURL == null || workerMetadata.EntryURL == String.Empty)//this is the first worker
            {
                existingWorkerMap.Add(workerMetadata.WorkerId,workerMetadata.ServiceURL);
            }
            else//connect to entryWorker and get urlList
            {
                existingWorkerMap = communicator.getExistingWorkerURLList(workerMetadata.EntryURL);
                //notify all others about entry
                communicator.notifyExistingWorkers(workerId,workerMetadata.ServiceURL, existingWorkerMap);
                existingWorkerMap.Add(workerMetadata.WorkerId,workerMetadata.ServiceURL);//add self
            }

            startWorkerTasks();
            return true;
        }

        public void displayStatus()
        {
            if (isJobTracker)
            {
                trackerTask.printStatus(workerId);
            }
            else
            {
                if (workerTask != null && workerTask.getMapTask() != null && workerTask.getMapTask().Status != null)
                {
                    Console.WriteLine("worker id is " + workerId + " status is" + workerTask.getMapTask().Status.PercentageCompleted + "%");
                }
            }
        }


        public void addNewWorker(int nodeId,String newWorkerURL)
        {
            existingWorkerMap.Add(nodeId, newWorkerURL);
        }

        public Dictionary<Int32,String> getExistingWorkers()
        {
            return existingWorkerMap;
        }
        #endregion

        #region specific
        public override object InitializeLifetimeService()
        {
            return null;
        }
        #endregion

    }


}
