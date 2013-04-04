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
            public class CreateFileException : PadiFsException
            {

                public CreateFileException(String message) : base(message) { }
                public CreateFileException(SerializationInfo info, StreamingContext contex) : base(info, contex) { }

          
        }
    }
}
