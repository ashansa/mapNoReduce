using PADIMapNoReduce;
using PADIMapNoReduce.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server.tracker
{
    public class TrackerTask
    {
        private Dictionary<int, IWorkerTracker> workerProxyMap = new Dictionary<int, IWorkerTracker>();
        Dictionary<int, Task> taskList = new Dictionary<int, Task>();
        Dictionary<Int32, String> existingWorkerMap = new Dictionary<Int32, string>();//workerId,url
        String clientURL;

        public String ClientURL
        {
            get { return clientURL; }
            set { clientURL = value; }
        }
        public Dictionary<Int32, String> ExistingWorkerMap
        {
            get { return existingWorkerMap; }
            set { existingWorkerMap = value; }
        }

        public Dictionary<int, Task> TaskList
        {
            get { return taskList; }
            set { taskList = value; }
        }

        public TrackerTask(String clientURL, Dictionary<int, String> existingMap)
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
        }

        private void distributeTasks()
        {
            //TODO : for now assume splits < workers
            // implement to check the completed status and send jobs
            // TODO: handle when trackerDetails.ExistingWorkerMap.Count = 0
  
                for (int i = 0; i < existingWorkerMap.Count; i++)
                {                  
                    KeyValuePair<Int32, string> entry = existingWorkerMap.ElementAt(i);
                    FileSplitMetadata splitData = getNextPendingSplitFromList(entry.Key);
                    if (splitData == null)
                    {
                        break;
                    }
                    
                    /* try to do this in a seperate thread */
                    Thread thread = new Thread(() => sendSplitToWorker(splitData,entry.Key,entry.Value));
                    thread.Start();
                }
        }
        private void sendSplitToWorker(FileSplitMetadata splitData, int workerId,string url)
        {
            IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker),url);
            worker.receiveTask(splitData);
            workerProxyMap.Add(workerId, worker);
        }

        //update status of rela
        public void resultSentToClient(int nodeId, int splitId)
        {
            lock (taskList[splitId])
            {
                taskList[splitId].StatusType = StatusType.COMPLETED;
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
                    Console.WriteLine(sb.ToString());
                }
            }
            sb.Append("Splits which still in progress are.....\r\n");
            foreach (KeyValuePair<int, Task> pair in taskList)
            {
                if (pair.Value.StatusType == StatusType.INPROGRESS)
                {
                    sb.Append("split is" + pair.Key + "node id is " + pair.Value.WorkerId + "\r\n");
                }
                Console.WriteLine(sb.ToString());
            }
            Console.WriteLine("################");
        }

        /*assign ranking to each node and assign tasks based on that*/
        internal void readyForNewTask(int nodeId)
        {
            IWorkerTracker worker = null;
            FileSplitMetadata splitMetadata = getNextPendingSplitFromList(nodeId);

            if (splitMetadata != null)
            {
                worker = workerProxyMap[nodeId];
                worker.receiveTask(splitMetadata);
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

    }
}
