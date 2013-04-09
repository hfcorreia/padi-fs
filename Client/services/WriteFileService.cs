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
    class WriteFileService : ClientService
    {
        public File NewFile { get; set; }

        public WriteFileService(ClientState clientState, File file) : base(clientState) 
        {
            NewFile = file;
        }

        override public void execute()
        {
            if (NewFile == null || NewFile.Content == null || NewFile.FileName == null)
            {
                throw new WriteFileException("Client - trying to write null file " + NewFile);
            }

            if (!State.fileMetadataContainer.containsFileMetadata(NewFile.FileName))
            {
                throw new WriteFileException("Client - tryng to write a file that is not open");
            }

            Console.WriteLine("#Client: writing file '" + NewFile.FileName + "' with content: '" + NewFile.Content + "', as string: " + System.Text.Encoding.UTF8.GetString(NewFile.Content));

            FileMetadata fileMetadata = State.fileMetadataContainer.getFileMetadata(NewFile.FileName);
            Task[] tasks = new Task[fileMetadata.FileServers.Count];
            for (int ds = 0; ds < fileMetadata.FileServers.Count; ds++)
            {
                IDataServer dataServer = fileMetadata.FileServers[ds].getObject<IDataServer>();
                tasks[ds] = Task.Factory.StartNew(() => { dataServer.write(NewFile); });
            }

            //State.fileContentContainer.addFileContent(NewFile);

            int writeQuorum = fileMetadata.WriteQuorum;

            waitVoidQuorum(tasks, writeQuorum);

        }
    }
}
