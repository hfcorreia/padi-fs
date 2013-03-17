using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    class File
    {
        private string name;
        private string id;
        private byte[] content;

        public File() { }

        public string Name 
        { 
            get { return  name; } 
            set { this.name = value; }
        }



    }
}
