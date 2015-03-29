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
        int startPosition;
        int endPosition;
        String clientUrl;//workers need this to connect to client

      public  FileSplitMetadata(int splitId, long startPosition, long endPosition, String clientUrl)
        {
            this.splitId = splitId;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.clientUrl = clientUrl;
        }
        public int SplitId
        {
            get { return splitId; }
            set { splitId = value; }
        }
        
        public int StartPosition
        {
            get { return startPosition; }
            set { startPosition = value; }
        }
       
        public int EndPosition
        {
            get { return endPosition; }
            set { endPosition = value; }
        }

        public String ClientUrl
        {
            get { return clientUrl; }
            set { clientUrl = value; }
        }

        string jobTrackerUrl;//we need this so that workers can connect back to send status updates

        public string JobTrackerUrl
        {
            get { return jobTrackerUrl; }
            set { jobTrackerUrl = value; }
        }

    }
}
