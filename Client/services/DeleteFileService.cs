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
            /*Task[] tasks = new Task[MetaInformationReader.Instance.MetaDataServers.Count];
            for (int md = 0; md < MetaInformationReader.Instance.MetaDataServers.Count; md++)
            {
                IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[md].getObject<IMetaDataServer>();
                tasks[md] = Task.Factory.StartNew(() => { metadataServer.delete(State.Id, FileName); });
            }*/
            Task[] tasks = new Task[] { deleteFileTask() };

            waitVoidQuorum(tasks, 1);

            State.FileMetadataContainer.removeFileMetadata(FileName);
            // State.fileContentContainer.removeFileContent(FileName); DEVE RETIRAR-SE O STRING REGISTER QUANDO HA UM DELETE???
        }

        private Task deleteFileTask()
        {
            return Task.Factory.StartNew(() =>
            {
                IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[0].getObject<IMetaDataServer>();
                bool found = false;
                int masterId = 0;
                while (!found)
                {
                    try
                    {
                        metadataServer = MetaInformationReader.Instance.MetaDataServers[masterId].getObject<IMetaDataServer>();
                        metadataServer.delete(State.Id, FileName);
                        found = true;
                    }
                    catch (NotMasterException exception)
                    {
                        masterId = exception.MasterId;
                    }
                }
            });
        }
    }
}
