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
            public class WriteFileException : PadiFsException
            {
                public WriteFileException(String message) : base(message) { }
                public WriteFileException(SerializationInfo info, StreamingContext contex) : base(info, contex) { }
        }
    }
}
