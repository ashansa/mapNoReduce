using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
   public interface IClient
    {
       WorkerTaskMetadata receiveTaskRequest(FileSplitMetadata splitMetadata);//fill this object and return to worker
       void receiveCompletedTask(String file);
       void receiveWorkCompleteStatus();
    }
}
