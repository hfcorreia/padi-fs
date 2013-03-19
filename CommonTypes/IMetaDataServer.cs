using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public interface IMetaDataServer
    {
        void create(string filename);
        void open(string filename);
        void close(string filename);
        void delete(string filename);

        void registDataServer(int id, string url);
        void exit();

    }
}
