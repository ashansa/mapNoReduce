using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
   public interface IWorker
    {
      void receiveTask();
      void executeTask(byte[] code, File file);
      void checkHeartbeat();
    }
}
