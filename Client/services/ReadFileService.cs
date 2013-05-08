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

        override public void execute()
        {

            Console.WriteLine("#Client: reading file.\r\n\tFileRegister: " + FileRegisterId + " StringRegister: " + StringRegisterId + " Semantics: " + Semantics);
            File file = null;

            FileMetadata fileMetadata = State.FileMetadataContainer.getFileMetadata(FileRegisterId);

            if (fileMetadata != null && fileMetadata.FileServers != null)
            {
                if (!fileMetadata.IsOpen)
                {
                    throw new ReadFileException("Client - The file " + fileMetadata.FileName + " is closed. Please open the file before reading.");
                }

                if (fileMetadata.FileServers.Count < fileMetadata.NumServers)
                {
                    updateReadFileMetadata(fileMetadata.FileName);
                    fileMetadata = State.FileMetadataContainer.getFileMetadata(fileMetadata.FileName);
                }

                Task<File>[] tasks = new Task<File>[fileMetadata.FileServers.Count];
                for (int ds = 0; ds < fileMetadata.FileServers.Count; ds++)
                {
                    tasks[ds] = createAsyncTask(fileMetadata, ds);
                }

                int readQuorum = fileMetadata.ReadQuorum;
                file = waitReadQuorum(tasks, readQuorum);
            }
            else
            {
                throw new ReadFileException("Client - Trying to read with a file-register that does not exist " + FileRegisterId);
            }

            ReadedFile = file;
        }

        private void updateReadFileMetadata(String filename)
        {

            Task<FileMetadata>[] tasks = new Task<FileMetadata>[MetaInformationReader.Instance.MetaDataServers.Count];
            for (int md = 0; md < MetaInformationReader.Instance.MetaDataServers.Count; md++)
            {
                IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[md].getObject<IMetaDataServer>();
                Console.WriteLine("#Client: Updating metadata - filename: " + filename + " metadataServer: " + md);
                tasks[md] = Task<FileMetadata>.Factory.StartNew(() => { return metadataServer.updateReadMetadata(State.Id, filename); });
            }

            FileMetadata fileMetadata = waitQuorum<FileMetadata>(tasks, 1);
            closeUncompletedTasks(tasks);
            int position = State.FileMetadataContainer.addFileMetadata(fileMetadata);
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
                                responses.Add(file);
                            }
                            else if (Semantics.ToLower().Equals(Util.MONOTONIC_READ_SEMANTICS))
                            {
                                if (file.Version >= State.findMostRecentVersion(file.FileName) ||
                                    responses.Count > 0)
                                {
                                    responses.Add(file);
                                }
                                else
                                {
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
                                FileMetadata fileMetadata = State.FileMetadataContainer.getFileMetadata(FileRegisterId);
                                tasks[i] = createAsyncTask(fileMetadata, i);
                            }
                        }
                    }
                }
            }

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
