using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace MetaDataServer
{
    [Serializable]
    public class FileInfo
    {
        public FileMetadata Metadata { get; set; }
        public List<ServerObjectWrapper> DataServers { get; set; }
        public int NumberOfClients { get; set; }

        public FileInfo(FileMetadata metadata, List<ServerObjectWrapper> dataServers)
        {
            Metadata = metadata;
            DataServers = dataServers;
            NumberOfClients = 0;
        }
    }

}
