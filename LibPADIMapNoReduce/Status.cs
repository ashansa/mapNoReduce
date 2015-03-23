using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    [Serializable]
   public class Status
    {
       double cpuUsage;
       double percentageCompleted;

       public double PercentageCompleted
       {
           get { return percentageCompleted; }
           set { percentageCompleted = value; }
       }
       double timeTakenTillNow;
       int splitId;
    }
}
