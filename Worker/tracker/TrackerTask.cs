using PADIMapNoReduce;
using PADIMapNoReduce.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

namespace Server.tracker
{
    public class TrackerTask
    {
        private Dictionary<int, IWorkerTracker> workerProxyMap = new Dictionary<int, IWorkerTracker>();
        Dictionary<int, Task> taskList = new Dictionary<int, Task>();
        Dictionary<Int32, WorkerDetails> existingWorkerMap = new Dictionary<Int32, WorkerDetails>();//workerId,url
        String clientURL;

        public String ClientURL
        {
            get { return clientURL; }
            set { clientURL = value; }
        }
        public Dictionary<Int32, WorkerDetails> ExistingWorkerMap
        {
            get { return existingWorkerMap; }
            set { existingWorkerMap = value; }
        }

        public Dictionary<int, Task> TaskList
        {
            get { return taskList; }
            set { taskList = value; }
        }

        public TrackerTask(String clientURL, Dictionary<int, WorkerDetails> existingMap)
        {
            this.clientURL = clientURL;
            this.existingWorkerMap = existingMap;
        }

        public void splitJob(JobMetadata jobMetadata)
        {
            Console.WriteLine("splitting job");
            long totalBytes = jobMetadata.TotalByteCount;
            long splits = jobMetadata.SplitCount;

            for (int i = 0; i < splits; i++)
            {
                long start = i * totalBytes / splits;
                long end = start + totalBytes / splits;

                //TODO : give tracker URL dynamically
                FileSplitMetadata metadata = new FileSplitMetadata(i, start, end, jobMetadata.ClientUrl, Worker.JOBTRACKER_URL);
                Task task = new Task(i, metadata, StatusType.NOT_SEND_TO_WORKER);
                taskList.Add(i, task);
            }
            distributeTasks();
            //Timer t = new Timer(TimerCallback, null, 0, 2000);
        }

        private void distributeTasks()
        {
            for (int i = 0; i < existingWorkerMap.Count; i++)
            {
                KeyValuePair<Int32, WorkerDetails> entry = existingWorkerMap.ElementAt(i);
                FileSplitMetadata splitData = getNextPendingSplitFromList(entry.Key);
                if (splitData == null)
                {
                    break;
                }
                /* try to do this in a seperate thread */
                // sendTaskToWorker(nodeId, splitData);
                Thread thread = new Thread(() => sendTaskToWorker(entry.Key, splitData, Constants.RETRY_ROUNDS));
                thread.Start();
            }
        }

