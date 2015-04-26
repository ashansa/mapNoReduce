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
        double cpuUsage;
        double percentageCompleted;
        double timeTakenTillNow;
        int splitId;
        StatusType statusType;

        public StatusType StatusType
        {
            get { return statusType; }
            set { statusType = value; }
        }
        public int NodeId
        {
            get { return nodeId; }
            set { nodeId = value; }
        }

       public double PercentageCompleted
       {
           get { return percentageCompleted; }
           set { percentageCompleted = value; }
       }
       

       public double TimeTakenTillNow
       {
           get { return timeTakenTillNow; }
           set { timeTakenTillNow = value; }
       }

       public int SplitId
       {
           get { return splitId; }
           set { splitId = value; }
       }
    }

    public enum StatusType
    {
    NOT_STARTED,
     INPROGRESS,
     COMPLETED,
     SUSPENDED,
     NOT_SEND_TO_WORKER
    }
}
