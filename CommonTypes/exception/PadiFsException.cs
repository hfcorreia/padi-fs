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
        public class PadiFsException : ApplicationException
        {

            public PadiFsException(String message) : base(message) { }
            public PadiFsException(SerializationInfo info, StreamingContext contex) : base(info, contex) { }
        }
    }
}
