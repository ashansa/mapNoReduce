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
        private IClient clientProxy = null;
        private IWorkerTracker trackerProxy = null;

        public WorkerTaskMetadata getTaskFromClient(FileSplitMetadata splitMetadata)
        {
            if (clientProxy == null)
            {
                clientProxy = (IClient)Activator.GetObject(
                    typeof(IClient),
                    Worker.CLIENT_URL);
            }
            WorkerTaskMetadata workerMetadata = clientProxy.receiveTaskRequest(splitMetadata);
            return workerMetadata;
        }


        public void sendResultsToClient(TaskResult taskResult)
        {
            if (clientProxy == null)
            {
                clientProxy = (IClient)Activator.GetObject(
                    typeof(IClient),
                    Worker.CLIENT_URL);
            }
            Boolean status = clientProxy.receiveCompletedTask(taskResult);
            // Console.WriteLine("receive status at client is" + status);
        }

        public void sendStatusUpdatesToTracker(Status status)
        {
            try
            {
                if (trackerProxy == null)
                {
                   trackerProxy = (IWorkerTracker)Activator.GetObject(
                               typeof(IWorkerTracker),
                               Worker.JOBTRACKER_URL);
                }
                trackerProxy.receiveStatus(status);
            }
            catch (Exception ex)
            {
                Common.Logger().LogError("unable to send status updates by " + status.NodeId + " for percentage " + status.PercentageCompleted, string.Empty, string.Empty);
            }
        }

        internal Dictionary<Int32, WorkerDetails> getExistingWorkerURLList(string entryURL)
        {
            IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entryURL);
            return worker.getExistingWorkers();
        }

        internal void notifyExistingWorkers(int workerId, String newWorkerURL, Dictionary<Int32, WorkerDetails> existingWorkerList)
        {
            foreach (KeyValuePair<Int32, WorkerDetails> entry in existingWorkerList)
            {
                IWorkerTracker worker = (IWorkerTracker)Activator.GetObject(typeof(IWorkerTracker), entry.Value.Nodeurl);
                worker.addNewWorker(workerId, newWorkerURL);
            }
        }


        internal void notifyTaskCompletedEvent(int workerId, int splitId)
        {
            try
            {
                Common.Logger().LogInfo("Notifying task completed event for split "+splitId, string.Empty, string.Empty);
                if (trackerProxy == null)
                {
                    trackerProxy = (IWorkerTracker)Activator.GetObject(
                                typeof(IWorkerTracker),
                                Worker.JOBTRACKER_URL);
                }
                trackerProxy.taskCompleted(workerId, splitId);
            }
            catch (Exception ex)
            {
                Common.Logger().LogInfo("exception thrown while sending copleted event ", string.Empty, string.Empty);
            }
        }

        /*important event so if not send add to list back*/
        internal void notifyResultsSentToClientEvent(int workerId, TaskResult taskResult, WorkerTask workerTask)
        {
            try
            {
                if (trackerProxy == null)
                {
                    trackerProxy = (IWorkerTracker)Activator.GetObject(
                               typeof(IWorkerTracker),
                               Worker.JOBTRACKER_URL);
                }
                trackerProxy.taskCompleted(workerId, taskResult.SplitId);
            }
            catch (Exception ex)
            {
                workerTask.addTaskToTaskResults(taskResult);
                Common.Logger().LogInfo("exception thrown while sending copleted event ", string.Empty, string.Empty);
                Common.Logger().LogInfo(ex.Message, string.Empty, string.Empty);
            }
        }

        internal void hasThresholdReached(int nodeId)
        {
            if (trackerProxy == null)
            {
                trackerProxy = (IWorkerTracker)Activator.GetObject(
                            typeof(IWorkerTracker),
                            Worker.JOBTRACKER_URL);
            }
            trackerProxy.readyForNewTask(nodeId);
        }
    }
}
