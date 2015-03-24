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
        long splitCount;
        String clientUrl;

        public ClientMetadata(long totalLineCount, long splitCount, String clientUrl)
        {
            this.totalLineCount = totalLineCount;
            this.splitCount = splitCount;
            this.clientUrl = clientUrl;
        }
        public long TotalLineCount
        {
            get { return totalLineCount; }
            set { totalLineCount = value; }
        }
       
        public long SplitCount
        {
            get { return splitCount; }
            set { splitCount = value; }
        }

        public String ClientUrl
        {
            get { return clientUrl; }
            set { clientUrl = value; }
        }
    }
}
