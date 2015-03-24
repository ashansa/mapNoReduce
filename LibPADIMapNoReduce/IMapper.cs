using System.Collections;
using System.Collections.Generic;

namespace PADIMapNoReduce {
    public interface IMapper
    {
        IList Map(string fileLine);
    }

   public class Key<T>
    {
        private T keydata;

        public T Keydata
        {
            get { return keydata; }
            set { keydata = value; }
        }

        public Key(T key)
        {
            this.keydata = key;
        }


    }
    public class Value<V>
    {
        private V valuedata;

        public V Valuedata
        {
            get { return valuedata; }
            set { valuedata = value; }
        }

        public Value(V value)
        {
            this.valuedata = value;
        }
    }
}
