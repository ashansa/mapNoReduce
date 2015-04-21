using PADIMapNoReduce;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace LibMapper {
    public class Mapper : IMapper {
        static int lineNum = 0;

        public Mapper(){
            Console.WriteLine(lineNum);
        }
        public IList<KeyValuePair<string, string>> Map(string fileLine)
        {
            lineNum++;
            IList<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            result.Add(new KeyValuePair<string, string>("testKey1", fileLine));
            result.Add(new KeyValuePair<string, string>("testKey2", fileLine));
            return result;
        }
    }
}
