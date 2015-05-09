using PADIMapNoReduce;
using PADIMapNoReduce.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;
using Server.worker;
using System.Configuration;

namespace Server.tracker
{
    public class TrackerTask
    {
        private Dictionary<int, IWorkerTracker> workerProxyMap = new Dictionary<int, IWorkerTracker>();
        Dictionary<int, Task> taskList = new Dictionary<int, Task>();
        Dictionary<int, Task> originalTaskList = new Dictionary<int, Task>();
        Dictionary<Int32, WorkerDetails> existingWorkerMap = new Dictionary<Int32, WorkerDetails>();//workerId,url
        String clientURL;
        Timer heartBeatTimer;
        int trackerChangeVotes = 0;
        HashSet<int> votedNodes = new HashSet<int>();
        int workerId = 0;
        bool isTrackerFreezed = false;
        Timer minorityNotifyTimer;

        public bool IsTrackerFreezed
        {
            get { return isTrackerFreezed; }
            set { isTrackerFreezed = value; }
        }

     
        WorkerCommunicator communicator = new WorkerCommunicator();

        int count = 0;

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

        public TrackerTask(String clientURL, Dictionary<int, WorkerDetails> existingMap, int workerId)
        {
            this.clientURL = clientURL;
            this.existingWorkerMap = existingMap;
            this.workerId = workerId;
            this.isTrackerFreezed = false;
        }

        public void splitJob(JobMetadata jobMetadata)
        {
             Common.Logger().LogInfo("splitting job",string.Empty,string.Empty);
            long totalBytes = jobMetadata.TotalByteCount;
            long splits = jobMetadata.SplitCount;

            for (int i = 0; i < splits; i++)
            {
                long start = i * totalBytes / splits;
                long end = start + totalBytes / splits;

                FileSplitMetadata metadata = new FileSplitMetadata(i, start, end, jobMetadata.ClientUrl, Worker.JOBTRACKER_URL);
                Task task = new Task(i, metadata, StatusType.NOT_SEND_TO_WORKER);
                taskList.Add(i, task);

                FileSplitMetadata metadataOrig = new FileSplitMetadata(i, start, end, jobMetadata.ClientUrl, Worker.JOBTRACKER_URL);
                Task taskOrig = new Task(i, metadataOrig, StatusType.NOT_SEND_TO_WORKER);
                originalTaskList.Add(i, taskOrig);
            }
            if (existingWorkerMap.Count > 1)
            {
                string bkpUrl = GetBackupTrackerUrl();
                communicator.SendTaskCopyToBackupTracker(bkpUrl, originalTaskList, clientURL);
                communicator.notifyBackupJobtrackerUrl(bkpUrl, existingWorkerMap);
            }
            distributeTasks();

        }

        private string GetBackupTrackerUrl()
        {
            string bkpUrl = null;
            Random rand = new Random();
            int randId = 0;
            while (true)
            {
                randId = rand.Next(1, (existingWorkerMap.Count));
                if (randId != workerId)
                    break;
            }
             Common.Logger().LogInfo("Backup Trackeser = " + existingWorkerMap[randId].Nodeurl + "**********************",string.Empty,string.Empty);
            return existingWorkerMap[randId].Nodeurl;

        }

        public void startHeartBeat()
        {
            int heartbeatTime = Convert.ToInt32(ConfigurationManager.AppSettings[Constants.JOB_TRACKER_HEARTBEAT_INTERVAL].ToString());
            heartBeatTimer = new Timer(TimerCallback, null, 0, 2000);
        }

        public void stopHeatBeat()
        {
            if (heartBeatTimer != null)
            {
                heartBeatTimer.Dispose();
                Console.WriteLine("Heart Beat timer disposed");
            }
        }

        public void SetCopyOfTasks(Dictionary<int, Task> tasks)
        {
            Console.WriteLine("Original task list received by " + workerId + "**************************");
            originalTaskList.Clear();
            taskList.Clear();
            foreach (var item in tasks)
            {
                item.Value.SplitMetadata.JobTrackerUrl = existingWorkerMap[workerId].Nodeurl;
                originalTaskList.Add(item.Key, item.Value);
                taskList.Add(item.Key, item.Value);

            }
        }

