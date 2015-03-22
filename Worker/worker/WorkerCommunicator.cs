using PADIMapNoReduce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;

namespace Server.worker
{
    public class WorkerCommunicator
    {
        public WorkerTaskMetadata getTaskFromClient(FileSplitMetadata splitMetadata)
        {
           // TcpChannel channel = new TcpChannel();
            //ChannelServices.RegisterChannel(channel, true);
            IClient client = (IClient)Activator.GetObject(
                typeof(IClient),
                "tcp://localhost:10000/Client");
            WorkerTaskMetadata workerMetadata = client.receiveTaskRequest(splitMetadata);
            return workerMetadata;
        }


        public void sendResultsToClient(TaskResult taskResult)
        {
            IClient client = (IClient)Activator.GetObject(
                     typeof(IClient),
                     "tcp://localhost:10000/Client");
            Boolean status = client.receiveCompletedTask(taskResult);
            Console.WriteLine("receive status at client is" + status);
        }
    }
}