        /*sending heartbeats*/
        /*retry 3 times and remove node if not available  */
        /* Only a dummy method, need to handle failures */
        private void TimerCallback(object state)
        {
            Console.WriteLine("heartbeat called");
            IWorkerTracker worker = null;

            for (int i = 0; i < existingWorkerMap.Count; i++)
            {
                KeyValuePair<Int32, WorkerDetails> entry = existingWorkerMap.ElementAt(i);
                int retryRounds = 0;


                for (; retryRounds < Constants.RETRY_ROUNDS; )
                {
                    try
                    {
                        lock (workerProxyMap)
                        {
                            if (!workerProxyMap.ContainsKey(entry.Key))
                            {
                                string url = entry.Value.Nodeurl;
                                worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), url);
                                if (worker != null)
                                {
                                    workerProxyMap.Add(entry.Key, worker);
                                }
                            }
                            else
                            {
                                worker = workerProxyMap[entry.Key];
                            }
                        }
                        worker.checkHeartbeat();
                        break;
                    }
                    catch (Exception ex)
                    {
                        retryRounds++;
                        if (retryRounds == Constants.RETRY_ROUNDS)
                        {
                            existingWorkerMap.Remove(entry.Key);
                            worker.getExistingWorkers().Remove(entry.Key);

                            Thread taskUpdateThread = new Thread(() => updateTaskList(entry.Key));
                            taskUpdateThread.Start();
                            /*do the failed notification sending in a seperate thread*/
                            Thread thread = new Thread(() => notifyWorkersAboutFailedNode(entry.Key));
                            thread.Start();
                            Common.Logger().LogError("node " + entry.Key + " was removed while during heartbeat", string.Empty, string.Empty);
                            Common.Logger().LogError(ex.Message, string.Empty, string.Empty);
                        }
                    }
                }
            }
        }

        /*change all splits processed by that node to inprogress*/
        private void updateTaskList(int failedNodeId)
        {
            lock (taskList)
            {
                foreach (var task in taskList)
                {
                    if (task.Value.WorkerId == failedNodeId && task.Value.StatusType != StatusType.COMPLETED)
                    {
                        taskList[failedNodeId].StatusType = StatusType.NOT_SEND_TO_WORKER;
                    }
                }
            }
        }


        private void notifyWorkersAboutFailedNode(int failedNodeId)
        {
            IWorkerTracker worker = null;
            for (int i = 0; i < existingWorkerMap.Count; i++)
            {
                KeyValuePair<Int32, WorkerDetails> entry = existingWorkerMap.ElementAt(i);
                try
                {
                    lock (workerProxyMap)
                    {
                        if (!workerProxyMap.ContainsKey(entry.Key))
                        {
                            string url = entry.Value.Nodeurl;
                            worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), url);
                            if (worker != null)
                            {
                                workerProxyMap.Add(entry.Key, worker);
                            }
                        }
                        else
                        {
                            worker = workerProxyMap[entry.Key];
                        }
                    }
                    worker.removeFailedNode(entry.Key);
                    break;
                }
                catch (Exception ex)
                {
                    Common.Logger().LogError("job failure notificatin of node ", string.Empty, string.Empty);
                }
            }
        }

        //update status of rela
        public void resultSentToClient(int nodeId, int splitId)
        {
            lock (taskList[splitId])
            {
                taskList[splitId].StatusType = StatusType.COMPLETED;
                existingWorkerMap[nodeId].ProcessedSplits.Add(splitId);
                if (existingWorkerMap[nodeId].State == WorkerState.ABOUT_TO_IDLE)
                {
                    existingWorkerMap[nodeId].State = WorkerState.IDLE;
                    ReplaceSlowTasks();
                }
            }
        }


        internal void updateStatus(Status status)
        {
            int splitId = status.SplitId;
            lock (taskList[splitId])
            {
                taskList[splitId].PercentageCompleted = status.PercentageCompleted;
            }
        }


        internal void printStatus(int trackerId)
        {
            StringBuilder sb = new StringBuilder();
            Console.WriteLine("########Tracker Status######");
            Console.WriteLine("tracker Id is " + trackerId);
            sb.Append("Splits which result sent to client are.... \r\n");
            lock (taskList)
            {
                foreach (KeyValuePair<int, Task> pair in taskList)
                {
                    if (pair.Value.StatusType == StatusType.COMPLETED)
                    {
                        sb.Append("split is" + pair.Key + "node id is " + pair.Value.WorkerId + "\r\n");
                    }
                }
            }
            Console.WriteLine(sb.ToString());
            sb = new StringBuilder();
            sb.Append("Splits which still in progress are.....\r\n");
            lock (taskList)
            {
                foreach (KeyValuePair<int, Task> pair in taskList)
                {
                    if (pair.Value.StatusType == StatusType.INPROGRESS)
                    {
                        sb.Append("split is" + pair.Key + "node id is " + pair.Value.WorkerId + "\r\n");
                    }
                }
            }
            Console.WriteLine(sb.ToString());
            Console.WriteLine("################");
        }

        /*greedy way, will try to complete remaining at the end by high performing nodes*/
        internal void readyForNewTask(int nodeId)
        {
            FileSplitMetadata splitMetadata = getNextPendingSplitFromList(nodeId);

            if (splitMetadata != null)
            {
                sendTaskToWorker(nodeId, splitMetadata, 1);
            }
            else
            {
                existingWorkerMap[nodeId].State = WorkerState.ABOUT_TO_IDLE;
            }
        }

        private void ReplaceSlowTasks()
        {
            if (!HasPendingTasks())
            {
                WorkerDetails bestWorker = null;
                FileSplitMetadata splitMetadata;
                lock (taskList)
                {
                    foreach (var pair in taskList)
                    {
                        /* later change this to have from suspended as well */
                        if (pair.Value.StatusType == StatusType.INPROGRESS)
                        {
                            int splitId = pair.Key;
                            bestWorker = GetBestWorker(pair.Value);
                            //suspend
                            if (bestWorker != null)
                            {
                                bestWorker.State = WorkerState.ABOUT_TO_BUSY;
                                string url = existingWorkerMap[taskList[splitId].WorkerId].Nodeurl;
                                IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), url);
                                worker.suspendTask(splitId);

                                //reschedule
                                splitMetadata = pair.Value.SplitMetadata;
                                taskList[splitId].StatusType = StatusType.INPROGRESS;
                                taskList[splitId].WorkerId = bestWorker.Nodeid;
                                sendTaskToWorker(bestWorker.Nodeid, splitMetadata, 1);
                            }

                        }
                    }
                }
            }
        }

        private WorkerDetails GetBestWorker(Task task)
        {
            WorkerDetails bestWorker = null;
            int completedSplitCount = existingWorkerMap[task.WorkerId].ProcessedSplits.Count;
            if (task.PercentageCompleted < Constants.jobReplaceBoundaryPercentage)
            {
                bestWorker = (from worker in existingWorkerMap.Values
                              where worker.ProcessedSplits.Count > completedSplitCount && worker.State == WorkerState.IDLE
                              orderby worker.ProcessedSplits.Count descending
                              select worker).FirstOrDefault();

            }
            return bestWorker;
        }

   
        private void sendTaskToWorker(int nodeId, FileSplitMetadata splitMetadata, int roundsToTry)
        {
            IWorkerTracker worker = null;
            try
            {
                lock (workerProxyMap)
                {
                    if (!workerProxyMap.ContainsKey(nodeId))
                    {
                        string url = existingWorkerMap[nodeId].Nodeurl;
                        worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), url);
                        if (worker != null)
                        {
                            workerProxyMap.Add(nodeId, worker);
                        }
                    }
                    else
                    {
                        worker = workerProxyMap[nodeId];
                    }
                }

                worker.receiveTask(splitMetadata);
                existingWorkerMap[nodeId].State = WorkerState.BUSY;
            }
            catch (Exception ex)
            {
                      taskList[splitMetadata.SplitId].StatusType = StatusType.NOT_SEND_TO_WORKER;
                    Common.Logger().LogError("Unable to send split " + splitMetadata.SplitId + " to node " + nodeId, string.Empty, string.Empty);
                    Common.Logger().LogError(ex.Message, string.Empty, string.Empty);
            }

        }

        private FileSplitMetadata getNextPendingSplitFromList(int workerId)
        {
            FileSplitMetadata splitMetadata = null;
            lock (taskList)
            {
                foreach (var pair in taskList)
                {
                    /* later change this to have from suspended as well */
                    if (pair.Value.StatusType == StatusType.NOT_SEND_TO_WORKER)
                    {
                        int splitId = pair.Key;
                        splitMetadata = pair.Value.SplitMetadata;
                        taskList[splitId].StatusType = StatusType.INPROGRESS;
                        taskList[splitId].WorkerId = workerId;
                        return splitMetadata;
                    }
                }
            }
            return null;
        }

        private bool HasPendingTasks()
        {
            bool hasPendingTasks = false;
            lock (taskList)
            {

                foreach (var pair in taskList)
                {
                    if (pair.Value.StatusType == StatusType.NOT_SEND_TO_WORKER)
                    {
                        hasPendingTasks = true;
                        break;
                    }
                }
            }
            return hasPendingTasks;
        }

        public bool isJobCompleted()
        {
            foreach (var pair in taskList)
            {
                if (pair.Value.StatusType != StatusType.COMPLETED)
                {
                    return false;
                }
            }
            return true;
        }

        internal Dictionary<StatusType, List<int>> getStatusForWorker(Dictionary<StatusType, List<int>> freezedWorkerStatus, int nodeId,string nodeURL)
        {
            //Worker has come back. first thing to do is add to worker map and notify others
            WorkerDetails workerObj = getWorker(nodeId, nodeURL);
            existingWorkerMap.Add(nodeId, workerObj);
            Thread thread = new Thread(() => notifyWorkersAboutUnfreezed(nodeId,nodeURL));
            thread.Start();
        
            //make him upto date
            Console.WriteLine("tracker received the status");
            List<Int32> inProgress = new List<int>();
            List<Int32> sendToClient = new List<int>();
            bool isTaskAvailable = false;
            Dictionary<StatusType, List<int>> result = new Dictionary<StatusType, List<int>>();
            for (int i = 0; i < freezedWorkerStatus.Count; i++)
            {
                KeyValuePair<StatusType, List<int>> entry = freezedWorkerStatus.ElementAt(i);
                switch (entry.Key)
                {
                    case StatusType.COMPLETED:
                        foreach (int split in entry.Value)
                        {
                            lock(taskList[split]){
                            if (taskList[split].StatusType == StatusType.INPROGRESS || taskList[split].StatusType == StatusType.NOT_SEND_TO_WORKER)
                            {
                                if (taskList[split].StatusType == StatusType.INPROGRESS)
                                {
                                    string url = existingWorkerMap[taskList[split].WorkerId].Nodeurl;
                                    IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), url);
                                    worker.suspendTask(split);
                                }
                                taskList[split].WorkerId = nodeId;//later will receive completed once result sent to client
                                sendToClient.Add(split);
                            }
                            }
                        }
                        break;

                    case StatusType.NOT_STARTED:
                        foreach (int split in entry.Value)
                        {
                            lock(taskList[split]){
                               if (taskList[split].StatusType == StatusType.INPROGRESS && taskList[split].WorkerId != nodeId && taskList[split].PercentageCompleted > Constants.jobReplaceBoundaryPercentage || taskList[split].StatusType == StatusType.COMPLETED)
                            {
                                isTaskAvailable = false;
                            }
                            else {
                                inProgress.Add(split);
                                isTaskAvailable = true;
                            }
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
            if (isTaskAvailable == false)
            {
                FileSplitMetadata newSplitData = getNextPendingSplitFromList(nodeId);
                if (newSplitData != null)
                {
                    sendTaskToWorker(nodeId, newSplitData, 3);
                    existingWorkerMap[nodeId].State = WorkerState.BUSY;
                }
            }
            else {
                Console.WriteLine("he has a job");
                existingWorkerMap[nodeId].State = WorkerState.BUSY;
            }
            result.Add(StatusType.COMPLETED, sendToClient);//cmpare completed and ResultTask and remove from resulttask if not in
            result.Add(StatusType.NOT_STARTED, inProgress);//compare with split metatdata,and remove from metadata if not
            return result;
        }

        private WorkerDetails getWorker(int nodeId, string nodeURL)
        {
            WorkerDetails worker = new WorkerDetails();
            worker.Nodeid = nodeId;
            worker.Nodeurl = nodeURL;
            worker.State = WorkerState.IDLE;
            worker.ProcessedSplits = new List<int>();
            return worker;
        }

        private void notifyWorkersAboutUnfreezed(int nodeId,String nodeURL)
        {
            IWorkerTracker worker = null;
            for (int i = 0; i < existingWorkerMap.Count; i++)
            {
                KeyValuePair<Int32, WorkerDetails> entry = existingWorkerMap.ElementAt(i);
                try
                {
                    lock (workerProxyMap)
                    {
                        if (!workerProxyMap.ContainsKey(entry.Key))
                        {
                            string url = entry.Value.Nodeurl;
                            worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), url);
                            if (worker != null)
                            {
                                workerProxyMap.Add(entry.Key, worker);
                            }
                        }
                        else
                        {
                            worker = workerProxyMap[entry.Key];
                        }
                    }
                    worker.addUnfreezedNode(nodeId,nodeURL);
                    break;
                }
                catch (Exception ex)
                {
                    Common.Logger().LogError("Unable to notify others about unfreeze by Job Tracker", string.Empty, string.Empty);
                }
            }

        }
    }
}
