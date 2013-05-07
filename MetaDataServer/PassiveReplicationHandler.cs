using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonTypes;
using System.Timers;
using CommonTypes.Exceptions;
using System.Collections.Concurrent;

namespace MetaDataServer
{
    public class PassiveReplicationHandler
    {
        public int MetadataServerId { get; set; }
        public int MasterNodeId { get; set; }
        public bool IsMaster { get { return MetadataServerId == MasterNodeId; } }
        private HashSet<int> aliveServers;

        private Timer[] NodeAliveTimers { get; set; }

        private static int NUMBER_OF_METADATA_SERVERS = Int32.Parse(Properties.Resources.NUMBER_OF_METADATA_SERVERS);
        private static double ALIVE_PERIOD = Int32.Parse(Properties.Resources.ALIVE_PERIOD);
        private static double MY_TIMER_PERIOD = ALIVE_PERIOD / 4;

        public PassiveReplicationHandler() { }
        public void init()
        {
            
            MasterNodeId = 0;

            NodeAliveTimers = new Timer[NUMBER_OF_METADATA_SERVERS];

            AliveServers = new HashSet<int>();
            for (int serverId = 0; serverId < NUMBER_OF_METADATA_SERVERS; ++serverId)
            {
                if (serverId != MetadataServerId)
                {
                    resetAliveTimer(serverId);
                }
                synchronizedAddAliveServer(serverId); //all servers are alive at the begin of life
            }

            resetMyTimer();
        }

        public PassiveReplicationHandler(int metadataServerId)
        {
            MetadataServerId = metadataServerId;
            init();
        }

        /**
         * synchOperations
         * gets all the operations in fault from another MDS
         **/ 
        public List<MetaDataOperation> synchOperations(int fromOperation)
        {
            List<MetaDataOperation> result = null;
            bool found = false;
            int masterId = (MasterNodeId + 1) % 3 ;
            while (!found)
            { // we assume that at least one MDS is allways available
                try
                {
                    if (masterId != MetadataServerId)
                    {
                        MetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[masterId].getObject<MetaDataServer>();
                        result = metadataServer.getOperationsFrom(fromOperation);
                        found = true;
                    }
                    else
                    {
                        masterId = (masterId + 1) % 3;
                    }
                }
                catch (NotMasterException exception)
                {
                    masterId = exception.MasterId;
                }
                catch (PadiFsException exception)
                {
                    throw exception;
                }
                catch (Exception)
                {
                    //consider as the server being down - try another server
                    masterId = (masterId + 1) % 3;
                }
                
            }
            return result;
        }

        public void registerNodeDie(int metadataServerId)
        {
            Console.WriteLine("#MD " + "ReplicationHandler - node - " + metadataServerId + "died");

            synchronizedRemoveAliveServer(metadataServerId);
            NodeAliveTimers[metadataServerId].Stop();
            electMaster();
            /*
            string aliveServersString = "#MD alive servers: [ ";
            foreach (int id in AliveServers)
            {
                aliveServersString += id + ", ";
            }
            aliveServersString += " ]";
            Console.WriteLine(aliveServersString);*/
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
                Console.WriteLine("#MD " + "the server " + metadataServerId + " has reborn");
                //is a reborn of a node that could be the one with the smallest id
                synchronizedAddAliveServer(metadataServerId);
                electMaster();
            }
        }

        public void sendAliveMessage(MetaDataServerAliveMessage aliveMessage)
        {
            foreach (int slaveId in AliveServers)
            {
                int nodeId = slaveId;
                Task.Factory.StartNew(() =>
                {
                    
                    if (nodeId != MetadataServerId)
                    {
                        MetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[nodeId].getObject<MetaDataServer>();
                        try
                        {
                            metadataServer.receiveAliveMessage(aliveMessage);
                        }
                        //catch (PadiFsException exception)
                        //{
                        //    throw exception;
                        //}
                        catch (Exception)
                        {
                            //Console.WriteLine("#MDS " + MetadataServerId + " - error sending alive message to server " + nodeId);
                            //registerNodeDie(nodeId);
                        }
                    }
                });
            }
            resetMyTimer();
        }

        public void syncOperation(MetaDataOperation operation)
        {
            if (IsMaster)
            {
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
                NodeAliveTimers[nodeId].Elapsed += (sender, args) => { int deadNode = nodeId; registerNodeDie(deadNode); };
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

        public void synchronizedAddAliveServer(int serverId)
        {
            HashSet<int> tmp = new HashSet<int>(AliveServers);
            tmp.Add(serverId);
            AliveServers = tmp;
        }

        public void synchronizedRemoveAliveServer(int serverId)
        {
            HashSet<int> tmp = new HashSet<int>(AliveServers);
            if (tmp.Contains(serverId))
            {
                tmp.Remove(serverId);
                AliveServers = tmp;
            }
        }

        public HashSet<int> AliveServers
        {
            get
            {
                HashSet<int> tmp = null;
                lock (this)
                {
                    tmp = new HashSet<int>(aliveServers);
                }
                return tmp;
            }
            set
            {
                lock (this)
                {
                    aliveServers = value;
                }
            }
        }
    }
    
}
