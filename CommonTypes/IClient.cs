using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public interface IClient
    {
        //void write(File file);

        void write(string filename, byte[] content);

        File read(string filename);
        
        void open(string filename);

        void close(string filename);

        void delete(string filename);

        void exeScript(string filename);

        FileMetadata create(string filename, int numberOfServers, int readQuorum, int writeQuorum);

        void exit();

        List<string> getAllStringRegisters();

        List<string> getAllFileRegisters();
    }
}
