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

        List<FileSplitMetadata> splitMetadataList = new List<FileSplitMetadata>();
        List<TaskResult> taskResultList = new List<TaskResult>();
        Thread splitProcessor;
        Thread resultSender;
        MapTask mapTask = new MapTask();

        public MapTask getMapTask()//this is the currently runing map task
        {
            return mapTask;
        }

        public void startWorkerThreads()
        {
            splitProcessor = new Thread(new ThreadStart(processSplits));
            splitProcessor.Start();

            resultSender = new Thread(new ThreadStart(sendResults));
            resultSender.Start();
        }

        private void sendResults()
        {
            WorkerCommunicator workerTask = new WorkerCommunicator();
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
                workerTask.sendResultsToClient(taskResult);
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
                WorkerCommunicator workerTask = new WorkerCommunicator();
                WorkerTaskMetadata workerTaskMetadata = workerTask.getTaskFromClient(fileSplitMetadata);
                mapTask.SplitId = fileSplitMetadata.SplitId;
                TaskResult taskResult = mapTask.processMapTask(workerTaskMetadata, fileSplitMetadata);
                addTaskToTaskList(taskResult);
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


        private void addTaskToTaskList(TaskResult taskResult)
        {
            lock (taskResultList)
            {
                taskResultList.Add(taskResult);
                Monitor.Pulse(taskResultList);
            }
        }


        public void addSplitToSplitList(FileSplitMetadata splitMetadata)
        {
            lock (splitMetadataList)
            {
                splitMetadataList.Add(splitMetadata);
                Monitor.Pulse(splitMetadataList);
            }
        }

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
