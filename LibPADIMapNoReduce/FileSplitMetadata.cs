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

        public int SplitId
        {
            get { return splitId; }
            set { splitId = value; }
        }
        long startPosition;

        public long StartPosition
        {
            get { return startPosition; }
            set { startPosition = value; }
        }
        long endPosition;

        public long EndPosition
        {
            get { return endPosition; }
            set { endPosition = value; }
        }
        String clientUrl;//workers need this to connect to client

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
