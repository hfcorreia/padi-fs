using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public class RemoteObjectWrapper
    {
        public int Port { get; set; }
        public int Id {get; set; }
        public string URL {get; set; }

        public RemoteObjectWrapper(int port, int id, string url) 
        {
            Port = port;
            Id = id;
            URL = url;
        }
    }
}
