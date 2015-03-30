using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIMapNoReduce.Interfaces;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Configuration;
using System.Runtime.Remoting.Channels.Tcp;


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
            String puppetURL = ConfigurationManager.AppSettings[puppetId].ToString();
            int puppetPort = Int32.Parse(puppetURL.Split(':')[1]);
            TcpChannel channel = new TcpChannel(puppetPort);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this, "Puppet", typeof(PuppetService));
        }
        #endregion
    }
}
