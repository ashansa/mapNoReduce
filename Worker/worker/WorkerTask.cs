using PADIMapNoReduce;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Server.worker
{
    public class WorkerTask
    {
        int workerId;
        List<FileSplitMetadata> splitMetadataList = new List<FileSplitMetadata>();
        List<TaskResult> taskResultList = new List<TaskResult>();
        Thread splitProcessor;
        Thread resultSender;
        Thread statusUpdateNotificationThread;
        MapTask mapTask = new MapTask();

        public WorkerTask(int workerId)
        {
           this.workerId = workerId;
        }

        public int WorkerId
        {
            get { return workerId; }
            set { workerId = value; }
        }

        public MapTask getMapTask()//this is the currently runing map task
        {
            return mapTask;
        }

        public void startWorkerThreads()
        {
            try
            {
                splitProcessor = new Thread(new ThreadStart(processSplits));
                splitProcessor.Start();

                resultSender = new Thread(new ThreadStart(sendResults));
                resultSender.Start();

                statusUpdateNotificationThread = new Thread(new ThreadStart(sendStatusUpdates));
                statusUpdateNotificationThread.Start();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
        }

        public void stopWorkerThreads()
        {
            if (splitProcessor != null)
            {
                splitProcessor.Abort();
                splitProcessor = null;
            }

            if (resultSender != null)
            {
                resultSender.Abort();
                resultSender = null;
            }

            if (statusUpdateNotificationThread != null)
            {
                statusUpdateNotificationThread.Abort();
                statusUpdateNotificationThread = null;
            }
            
        }

        private void sendResults()
        {
            WorkerCommunicator communicator = new WorkerCommunicator();
            TaskResult taskResult;
            while (true)
            {
                lock (taskResultList)
                {
                    if (taskResultList.Count == 0)
                    {
                        Monitor.Wait(taskResultList);
                        continue;
                    }
                    taskResult = taskResultList[0];
                    taskResultList.RemoveAt(0);
                }
                communicator.sendResultsToClient(taskResult);
                communicator.notifyResultsSentToClientEvent(workerId,taskResult.SplitId);

            }
        }

        private void processSplits()
        {
            FileSplitMetadata fileSplitMetadata = null;
            while (true)
            {
                lock (splitMetadataList)
                {
                    if (splitMetadataList.Count > 0)
                    {
                        fileSplitMetadata = splitMetadataList[0];
                        splitMetadataList.RemoveAt(0);
                    }
                    else
                    {
                        Monitor.Wait(splitMetadataList);
                        continue;
                    }
                }
                WorkerCommunicator communicator = new WorkerCommunicator();
                WorkerTaskMetadata workerTaskMetadata = communicator.getTaskFromClient(fileSplitMetadata);
                mapTask.SplitId = fileSplitMetadata.SplitId;
                TaskResult taskResult = mapTask.processMapTask(workerTaskMetadata, fileSplitMetadata);
                communicator.notifyTaskCompletedEvent(workerId,fileSplitMetadata.SplitId);
                addTaskToTaskResults(taskResult);
            }
        }
        private void sendStatusUpdates()
        {
            while (true)
            {
                lock (mapTask.Status)
                {
                    Monitor.Wait(mapTask.Status);
                }
                WorkerCommunicator communicator = new WorkerCommunicator();
                communicator.sendStatusUpdatesToTracker(mapTask.Status);
            }
        }


        private void addTaskToTaskResults(TaskResult taskResult)
        {
            lock (taskResultList)
            {
                taskResultList.Add(taskResult);

                if (taskResultList.Count == 1)
                    Monitor.Pulse(taskResultList);
            }
        }


        public void addSplitToSplitList(FileSplitMetadata splitMetadata)
        {
            lock (splitMetadataList)
            {
                splitMetadataList.Add(splitMetadata);

                if (splitMetadataList.Count == 1)
                    Monitor.Pulse(splitMetadataList);
            }
        }

        /*suspend the current map or suspend the map which has not yet started but in split list*/
        public bool suspendOrRemoveMapTask(int splitId)
        {
            if (mapTask.SplitId == splitId)
            {
                mapTask.IsMapSuspended = true;
                return true;
            }
            else
            {
                lock (splitMetadataList)
                {
                    if (splitMetadataList.Count > 0)
                    {
                        for (int i = 0; i < splitMetadataList.Count; i++)
                        {
                            if (splitMetadataList[i].SplitId == splitId)
                            {
                                splitMetadataList.RemoveAt(i);
                                return true;
                            }
                        }
                    }
                }

            }
            return false;
        }
    }
}
