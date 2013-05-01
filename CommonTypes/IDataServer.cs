using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

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
