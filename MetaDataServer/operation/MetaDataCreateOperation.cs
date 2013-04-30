using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes.Exceptions;
using CommonTypes;
using System.Threading;

namespace MetaDataServer
{
    class MetaDataCreateOperation : MetaDataOperation
    {

        String ClientID {get;set;}
        string Filename {get;set;}
        int NumberOfDataServers {get;set;}
        int ReadQuorum {get;set;}
        int WriteQuorum { get; set; }

        public FileMetadata Result { get; set; }


        public MetaDataCreateOperation(String clientID, string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            ClientID = clientID;
            Filename = filename;
            NumberOfDataServers = numberOfDataServers;
            ReadQuorum = readQuorum;
            WriteQuorum = writeQuorum;
        }

         public override void execute(MetaDataServer md)
        {
            if ((WriteQuorum > NumberOfDataServers) || (ReadQuorum > NumberOfDataServers))
            {
                throw new CreateFileException("Invalid quorums values in create " + Filename);
            }

            if (md.FileMetadata.ContainsKey(Filename))
            {
                throw new CreateFileException("#MDS.create - The file " + Filename + " already exists ");
            }

            List<ServerObjectWrapper> newFileDataServers = getFirstServers(md, NumberOfDataServers);

            FileMetadata newFileMetadata = new FileMetadata(Filename, NumberOfDataServers, ReadQuorum, WriteQuorum, newFileDataServers);

            md.FileMetadata.Add(Filename, newFileMetadata);
            md.FileMetadataLocks.Add(Filename, new ManualResetEvent(false));
            Console.WriteLine("#MDS: Created " + Filename);
            md.makeCheckpoint();

            Result = md.FileMetadata[Filename];
        }

         private List<ServerObjectWrapper> getFirstServers(MetaDataServer md, int numDataServers)
         {
             List<ServerObjectWrapper> firstDataServers = new List<ServerObjectWrapper>();

             foreach (ServerObjectWrapper dataserverWrapper in md.DataServers.Values)
             {
                 if (firstDataServers.Count < numDataServers)
                 {
                     firstDataServers.Add(new ServerObjectWrapper(dataserverWrapper));
                 }
             }
             return firstDataServers;
         }


         
    }
}
