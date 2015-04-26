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
using PADIMapNoReduce;
using System.Collections;
using System.Threading;


namespace Puppet_Master
{
    class PuppetService : MarshalByRefObject, IPuppetMaster
    {
        String puppetUrl;
        List<String> puppetUrlList = new List<string>();
        Dictionary<int, string> workerPuppetMap = new Dictionary<int, string>();


        Dictionary<string, int> urlIdMap = new Dictionary<string, int>();
        Dictionary<int, Worker> workerIdMap = new Dictionary<int, Worker>();

        public Dictionary<string, int> UrlIdMap
        {
            get { return urlIdMap; }
            set { urlIdMap = value; }
        } 

        public Dictionary<int, string> WorkerPuppetMap
        {
            get { return workerPuppetMap; }
            set { workerPuppetMap = value; }
        }

        public List<String> PuppetUrlList
        {
            get { return puppetUrlList; }
            set { puppetUrlList = value; }
        }
        public String PuppetUrl
        {
            get { return puppetUrl; }
            set { puppetUrl = value; }
        }
        #region interface implementation
        /*check whether u can do it in a thread*/
        public bool createLocalWorker(WorkerMetadata workerMetadata)
        {         
            Console.WriteLine("service url is " + workerMetadata.ServiceURL);
            Worker  worker = new Worker(workerMetadata.WorkerId);
            workerIdMap.Add(workerMetadata.WorkerId, worker);
          /*  new Thread(delegate()
            {
                worker.initWorker(workerMetadata);
            }).Start();*/
            worker.initWorker(workerMetadata);
            return true;
        }

        public void slowWorker(int seconds,int workerId)
        {
            Worker worker=workerIdMap[workerId];
            worker.slowWorker(seconds);
        }

        public void freezeWorker(int workerId)
        {
            Worker worker = workerIdMap[workerId];
            worker.freezeWorker();
        }

        public void unfreezeWorker(int workerId) {
            Worker worker = workerIdMap[workerId];
            worker.unfreezeWorker();
        }

        public void freezeTracker(int trackerId)
        {
            Worker worker = workerIdMap[trackerId];
            worker.freezeTracker();
        }

        public void unfreezeTracker(int trackerId)
        {
            Worker worker = workerIdMap[trackerId];
            worker.unfreezeTracker();
        }

       public void displayStatus()
        {
            foreach ( KeyValuePair<Int32, Worker> entry in workerIdMap)
            {
                new Thread(delegate()
                {
                    entry.Value.displayStatus();
                }).Start();
            }
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

        internal void callPuppetsDisplayStatus()
        {
          //  displayStatus();//display status on local node

            for (int i = 0; i < puppetUrlList.Count; i++)//including myself
            {
                IPuppetMaster puppet = (IPuppetMaster)Activator.GetObject(
                 typeof(IPuppetMaster),
               puppetUrlList[i]);
               puppet.displayStatus();//display status on remote nodes
            }
        }

        internal void callRemoteWaitWorker(int workerId,int seconds)
        {
            String puppetToConnect = workerPuppetMap[workerId];
            IPuppetMaster puppet = (IPuppetMaster)Activator.GetObject(
                typeof(IPuppetMaster),
           puppetToConnect);
           puppet.slowWorker(seconds,workerId);
        }

        internal void callRemoteFreezeWorker(int workerId){
            String puppetToConnect = workerPuppetMap[workerId];
            IPuppetMaster puppet = (IPuppetMaster)Activator.GetObject(
                typeof(IPuppetMaster),
           puppetToConnect);
            puppet.freezeWorker(workerId);
        }

       internal void callRemoteUnfreezeWorker(int workerId){
           String puppetToConnect = workerPuppetMap[workerId];
           IPuppetMaster puppet = (IPuppetMaster)Activator.GetObject(
               typeof(IPuppetMaster),
          puppetToConnect);
           puppet.unfreezeWorker(workerId);
        }

       internal void callRemoteFreezeTracker(int workerId)
       {
           String puppetToConnect = workerPuppetMap[workerId];
           IPuppetMaster puppet = (IPuppetMaster)Activator.GetObject(
               typeof(IPuppetMaster),
          puppetToConnect);
           puppet.freezeTracker(workerId);
       }

       internal void callRemoteUnfreezeTracker(int trackerId)
       {
           String puppetToConnect = workerPuppetMap[trackerId];
           IPuppetMaster puppet = (IPuppetMaster)Activator.GetObject(
               typeof(IPuppetMaster),
          puppetToConnect);
           puppet.unfreezeTracker(trackerId);
       }

        #endregion
        #region specific
        public override object InitializeLifetimeService()
        {
            return null;
        }
        #endregion

    }
}
