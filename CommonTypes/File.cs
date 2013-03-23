using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    [Serializable]
    public class File
    {
        private string Name { get; set; }
        private Int32 Version { get; set; }
        private byte[] Content { get; set; }

        public File() { }

    }
}
