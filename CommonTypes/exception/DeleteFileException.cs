using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace CommonTypes {

    namespace Exceptions
    {
        [Serializable]
        public class DeleteFileException : PadiFsException
        {

            public DeleteFileException(String message) : base(message) { }
            public DeleteFileException(SerializationInfo info, StreamingContext contex) : base(info, contex) { }
        }
    }
}
