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
        public class CloseFileException : PadiFsException
        {
            public CloseFileException(String message) : base(message) { }
            public CloseFileException(SerializationInfo info, StreamingContext contex) : base(info, contex) { }
        }
    }
}
