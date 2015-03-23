using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    [Serializable]
    public class TaskResult
    {
        byte[] result;

        public byte[] Result
        {
            get { return result; }
            set { result = value; }
        }
        int splitId;
      
        public int SplitId
        {
            get { return splitId; }
            set { splitId = value; }
        }
    }
}
