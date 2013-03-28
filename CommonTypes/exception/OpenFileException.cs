using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    namespace Exceptions
    {

        [Serializable]
        public class OpenFileException : ApplicationException
        {

            public OpenFileException(String message) : base(message) { }
            public OpenFileException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext contex)
                : base(info, contex)
            {
            }
        }
    }
}
