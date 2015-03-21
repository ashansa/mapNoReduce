using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
   public interface IClient
    {
       void receiveTaskRequest();
       void receiveCompletedTask(File file);
       void receiveWorkCompleteStatus();
    }
}
