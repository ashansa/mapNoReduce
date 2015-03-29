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
      void jobCompleted(int nodeId,int splitId);
      void resultSentToClient(int nodeId, int splitId);
    }
}
