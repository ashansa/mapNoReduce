﻿using PADIMapNoReduce.Entities;
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
        public static string JOBTRACKER_URL = null;
        public static string BKP_JOBTRACKER_URL;
        public static string CLIENT_URL;
        IClient client;
        //TODO: change this to get from puppet
        WorkerTask workerTask;
        TrackerTask trackerTask;
        int workerId;
        Dictionary<Int32, WorkerDetails> existingWorkerMap = new Dictionary<Int32, WorkerDetails>();
        public static string serviceUrl;
        public static bool isJobTracker;


        public Worker(int id)
        {
            this.workerId = id;
        }

        public int WorkerId
        {
            get { return workerId; }
            set { workerId = value; }
        }


        public Dictionary<Int32, WorkerDetails> ExistingWorkerList
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
            props["port"] = Convert.ToInt32(portNamePair[0]);
            props["name"] = "worker" + workerId;
            // props["timeout"] = 100000; // in milliseconds
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this, portNamePair[1], typeof(Worker));

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
            if (!WorkerTask.IS_WORKER_FREEZED)
            {
                Common.Logger().LogInfo("Task Received ++++++++++++++++ from " + splitMetadata.JobTrackerUrl, string.Empty, string.Empty);
                CLIENT_URL = splitMetadata.ClientUrl;
                JOBTRACKER_URL = splitMetadata.JobTrackerUrl;
                workerTask.addSplitToSplitList(splitMetadata);
                //we don't block the job tracker as we execute task seperately 
            }
            else
            {
                // workerTask.stopTimer();
                throw new RemoteComException();
            }
        }

        public void SetBackupJobTrackerUrl(string Url)
        {
            BKP_JOBTRACKER_URL = Url;
        }

        public void TrackerStabilized()
        {
            workerTask.TrackerStabilized();
        }

        public void TrackerRevert()
        {
            workerTask.TrackerRevert();
        }


        public bool checkHeartbeat(string url)//job tracker will invoke this
        {
            if (!WorkerTask.IS_WORKER_FREEZED)
            {
                if (url.Equals(JOBTRACKER_URL))
                {
                    workerTask.ProcessHeartBeat();
                    return true;
                }
            }
            else
            {
                Console.WriteLine("I m freeze thrown exec");
                throw new RemoteComException();
            }
            return false;
        }

        public bool suspendTask(int splitId)//job tracker will invoke this after certain slowness
        {
            Console.WriteLine("one task suspended " + workerId);
            return workerTask.suspendOrRemoveMapTask(splitId);
        }

        public void removeFailedNode(int key)
        {
            if (existingWorkerMap.ContainsKey(key))
            {
                existingWorkerMap.Remove(key);
            }
            if (trackerTask != null)
            {
                trackerTask.removeWorker(key);
            }
        }

        public void addUnfreezedNode(int nodeId, string nodeURL)
        {
            addNewWorker(nodeId, nodeURL);
        }

        public void jobCompleted()
        {
            // workerTask.stopWorkerThreads();
            if (existingWorkerMap.Count > 1)
                workerTask.stopTimer();
        }
        public void forceTrackerChange()
        {
            Common.Logger().LogInfo("TRACKER CHANGE FORCED RECEIVED", string.Empty, string.Empty);
            workerTask.InitiateTrackerTransition(true);
        }

        public void receiveNewJob()
        {
            workerTask = new WorkerTask(WorkerId);//starting a fresh worker for next job
            startWorkerTasks();
            if (existingWorkerMap.Count > 1)//no timers in case of one node
                workerTask.startTimer();
        }
        public Dictionary<StatusType, List<int>> getRecoveryStatus()
        {
            return workerTask.getStatusOnFreezed();
        }

        public void updateRecoveredWorker(Dictionary<StatusType, List<int>> updatedStatus)
        {
            Console.WriteLine("fail node recovered its status");
            workerTask.TrackerRevert();
            workerTask.updateDataStructures(updatedStatus);
        }
        #endregion
        #endregion


        #region JobTracker
        public void receiveJobRequest(JobMetadata jobMetadata)
        {
            Common.Logger().LogInfo("Job Request received", string.Empty, string.Empty);
            //now he is a Job Tracker. Implement all job tracker functions upon this
            //Start channel with other workers as Job tracker

            JOBTRACKER_URL = serviceUrl;//I set my own as job tracker url
            isJobTracker = true;

            CLIENT_URL = jobMetadata.ClientUrl;

            Console.WriteLine("Job request received for " + JOBTRACKER_URL, string.Empty, string.Empty);

            //start jobtracker threads
            //TrackerDetails trackerDetails = new TrackerDetails(CLIENT_URL, existingWorkerMap);
            trackerTask = new TrackerTask(CLIENT_URL, existingWorkerMap, workerId);
            client = (IClient)Activator.GetObject(typeof(IClient), CLIENT_URL);

            trackerTask.notifyWorkersForJobStart();
            if (existingWorkerMap.Count > 1)
            {
                trackerTask.startHeartBeat();//should work even with 1 
            }

            trackerTask.splitJob(jobMetadata);

        }

        public void receiveStatus(Status status)
        {
            /*Console.WriteLine("result received " + status.PercentageCompleted + "%");
            Common.Logger().LogInfo("Results received " + status.PercentageCompleted + "%", string.Empty, string.Empty);*/
            trackerTask.updateStatus(status);
        }


        public void taskCompleted(int nodeId, int splitId, string url)
        {
            try
            {
                trackerTask.resultSentToClient(nodeId, splitId, url);
                if (trackerTask.isJobCompleted())
                {
                    client.receiveJobCompletedNotification();
                    Common.Logger().LogInfo("Job completed Hit", string.Empty, string.Empty);
                    Console.WriteLine("Job completed Hit", string.Empty, string.Empty);
                    trackerTask.stopHeatBeat();
                    trackerTask.notifyJobCompleteToWorker();
                    //GC.Collect();
                    isJobTracker = false;//he is no longer the job tracker
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR sending completed****************8");
                Common.Logger().LogError(ex.Message, ex.StackTrace, string.Empty);
            }
        }
        public void notifyWorkerRecovery(int workerID, string nodeURL, List<int> processingSplits, List<int> alreadySentSplits)
        {
            WorkerCommunicator communicator = new WorkerCommunicator();
            Dictionary<StatusType, List<int>> statusOfWorker = communicator.getRecoveredStatus(nodeURL);
            Dictionary<StatusType, List<int>> updatedStatus = trackerTask.getStatusForWorker(statusOfWorker, workerId, nodeURL);
            communicator.updateRecoveredWorker(updatedStatus, nodeURL);
        }

        public void readyForNewTask(int nodeId)
        {
            trackerTask.readyForNewTask(nodeId);
        }

        public void ChangeTracker(int workerID, String workerURL, List<int> processingSplits, List<int> alreadySentSplits)
        {
            trackerTask.ChangeTracker(workerID, workerURL, processingSplits, alreadySentSplits);
        }

        public void SetCopyOfTasks(Dictionary<int, Task> tasks, string clientUrl)
        {
            trackerTask = new TrackerTask(clientUrl, existingWorkerMap, workerId);
            client = (IClient)Activator.GetObject(typeof(IClient), clientUrl);
            trackerTask.SetCopyOfTasks(tasks);
            Console.WriteLine("Tasklist copied to backup with id***********************" + workerId);
            Common.Logger().LogInfo("Tasklist copied to backup with id***********************" + workerId, string.Empty, string.Empty);

        }

        public Dictionary<StatusType, List<int>> receiveFreezedWorkerStatus(Dictionary<StatusType, List<int>> freezedWorkerStatus, int nodeId, String nodeURL)
        {
            Dictionary<StatusType, List<int>> statusToWorker = trackerTask.getStatusForWorker(freezedWorkerStatus, nodeId, nodeURL);
            return statusToWorker;

        }
        #endregion

        #region services exposed to puppet
        //will be called by puppet master

        public Boolean initWorker(WorkerMetadata workerMetadata)
        {
            serviceUrl = workerMetadata.ServiceURL;
            startWorker();
            WorkerCommunicator communicator = new WorkerCommunicator();
            this.WorkerId = workerMetadata.WorkerId;
            // workerTask = new WorkerTask(WorkerId);

            if (workerMetadata.EntryURL == null || workerMetadata.EntryURL == String.Empty)//this is the first worker
            {
                addNewWorker(workerMetadata.WorkerId, workerMetadata.ServiceURL);
            }
            else//connect to entryWorker and get urlList
            {
                existingWorkerMap = communicator.getExistingWorkerURLList(workerMetadata.EntryURL);
                //notify all others about entry
                communicator.notifyExistingWorkers(workerId, workerMetadata.ServiceURL, existingWorkerMap);
                addNewWorker(workerMetadata.WorkerId, workerMetadata.ServiceURL); //add self
            }

            // startWorkerTasks();
            return true;
        }

        public void displayStatus()
        {
            if (isJobTracker && trackerTask != null)
            {
                trackerTask.printStatus(workerId);
            }
            Status status = workerTask.getMapTask().CurrentStatus;
            if (workerTask != null && workerTask.getMapTask() != null && status != null)
            {
                Console.WriteLine("###########Worker Status #########");
                Console.WriteLine("worker id is " + workerId + " Split id is " + status.SplitId + " status is" + status.PercentageCompleted + "%");
                Console.WriteLine("########################");
            }
        }


        public void slowWorker(int seconds)
        {
            workerTask.slowWorkerThreads(seconds);
        }

        public void freezeWorker()
        {
            WorkerTask.IS_WORKER_FREEZED = true;
            workerTask.stopTimer();
            Console.WriteLine("going to freeze worker " + workerId);
            Common.Logger().LogInfo("going to freeze " + workerId, string.Empty, string.Empty);
        }

        public void unfreezeWorker()
        {
            Console.WriteLine("going to unfreeze " + workerId);
            Common.Logger().LogInfo("going to unfreeze " + workerId, string.Empty, string.Empty);
            WorkerTask.IS_WORKER_FREEZED = false;
            workerTask.startTimer();//to restart heartbeat

            /*  WorkerCommunicator communicator = new WorkerCommunicator();
              Dictionary<StatusType, List<Int32>> freezedWorkerStatus = workerTask.getStatusOnFreezed();
              Dictionary<StatusType, List<Int32>> updatedStatus = communicator.notifyTrackerOnUnfreeze(freezedWorkerStatus, workerId, serviceUrl);
              workerTask.updateDataStructures(updatedStatus);*/
            /*this should happen after getting all major details*/
            workerTask.checkWorkerFreezed();
        }

        public void freezeTracker()
        {
            if (trackerTask != null && isJobTracker)
            {
                trackerTask.IsTrackerFreezed = true;
                trackerTask.stopHeatBeat();//to block communication
                Console.WriteLine("going to freeze tracker" + workerId);
                Common.Logger().LogInfo("going to freeze tracker" + workerId, string.Empty, string.Empty);
            }

        }

        public void unfreezeTracker()
        {
            if (trackerTask != null && isJobTracker)
            {
                Console.WriteLine("going to unfreeze tracker" + workerId);
                trackerTask.startHeartBeat();//restart communication, but eventually he will detect that he is no longer the jt and then will giveup
                trackerTask.IsTrackerFreezed = false;
                Common.Logger().LogInfo("Going to unfreeze tracker. But current tracker URL is " + Worker.JOBTRACKER_URL, string.Empty, string.Empty);
            }
            }

        public void addNewWorker(int nodeId, String newWorkerURL)
        {
            WorkerDetails worker = new WorkerDetails();
            worker.Nodeid = nodeId;
            worker.Nodeurl = newWorkerURL;
            worker.State = WorkerState.IDLE;
            worker.ProcessedSplits = new List<int>();
            existingWorkerMap.Add(nodeId, worker);
        }

        public Dictionary<Int32, WorkerDetails> getExistingWorkers()
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
