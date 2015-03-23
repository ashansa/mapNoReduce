using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    [Serializable]
    public class TaskResult
    {
        IList<KeyValuePair<string, string>> result;
        int splitId;

        public IList<KeyValuePair<string, string>> Result
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
