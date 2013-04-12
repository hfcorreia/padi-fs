using System;
using System.Collections.Generic;
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

        public ReadFileService(ClientState clientState, string semantics, int fileRegisterId)
            : base(clientState)
        {
            FileRegisterId = fileRegisterId;
            Semantics = semantics;
            ReadedFile = null;
        }

        public File waitReadQuorum(Task<File>[] tasks, int quorum)
        {
            List<Object> responses = new List<Object>();
            while (responses.Count < quorum)
            {
                responses = new List<Object>();
                for (int i = 0; i < tasks.Length; ++i)
                {
                    if (tasks[i].IsCompleted)
                    {
                        if (tasks[i].Exception == null && tasks[i].Result is File)
                        {
                            File file = (File)tasks[i].Result;
                            if (Semantics.ToLower().Equals(Util.DEFAULT_READ_SEMANTICS))
                            {
                                Console.WriteLine("#Client read - found a valid response for semantics " + Semantics);
                                responses.Add(file);
                            }
                            else if (Semantics.ToLower().Equals(Util.MONOTONIC_READ_SEMANTICS))
                            {
                                if (file.Version >= State.findMostRecentVersion(file.FileName) ||
                                    responses.Count > 0)
                                {
                                    Console.WriteLine("#Client read - found a valid response for semantics " + Semantics);
                                    responses.Add(file);
                                }
                                else
                                {
                                    Console.WriteLine("#Client read - Invalid response -> starting new task...");
                                    FileMetadata fileMetadata = State.FileMetadataContainer.getFileMetadata(FileRegisterId);
                                    tasks[i] = createAsyncTask(fileMetadata, i);
                                }
                            }
                        }
                        else if (tasks[i].Exception != null)
                        {
                            Exception exception = tasks[i].Exception.Flatten().InnerException;
                            if (exception is ReadFileException)
                            {
                                responses.Add(exception);
                            }
                            else
                            {
                                Console.WriteLine("#Client read - received an invalid execption[" + exception.Message + "]. Starting a new task... ");
                                FileMetadata fileMetadata = State.FileMetadataContainer.getFileMetadata(FileRegisterId);
                                tasks[i] = createAsyncTask(fileMetadata, i);
                            }
                        }
                    }
                }
            }

            Console.WriteLine("#Client - Received enough valid responses -> canceling the remaining tasks... \n " + responses.Count);
            closeUncompletedTasks(tasks);

            //choose the better option
            Object result = findMostRecentVersion(responses);

            if (result is ReadFileException)
            {
                throw (ReadFileException)result;
            }
            else
            {
                return (File)result;
            }
        }

        private Object findMostRecentVersion(List<Object> responses)
        {
            Object result = null;
            int moreRecentVersion = -1;
            foreach (Object obj in responses)
            {
                if (obj is File)
                {
                    CommonTypes.File file = (File)obj;
                    if (file.Version > moreRecentVersion)
                    {
                        moreRecentVersion = file.Version;
                        result = file;
                    }
                }
                if (result == null && (obj is ReadFileException))
                {
                    result = obj;
                }
            }
            return result;
        }


        override public void execute()
        {

            Console.WriteLine("#Client: reading file. fileRegister: " + FileRegisterId + ", sringRegister: " + StringRegisterId + ", semantics: " + Semantics);
            File file = null;

            FileMetadata fileMetadata = State.FileMetadataContainer.getFileMetadata(FileRegisterId);
            if (fileMetadata != null && fileMetadata.FileServers != null)
            {
                if (fileMetadata.FileServers.Count < fileMetadata.ReadQuorum)
                {
                    throw new WriteFileException("Client - trying to read in a quorum of " + fileMetadata.ReadQuorum + ", but we only have " + fileMetadata.FileServers.Count + " in the local metadata ");
                }

                Task<File>[] tasks = new Task<File>[fileMetadata.FileServers.Count];
                for (int ds = 0; ds < fileMetadata.FileServers.Count; ds++)
                {
                    tasks[ds] = createAsyncTask(fileMetadata, ds);
                }

                int readQuorum = fileMetadata.ReadQuorum;
                Console.WriteLine("#Client: waiting read quorum...");
                file = waitReadQuorum(tasks, readQuorum);

            }
            else
            {
                throw new ReadFileException("Client - Trying to read with a file-register that does not exist " + FileRegisterId);
            }

            Console.WriteLine("#Client: reading file - end - fileContentContainer: " + State.FileContentContainer.getAllFileContentAsString());

            ReadedFile = file;
        }

        private Task<File> createAsyncTask(FileMetadata fileMetadata, int ds)
        {
            IDataServer dataServer = fileMetadata.FileServers[ds].getObject<IDataServer>();
            return Task<File>.Factory.StartNew(() =>
            {
                return dataServer.read(fileMetadata.FileName);
            }
            );
        }


    }
}
