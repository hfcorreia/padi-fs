using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataServer
{
    class BufferedReadVersionRequest : BufferedRequest
    {
        public string Filename { get; set; }

        public BufferedReadVersionRequest(string filename) 
        {
            Filename = filename;
        }
    }
}
