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
            /* long totalBytes = jobMetadata.TotalByteCount;
            long splits = jobMetadata.SplitCount;

           for (int i = 0; i < splits; i++)
           {
               long start = i * totalBytes / splits;
               long end = start + totalBytes / splits;
               //TODO give client url
              FileSplitMetadata metadata = new FileSplitMetadata(i, start, end, null);
               WorkerTaskMetadata workerData = receiveTaskRequest(metadata);
               Console.WriteLine("+====================================" + i);


               byte[] result = System.Text.Encoding.UTF8.GetBytes(workerData.Chunk);
               TaskResult taskResult = new TaskResult(result, i);
               receiveCompletedTask(taskResult);
           }*/
        }

    }
}
