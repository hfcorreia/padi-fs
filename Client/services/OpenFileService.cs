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
    class OpenFileService : ClientService
    {
        private Client Client { get; set; }
        private String FileName { get; set; }

        public OpenFileService(ClientState clientState, String fileName) : base(clientState) 
        {
            FileName = fileName;
        }

        override public void execute()
        {
            Console.WriteLine("#Client: opening file '" + FileName + "'");
            if (State.FileMetadataContainer.containsFileMetadata(FileName))
            {
                //if we allready have the file in the metadataContainer, we only mark it has being open
                State.FileMetadataContainer.markOpenFile(FileName);
            }
            else
            {
                //if we don't have metadata of the file, we go get it on the MetadataServers
                /*Task<FileMetadata>[] tasks = new Task<FileMetadata>[MetaInformationReader.Instance.MetaDataServers.Count];
                for (int md = 0; md < MetaInformationReader.Instance.MetaDataServers.Count; md++)
                {
                    IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[md].getObject<IMetaDataServer>();
                    tasks[md] = Task<FileMetadata>.Factory.StartNew(() => { return metadataServer.open(State.Id, FileName); });
                }*/

                Task<FileMetadata>[] tasks = new Task<FileMetadata>[] { openFileTask() };

                FileMetadata fileMetadata = waitQuorum<FileMetadata>(tasks, 1);

                int position = State.FileMetadataContainer.addFileMetadata(fileMetadata);
                Console.WriteLine("#Client: metadata saved in position " + position);
            }
        }

        private Task<FileMetadata> openFileTask()
        {
            return Task<FileMetadata>.Factory.StartNew(() =>
            {
                IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[0].getObject<IMetaDataServer>();
                FileMetadata result = null;
                bool found = false;
                int masterId = 0;
                while (!found)
                {
                    try
                    {
                        metadataServer = MetaInformationReader.Instance.MetaDataServers[masterId].getObject<IMetaDataServer>();
                        result = metadataServer.open(State.Id, FileName);
                        found = true;
                    }
                    catch (NotMasterException exception)
                    {
                        masterId = exception.MasterId;
                    }
                }
                return result;
            });
        }
    }
}
