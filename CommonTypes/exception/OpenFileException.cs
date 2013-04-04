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
        public class OpenFileException : PadiFsException
        {

            public OpenFileException(String message) : base(message) { }
            public OpenFileException(SerializationInfo info, StreamingContext contex) : base(info, contex) { }
        }
    }
}
