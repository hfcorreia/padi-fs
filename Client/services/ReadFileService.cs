using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;
using CommonTypes;
using System.Threading.Tasks;
using CommonTypes.Exceptions;

namespace Client.services
{
    class ReadFileService : ClientService
    {
        private Client Client { get; set; }

        private int FileRegisterId { get; set; }
        private string Semantics { get; set; }
        private int StringRegisterId { get; set; }
        public File ReadedFile { get; set; }

        public ReadFileService(ClientState clientState, string semantics, int fileRegisterId, int stringRegisterId) : base(clientState) 
        {
            FileRegisterId = fileRegisterId;
            Semantics = semantics;
            StringRegisterId = stringRegisterId;
            ReadedFile = null;
        }

        override public void execute()
        {
            Console.WriteLine("#Client: reading file. fileRegister: " + FileRegisterId + ", sringRegister: " + StringRegisterId + ", semantics: " + Semantics);
            File file = null;
            FileMetadata fileMetadata = State.fileMetadataContainer.getFileMetadata(FileRegisterId);
            if (fileMetadata != null && fileMetadata.FileServers != null)
            {
                Task<File>[] tasks = new Task<File>[fileMetadata.NumServers];
                for (int ds = 0; ds < fileMetadata.NumServers; ds++)
                {
                    IDataServer dataServer = fileMetadata.FileServers[ds].getObject<IDataServer>();
                    tasks[ds] = Task.Factory.StartNew(() => { return dataServer.read(fileMetadata.FileName); });
                }

                int readQuorum = fileMetadata.ReadQuorum;

                file = waitQuorum<File>(tasks, readQuorum);

            }
            else
            {
                throw new ReadFileException("Client - Trying to read with a file-register that does not exist " + FileRegisterId);
            }

            State.fileContentContainer.setFileContent(StringRegisterId, file);
            Console.WriteLine("#Client: reading file - end - fileContentContainer: " + State.fileContentContainer.getAllFileContentAsString());

            ReadedFile = file;
        }
    }
}
