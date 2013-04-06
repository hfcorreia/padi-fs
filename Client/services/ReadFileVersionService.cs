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
    class ReadFileVersionService : ClientService
    {
        private Client Client { get; set; }
        private String FileName { get; set; }
        public int FileVersion { get; set; }

        public ReadFileVersionService(ClientState clientState, String fileName) : base(clientState) 
        {
            FileName = fileName;
            FileVersion = -1;
        }

        override public void execute()
        {
            Console.WriteLine("#Client: reading async file version for file '" + FileName + "'");
            int fileVersion = 0;

            FileMetadata fileMetadata = State.fileMetadataContainer.getFileMetadata(FileName);
            Task<int>[] tasks = new Task<int>[fileMetadata.NumServers];
            for (int ds = 0; ds < fileMetadata.NumServers; ds++)
            {
                IDataServer dataServer = fileMetadata.FileServers[ds].getObject<IDataServer>();
                tasks[ds] = Task.Factory.StartNew(() => { return dataServer.readFileVersion(FileName); });
            }

            for (int i = 0; i < tasks.Length; i++)
            {
                try
                {
                    fileVersion = tasks[i].Result;
                }
                catch (AggregateException aggregateException)
                {
                    throw aggregateException.Flatten().InnerException;
                }
                Console.WriteLine("#Client: readFileVersion from server " + i + " = " + fileVersion);
            }

            FileVersion = fileVersion;
        }
    }
}
