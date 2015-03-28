using PADIMapNoReduce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;

namespace Server.worker
{
    public class WorkerCommunicator
    {
        public WorkerTaskMetadata getTaskFromClient(FileSplitMetadata splitMetadata)
        {
            IClient client = (IClient)Activator.GetObject(
                typeof(IClient),
                "tcp://localhost:10000/Client");
            WorkerTaskMetadata workerMetadata = client.receiveTaskRequest(splitMetadata);
            return workerMetadata;
        }


        public void sendResultsToClient(TaskResult taskResult)
        {
            IClient client = (IClient)Activator.GetObject(
                     typeof(IClient),
                     "tcp://localhost:10000/Client");
            Boolean status = client.receiveCompletedTask(taskResult);
            Console.WriteLine("receive status at client is" + status);
        }

        public void sendStatusUpdatesToTracker(Status status)
        {
            Console.WriteLine("sending status updates " + status.PercentageCompleted);

               IWorkerTracker tracker = (IWorkerTracker)Activator.GetObject(
                          typeof(IWorkerTracker),
                          Worker.JOBTRACKER_URL);
                tracker.receiveStatus(status);
        }

        internal Dictionary<Int32,string> getExistingWorkerURLList(string entryURL)
        {
            IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entryURL);
            return worker.getExistingWorkers();
        }

        internal void notifyExistingWorkers(int workerId,String newWorkerURL,Dictionary<Int32,string> existingWorkerList)
        {
            foreach (KeyValuePair<Int32, string> entry in existingWorkerList)
            {
               IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entry.Value);
               worker.addNewWorker(entry.Key,newWorkerURL);
           }
        }

        internal void notifyTaskCompletedEvent(int workerId, int splitId)
        {
            IWorkerTracker tracker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entry.Value);
            tracker.jobCompleted(workerId, splitId);
        }

        internal void notifyResultsSentToClientEvent(int workerId, int splitId)
        {
            IWorkerTracker tracker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entry.Value);
            tracker.resultSentToClient(workerId, splitId);
        }
    }
}
