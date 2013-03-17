using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public interface IMetaDataServer
    {
        void open(String filename);
        void close(String filename);
        void delete(string filename);
    }
}
