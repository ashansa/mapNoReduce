using PADIMapNoReduce;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server.worker
{
    public class WorkerTask
    {
        int workerId;
        public static Boolean IS_WORKER_FREEZED = false;
        List<FileSplitMetadata> splitMetadataList = new List<FileSplitMetadata>();
        List<TaskResult> taskResultList = new List<TaskResult>();
        List<int> resultSentSplits = new List<int>();
        Thread splitProcessor;
        Thread resultSender;
        Thread statusUpdateNotificationThread;
        MapTask mapTask = new MapTask();
        WorkerCommunicator communicator = new WorkerCommunicator();
        object freezeLock = new object();
        bool hasHeartBeatInitiated = false;
        bool hasTrackerChanged = false;
        DateTime latestPingTime;

        public DateTime LatestPingTime
        {
            get { return latestPingTime; }
            set { latestPingTime = value; }
        }
        int taskTrackerTimeoutSeconds =Convert.ToInt32(ConfigurationManager.AppSettings[Constants.TASK_TRACKER_TIMEOUT_SECONDS].ToString());
        Timer heartBeatTimer = null;
        int count = 0;

        public WorkerTask(int workerId)
        {
            this.workerId = workerId;
        }

        public int WorkerId
        {
            get { return workerId; }
            set { workerId = value; }
        }

        public MapTask getMapTask()//this is the currently runing map task
        {
            return mapTask;
        }

        public void startTimer()
        {
            int heartbeatInterval = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.TASK_TRACKER_HEARTBEAT_INTERVAL].ToString());
            heartBeatTimer = new Timer(CheckTaskTrackerFailure, null, 0, heartbeatInterval);
            Console.WriteLine("Worker Timer Started");
        }

        public void stopTimer()
        {
            if (heartBeatTimer != null)
            {
                heartBeatTimer.Dispose();
                Console.WriteLine("Worker timer disposed");
            }

        }

        public void startWorkerThreads()
        {
            try
            {
                splitProcessor = new Thread(new ThreadStart(processSplits));
                splitProcessor.Start();

                resultSender = new Thread(new ThreadStart(sendResults));
                resultSender.Start();

                statusUpdateNotificationThread = new Thread(new ThreadStart(sendStatusUpdates));
                statusUpdateNotificationThread.Start();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        public void stopWorkerThreads()
        {
            if (splitProcessor != null)
            {
                splitProcessor.Abort();
                splitProcessor = null;
            }

            if (resultSender != null)
            {
                resultSender.Abort();
                resultSender = null;
            }

            if (statusUpdateNotificationThread != null)
            {
                statusUpdateNotificationThread.Abort();
                statusUpdateNotificationThread = null;
            }

        }

        /*Depricated but seems ok*/
        public void slowWorkerThreads(int seconds)
        {
            Console.WriteLine("worker is going to slow");
            splitProcessor.Suspend();
            Thread.Sleep(seconds * 1000);
            splitProcessor.Resume();

        }

        private void sendResults()
        {
            TaskResult taskResult;
            while (true)
            {
                checkWorkerFreezed();
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
            
                resultSentSplits.Add(taskResult.SplitId);
                communicator.sendResultsToClient(taskResult, this);
                communicator.notifyResultsSentToClientEvent(workerId, taskResult, this);
                Common.Logger().LogInfo("result sent by " + workerId + " Split ID = " + taskResult.SplitId + "trackerURL" + Worker.JOBTRACKER_URL, string.Empty, string.Empty);
            }
        }

        private void processSplits()
        {
            FileSplitMetadata fileSplitMetadata = null;
            while (true)
            {
                checkWorkerFreezed();
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
                WorkerTaskMetadata workerTaskMetadata = communicator.getTaskFromClient(fileSplitMetadata);
                mapTask.SplitId = fileSplitMetadata.SplitId;
                mapTask.Hasthresholdreached = false;
                TaskResult taskResult = mapTask.processMapTask(workerTaskMetadata, fileSplitMetadata, workerId);
                if (taskResult != null)
                    addTaskToTaskResults(taskResult);
            }
        }
        private void sendStatusUpdates()
        {
            Status status;
            while (true)
            {
                checkWorkerFreezed();
                lock (mapTask.StatusList)
                {
                    if (mapTask.StatusList.Count > 0)
                    {
                        status = mapTask.StatusList[0];
                        mapTask.StatusList.RemoveAt(0);
                    }
                    else
                    {
                        Monitor.Wait(mapTask.StatusList);
                        continue;
                    }
                }
                communicator.sendStatusUpdatesToTracker(status, this);
            }
        }


        public void addTaskToTaskResults(TaskResult taskResult)
        {
            lock (taskResultList)
            {
                taskResultList.Add(taskResult);

                if (taskResultList.Count == 1)
                    Monitor.Pulse(taskResultList);
            }
        }


        public void addSplitToSplitList(FileSplitMetadata splitMetadata)
        {
            lock (splitMetadataList)
            {
                splitMetadataList.Add(splitMetadata);

                if (splitMetadataList.Count == 1)
                    Monitor.Pulse(splitMetadataList);
            }
        }

        /*suspend the current map or suspend the map which has not yet started but in split list*/
        public bool suspendOrRemoveMapTask(int splitId)
        {
            if (mapTask.SplitId == splitId)
            {
                mapTask.IsMapSuspended = true;
                return true;
            }
            else
            {
                lock (splitMetadataList)
                {
                    if (splitMetadataList.Count > 0)
                    {
                        for (int i = 0; i < splitMetadataList.Count; i++)
                        {
                            if (splitMetadataList[i].SplitId == splitId)
                            {
                                splitMetadataList.RemoveAt(i);
                                return true;
                            }
                        }
                    }
                }

            }
            return false;
        }

        public void ProcessHeartBeat()
        {
            latestPingTime = DateTime.Now;
            if (!hasHeartBeatInitiated)
                hasHeartBeatInitiated = true;

        }

        public void checkWorkerFreezed()
        {

            lock (freezeLock)
            {
                if (IS_WORKER_FREEZED)
                {
                    Monitor.Wait(freezeLock);
                }
                else
                {
                    Monitor.PulseAll(freezeLock);
                }
            }
        }

        private void CheckTaskTrackerFailure(object state)
        {
            if (hasHeartBeatInitiated && !IS_WORKER_FREEZED)//don,t do this if worker has failed
            {
                if (DateTime.Now.Subtract(latestPingTime).Seconds > taskTrackerTimeoutSeconds)
                {
                    Console.WriteLine("Failed Task tracker detected****************************");
                    hasHeartBeatInitiated = false;
                    this.stopTimer();
                    hasTrackerChanged = true;
                    InitiateTrackerTransition(false);
                }
            }
        }

        public void InitiateTrackerTransition(Boolean hasForced)
        {
            communicator.IsTrackerChanging = true;

            if(!hasForced)
            Thread.Sleep(1000);//This allow the working threads to stop detacting jobtracker failure
            List<int> inprogressSplits = new List<int>();
            lock (splitMetadataList)
            {
                if (splitMetadataList.Count > 0)
                {
                    foreach (var item in splitMetadataList)
                    {
                        inprogressSplits.Add(item.SplitId);
                    }
                }
            }
            if (!inprogressSplits.Contains(mapTask.SplitId))
            {
                inprogressSplits.Add(mapTask.SplitId);
            }
            lock (taskResultList)
            {
                foreach (var item in taskResultList)
                {
                    if (!inprogressSplits.Contains(item.SplitId))
                    {
                        inprogressSplits.Add(item.SplitId);
                    }
                }
            }
           // Worker.JOBTRACKER_URL = Worker.BKP_JOBTRACKER_URL;
            communicator.FeedNewTracker(workerId, inprogressSplits, resultSentSplits);
            Common.Logger().LogInfo("Feed Message Sent by " + workerId + "********************", string.Empty, string.Empty);
            // }
        }

        public void TrackerStabilized()
        {
            Worker.JOBTRACKER_URL = Worker.BKP_JOBTRACKER_URL;
            hasTrackerChanged = false;
            communicator.IsTrackerChanging = false;
            communicator.CheckSystemStability();
            getSplitIfIdle();

            latestPingTime = DateTime.Now;
            hasHeartBeatInitiated = true;
            this.startTimer();
            
            Console.WriteLine("Tracker Stabilized " + workerId + " current tracker is " + Worker.JOBTRACKER_URL);
            Console.WriteLine("Backup url  " + Worker.BKP_JOBTRACKER_URL);

        }

        private void getSplitIfIdle()
        {

            if (splitMetadataList.Count == 0)
            {
                Common.Logger().LogInfo("Requesting new task after stabilization", string.Empty, string.Empty);
                communicator.hasThresholdReached(workerId);
            }
            else
                Common.Logger().LogInfo("I have a split to process", string.Empty, string.Empty);
        }




        internal Dictionary<StatusType, List<Int32>> getStatusOnFreezed()
        {
            Dictionary<StatusType, List<Int32>> status = new Dictionary<StatusType, List<Int32>>();

            if (splitMetadataList.Count > 0)
            {
                List<Int32> inProgressSplits = new List<Int32>();
                foreach (FileSplitMetadata meta in splitMetadataList)
                {
                    inProgressSplits.Add(meta.SplitId);
                }
                status.Add(StatusType.NOT_STARTED, inProgressSplits);
            }

            if (taskResultList.Count > 0)
            {
                List<Int32> resultList = new List<Int32>();
                foreach (TaskResult result in taskResultList)
                {
                    resultList.Add(result.SplitId);
                }
                status.Add(StatusType.COMPLETED, resultList);
            }
            return status;
        }


        internal void updateDataStructures(Dictionary<StatusType, List<int>> updatedStatus)
        {
            List<FileSplitMetadata> copySplit = new List<FileSplitMetadata>(splitMetadataList);
            List<TaskResult> copyResults = new List<TaskResult>(taskResultList);
            for (int i = 0; i < updatedStatus.Count; i++)
            {
                KeyValuePair<StatusType, List<int>> entry = updatedStatus.ElementAt(i);
                switch (entry.Key)
                {
                    case StatusType.NOT_STARTED:
                        foreach (FileSplitMetadata meta in copySplit)
                        {
                            lock (splitMetadataList)
                            {
                                Boolean isAvailable = false;
                                foreach (int split in entry.Value)
                                {
                                    if (meta.SplitId == split)
                                    {
                                        isAvailable = true;
                                    }
                                }
                                if (!isAvailable && splitMetadataList.Contains(meta)) splitMetadataList.Remove(meta);
                            }
                        }
                        break;

                    case StatusType.COMPLETED:
                        foreach (int split in entry.Value)
                        {
                            lock (taskResultList)
                            {
                                foreach (TaskResult result in copyResults)
                                {
                                    Boolean isAvailable = false;
                                    if (result.SplitId == split)
                                    {
                                        isAvailable = true;
                                    }
                                    if (!isAvailable && taskResultList.Contains(result)) taskResultList.Remove(result);

                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
    }

    

