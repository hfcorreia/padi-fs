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

    class CloseFileService : ClientService
    {
        private Client Client { get; set; }
        private String FileName { get; set; }

        public CloseFileService(ClientState clientState, String fileName)
            : base(clientState) 
        {
            FileName = fileName;
        }

        override public void execute()
        {
            Console.WriteLine("#Client: closing file " + FileName);

            Func<IMetaDataServer, Object> closeFileFunc = (IMetaDataServer metadataServer) =>
            {
                metadataServer.close(State.Id, FileName);
                return null;
            };

            Task[] tasks = new Task[] { createExecuteOnMDSTask(closeFileFunc) };

            State.FileMetadataContainer.markClosedFile(FileName);

            waitVoidQuorum(tasks, 1);

        }
    }
}
