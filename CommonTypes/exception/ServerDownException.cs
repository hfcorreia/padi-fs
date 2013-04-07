using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    namespace Exceptions
    {

        [Serializable]
        public class ServerDownException : ApplicationException
        {

            public ServerDownException(String message) : base(message) { }
            public ServerDownException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext contex)
                : base(info, contex)
            {
            }
        }
    }
}
