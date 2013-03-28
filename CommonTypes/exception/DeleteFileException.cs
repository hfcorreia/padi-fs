using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes {

    namespace Exceptions
    {
        [Serializable]
        public class DeleteFileException : ApplicationException
        {

            public DeleteFileException(String message) : base(message) { }
            public DeleteFileException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext contex)
                : base(info, contex)
            {
            }
        }
    }
}
