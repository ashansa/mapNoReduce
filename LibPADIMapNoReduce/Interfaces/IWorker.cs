using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
   public interface IWorker
    {
      void receiveTask(FileSplitMetadata splitMetadata);//recive split details from jobTracker
      void checkHeartbeat();
      bool suspendTask(int splitId);//job tracker invoke this to suspend a job on worker
      void addNewWorker(int workerId,String newWorkerURL) ;
      Dictionary<Int32,WorkerDetails> getExistingWorkers();
      Boolean initWorker(WorkerMetadata workerMetadata);
      void removeFailedNode(int failedId);
      void addUnfreezedNode(int nodeId, String nodeURL);
    }
}
