using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CommonTypes
{
    namespace Exceptions
    {
        [Serializable]
        public class ReadFileVersionException : PadiFsException
        {
            public ReadFileVersionException(String message) : base(message) { }   
            public ReadFileVersionException(SerializationInfo info, StreamingContext contex) : base(info, contex) { }
        }
    }
}
