using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;
using System.Collections;
using CommonTypes.Exceptions;
using System.Threading;
using System.Timers;
using System.Diagnostics;
using CommonTypes;

namespace MetaDataServer
{
    [Serializable]
    public class MetaDataServer : MarshalByRefObject, IMetaDataServer
    {
        public int Port { get; set; }
        public String Id { get; set; }
        public string Url { get { return "tcp://localhost:" + Port + "/" + Id; } }

        public MetaDataLog Log { get; set; }
        private bool isFailing;
        
        public Dictionary<String, ServerObjectWrapper> DataServers { get; set; }
        public SerializableDictionary<String, FileMetadata> FileMetadata { get; set; }
        public SerializableDictionary<String, ManualResetEvent> FileMetadataLocks { get; set; }
        public SerializableDictionary<String, HeartbeatMessage> Heartbeats { get; set; }
        public PassiveReplicationHandler ReplicationHandler { get; set; }
        private int CheckpointCounter;

        /**
        * Implementation of all the initilization stuff
        **/

        #region Initilization


        static void Main(string[] args)
        {
            Console.SetWindowSize(80, 15);
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: port metadataServerId");
                Console.ReadLine();
            }
            else
            {
                MetaDataServer metadataServer = new MetaDataServer();
                metadataServer.initialize(Int32.Parse(args[0]), args[1]);
                Util.createDir(CommonTypes.Properties.Resources.TEMP_DIR);
                metadataServer.startConnection(metadataServer);

                Console.WriteLine("#MDS: Registered " + metadataServer.Id + " at " + metadataServer.Url);
                Console.ReadLine();
            }
        }

        public void startConnection(MetaDataServer metadataServer)
        {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = Port;
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            RemotingServices.Marshal(metadataServer, Id, typeof(MetaDataServer));
        }

        public void initialize(int port, String id)
        {
            Port = port;
            Id = id;
            FileMetadata = new SerializableDictionary<String, FileMetadata>();
            FileMetadataLocks = new SerializableDictionary<string, ManualResetEvent>();
            DataServers = new Dictionary<String, ServerObjectWrapper>(); // <serverID, DataServerWrapper>
            Heartbeats = new SerializableDictionary<string, HeartbeatMessage>();
            Log = new MetaDataLog();
			Log.init(this);
            isFailing = false;

            Console.Title = "MDS " + Id;

            ReplicationHandler = new PassiveReplicationHandler(IdAsNumber);

            //atach a debugger - we should add some parameter to enable/disable this!
            if (Boolean.Parse(Properties.Resources.RUN_IN_DEBUG_MODE) && !Debugger.IsAttached)
            {
                Debugger.Launch();
            }
        }

        #endregion Initilization

        /**
         * Operations that change the state of the Metadata
         **/

        #region OperationsThatChangeTheState

        public void registDataServer(String dataserverId, string dataserverHost, int dataserverPort)
        {
            executeOperation(new MetaDataRegisterServerOperation(dataserverId, dataserverHost, dataserverPort));
        }

        public FileMetadata open(String clientID, string filename)
        {
            MetaDataOpenOperation openOperation = new MetaDataOpenOperation(clientID, filename);
            executeOperation(openOperation);
            return openOperation.Result;
        }

        public void close(String clientID, string filename)
        {
            executeOperation(new MetaDataCloseOperation(clientID, filename));
        }

        public void delete(string clientId, string filename)
        {
            executeOperation(new MetaDataDeleteOperation(clientId, filename));
        }

        public FileMetadata create(String clientID, string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            
            MetaDataCreateOperation createOperation = new MetaDataCreateOperation(clientID, filename, numberOfDataServers, readQuorum, writeQuorum);
            executeOperation(createOperation);

            executeOperation(new MetaDataOpenOperation(clientID, filename));

            return createOperation.Result;
        }

        public void addServerToUnbalancedFiles(string id)
        {
            foreach (String fileName in FileMetadata.Keys)
            {
                FileMetadata metadata = FileMetadata[fileName];
                if (metadata.FileServers.Count < metadata.NumServers)
                {
                    metadata.FileServers.Add(DataServers[id]);
                    FileMetadataLocks[fileName].Set();
                }
            }

        }

        private void executeOperation(MetaDataOperation operation)
        {
            if (ReplicationHandler.IsMaster)
            {
                safeExecuteOperation(operation);
            }
            else
            {
                Console.WriteLine("#MDS " + Id + " [SLAVE] - " + " executeOperation " + operation);
                int masterId = ReplicationHandler.MasterNodeId;
                throw new NotMasterException("please execute the operation on the master: " + masterId, masterId);
            }
        }

        private void safeExecuteOperation(MetaDataOperation operation)
        {
            if (isFailing)
            {
                throw new Exception("the mds " + Id + " is failing");
            }
            Console.WriteLine("#MDS " + Id + (ReplicationHandler.IsMaster ? " [MASTER] - " : " [SLAVE] ") + " executeOperation " + operation);
            Log.registerOperation(this, operation);
            operation.execute(this);
            Log.incrementStatus();
        }

        #endregion OperationsThatChangeTheState
        
        /**
        * Operations that don't change the state of the metadata
        **/

        #region OperationsThatDontChangeState

        public int getMasterId()
        {
            if (isFailing)
            {
                throw new Exception("the mds " + Id + " is failing");
            }
            return ReplicationHandler.MetadataServerId;
        }

        public FileMetadata updateReadMetadata(string clientId, string filename)
        {
            if (isFailing)
            {
                throw new Exception("the mds " + Id + " is failing");
            }
            while (FileMetadata[filename].FileServers.Count < FileMetadata[filename].ReadQuorum)
            {
                FileMetadataLocks[filename].WaitOne();
            }

            return FileMetadata[filename];
        }

