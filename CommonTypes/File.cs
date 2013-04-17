using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    [Serializable]
    public class File
    {
        public string FileName { get; set; }
        public int Version { get; set; }
        public byte[] Content { get; set; }
        public string clientId { get; set; }

        public File() { }
        public File(string fileName, int version, byte[] content) 
        {
            FileName = fileName;
            Version = version;
            Content = content;
        }
        public File(File file)
        {
            FileName = file.FileName;
            Version = file.Version;
            Content = file.Content;
        }

        public override string ToString() {
            return "File: " + FileName + " version: " + Version + "Content: " + Content;
        }

    }

}
