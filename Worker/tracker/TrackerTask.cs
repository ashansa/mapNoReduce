using PADIMapNoReduce;
using PADIMapNoReduce.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.tracker
{
    public class TrackerTask
    {
        TrackerDetails trackerDetails;

        public TrackerTask(TrackerDetails trackerDetails)
        {
            this.trackerDetails = trackerDetails;
        }

        public TrackerDetails TrackerDetails
        {
            get { return trackerDetails; }
            set { trackerDetails = value; }
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
                trackerDetails.addFileSplit(metadata);
                upsertAllSplits(i);

                //call worker remote object.receiveTaskReq.
                /* WorkerTaskMetadata workerData = receiveTaskRequest(metadata);
                 Console.WriteLine("+====================================" + i);


                 byte[] result = System.Text.Encoding.UTF8.GetBytes(workerData.Chunk);
                 TaskResult taskResult = new TaskResult(result, i);
                 receiveCompletedTask(taskResult);*/
            }

            distributeTasks();
        }

        private void distributeTasks()
        {
            //TODO : for now assume splits < workers
            // implement to check the completed status and send jobs
            // TODO: handle when trackerDetails.ExistingWorkerMap.Count = 0

            for (int i = 0; i < trackerDetails.FileSplitData.Count; i++)
            {
                FileSplitMetadata splitData = trackerDetails.FileSplitData[i];

                KeyValuePair<Int32, string> entry = trackerDetails.ExistingWorkerMap.ElementAt(i % trackerDetails.ExistingWorkerMap.Count);
                IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entry.Value);
                worker.receiveTask(splitData);

                upsertSubmittedSplits(splitData.SplitId);//in case of performance based scheduling we can use (all-submitted-completed) to submit;

                Status status = createStartingStatusObject(splitData.SplitId, entry.Key);

                upsertMapTaskDetails(status);
            }
        }

        private Status createStartingStatusObject(int splitId, int nodeId)
        {
            Status status = new Status();
            status.SplitId = splitId;
            status.NodeId = nodeId;
            status.PercentageCompleted = 0;
            return status;
        }


        //update status of rela
        public void resultSentToClient(int nodeId, int splitId)
        {
            lock (trackerDetails)
            {
                trackerDetails.resultSentToClientSplits.Add(splitId);
            }
        }


        internal void updateStatus(Status status)
        {
            upsertMapTaskDetails(status);
        }

        private void upsertMapTaskDetails(Status status)
        {
            lock (trackerDetails.MapTaskDetails)
            {
                if (TrackerDetails.MapTaskDetails.ContainsKey(status.SplitId))
                {
                    trackerDetails.MapTaskDetails.Remove(status.SplitId);
                }
                trackerDetails.MapTaskDetails.Add(status.SplitId, status);
            }
        }

        private void upsertSubmittedSplits(int splitId)
        {
            lock (trackerDetails.SubmittedSplits)
            {
                if (!trackerDetails.SubmittedSplits.Contains(splitId))
                    trackerDetails.SubmittedSplits.Add(splitId);
            }
        }
        private void upsertAllSplits(int splitId)
        {
            lock (trackerDetails.AllSplits)
            {
                if (!trackerDetails.AllSplits.Contains(splitId))
                    trackerDetails.AllSplits.Add(splitId);
            }
        }

        internal void printStatus(int trackerId)
        {
            Console.WriteLine("tracker Id is " + trackerId);

            lock (trackerDetails.resultSentToClientSplits)
            {
                if (trackerDetails.resultSentToClientSplits.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("Splits which result sent to client are.... \r\n");
                    foreach (int id in trackerDetails.resultSentToClientSplits)
                    {
                        sb.Append("split is" + id + "node id is " + trackerDetails.MapTaskDetails[id].NodeId + "\r\n");
                    }
                    Console.WriteLine(sb.ToString());
                }
            }

            lock (trackerDetails.MapTaskDetails)
            {
                if (trackerDetails.MapTaskDetails.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("runing task details are.....\r\n");
                    foreach (KeyValuePair<int, Status> pair in trackerDetails.MapTaskDetails)
                    {
                        if (!trackerDetails.resultSentToClientSplits.Contains(pair.Key))
                        {
                            sb.Append("split id " + pair.Key + "worker id " + pair.Value.NodeId + " percentage completed " + pair.Value.PercentageCompleted + "%\r\n");
                        }
                    }
                    Console.WriteLine(sb.ToString());
                }
            }
        }

        internal void readyForNewTask(int nodeId)
        {

        }
    }
}
