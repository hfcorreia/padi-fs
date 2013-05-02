using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Timers;

namespace MetaDataServer
{
    public class PassiveReplicationHandler
    {
        public int MetadataServerId { get; set; }
        public int MasterNodeId { get; set; }
        public bool IsMaster { get { return MetadataServerId == MasterNodeId; } }
        public HashSet<int> AliveServers { get; set; }
        private Timer[] NodeAliveTimers { get; set; }

        private static int NUMBER_OF_METADATA_SERVERS = Int32.Parse(Properties.Resources.NUMBER_OF_METADATA_SERVERS);
        private static double ALIVE_PERIOD = Int32.Parse(Properties.Resources.ALIVE_PERIOD);
        private static double MY_TIMER_PERIOD = ALIVE_PERIOD / 4;

        public PassiveReplicationHandler(int metadataServerId)
        {
            MetadataServerId = metadataServerId;
            MasterNodeId = 0;

            NodeAliveTimers = new Timer[NUMBER_OF_METADATA_SERVERS];

            AliveServers = new HashSet<int>();
            for (int serverId = 0; serverId < NUMBER_OF_METADATA_SERVERS; ++serverId)
            {
                if (serverId != MetadataServerId)
                {
                    resetAliveTimer(serverId);
                }
                AliveServers.Add(serverId); //all servers are alive at the begin of life
            }

            resetMyTimer();
            
        }   

        public void registerNodeDie(int metadataServerId)
        {
            Console.WriteLine("#MD " + "ReplicationHandler - node - " + metadataServerId + "died");
            AliveServers.Remove(metadataServerId);
            NodeAliveTimers[metadataServerId].Stop();
            electMaster();
        }

        public void electMaster()
        {
            MasterNodeId = AliveServers.Min();
            Console.WriteLine("#MD " + "ReplicationHandler - The new master is the node - " + MasterNodeId);
        }

        public void registerAliveMessage(int metadataServerId)
        {
            resetAliveTimer(metadataServerId);
            if (!AliveServers.Contains(metadataServerId))
            {
                AliveServers.Add(metadataServerId);
                electMaster();
            }
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
                        try
                        {
                            metadataServer.receiveAliveMessage(aliveMessage);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("#MDS " + MetadataServerId + " - sendAliveMessage - server " + slaveId + " is down.");
                        }
                    });
                }
            }
            resetMyTimer();
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

        public void resetAliveTimer(int nodeId)
        {
            if (NodeAliveTimers[nodeId] == null)
            {
                NodeAliveTimers[nodeId] = new Timer();
                NodeAliveTimers[nodeId].Interval = ALIVE_PERIOD;
                NodeAliveTimers[nodeId].Elapsed += (sender, args) => registerNodeDie(nodeId);
                NodeAliveTimers[nodeId].Enabled = true;
                NodeAliveTimers[nodeId].Start();
            }
            else
            {
                NodeAliveTimers[nodeId].Stop();
                NodeAliveTimers[nodeId].Start();
            }
        }

        public void resetMyTimer()
        {
            if (NodeAliveTimers[MetadataServerId] == null)
            {
                NodeAliveTimers[MetadataServerId] = new Timer();
                NodeAliveTimers[MetadataServerId].Interval = MY_TIMER_PERIOD;
                NodeAliveTimers[MetadataServerId].Elapsed += (sender, args) => sendAliveMessage(new MetaDataServerAliveMessage(MetadataServerId, IsMaster));
                NodeAliveTimers[MetadataServerId].Enabled = true;
                NodeAliveTimers[MetadataServerId].Start();
            } else {
                NodeAliveTimers[MetadataServerId].Stop();
                NodeAliveTimers[MetadataServerId].Start();
            }
        }
    }
    
}
