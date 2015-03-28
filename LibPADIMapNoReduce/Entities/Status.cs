using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    [Serializable]
   public class Status
    {
        int nodeId;

        public int NodeId
        {
            get { return nodeId; }
            set { nodeId = value; }
        }
       double cpuUsage;
       double percentageCompleted;

       public double PercentageCompleted
       {
           get { return percentageCompleted; }
           set { percentageCompleted = value; }
       }
       double timeTakenTillNow;

       public double TimeTakenTillNow
       {
           get { return timeTakenTillNow; }
           set { timeTakenTillNow = value; }
       }
       int splitId;
    }
}
