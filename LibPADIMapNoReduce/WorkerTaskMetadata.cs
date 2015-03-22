using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PADIMapNoReduce
{
    /*when worker calls TaskRequest on Client, client create a channel and invoke execute map
     * on worker by passing this object*/
    [Serializable]
   public class WorkerTaskMetadata
    {
        byte[] code;

        public byte[] Code
        {
            get { return code; }
            set { code = value; }
        }
        String mapperClassName;

        public String MapperClassName
        {
            get { return mapperClassName; }
            set { mapperClassName = value; }
        }
        String chunk;

        public String Chunk
        {
            get { return chunk; }
            set { chunk = value; }
        }
    }
}
