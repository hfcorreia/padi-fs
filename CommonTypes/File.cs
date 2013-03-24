using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public class File
    {
        public string Name { get; set; }
        public Int32 Version { get; set; }
        public byte[] Content { get; set; }

        public File() { }

        public File(String name, Int32 verion, Byte[] content) {
            Name = name;
            Version = verion;
            Content = content;
        }

    }

}
