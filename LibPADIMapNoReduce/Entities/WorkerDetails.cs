using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    [Serializable]
    public class WorkerDetails
    {
        private int nodeid;

        public int Nodeid
        {
            get { return nodeid; }
            set { nodeid = value; }
        }

        private string nodeurl;

        public string Nodeurl
        {
            get { return nodeurl; }
            set { nodeurl = value; }
        }

        private List<int> processedSplits;

        public List<int> ProcessedSplits
        {
            get { return processedSplits; }
            set { processedSplits = value; }
        }

        private WorkerState state;

        public WorkerState State
        {
            get { return state; }
            set { state = value; }
        }


    }

    public enum WorkerState
    {
        IDLE,
        ABOUT_TO_IDLE,
        ABOUT_TO_BUSY,
        BUSY
    }
}
