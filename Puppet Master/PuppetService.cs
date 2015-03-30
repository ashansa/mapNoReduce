using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIMapNoReduce.Interfaces;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;


namespace Puppet_Master
{
    class PuppetService : MarshalByRefObject, IPuppetMaster
    {
        #region interface implementation
        public bool createWorker(PADIMapNoReduce.WorkerMetadata workerMetadata)
        {

        }
        #endregion


        #region puppet initialization
        public void initPuppet(string puppetId)
        {

        }
        #endregion
    }
}
