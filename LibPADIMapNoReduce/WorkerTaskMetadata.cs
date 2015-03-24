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
        String mapperClassName;
        String chunk;

        public WorkerTaskMetadata(byte[] code,String mapperClassName,String chunk)
        {
            this.code = code;
            this.mapperClassName = mapperClassName;
            this.chunk = chunk;
        }

        public byte[] Code
        {
            get { return code; }
            set { code = value; }
        }
        
        public String MapperClassName
        {
            get { return mapperClassName; }
            set { mapperClassName = value; }
        }
       
        public String Chunk
        {
            get { return chunk; }
            set { chunk = value; }
        }
    }
}
