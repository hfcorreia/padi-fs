﻿using System;
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

            Task[] tasks = new Task[MetaInformationReader.Instance.MetaDataServers.Count];
            for (int md = 0; md < MetaInformationReader.Instance.MetaDataServers.Count; md++)
            {
                IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[md].getObject<IMetaDataServer>();
                tasks[md] = Task.Factory.StartNew(() => { metadataServer.close(State.Id, FileName); });
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
    }
}
