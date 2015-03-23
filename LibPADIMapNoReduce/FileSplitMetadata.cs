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
        String clientUrl;//workers need this to connect to client

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
    }
}
