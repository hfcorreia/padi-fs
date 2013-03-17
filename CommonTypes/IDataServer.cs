using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public interface IDataServer
    {
        void write(File file);
        File read(string filename);
    }
}
