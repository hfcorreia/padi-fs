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
    class DeleteFileService : ClientService
    {
        private Client Client { get; set; }
        private String FileName { get; set; }

        public DeleteFileService(ClientState clientState, String fileName)
            : base(clientState)
        {
            FileName = fileName;
        }

        override public void execute()
        {
            Console.WriteLine("#Client: Deleting file " + FileName);

            Func<IMetaDataServer, Object> deleteFileFunc = (IMetaDataServer metadataServer) =>
            {
                metadataServer.delete(State.Id, FileName);
                return null;
            };

            Task[] tasks = new Task[] { createExecuteOnMDSTask(deleteFileFunc) };

            waitVoidQuorum(tasks, 1);

            State.FileMetadataContainer.removeFileMetadata(FileName);
        }
    }
}
