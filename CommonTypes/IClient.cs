using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public interface IClient
    {
        void write(File file);

        void write(string filename, byte[] content);

        void read(string filename);
        
        void open(string filename);

        void close(string filename);

        void delete(string filename);

        void create(string filename, int numberOfServers, int readQuorum, int writeQuorum);

        void exit();
    }
}
