using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    namespace Exceptions
    {

        [Serializable]
        public class CloseFileException : ApplicationException
        {

            public CloseFileException(String message) : base(message) { }
            public CloseFileException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext contex)
                : base(info, contex)
            {
            }
        }
    }
}
