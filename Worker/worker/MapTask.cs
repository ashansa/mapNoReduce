using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Server.worker
{
   public class MapTask
    {

       public bool runMapperForLine(byte[] code, string className)
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
                       object[] args = new object[] { "testValue" };
                       object resultObject = type.InvokeMember("Map",
                         BindingFlags.Default | BindingFlags.InvokeMethod,
                              null,
                              ClassObj,
                              args);
                       IList<KeyValuePair<string, string>> result = (IList<KeyValuePair<string, string>>)resultObject;
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
    }
}
