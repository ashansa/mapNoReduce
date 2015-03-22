using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    [Serializable]
   public class ClientMetadata //to be sent to the jobtracker
    {
        long totalLineCount;

        public long TotalLineCount
        {
            get { return totalLineCount; }
            set { totalLineCount = value; }
        }
        long splitCount;

        public long SplitCount
        {
            get { return splitCount; }
            set { splitCount = value; }
        }
        String clientUrl;

        public String ClientUrl
        {
            get { return clientUrl; }
            set { clientUrl = value; }
        }
    }
}
