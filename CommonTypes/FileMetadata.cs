using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    [Serializable]
    public class FileMetadata
    {
        public string FileName { get; set; }

        public int ReadQuorum { get; set; }
        public int WriteQuorum { get; set; }
        public int NumServers { get; set; }
        
        public FileMetadata() { }
        public FileMetadata(String filename, int numServers, int readQuorum, int writeQuorum)
        {
            FileName = filename;
            NumServers = numServers;
            ReadQuorum = readQuorum;
            WriteQuorum = writeQuorum;
        }
    }
}
