using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using System.Threading.Tasks;

namespace MetaDataServer
{
    public class PassiveReplicationHandler
    {
        public int MetadataServerId { get; set; }
        public bool IsMaster { get; set; }
        public HashSet<int> AliveServers { get; set; }

        //TODOOOOO
        //Timer[3] -> timers que controlam o tempo que passou desde a ultima vez que
        //recebeu uma mensagem de i'm alive de um dado metadataserver.
        ////quando o timer do metadataserver 'id' despara -> registerDieMessage(id)
        ////quando chega uma mensage de i'm alive do metadataServer 'id' -> registerAliveMessage(id) 

        public PassiveReplicationHandler(int metadataServerId)
        {
            MetadataServerId = metadataServerId;
            IsMaster = (MetadataServerId == 0);         //WARNING !!!!!!! THIS IS JUST FOR DEBUG!!!!! <<<------
            AliveServers = new HashSet<int>();
            AliveServers.Add(0);
            AliveServers.Add(1);
            AliveServers.Add(2);
        }

        public void registerDieMessage(int metadataServerId)
        {
            AliveServers.Remove(metadataServerId);
        }

        public void registerAliveMessage(int metadataServerId)
        {
            AliveServers.Add(metadataServerId);
        }

        public void sendAliveMessage(MetaDataServerAliveMessage aliveMessage)
        {
            foreach (int slaveId in AliveServers)
            {
                if (slaveId != MetadataServerId)
                {
                    MetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[slaveId].getObject<MetaDataServer>();

                    Task.Factory.StartNew(() =>
                    {
                        Console.WriteLine("#MD " + "ReplicationHandler - sendAliveMessage - " + aliveMessage);
                        try
                        {
                            metadataServer.receiveAliveMessage(aliveMessage);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message + "\n" + e.StackTrace);
                            Console.WriteLine("#MDS " + MetadataServerId + " - sendAliveMessage - server " + slaveId + "is down.");
                        }
                    });
                }
            }
        }

        public void syncOperation(MetaDataOperation operation)
        {
            
            if (IsMaster)
            {
                Console.WriteLine("#MD " + "ReplicationHandler - syncOperation - " + operation);

                List<MetaDataOperation> operations = new List<MetaDataOperation>();
                operations.Add(operation);
                sendAliveMessage(new MetaDataServerAliveMessage(MetadataServerId, IsMaster, operations));
            }
        }
    }
}
