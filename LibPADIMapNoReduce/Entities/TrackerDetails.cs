using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce.Entities
{
    /*hold all the details tracked by task tracker*/
   [Serializable]
   public class TrackerDetails
    {
        String clientURL;
        String trackerURL;
        List<int> completedSplits = new List<int>();
        List<int> submittedLists = new List<int>();
        List<int> allSplits = new List<int>();
        Dictionary<Int32, String> existingWorkerMap = new Dictionary<Int32, string>();//workerId,url
        Dictionary<Int32, Status> mapTaskDetails = new Dictionary<Int32, Status>();//splitId,status
        List<FileSplitMetadata> fileSplitData = new List<FileSplitMetadata>();

        public Dictionary<Int32, Status> MapTaskDetails
        {
            get { return mapTaskDetails; }
            set { mapTaskDetails = value; }
        }
        public TrackerDetails(string clientURL, Dictionary<Int32, String> workerMap)
        {
            this.clientURL = clientURL;
            this.existingWorkerMap = workerMap;
        }

        public String ClientURL
        {
            get { return clientURL; }
            set { clientURL = value; }
        }    

        public Dictionary<Int32, String> ExistingWorkerMap
        {
            get { return existingWorkerMap; }
            set { existingWorkerMap = value; }
        }
        public void addTaskToDetailsList(short splitId, Status status)
        {
            mapTaskDetails.Add(splitId, status);
        }

        public List<FileSplitMetadata> FileSplitData
        {
            get { return fileSplitData; }
        }

        public void addFileSplit(FileSplitMetadata splitData)
        {
            fileSplitData.Add(splitData);
        }

        public List<int> resultSentToClientSplits
        {
            get { return completedSplits; }
        }

        public List<int> SubmittedSplits
        {
            get { return submittedLists; }
        }
        public List<int> AllSplits
        {
            get { return allSplits; }
        }
    }
}
