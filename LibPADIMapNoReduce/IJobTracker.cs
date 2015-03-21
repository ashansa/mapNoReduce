using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
  public  interface IJobTracker
    {
      void receiveJobRequest(int lineCount);
      void receiveStatus(Status status);
      void receiveHeartbeat();

    }
}