        Boolean hasForced = false;
        public void ChangeTracker(int workerID, List<int> processingSplits, List<int> alreadySentSplits)
        {
            bool hasAllReplied=false;
            lock (taskList)
            {
                trackerChangeVotes++;
                votedNodes.Add(workerID);

                foreach (int item in processingSplits)
                {
                    taskList[item].StatusType = StatusType.INPROGRESS;
                    taskList[item].WorkerId = workerID;
                }
                foreach (int item in alreadySentSplits)
                {
                    taskList[item].StatusType = StatusType.COMPLETED;
                    taskList[item].WorkerId = workerID;
                }

                if(hasReceivedAllVotes()){
                    hasAllReplied = true;
                    foreach (var item in taskList)
                    {
                        Common.Logger().LogInfo("Split ID= " + item.Key + " WorkerID = " + item.Value.WorkerId + " Status= " + item.Value.StatusType.ToString(), string.Empty, string.Empty);
                    }
                }
            }

            //TODO:start timer to notify minority

            if (hasAllReplied && !Worker.isJobTracker)
            {
                Worker.isJobTracker = true;
                communicator.TrackerStabilized(existingWorkerMap);//verify bug: different nodes having diff trackers
                string bkpUrl = GetBackupTrackerUrl();
                communicator.SendTaskCopyToBackupTracker(bkpUrl, originalTaskList, clientURL);
                communicator.notifyBackupJobtrackerUrl(bkpUrl, existingWorkerMap);
                startHeartBeat();
                Console.WriteLine("all votes received for tracker");

                lock (taskList)
                {
                    trackerChangeVotes = 0;
                    votedNodes = new HashSet<int>();
                    hasForced = false;
                }

            }
            else if (hasMajorityReplied() && !hasForced) {
                hasForced = true;
                forceMinorityForVote();
            }
        }

        /* if after a timeout still doenst have majority, notify them as tracker is alive */

        private void forceMinorityForVote()
        {
            HashSet<int> keySet = new HashSet<int>(existingWorkerMap.Keys);
            var minoritySet = keySet.Except(votedNodes);
            IWorkerTracker worker;
            foreach (int workerId in minoritySet)
            {
                try
                {
                    lock (workerProxyMap)
                    {
                        if (!workerProxyMap.ContainsKey(workerId))
                        {
                            string url = existingWorkerMap[workerId].Nodeurl;
                            worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), url);
                            if (worker != null)
                            {
                                workerProxyMap.Add(workerId, worker);
                            }
                        }
                        else
                        {
                            worker = workerProxyMap[workerId];
                        }
                    }
                    Thread thread = new Thread(() => worker.forceTrackerChange());
                    thread.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private bool hasMajorityReplied()
        {
            return (votedNodes.Count == existingWorkerMap.Count - 1) ?true:false;
        }

        private bool hasReceivedAllVotes()
        {
        return votedNodes.Count==existingWorkerMap.Count?true:false;
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
                Thread thread = new Thread(() => sendTaskToWorker(entry.Key, splitData));
                thread.Start();
            }
        }

