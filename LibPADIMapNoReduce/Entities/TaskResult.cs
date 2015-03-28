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
        int splitId;

        public TaskResult(byte[] result, int splitId)
        {
            this.result=result;
            this.splitId = splitId;
        }

        public byte[] Result
        {
            get { return result; }
            set { result = value; }
        }
        
        public int SplitId
        {
            get { return splitId; }
            set { splitId = value; }
        }
    }
}
