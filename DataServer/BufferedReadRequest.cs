using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataServer
{
    class BufferedReadRequest : BufferedRequest
    {
        public string Filename { get; set; }

        public BufferedReadRequest(string filename) 
        {
            Filename = filename;
        }
    }
}