        private void TimerCallback(object state)
        {

            IWorkerTracker worker = null;
            int workerCount = existingWorkerMap.Count;
            List<int> workerIdLIst = new List<int>(existingWorkerMap.Keys);
            for (int i = 0; i < workerIdLIst.Count; i++)
            {
                if (existingWorkerMap.ContainsKey(workerIdLIst[i]))
                {
                    try
                    {
                        lock (workerProxyMap)
                        {
                            if (!workerProxyMap.ContainsKey(workerIdLIst[i]))
                            {
                                string url = existingWorkerMap[workerIdLIst[i]].Nodeurl;
                                worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), url);
                                if (worker != null)
                                {
                                    workerProxyMap.Add(workerIdLIst[i], worker);
                                }
                            }
                            else
                            {
                                worker = workerProxyMap[workerIdLIst[i]];
                            }
                        }
                        worker.checkHeartbeat();
                    }
                    catch (Exception ex)
                    {
                        if(existingWorkerMap.ContainsKey(workerIdLIst[i])){
                        existingWorkerMap.Remove(workerIdLIst[i]);
                        workerCount--;

                        Thread taskUpdateThread = new Thread(() => updateTaskList(workerIdLIst[i]));
                        taskUpdateThread.Start();
                        /*do the failed notification sending in a seperate thread*/
                        Thread thread = new Thread(() => notifyWorkersAboutFailedNode(workerIdLIst[i]));
                        thread.Start();
                        Common.Logger().LogError("node " + workerIdLIst[i] + " was removed while during heartbeat", string.Empty, string.Empty);
                        Console.WriteLine("Removed worker from map*****************" + "node is " + workerIdLIst[i]);
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
                        task.Value.StatusType = StatusType.NOT_SEND_TO_WORKER;
                        task.Value.WorkerId = 0;
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
                    worker.removeFailedNode(failedNodeId);
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
                Common.Logger().LogInfo("*************************************", string.Empty, string.Empty);
                foreach (var item in taskList)
                {
                    Common.Logger().LogInfo("on result send Split ID= " + item.Key + " WorkerID = " + item.Value.WorkerId + " Status= " + item.Value.StatusType.ToString(), string.Empty, string.Empty);
                }
                taskList[splitId].StatusType = StatusType.COMPLETED;
                existingWorkerMap[nodeId].ProcessedSplits.Add(splitId);
            }
                
                if (existingWorkerMap[nodeId].State == WorkerState.ABOUT_TO_IDLE)
                {
                    existingWorkerMap[nodeId].State = WorkerState.IDLE;
                    ReplaceSlowTasks(nodeId);
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
            if (existingWorkerMap.ContainsKey(nodeId))
            {
                FileSplitMetadata splitMetadata = getNextPendingSplitFromList(nodeId);

                if (splitMetadata != null)
                {
                    sendTaskToWorker(nodeId, splitMetadata);
                }
                else
                {
                    existingWorkerMap[nodeId].State = WorkerState.ABOUT_TO_IDLE;
                }
            }
        }

        /*going to replace one slow task*/
        private void ReplaceSlowTasks(int nodeId)
        {
            if (!HasPendingTasks())
            {
                WorkerDetails bestWorker = null;
                FileSplitMetadata splitMetadata = null;
                int splitId = 0;
                lock (taskList)
                {
                    foreach (var pair in taskList)
                    {
                        /* later change this to have from suspended as well */
                        if (pair.Value.StatusType == StatusType.INPROGRESS)
                        {
                            splitId = pair.Key;
                            bestWorker = GetBestWorker(pair.Value, nodeId);
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
                                sendTaskToWorker(bestWorker.Nodeid, splitMetadata);
                            }

                        }
                    }
                }
            }
            else {
                readyForNewTask(nodeId);
            }
        }

        private WorkerDetails GetBestWorker(Task task, int nodeId)
        {
            WorkerDetails bestWorker = null;

            int completedSplitCount = existingWorkerMap[task.WorkerId].ProcessedSplits.Count;
            if (task.PercentageCompleted <= Constants.jobReplaceBoundaryPercentage)
            {
                bestWorker = (from worker in existingWorkerMap.Values
                              where worker.ProcessedSplits.Count > completedSplitCount && worker.State == WorkerState.IDLE && worker.Nodeid != nodeId
                              orderby worker.ProcessedSplits.Count descending
                              select worker).FirstOrDefault();

            }

            return bestWorker;
        }


        private void sendTaskToWorker(int nodeId, FileSplitMetadata splitMetadata)
        {
            if (!isTrackerFreezed)
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
                    existingWorkerMap[nodeId].State = WorkerState.BUSY;
                    worker.receiveTask(splitMetadata);
                   
                }
                catch (Exception ex)
                {
                    taskList[splitMetadata.SplitId].StatusType = StatusType.NOT_SEND_TO_WORKER;
                    Common.Logger().LogError("Unable to send split " + splitMetadata.SplitId + " to node " + nodeId, string.Empty, string.Empty);
                    Common.Logger().LogError(ex.Message, string.Empty, string.Empty);
                }
            }
           else
            {
                Common.Logger().LogError("tracker with id " + workerId + " has been freezed so no new splits sent by him",string.Empty,string.Empty);
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

        internal Dictionary<StatusType, List<int>> getStatusForWorker(Dictionary<StatusType, List<int>> freezedWorkerStatus, int nodeId, string nodeURL)
        {
            //Worker has come back. first thing to do is add to worker map and notify others
            WorkerDetails workerObj = getWorker(nodeId, nodeURL);

            if (!existingWorkerMap.ContainsKey(nodeId))
            {
                existingWorkerMap.Add(nodeId, workerObj);
                Thread thread = new Thread(() => notifyWorkersAboutUnfreezed(nodeId, nodeURL));
                thread.Start();
            }

            //make him upto date
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
                            lock (taskList[split])
                            {
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
                            lock (taskList[split])
                            {
                                if (taskList[split].StatusType == StatusType.INPROGRESS && taskList[split].WorkerId != nodeId && taskList[split].PercentageCompleted > Constants.jobReplaceBoundaryPercentage || taskList[split].StatusType == StatusType.COMPLETED)
                                {
                                    isTaskAvailable = false;
                                }
                                else
                                {
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
                    sendTaskToWorker(nodeId, newSplitData);
                    existingWorkerMap[nodeId].State = WorkerState.BUSY;
                }
            }
            else
            {
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

        private void notifyWorkersAboutUnfreezed(int nodeId, String nodeURL)
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
                    worker.addUnfreezedNode(nodeId, nodeURL);
                }
                catch (Exception ex)
                {
                    Common.Logger().LogError("Unable to notify others about unfreeze by Job Tracker", string.Empty, string.Empty);
                }
            }

        }

        internal void notifyJobCompleteToWorker()
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
                        }
                        else
                        {
                            worker = workerProxyMap[entry.Key];
                        }
                    }
                    worker.jobCompleted();
                }
                catch (Exception ex)
                {
                    Common.Logger().LogError("Unable to notify others about unfreeze by Job Tracker", string.Empty, string.Empty);
                }
            }
        }

        internal void notifyWorkersForJobStart()
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
                        }
                        else
                        {
                            worker = workerProxyMap[entry.Key];
                        }
                    }
                    worker.receiveNewJob();
                }
                catch (Exception ex)
                {
                    Common.Logger().LogError("Unable to notify others about unfreeze by Job Tracker", string.Empty, string.Empty);
                }
            }
        }
    }
}
