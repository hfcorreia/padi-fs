using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public interface IClient : IRemote
    {


        void write(int fileRegisterId, int stringRegisterId);

        void write(int fileRegisterId, byte[] content);

        File read(int fileRegisterId, string semantics, int stringRegisterId);

        File read(int fileRegisterId, string semantics);

        void open(string filename);

        void close(string filename);

        void delete(string filename);

        void exeScript(string filename);

        FileMetadata create(string filename, int numberOfServers, int readQuorum, int writeQuorum);

        List<string> getAllStringRegisters();

        List<string> getAllFileRegisters();

        void copy(int sourceFileRegisterId, string semantics, int targetFileRegisterId, string salt);
    }
}
