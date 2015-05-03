using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce.Entities
{
    [Serializable]
    public class Task
    {
        int splitId;

        public int SplitId
        {
            get { return splitId; }
            set { splitId = value; }
        }
        FileSplitMetadata splitMetadata;

        public FileSplitMetadata SplitMetadata
        {
            get { return splitMetadata; }
            set { splitMetadata = value; }
        }
        StatusType statusType;

        public StatusType StatusType
        {
            get { return statusType; }
            set { statusType = value; }
        }
        int workerId;

        public int WorkerId
        {
            get { return workerId; }
            set { workerId = value; }
        }
        List<int> replicatedNodes = new List<int>();

        public List<int> ReplicatedNodes
        {
            get { return replicatedNodes; }
            set { replicatedNodes = value; }
        }
        double percentageCompleted;

        public double PercentageCompleted
        {
            get { return percentageCompleted; }
            set { percentageCompleted = value; }
        }

        public Task(int splitId, FileSplitMetadata splitMetadata, StatusType statusType)
        {
            this.splitId = splitId;
            this.splitMetadata = splitMetadata;
            this.statusType = statusType;
        }

    }


   /* public enum StatusType
    {
        INPROGRESS,
        COMPLETED,
        SUSPENDED,
        FILES_SENT,
        NOT_SEND_TO_WORKER
    }*/
}
