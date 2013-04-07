using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public interface IDataServer : IRemote
    {
        void write(File file);

        File read(string filename);

        int readFileVersion(string filename);
        void exit();

        void fail();
        void recover();
        void freeze();
        void unfreeze();
    }
}
