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
    class OpenFileService : ClientService
    {
        private Client Client { get; set; }
        private String FileName { get; set; }

        public OpenFileService(ClientState clientState, String fileName)
            : base(clientState)
        {
            FileName = fileName;
        }

        override public void execute()
        {
            Console.WriteLine("#Client: opening file '" + FileName + "'");
            if (State.FileMetadataContainer.containsFileMetadata(FileName))
            {
                //if we allready have the file in the metadataContainer, we only mark it has being open
                State.FileMetadataContainer.markOpenFile(FileName);
            }

            //if we don't have metadata of the file, we go get it on the MetadataServers
            Func<IMetaDataServer, FileMetadata> openFileFunc = (IMetaDataServer metadataServer) => { return metadataServer.open(State.Id, FileName); };

            Task<FileMetadata>[] tasks = new Task<FileMetadata>[] { createExecuteOnMDSTask<FileMetadata>(openFileFunc) };

            FileMetadata fileMetadata = waitQuorum<FileMetadata>(tasks, 1);

            printReceivedDataServers(fileMetadata);

            int position = State.FileMetadataContainer.addFileMetadata(fileMetadata);
        }

        private static void printReceivedDataServers(FileMetadata fileMetadata)
        {
            Console.Write("#Client: available data servers for " + fileMetadata.FileName + " :\r\n\t[ ");
            foreach (ServerObjectWrapper server in fileMetadata.FileServers)
            {
                Console.Write(server.Id + " ");
            }
            Console.WriteLine("]");
        }

    }
}
