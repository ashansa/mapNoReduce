using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    [Serializable]
    public class WorkerMetadata
    {
        int workerId;

        public int WorkerId
        {
            get { return workerId; }
            set { workerId = value; }
        }
        string serviceURL;

        public string ServiceURL
        {
            get { return serviceURL; }
            set { serviceURL = value; }
        }
        string entryURL;

        public string EntryURL
        {
            get { return entryURL; }
            set { entryURL = value; }
        }
        string puppetRUL;

        public string PuppetRUL
        {
            get { return puppetRUL; }
            set { puppetRUL = value; }
        }
    }
}
