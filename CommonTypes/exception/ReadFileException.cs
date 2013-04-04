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
        public class ReadFileException : PadiFsException
            {
                public ReadFileException(String message) : base(message) { }
                public ReadFileException(SerializationInfo info, StreamingContext contex) : base(info, contex) { }
        }
    }
}
