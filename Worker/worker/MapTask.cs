using PADIMapNoReduce;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Server.worker
{
   public class MapTask
    {
        private Status status = new Status();

        public Status Status
        {
            get { return status; }
            set { status = value; }
        }
       public IList<KeyValuePair<string, string>> result=new List<KeyValuePair<string, string>>();
       public bool runMapperForLine(byte[] code, string className,long lineNumber,String line)
       {
           Assembly assembly = Assembly.Load(code);
           // Walk through each type in the assembly looking for our class
           foreach (Type type in assembly.GetTypes())
           {
               if (type.IsClass == true)
               {
                   if (type.FullName.EndsWith("." + className))
                   {
                       // create an instance of the object
                       object ClassObj = Activator.CreateInstance(type);

                       // Dynamically Invoke the method
                       object[] args = new object[] { line};
                       object resultObject = type.InvokeMember("Map",
                         BindingFlags.Default | BindingFlags.InvokeMethod,
                              null,
                              ClassObj,
                              args);
                       result.Concat((IList<KeyValuePair<string, string>>)resultObject);
                      // result= (IList<KeyValuePair<string, string>>)resultObject;
                       Console.WriteLine("Map call result was: ");
                       foreach (KeyValuePair<string, string> p in result)
                       {
                           Console.WriteLine("key: " + p.Key + ", value: " + p.Value);
                       }
                       return true;
                   }
               }
           }
           throw (new System.Exception("could not invoke method"));
           return true;
       }

       internal TaskResult processMapTask(WorkerTaskMetadata workerTaskMetadata,FileSplitMetadata splitMetaData)
       {
           String chunk=workerTaskMetadata.Chunk;
           long lineNumber=splitMetaData.StartPosition;
           
          // using (StringReader reader = new System.IO.StringReader(chunk)) {
          // string line = reader.ReadLine();
               runMapperForLine(workerTaskMetadata.Code, workerTaskMetadata.MapperClassName,lineNumber++,"this is test");
          // }

               return null;
       }
    }
}
