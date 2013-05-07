using System;
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
            List<ServerObjectWrapper> newFileDataServers = getSortedServers(md, NumberOfDataServers);

            FileMetadata newFileMetadata = new FileMetadata(Filename, NumberOfDataServers, ReadQuorum, WriteQuorum, newFileDataServers);

            md.FileMetadata.Add(Filename, newFileMetadata);
            md.addMetadataLock(Filename, new ManualResetEvent(false));
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

         private List<ServerObjectWrapper> getSortedServers(MetaDataServer md, int numDataServers)
         {
             List<ServerObjectWrapper> servers = new List<ServerObjectWrapper>();
             List<ListElem> serversWeight = new List<ListElem>();

             foreach (ServerObjectWrapper dataserverWrapper in md.DataServers.Values)
             {
                    serversWeight.Add(new ListElem(new ServerObjectWrapper(dataserverWrapper), md.calculateServerWeight(dataserverWrapper.Id)));
             }

             serversWeight = serversWeight.OrderBy(q => q.Weight).ToList();

             foreach (ListElem elem in serversWeight)
             {
                 if (servers.Count < numDataServers)
                 {
                     servers.Add(elem.Server);
                 }
             }

             return servers;
         }

         private class ListElem
         {
             public ServerObjectWrapper Server {get;set;}
             public double Weight { get; set; }

             public ListElem(ServerObjectWrapper server, double weight) {
                 Server = server;
                 Weight = weight;
             }
         }


         
    }
}
