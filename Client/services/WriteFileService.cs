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

        public WriteFileService(ClientState clientState, File file)
            : base(clientState)
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

            FileMetadata fileMetadata = State.fileMetadataContainer.getFileMetadata(NewFile.FileName);
            if (fileMetadata.FileServers.Count < fileMetadata.WriteQuorum)
            {
                throw new WriteFileException("Client - trying to write in a quorum of " + fileMetadata.WriteQuorum + ", but we only have " + fileMetadata.FileServers.Count + " in the local metadata ");
            }

            Console.WriteLine("#Client: writing file '" + NewFile.FileName + "' with content: '" + NewFile.Content + "', as string: " + System.Text.Encoding.UTF8.GetString(NewFile.Content));

            Task[] tasks = new Task[fileMetadata.FileServers.Count];
            for (int ds = 0; ds < fileMetadata.FileServers.Count; ds++)
            {
                tasks[ds] = createAsyncWriteTask(fileMetadata, ds);
            }

            waitWriteQuorum(tasks, fileMetadata.WriteQuorum);

        }

        public void waitWriteQuorum(Task[] tasks, int quorum)
        {
            int responsesCounter = 0;
            while (responsesCounter < quorum)
            {
                responsesCounter = 0;
                for (int i = 0; i < tasks.Length; ++i)
                {
                    if(tasks[i].IsCompleted){

                        if (tasks[i].Exception != null)
                        {
                            //in case the write gives an error we resend the message until we get a quorum
                            FileMetadata fileMetadata = State.fileMetadataContainer.getFileMetadata(NewFile.FileName);
                            tasks[i] = createAsyncWriteTask(fileMetadata, i);
                        }
                        else {
                            responsesCounter++;
                        }
                    }
                }
            }

            foreach (Task task in tasks)
            {
                if (!task.IsCompleted)
                {
                    Util.IgnoreExceptions(task);
                }
            }

        }

        private Task createAsyncWriteTask(FileMetadata fileMetadata, int ds)
        {
            IDataServer dataServer = fileMetadata.FileServers[ds].getObject<IDataServer>();
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    dataServer.write(NewFile);
                }
                catch (Exception) { }
            });
        
        }
        
    }
}
