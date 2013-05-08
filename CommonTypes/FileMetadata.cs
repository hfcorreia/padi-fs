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
        public List<ServerObjectWrapper> FileServers { get; set; }
        public List<String> Clients { get; set; }
        public bool IsOpen { get; set; }

        public FileMetadata() { }
        public FileMetadata(String filename, int numServers, int readQuorum, int writeQuorum, List<ServerObjectWrapper> fileServers)
        {
            FileName = filename;
            NumServers = numServers;
            ReadQuorum = readQuorum;
            WriteQuorum = writeQuorum;
            FileServers = fileServers;
            Clients = new List<string>();
            IsOpen = false;
        }

        public override string ToString()
        {
            return "File: " + FileName + "Nº ds: " + NumServers+ " ReadQ: " + ReadQuorum + " WriteQ: " + WriteQuorum + " Nº available ds: " + FileServers.Count + " Nº Clients: " + Clients.Count;
        }

        
    }
}
