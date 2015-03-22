using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
  public  interface IJobTracker
  {
      void receiveJobRequest(ClientMetadata clientMetadata);
      void receiveStatus(String status);
      void receiveHeartbeat();

    }
}
