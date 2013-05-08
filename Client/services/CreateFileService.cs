using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;
using CommonTypes;
using CommonTypes.Exceptions;

namespace Client.services
{
    class CreateFileService : ClientService
    {
        private Client Client { get; set; }
        private String FileName { get; set; }
        private int NumberOfDataServers { get; set; }
        private int ReadQuorum { get; set; }
        private int WriteQuorum { get; set; }
        public FileMetadata CreatedFileMetadata { get; set; }
        public int FileRegisterId { get; set; }

        public CreateFileService(ClientState clientState, string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
            : base(clientState)
        {
            FileName = filename;
            NumberOfDataServers = numberOfDataServers;
            ReadQuorum = readQuorum;
            WriteQuorum = writeQuorum;
        }

        override public void execute()
        {
            Console.WriteLine("#Client: creating " + FileName + "\r\n\tNº DS: " + NumberOfDataServers + " ReadQ: " + ReadQuorum + " WriteQ: " + WriteQuorum);

            Func<IMetaDataServer, FileMetadata> createFileFunc = (IMetaDataServer metadataServer) =>
            {
                return metadataServer.create(State.Id, FileName, NumberOfDataServers, ReadQuorum, WriteQuorum);
            };

            Task<FileMetadata>[] tasks = new Task<FileMetadata>[] { createExecuteOnMDSTask<FileMetadata>(createFileFunc) };

            CreatedFileMetadata = waitQuorum<FileMetadata>(tasks, 1);

            FileRegisterId = State.FileMetadataContainer.addFileMetadata(CreatedFileMetadata);

        }

    }
}
