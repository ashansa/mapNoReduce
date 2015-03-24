using PADIMapNoReduce;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace LibMapper {
    public class Mapper : IMapper {

        public IList Map(string fileLine)
        {
          
            Key<string> k = new Key<string>(fileLine);
            Value<int> v = new Value<int>(100);
            IList result = new List<KeyValuePair<Key<string>, Value<int>>>();
            KeyValuePair<Key<string>, Value<int>> pair = new KeyValuePair<Key<string>, Value<int>>(k, v);

            result.Add(pair);
            return result;
        }
    }
}