        public FileMetadata updateWriteMetadata(string clientId, string filename)
        {
            if (isFailing)
            {
                throw new Exception("the mds " + Id + " is failing");
            }
            while (FileMetadata[filename].FileServers.Count < FileMetadata[filename].WriteQuorum)
            {
                FileMetadataLocks[filename].WaitOne();
            }

            return FileMetadata[filename];
        }

        public void receiveAliveMessage(MetaDataServerAliveMessage aliveMessage)
        {
            if (isFailing)
            {
                throw new Exception("the mds " + Id + " is failing");
            }
            try
            {
                ReplicationHandler.registerAliveMessage(aliveMessage.MetadataServerId);
            }
            catch (Exception)
            {
                throw;
            }
            if (aliveMessage.IsMaster && aliveMessage.Operations!=null)
            {
                foreach (MetaDataOperation operation in aliveMessage.Operations)
                {
                    safeExecuteOperation(operation);
                }
            }
        }

        #endregion OperationsThatDontChangeState
        
        /**
         * implementation of all the fail and recover operations
         **/

        #region FailAndRecover

        public void fail()
        {
            Console.WriteLine("Fail...");
            isFailing = true;
        }

        public void recover()
        {
            if (isFailing)
            {
                Console.WriteLine("Recovering...");
                List<MetaDataOperation> operations = ReplicationHandler.synchOperations(Log.Status);
                Console.WriteLine("MDS recover - the server has " + operations.Count + " operations in fault ");
                Log.registerOperations(this, operations);
                while (Log.Status < Log.NextId)
                {
                    Log.getOperation(Log.Status).execute(this);
                    Log.incrementStatus();
                }
                isFailing = false;
                Console.WriteLine("Recover - good morning =D");
            }
            else
            {
                Console.WriteLine("MDS is already alive");
            }
        }

        public void recoverFromLog()
        {
            while (Log.Status < Log.NextId)
            {
                safeExecuteOperation(Log.getOperation(Log.Status));
            }
        }

        public List<MetaDataOperation> getOperationsFrom(int status)
        {
            return Log.getOperationsFrom(status);
        }

        #endregion FailAndRecover

        /**
         * CheckPoint opperations
         **/

        #region Checkpoint
        
        public void makeCheckpoint()
        {
            //lock (typeof(MetaDataServer))
            //{

            //    String metadataServerId = Id;
            //    Console.WriteLine("#MDS: making checkpoint " + CheckpointCounter++ + " from server " + Id);

            //    string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\" + metadataServerId;
            //    Util.createDir(dirName);

            //    System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(MetaDataServer));

            //    System.IO.StreamWriter fileWriter = new System.IO.StreamWriter(@dirName + "\\checkpoint.xml");
            //    writer.Serialize(fileWriter, this);

            //    fileWriter.Close();

            //    Console.WriteLine("#MDS: checkpoint " + CheckpointCounter + " from metaDataServer " + Id + " done");
            //}
        }

        public static MetaDataServer getCheckpoint(String metadataServerId)
        {
            //System.Xml.Serialization.XmlSerializer reader =
            //          new System.Xml.Serialization.XmlSerializer(typeof(MetaDataServer));

            //string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\" + metadataServerId + "\\checkpoint.xml";
            //System.IO.StreamReader fileReader = new System.IO.StreamReader(dirName);

            //MetaDataServer metadaServer = new MetaDataServer();
            //metadaServer = (MetaDataServer)reader.Deserialize(fileReader);

            //return metadaServer;
            return null;
        }

        #endregion Checkpoint
        
        /**
         * Auxiliar code
         **/
        
        #region otherCode
        public void dump()
        {
            Console.WriteLine("#MDS: Dumping!\r\n");
            Console.WriteLine(" URL: " + Url);
            Console.WriteLine(" Registered Data Servers:");
            foreach (KeyValuePair<String, ServerObjectWrapper> dataServer in DataServers)
            {
                Console.WriteLine("\t" + dataServer.Key);
            }

            Console.WriteLine(" Opened Files: ");
            foreach (KeyValuePair<String, FileMetadata> files in FileMetadata)
            {
                if (files.Value.IsOpen)
                {
                    Console.Write("\t" + files.Key + " - Clients: ");
                    foreach (String name in files.Value.Clients)
                    {
                        Console.WriteLine("\t" + name + " ");
                    }
                    Console.WriteLine("nb " + files.Value.NumServers + " rq " + files.Value.ReadQuorum + " wq" + files.Value.WriteQuorum);
                }
            }

            Console.WriteLine();

        }

        public int IdAsNumber
        {
            get
            {
                String[] parsedId = Id.Split('-');
                return Int32.Parse(parsedId[1]);
            }
        }

        public void exit()
        {
            Console.WriteLine("#MDS: Exiting!");
            System.Environment.Exit(0);
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }


        public void receiveHeartbeat(HeartbeatMessage heartbeat)
        {
            Console.WriteLine("#MD: Heartbeat received: " + heartbeat.ToString());
            Heartbeats[heartbeat.ServerId] = heartbeat;

            //debug
            //foreach (KeyValuePair<String, HeartbeatMessage> serverHeartbeat in Heartbeats)
            //{
            //    Console.WriteLine(serverHeartbeat.ToString());
            //}
             
        }


        public int calculateServerWeight(String id)
        {
            HeartbeatMessage heartbeat = Heartbeats[id];
            return heartbeat.ReadCounter + heartbeat.ReadVersionCounter + heartbeat.WriteCounter;
        }

		#endregion otherCode
    }

}
