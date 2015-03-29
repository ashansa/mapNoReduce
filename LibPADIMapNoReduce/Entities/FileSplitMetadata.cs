using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    [Serializable]
    public class FileSplitMetadata//to be sent to Worker by job tracker
    {
        int splitId;
        long startPosition;
        long endPosition;
        string clientUrl;//workers need this to connect to client
        string jobTrackerUrl;//we need this so that workers can connect back to send status updates

      public  FileSplitMetadata(int splitId, long startPosition, long endPosition, string clientUrl, string jobTrackerUrl)
        {
            this.splitId = splitId;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.clientUrl = clientUrl;
            this.jobTrackerUrl = jobTrackerUrl;
        }
        public int SplitId
        {
            get { return splitId; }
            set { splitId = value; }
        }
        
        public long StartPosition
        {
            get { return startPosition; }
            set { startPosition = value; }
        }
       
        public long EndPosition
        {
            get { return endPosition; }
            set { endPosition = value; }
        }

        public String ClientUrl
        {
            get { return clientUrl; }
            set { clientUrl = value; }
        }

        public string JobTrackerUrl
        {
            get { return jobTrackerUrl; }
            set { jobTrackerUrl = value; }
        }

    }
}
