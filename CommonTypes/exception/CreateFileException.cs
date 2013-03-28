using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    namespace Exceptions
    {
      
            [Serializable]
            public class CreateFileException : ApplicationException
            {

                public CreateFileException(String message) : base(message) { }
                public CreateFileException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext contex)
                    : base(info, contex)
                {
                }

          
        }
    }
}
