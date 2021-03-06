﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes.Exceptions;
using CommonTypes;
using System.Threading;

namespace MetaDataServer
{
    [Serializable]
    public class MetaDataCreateOperation : MetaDataOperation
    {

        public String ClientID {get;set;}
        public string Filename {get;set;}
        public int NumberOfDataServers {get;set;}
        public int ReadQuorum {get;set;}
        public int WriteQuorum { get; set; }

        public FileMetadata Result { get; set; }

        public MetaDataCreateOperation() { }
        public MetaDataCreateOperation(String clientID, string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            ClientID = clientID;
            Filename = filename;
            NumberOfDataServers = numberOfDataServers;
            ReadQuorum = readQuorum;
            WriteQuorum = writeQuorum;
        }

        public override string ToString()
        {
            return "Create Operation";
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

           // List<ServerObjectWrapper> newFileDataServers = getFirstServers(md, NumberOfDataServers);
            List<ServerObjectWrapper> newFileDataServers = md.getSortedServers(NumberOfDataServers);

            FileMetadata newFileMetadata = new FileMetadata(Filename, NumberOfDataServers, ReadQuorum, WriteQuorum, newFileDataServers);

            md.FileMetadata.Add(Filename, newFileMetadata);
            md.addMetadataLock(Filename, new ManualResetEvent(false));
            md.getMigratingFiles().Add(Filename, new List<Tuple<string, string>>());
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
