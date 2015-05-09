using PADIMapNoReduce;
using PADIMapNoReduce.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;

namespace Server.worker
{
    public class WorkerCommunicator
    {
        private IClient clientProxy = null;
        private IWorkerTracker trackerProxy = null;
        private bool isTrackerChanging = false;
        object trackerLock = new object();

        public bool IsTrackerChanging
        {
            get { return isTrackerChanging; }
            set { isTrackerChanging = value; }
        }



        public WorkerTaskMetadata getTaskFromClient(FileSplitMetadata splitMetadata)
        {
            if (clientProxy == null)
            {
                clientProxy = (IClient)Activator.GetObject(
                    typeof(IClient),
                    Worker.CLIENT_URL);
            }
            WorkerTaskMetadata workerMetadata = clientProxy.receiveTaskRequest(splitMetadata);
            return workerMetadata;
        }


        public void sendResultsToClient(TaskResult taskResult, WorkerTask workerTask)
        {
            try
            {
                //workerTask.checkWorkerFreezed();
                //CheckSystemStability();
                if (clientProxy == null)
                {
                    clientProxy = (IClient)Activator.GetObject(
                        typeof(IClient),
                        Worker.CLIENT_URL);
                }

                Boolean status = clientProxy.receiveCompletedTask(taskResult);
            }
            catch (Exception ex)
            {
                workerTask.addTaskToTaskResults(taskResult);
            }
            // Console.WriteLine("receive status at client is" + status);
        }

        public void sendStatusUpdatesToTracker(Status status, WorkerTask workerTask)
        {
            try
            {
                workerTask.checkWorkerFreezed();
                CheckSystemStability();
                if (trackerProxy == null)
                {
                    trackerProxy = (IWorkerTracker)Activator.GetObject(
                                typeof(IWorkerTracker),
                                Worker.JOBTRACKER_URL);
                }
                trackerProxy.receiveStatus(status);
            }
            catch (Exception ex)
            {
                Common.Logger().LogError("unable to send status updates by " + status.NodeId + " for percentage " + status.PercentageCompleted, string.Empty, string.Empty);
            }
        }

        public void FeedNewTracker(int workerID, List<int> processingSplits, List<int> alreadySentSplits)
        {
            trackerProxy = (IWorkerTracker)Activator.GetObject(
                               typeof(IWorkerTracker),
                               Worker.BKP_JOBTRACKER_URL);
            trackerProxy.ChangeTracker(workerID, processingSplits, alreadySentSplits);
        }

        public void TrackerStabilized(Dictionary<Int32, WorkerDetails> existingWorkerList)
        {
            foreach (KeyValuePair<Int32, WorkerDetails> entry in existingWorkerList)
            {
                IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entry.Value.Nodeurl);
                worker.TrackerStabilized();
            }
        }

        public void SendTaskCopyToBackupTracker(string url, Dictionary<int, Task> taskList, string clientUrl)
        {
            IWorkerTracker bkpTracker = (IWorkerTracker)Activator.GetObject(
                               typeof(IWorkerTracker),
                               url);
            bkpTracker.SetCopyOfTasks(taskList, clientUrl);
        }

        public void notifyBackupJobtrackerUrl(string url, Dictionary<Int32, WorkerDetails> existingWorkerList)
        {
            foreach (KeyValuePair<Int32, WorkerDetails> entry in existingWorkerList)
            {
                IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entry.Value.Nodeurl);
                worker.SetBackupJobTrackerUrl(url);
            }
        }

        internal Dictionary<Int32, WorkerDetails> getExistingWorkerURLList(string entryURL)
        {
            IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entryURL);
            return worker.getExistingWorkers();
        }

        internal void notifyExistingWorkers(int workerId, String newWorkerURL, Dictionary<Int32, WorkerDetails> existingWorkerList)
        {
            foreach (KeyValuePair<Int32, WorkerDetails> entry in existingWorkerList)
            {
                IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entry.Value.Nodeurl);
                worker.addNewWorker(workerId, newWorkerURL);
            }
        }


        internal void notifyTaskCompletedEvent(int workerId, int splitId)
        {
            try
            {
                CheckSystemStability();
                Common.Logger().LogInfo("Notifying task completed event for split " + splitId, string.Empty, string.Empty);
                if (trackerProxy == null)
                {
                    trackerProxy = (IWorkerTracker)Activator.GetObject(
                                typeof(IWorkerTracker),
                                Worker.JOBTRACKER_URL);
                }
                trackerProxy.taskCompleted(workerId, splitId);
            }
            catch (Exception ex)
            {
                Common.Logger().LogInfo("exception thrown while sending copleted event ", string.Empty, string.Empty);
            }
        }

        /*important event so if not send add to list back*/
        internal void notifyResultsSentToClientEvent(int workerId, TaskResult taskResult, WorkerTask workerTask)
        {
            try
            {
                workerTask.checkWorkerFreezed();
                CheckSystemStability();
                if (trackerProxy == null)
                {
                    trackerProxy = (IWorkerTracker)Activator.GetObject(
                               typeof(IWorkerTracker),
                               Worker.JOBTRACKER_URL);
                }
                Common.Logger().LogInfo("Sent to taskCompleted by Worker ID = " + workerId + " Split Id= " + taskResult.SplitId, string.Empty, string.Empty);
                trackerProxy.taskCompleted(workerId, taskResult.SplitId);

            }
            catch (Exception ex)
            {
                workerTask.addTaskToTaskResults(taskResult);
                Common.Logger().LogInfo("exception thrown while sending copleted event ", string.Empty, string.Empty);
                Common.Logger().LogInfo(ex.Message, ex.StackTrace, string.Empty);
            }
        }


        internal void hasThresholdReached(int nodeId)
        {
            if (IsTrackerChanging)
            {
                return;
            }

            if (!WorkerTask.IS_WORKER_FREEZED)
            {
                if (trackerProxy == null)
                {
                    trackerProxy = (IWorkerTracker)Activator.GetObject(
                                typeof(IWorkerTracker),
                                Worker.JOBTRACKER_URL);
                }
                Common.Logger().LogInfo("requesting task in threashold reached",string.Empty,string.Empty);

                trackerProxy.readyForNewTask(nodeId);
            }
        }

        internal Dictionary<StatusType, List<int>> notifyTrackerOnUnfreeze(Dictionary<StatusType, List<int>> freezedWorkerStatus, int nodeId, string nodeURL)
        {
            if (trackerProxy == null)
            {
                trackerProxy = (IWorkerTracker)Activator.GetObject(
                            typeof(IWorkerTracker),
                            Worker.JOBTRACKER_URL);
            }
            Dictionary<StatusType, List<int>> updatedStatus = trackerProxy.receiveFreezedWorkerStatus(freezedWorkerStatus, nodeId, nodeURL);
            return updatedStatus;
        }

        public void CheckSystemStability()
        {
            lock (trackerLock)
            {
                if (IsTrackerChanging)
                {
                    Monitor.Wait(trackerLock);
                }

                else
                {
                    Monitor.PulseAll(trackerLock);
                }
            }
        }
    }
}
