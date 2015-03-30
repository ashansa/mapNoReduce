﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
   public interface IClient
    {
        WorkerTaskMetadata receiveTaskRequest(FileSplitMetadata splitMetadata);//fill this object and return to worker
        Boolean receiveCompletedTask(TaskResult taskResult);
        void receiveJobCompletedNotification();
       
       // DECIDE WHETHER CLIENT DECIDE OR TRACKER SAY 
       //void receiveWorkCompleteStatus();//once client receive this, job tracker became a worker
    }
}
