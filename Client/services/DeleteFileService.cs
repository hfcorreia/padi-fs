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
/*
        override public void execute()
        {
            Console.WriteLine("#Client: Deleting file " + FileName);
            if (State.fileMetadataContainer.containsFileMetadata(FileName))
            {
                Task[] tasks = new Task[MetaInformationReader.Instance.MetaDataServers.Count];
                for (int md = 0; md < MetaInformationReader.Instance.MetaDataServers.Count; md++)
                {
                    IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[md].getObject<IMetaDataServer>();
                    tasks[md] = Task.Factory.StartNew(() => { metadataServer.delete(State.Id, FileName); });
                }

                State.fileMetadataContainer.removeFileMetadata(FileName);
                State.fileContentContainer.removeFileContent(FileName);
                try
                {
                    Task.WaitAll(tasks);
                }
                catch (AggregateException aggregateException)
                {
                    throw aggregateException.Flatten().InnerException;
                }
            }
            else
            {
                throw new DeleteFileException("Trying to delete a file that is not in the file-register.");
            }
        }
 * */
        override public void execute()
        {
            Console.WriteLine("#Client: Deleting file " + FileName);
            if (State.fileMetadataContainer.containsFileMetadata(FileName))
            {
                Task[] tasks = new Task[MetaInformationReader.Instance.MetaDataServers.Count];
                for (int md = 0; md < MetaInformationReader.Instance.MetaDataServers.Count; md++)
                {
                    IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[md].getObject<IMetaDataServer>();
                    tasks[md] = Task.Factory.StartNew(() => { metadataServer.delete(State.Id, FileName); });
                }

                State.fileMetadataContainer.removeFileMetadata(FileName);
                State.fileContentContainer.removeFileContent(FileName);

                waitVoidQuorum(tasks, tasks.Length);
            }
            else
            {
                throw new DeleteFileException("Trying to delete a file that is not in the file-register.");
            }
        }
    }
}
