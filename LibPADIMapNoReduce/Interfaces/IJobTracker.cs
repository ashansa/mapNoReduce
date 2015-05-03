using PADIMapNoReduce.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
  public  interface IJobTracker
  {
      void receiveJobRequest(JobMetadata clientMetadata);
      void receiveStatus(Status status);
      void taskCompleted(int nodeId, int splitId);
      void readyForNewTask(int nodeId);
      Dictionary<StatusType, List<int>> receiveFreezedWorkerStatus(Dictionary<StatusType, List<int>> freezedWorkerStatus, int nodeId, string nodeURL);
      void ChangeTracker(int workerID, List<int> processingSplits, List<int> alreadySentSplits);
      void SetCopyOfTasks(Dictionary<int, Task> tasks, string clientUrl);
    }
}
