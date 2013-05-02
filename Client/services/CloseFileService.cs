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

            //Task[] tasks = new Task[] { createCloseFileTask() };
            
            Func<IMetaDataServer, Object> closeFileFunc = (IMetaDataServer metadataServer) =>
            {
                metadataServer.close(State.Id, FileName);
                return null;
            };

            Task[] tasks = new Task[] { createExecuteOnMDSTask(closeFileFunc) };

            //Task<FileMetadata>[] tasks = new Task<FileMetadata>[] { createExecuteOnMDSTask<FileMetadata>(deleteFileFunc) };

            State.FileMetadataContainer.markClosedFile(FileName);

            waitVoidQuorum(tasks, 1);

        }
        /*
        private Task closeFileTask()
        {
            return Task.Factory.StartNew(() =>
            {
                IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[1].getObject<IMetaDataServer>();
                bool found = false;
                while (!found)
                {
                    try
                    {
                        metadataServer.close(State.Id, FileName);
                        found = true;
                    }
                    catch (NotMasterException exception)
                    {
                        metadataServer = MetaInformationReader.Instance.MetaDataServers[exception.MasterId].getObject<IMetaDataServer>();
                    }
                }
            });
        }

        private Task createCloseFileTask()
        {
            return Task.Factory.StartNew(() => {
                IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[0].getObject<IMetaDataServer>();
                bool found = false;
                int masterId = 0;
                while (!found)
                {
                    try
                    {
                        metadataServer = MetaInformationReader.Instance.MetaDataServers[masterId].getObject<IMetaDataServer>();
                        metadataServer.close(State.Id, FileName);
                        found = true;
                    }
                    catch (NotMasterException exception)
                    {
                        masterId = exception.MasterId;
                    }
                    catch (Exception exception)
                    {
                        //consider as the server being down - try another server
                        masterId = (masterId + 1) % 3;
                    }
                }
            });
        }
         */ 
    }
}
