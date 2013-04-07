using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace DataServer
{
    class BufferedWriteRequest : BufferedRequest
    {
        public File File { get; set; }

        public BufferedWriteRequest(File file)
        {
            File = file;
        }
    }
}
