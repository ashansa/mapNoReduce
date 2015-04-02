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
              FileSplitMetadata metadata = new FileSplitMetadata(i, start, end, jobMetadata.ClientUrl, "tcp://localhost:10001/Worker");
              trackerDetails.addFileSplit(metadata);
             
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
          
            for(int i = 0; i < trackerDetails.FileSplitData.Count; i++) 
            {
                FileSplitMetadata jobData = trackerDetails.FileSplitData[i];
              
                KeyValuePair<Int32, string> entry = trackerDetails.ExistingWorkerMap.ElementAt(i%trackerDetails.ExistingWorkerMap.Count);
                IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entry.Value);
                //IWorker worker = new Worker(10+i);
                worker.receiveTask(jobData);
            }
        }

        public void taskCompleted(int nodeId, int splitId)
        {
            TrackerDetails.addCompletedSplit(splitId);
        }

    }
}
