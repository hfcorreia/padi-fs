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
            
            Task<FileMetadata>[] tasks = new Task<FileMetadata>[MetaInformationReader.Instance.MetaDataServers.Count];
            for (int md = 0; md < MetaInformationReader.Instance.MetaDataServers.Count; md++)
            {
                IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[md].getObject<IMetaDataServer>();
                tasks[md] = Task<FileMetadata>.Factory.StartNew(() => { return metadataServer.open(State.Id, FileName); });
            }

            FileMetadata fileMetadata = waitQuorum<FileMetadata>(tasks, 1);

            int postion = State.fileMetadataContainer.addFileMetadata(fileMetadata);

            Console.WriteLine("#Client: metadata saved in position " + postion);
        }
    }
}
