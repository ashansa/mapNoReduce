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
    }
}
