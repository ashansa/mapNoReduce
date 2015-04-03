﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PADIMapNoReduce.Interfaces;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Configuration;
using System.Runtime.Remoting.Channels.Tcp;
using PADIMapNoReduce;
using System.Collections;
using System.Threading;


namespace Puppet_Master
{
    class PuppetService : MarshalByRefObject, IPuppetMaster
    {
        String puppetUrl;

        public String PuppetUrl
        {
            get { return puppetUrl; }
            set { puppetUrl = value; }
        }
        #region interface implementation
        public bool createLocalWorker(WorkerMetadata workerMetadata)
        {
         
            new Thread(delegate()
            {
                Worker worker = new Worker(workerMetadata.WorkerId);
                worker.initWorker(workerMetadata);
            }).Start();
           // worker.initWorker(workerMetadata);
            return true;
        }
        #endregion


        #region locat-puppet
        public void initPuppet(string puppetURL)
        {
            puppetUrl = puppetURL;
            String[] portNamePair=puppetURL.Split(Constants.COLON_STR)[2].Split(Constants.SEP_PIPE);
            int port = Convert.ToInt32(portNamePair[0]);
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();

            IDictionary props = new Hashtable();
            props["name"] = "puppet";
            props["port"] = Convert.ToInt32(portNamePair[0]);
           // props["timeout"] = 100000; // in milliseconds
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this, portNamePair[1], typeof(PuppetService));
        }

        internal void createWorker(WorkerMetadata workerMetadata)
        {
            String puppetToConnect = workerMetadata.PuppetRUL;
            if(puppetToConnect.ToLower().Equals(puppetUrl.ToLower())){//worker can be started through local puppet
                createLocalWorker(workerMetadata);
            }
            else{//worker started through remote puppet
            IPuppetMaster puppet = (IPuppetMaster)Activator.GetObject(
                    typeof(IPuppetMaster),
                  puppetToConnect);
            puppet.createLocalWorker(workerMetadata);
            }
        }
        #endregion
    }
}