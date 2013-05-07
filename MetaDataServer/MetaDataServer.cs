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
        private static int MAX_HEARTBEATS = Int32.Parse(Properties.Resources.MAX_HEARTBEATS);
        private static double OVERLOAD_MULTIPLIER = 1.3;

        public int Port { get; set; }
        public String Id { get; set; }
        public string Url { get { return "tcp://localhost:" + Port + "/" + Id; } }

        public MetaDataLog Log { get; set; }
        private bool isFailing;
        private bool isRecovering;
        [NonSerialized]
        private Queue<MetaDataOperation> requestsQueue;

        public SerializableDictionary<String, ServerObjectWrapper> DataServers { get; set; }
        public SerializableDictionary<String, FileMetadata> FileMetadata { get; set; }

        private Dictionary<String, ManualResetEvent> fileMetadataLocks;

        [NonSerialized]
        private SerializableDictionary<String, Queue<HeartbeatMessage>> heartbeats;

        [NonSerialized]
        private PassiveReplicationHandler replicationHandler;
        public int CheckpointCounter { get; set; }


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
            fileMetadataLocks = new SerializableDictionary<string, ManualResetEvent>();
            DataServers = new SerializableDictionary<String, ServerObjectWrapper>();
            Heartbeats = new SerializableDictionary<string, Queue<HeartbeatMessage>>();
            Log = new MetaDataLog();
            Log.init(this);
            isFailing = false;
            isRecovering = false;
            requestsQueue = new Queue<MetaDataOperation>();
            CheckpointCounter = 0;

            Console.Title = "MDS " + Id;

            this.replicationHandler = new PassiveReplicationHandler(IdAsNumber);
            getCheckpoint(Id);

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
                    getMetdataLock(fileName).Set();
                }
            }

        }

        private void executeOperation(MetaDataOperation operation)
        {
            if (replicationHandler.IsMaster)
            {
                safeExecuteOperation(operation);
            }
            else
            {
                Console.WriteLine("#MDS " + Id + " [SLAVE] - " + " executeOperation " + operation);
                int masterId = replicationHandler.MasterNodeId;
                throw new NotMasterException("please execute the operation on the master: " + masterId, masterId);
            }
        }

        private void safeExecuteOperation(MetaDataOperation operation)
        {
            if (isFailing)
            {
                throw new Exception("the mds " + Id + " is failing");
            }
            if (isRecovering)
            {
                requestsQueue.Enqueue(operation);
            }
            Console.WriteLine("#MDS " + Id + (replicationHandler.IsMaster ? " [MASTER] - " : " [SLAVE] ") + " executeOperation " + operation);
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
            return replicationHandler.MetadataServerId;
        }

        public FileMetadata updateReadMetadata(string clientId, string filename)
        {
            if (isFailing || isRecovering)
            {
                throw new Exception("the mds " + Id + " is failing");
            }
            while (FileMetadata[filename].FileServers.Count < FileMetadata[filename].ReadQuorum)
            {
                getMetdataLock(filename).WaitOne();
            }

            return FileMetadata[filename];
        }

        public FileMetadata updateWriteMetadata(string clientId, string filename)
        {
            while (FileMetadata[filename].FileServers.Count < FileMetadata[filename].WriteQuorum)
            {
                if (isFailing || isRecovering)
                {
                    throw new Exception("the mds " + Id + " is failing");
                }
                getMetdataLock(filename).WaitOne();
            }

            return FileMetadata[filename];
        }

        public void receiveAliveMessage(MetaDataServerAliveMessage aliveMessage)
        {
            if (isFailing)
            {
                throw new Exception("the mds " + Id + " is failing");
            }
            if (isRecovering)
            {
                aliveMessage.Operations.Sort(new OperationComparer());
                foreach (MetaDataOperation op in aliveMessage.Operations)
                {
                    requestsQueue.Enqueue(op);
                }
                return;
            }

            try
            {
                replicationHandler.registerAliveMessage(aliveMessage.MetadataServerId);
            }
            catch (Exception)
            {
                throw;
            }

            if (aliveMessage.IsMaster && aliveMessage.Operations != null)
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
                isRecovering = true;
                isFailing = false;

                replicationHandler.init();
                List<MetaDataOperation> operations = replicationHandler.synchOperations(Log.Status);
                Console.WriteLine("MDS recover - the server has " + operations.Count + " operations in fault ");
                Log.registerOperations(this, operations);
                while (Log.Status < Log.NextId)
                {
                    Log.getOperation(Log.Status).execute(this);
                    Log.incrementStatus();
                }


                while (requestsQueue.Count != 0)
                {
                    MetaDataOperation operation = requestsQueue.Dequeue();
                    Log.registerOperation(this, operation);
                    operation.execute(this);
                    Log.incrementStatus();
                }



                isRecovering = false;

                Console.WriteLine("Recover - good morning =D");
            }
            else
            {
                Console.WriteLine("MDS is already alive");
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
            try
            {
                String metadataServerId = Id;
                Console.WriteLine("#MDS: making checkpoint " + CheckpointCounter++ + " from server " + Id);

                string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\" + metadataServerId;
                Util.createDir(dirName);

                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(MetaDataServer),
                    new Type[]{ typeof(MetaDataCloseOperation), typeof(MetaDataCreateOperation), typeof(MetaDataDeleteOperation),
                        typeof(MetaDataRegisterServerOperation), typeof(MetaDataOpenOperation)});
                System.IO.StreamWriter fileWriter = new System.IO.StreamWriter(@dirName + "\\checkpoint.xml");

                writer.Serialize(fileWriter, this);

                fileWriter.Close();

                Console.WriteLine("#MDS: checkpoint " + CheckpointCounter + " from metaDataServer " + Id + " done");
            }
            catch (Exception e)
            {
                Console.WriteLine("#MDS: GetCheckpoint Failed: " + e.Message);

            }

        }

        public void getCheckpoint(String metadataServerId)
        {
            try
            {
                Console.WriteLine("#MDS Recovering Checkpoint!");
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(MetaDataServer), new Type[]{ typeof(MetaDataCloseOperation), typeof(MetaDataCreateOperation), typeof(MetaDataDeleteOperation),
                        typeof(MetaDataRegisterServerOperation), typeof(MetaDataOpenOperation)});

                string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\" + metadataServerId + "\\checkpoint.xml";
                System.IO.StreamReader fileReader = new System.IO.StreamReader(dirName);

                MetaDataServer metadaServer = new MetaDataServer();
                metadaServer = (MetaDataServer)reader.Deserialize(fileReader);

                this.CheckpointCounter = metadaServer.CheckpointCounter;
                this.DataServers = metadaServer.DataServers;
                this.FileMetadata = metadaServer.FileMetadata;
                this.Log = metadaServer.Log;
                this.Port = metadaServer.Port;
                foreach (String filename in FileMetadata.Keys)
                {
                    addMetadataLock(filename, new ManualResetEvent(false));
                }
                fileReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("#MDS: Checkpoint Failed: " + e.Message);
             }
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
                    Console.WriteLine("\t\tNumber of ds: " + files.Value.NumServers);
                    Console.WriteLine("\t\tRead Quorum: " + files.Value.ReadQuorum);
                    Console.WriteLine("\t\tWrite Quorum: " + files.Value.WriteQuorum);
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

        private SerializableDictionary<String, Queue<HeartbeatMessage>> Heartbeats
        {
            get { return heartbeats; }
            set { heartbeats = value; }
        }


        public void addMetadataLock(string filename, ManualResetEvent e)
        {
            fileMetadataLocks.Add(filename, e);
        }

        public ManualResetEvent getMetdataLock(string filename)
        {
            return fileMetadataLocks[filename];
        }

     
        public PassiveReplicationHandler getReplicationHandler()
        {
            return replicationHandler;
        }

        public void setReplicationHandler(PassiveReplicationHandler replicationHandler)
        {
            this.replicationHandler = replicationHandler;
        }

        public void receiveHeartbeat(HeartbeatMessage heartbeat)
        {
            Console.WriteLine("#MDS: Heartbeat received: " + heartbeat.ToString());

            string serverID = heartbeat.ServerId;

            if (!Heartbeats.ContainsKey(serverID))
            {
                Heartbeats.Add(serverID, new Queue<HeartbeatMessage>());
            }

            if (Heartbeats[serverID].Count == MAX_HEARTBEATS)
            {
                Heartbeats[serverID].Dequeue();
            }

            Heartbeats[serverID].Enqueue(heartbeat);

            Console.WriteLine("actual file access at " + serverID + " w/ size: " + heartbeat.AccessCounter.Count);
            foreach (KeyValuePair<String, FileAccessCounter> entry in heartbeat.AccessCounter)
            {
                Console.WriteLine("AcessCounter: " + entry.Value.ToString());
            }


            double avg = calculateAverageWeight();
            if (calculateServerWeight(serverID) > (avg * OVERLOAD_MULTIPLIER))
            {
                //migration!!!
                Console.WriteLine("Server within overload: " + serverID);
                List<FileMetadata> closedFiles = getClosedFiles(serverID);
                Console.WriteLine("files to move: " + closedFiles.Count);
                List<ServerObjectWrapper> servers = getSortedServers(DataServers.Count);
                //escolher ficheiro!!!
                List<ServerObjectWrapper> cleanServers = getUnderweightServersWithoutFile(servers, avg, closedFiles[0].FileName);
                Console.WriteLine("servers available: " + cleanServers.Count);
                foreach (ServerObjectWrapper srv in cleanServers)
                {
                    Console.WriteLine("server avail: " + srv.Id);
                }
            }
        }


        public double calculateServerWeight(String id)
        {
            double result = 0;
            Queue<HeartbeatMessage> heartbeats = Heartbeats[id];

            foreach (HeartbeatMessage heartbeat in heartbeats)
            {
                result += (((heartbeat.ReadCounter * 0.2) + (heartbeat.ReadVersionCounter * 0.3) + (heartbeat.WriteCounter * 0.5)) * 0.6)
                          + ((heartbeat.FileCounter) * 0.4);
            }

            return result;
        }

        public List<ServerObjectWrapper> getSortedServers(int numDataServers)
        {
            List<ServerObjectWrapper> servers = new List<ServerObjectWrapper>();
            List<ListElem> serversWeight = new List<ListElem>();

            foreach (ServerObjectWrapper dataserverWrapper in DataServers.Values)
            {
                serversWeight.Add(new ListElem(new ServerObjectWrapper(dataserverWrapper), calculateServerWeight(dataserverWrapper.Id)));
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
            public ServerObjectWrapper Server { get; set; }
            public double Weight { get; set; }

            public ListElem(ServerObjectWrapper server, double weight)
            {
                Server = server;
                Weight = weight;
            }
        }

        #endregion otherCode



        #region migration

        /*
         * se servidor com muitos pedidos (o que sao muitos? => mais que a media dos servidores?)
         *   ve que ficheiros pode retirar do servidor
         *   escolher melhor ficheiro (mais acessos?)
         *    ve que servidores estao ilegiveis para receber ficheiro (sem ficheiro e abaixo da avg)
         *    mete no outro
         *    remove do antigo
         *    MD's actualizados
         */


        public double calculateAverageWeight()
        {
            double total = 0;
            int nServers = Heartbeats.Count();

            foreach (KeyValuePair<string, Queue<HeartbeatMessage>> entry in Heartbeats)
            {
                total += calculateServerWeight(entry.Key);
            }

            return total / nServers;
        }


        public List<FileMetadata> getClosedFiles(String dsID)
        {
            List<FileMetadata> closedFiles = new List<FileMetadata>();
            foreach (KeyValuePair<String, FileMetadata> entry in FileMetadata)
            {
                if (!entry.Value.IsOpen && fileInDS(entry.Value, dsID))
                {
                    closedFiles.Add(entry.Value);
                }
            }

            //ORDENAR
            return closedFiles;
        }

        public bool fileInDS(FileMetadata fileMetaData, string dsID)
        {
            foreach (ServerObjectWrapper server in fileMetaData.FileServers)
            {
                if (server.Id.Equals(dsID))
                {
                    return true;
                }
            }

            return false;
        }

        public List<ServerObjectWrapper> getUnderweightServersWithoutFile(List<ServerObjectWrapper> servers, double avg, string filename)
        {
            List<ServerObjectWrapper> result = new List<ServerObjectWrapper>();

            foreach (ServerObjectWrapper s in servers)
            {
                bool add = true;
                foreach (ServerObjectWrapper fileServer in FileMetadata[filename].FileServers)
                {
                    if (s.Id.Equals(fileServer.Id))
                    {
                        add = false;
                    }
                }

                if (add && (calculateServerWeight(s.Id) < avg))
                {
                    result.Add(s);
                }
            }

            return result;
        }


        #endregion migration
    }

}
