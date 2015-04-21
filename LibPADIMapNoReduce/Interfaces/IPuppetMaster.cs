using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce.Interfaces
{
   public interface IPuppetMaster
    {
       bool createLocalWorker(WorkerMetadata workerMetadata);
       void displayStatus();
       void slowWorker(int seconds,int id);
    }
}
