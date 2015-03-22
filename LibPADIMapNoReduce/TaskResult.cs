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

        public IList<KeyValuePair<string, string>> Result
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
