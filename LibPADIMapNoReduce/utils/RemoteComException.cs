#region Directive Section

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

#endregion

namespace PADIMapNoReduce
{
    [Serializable()]
    public class RemoteComException : Exception
    {
        #region Initialization

        public RemoteComException() : base() { }
        public RemoteComException(string message) : base(message) { }
        public RemoteComException(string message, System.Exception inner) : base(message, inner) { } 

        protected RemoteComException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        [SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter = true)]
        public override void  GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion
    }
}
