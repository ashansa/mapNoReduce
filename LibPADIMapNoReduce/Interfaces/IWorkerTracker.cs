using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    /*As we habve to marshall worker object(we don't have different url for job tracker
     * ) activator should also be the same type*/
   public interface IWorkerTracker:IWorker,IJobTracker
    {
      void displayStatus();
    }
}
