using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace Client
{
    public class ClientState
    {
        public int Port { get; set; }
        public String Id { get; set; }
        public String Url { get { return "tcp://localhost:" + Port + "/" + Id; } }
        public FileMetadataContainer fileMetadataContainer { get; set; }
        public FileContentContainer fileContentContainer { get; set; }

        public ClientState(int port, String id) 
        {
            Port = port;
            Id = id;
            fileMetadataContainer = new FileMetadataContainer(Int32.Parse(Properties.Resources.FILE_REGISTER_CAPACITY));
            fileContentContainer = new FileContentContainer(Int32.Parse(Properties.Resources.FILE_STRING_CAPACITY));
        }

    }
}
