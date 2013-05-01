using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CommonTypes {

    namespace Exceptions
    {
        [Serializable]
        public class NotMasterException : PadiFsException
        {
            public int MasterId { get; set; }
            public NotMasterException(String message, int masterId) : base(message) { MasterId = masterId; }
            public NotMasterException(SerializationInfo info, StreamingContext contex) : base(info, contex) { }
        }
    }
}
