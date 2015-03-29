using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    [Serializable]
   public class JobMetadata //to be sent to the jobtracker
    {
        long totalByteCount;
        long splitCount;
        String clientUrl;

        public JobMetadata(long totalByteCount, long splitCount, String clientUrl)
        {
            this.totalByteCount = totalByteCount;
            this.splitCount = splitCount;
            this.clientUrl = clientUrl;
        }
        public long TotalByteCount
        {
            get { return totalByteCount; }
            set { totalByteCount = value; }
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
