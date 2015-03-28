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
      void addNewWorker(String newWorkerURL) ;
      List<String> getExistingWorkers();
      Boolean initWorker(WorkerMetadata workerMetadata);

   }
}
