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
        public FileMetadataContainer FileMetadataContainer { get; set; }
        public FileContentContainer FileContentContainer { get; set; }
        public Dictionary<string, int> FileMostRecentVersion { get; set; }


        public void saveMostRecentVersion(string filename, int version)
        {
            if (FileMostRecentVersion.ContainsKey(filename))
            {
                if (FileMostRecentVersion[filename] >= version)
                {
                    return;
                }
                else
                {
                    FileMostRecentVersion.Remove(filename);
                }
            }
            FileMostRecentVersion.Add(filename, version);
        }

        public int findMostRecentVersion(string filename)
        {
            return (FileMostRecentVersion.ContainsKey(filename)) ? FileMostRecentVersion[filename] : -1;
        }

        public ClientState(int port, String id)
        {
            Port = port;
            Id = id;
            FileMetadataContainer = new FileMetadataContainer(Int32.Parse(Properties.Resources.FILE_REGISTER_CAPACITY));
            FileContentContainer = new FileContentContainer(Int32.Parse(Properties.Resources.FILE_STRING_CAPACITY));
            FileMostRecentVersion = new Dictionary<string, int>();
        }

    }
}
