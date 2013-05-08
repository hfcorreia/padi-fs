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
    public class MetaDataMigrateOperation : MetaDataOperation
    {

        public String ReceiverId { get; set; }
        public String Filename { get; set; }
        public String SourceId { get; set; }

        public MetaDataMigrateOperation() { }
        public MetaDataMigrateOperation(String sourceId, String receiverId, String filename)
        {
            SourceId = sourceId;
            ReceiverId = receiverId;
            Filename = filename;
        }

        public override string ToString()
        {
            return "Migrate Operation";
        }
        public override void execute(MetaDataServer md)
        {

            FileMetadata metadata = md.FileMetadata[Filename];
            ServerObjectWrapper sourceDataServer = md.DataServers[SourceId];
            ServerObjectWrapper receiverDataServer = md.DataServers[ReceiverId];

            lock (Filename)
            {
                
                if (canRemove(md) && !metadata.IsOpen)
                {
                    metadata.FileServers.Remove(sourceDataServer);
                }

                md.registerMigratingFile(Filename, SourceId, ReceiverId);

                metadata.FileServers.Add(receiverDataServer);

                Console.WriteLine("#MDS: migrating file: " + Filename + "from [ DS " + sourceDataServer.Id + " ] to [ DS " + receiverDataServer.Id + " ]");
                string servers = "";
                foreach(ServerObjectWrapper server in metadata.FileServers){
                    servers += server.Id + "  ";
                }
                 Console.WriteLine("#MDS: file " + Filename + " is in servers: " + servers);
            }
        }

        private Boolean canRemove(MetaDataServer md)
        {
            FileMetadata metadata = md.FileMetadata[Filename];

            int numActualServers = metadata.FileServers.Count;

            int numMigratingServers = md.getMigratingFiles()[Filename].Count;
            int quorumMax = Math.Max(metadata.WriteQuorum, metadata.ReadQuorum);
            int maxMigrating = metadata.FileServers.Count - quorumMax - numMigratingServers;

            Console.WriteLine("canMigrate [ numActualServers: " + numActualServers + ", numMigratingServers: " + numMigratingServers + ", quorumMax: " + quorumMax + ", maxMigrating: " + maxMigrating + "] - can? " + (maxMigrating > 0));
            return maxMigrating > 0;
        }
    }
}
