using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;
using CommonTypes;
using System.Collections;
using CommonTypes.Exceptions;
using System.Threading;

namespace MetaDataServer
{
    [Serializable]
    public class MetaDataServer : MarshalByRefObject, IMetaDataServer
    {
        public int Port { get; set; }
        public String Id { get; set; }
        public string Url { get { return "tcp://localhost:" + Port + "/" + Id; } }
        private Dictionary<String, ServerObjectWrapper> dataServers = new Dictionary<String, ServerObjectWrapper>(); // <serverID, DataServerWrapper>
        //private Dictionary<String, FileInfo> filesInfo = new Dictionary<string, FileInfo>();
        private Dictionary<String, FileMetadata> fileMetadata = new Dictionary<String, FileMetadata>();
        private Dictionary<String, ManualResetEvent> fileMetadataLocks = new Dictionary<string, ManualResetEvent>();

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

        public void initialize(int port, String id)
        {
            Port = port;
            Id = id;
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

        public void registDataServer(String id, string host, int port)
        {
            Console.WriteLine("#MDS: Registering DS " + id);
            ServerObjectWrapper remoteObjectWrapper = new ServerObjectWrapper(port, id, host);
            dataServers.Add(id, remoteObjectWrapper);
            addServerToUnbalancedFiles(id);
            makeCheckpoint();
        }

        private void addServerToUnbalancedFiles(string id)
        {
            foreach (String fileName in fileMetadata.Keys) 
            { 
                FileMetadata metadata = fileMetadata[fileName];
                if (metadata.FileServers.Count < metadata.ReadQuorum || metadata.FileServers.Count < metadata.WriteQuorum) 
                {
                    metadata.FileServers.Add(dataServers[id]);
                    fileMetadataLocks[fileName].Set();
                }
            }
        }

        public FileMetadata open(String clientID, string filename)
        {
            if (!fileMetadata.ContainsKey(filename))
            {
                throw new OpenFileException("#MDS.open - File " + filename + " does not exist");
            }
            else if (fileMetadata[filename].Clients.Contains(clientID))
            {
                Console.WriteLine("#MDS.open - File " + filename + " is allready open for user " + clientID);
                return fileMetadata[filename];
            }
            else
            {
                Console.WriteLine("#MDS: opening file: " + filename);
                fileMetadata[filename].Clients.Add(clientID);
                makeCheckpoint();
                return fileMetadata[filename];
            }
        }

        public void close(String clientID, string filename)
        {
           if (!fileMetadata.ContainsKey(filename))
            {
                throw new CloseFileException("#MDS.close - File " + filename + " does not exist");
            }

            if (fileMetadata.ContainsKey(filename) && !fileMetadata[filename].Clients.Contains(clientID))
            {
                //throw new CloseFileException("#MDS.close - File " + filename + " is not open for user " + clientID);
                Console.WriteLine("#MDS.close - File " + filename + " is allready closed for user " + clientID);
                return;
            }

            Console.WriteLine("#MDS: closing file " + filename + "...");
            fileMetadata[filename].Clients.Remove(clientID);
            makeCheckpoint();

        }

        public void delete(string clientId, string filename)
        {
            if (!fileMetadata.ContainsKey(filename))
                    throw new CommonTypes.Exceptions.DeleteFileException("#MDS.delete - File " + filename + " does not exist");

            /*
            if (fileMetadata[filename].Clients.Count == 1 && fileMetadata[filename].Clients.Contains(clientId))
            {
                close(clientId, filename);
            }
            */

            if (fileMetadata[filename].Clients.Count > 0)
            {
                string clients = "";
                foreach (string c in fileMetadata[filename].Clients) 
                {
                    clients += c + ", ";
                }
                throw new CommonTypes.Exceptions.DeleteFileException("#MDS.delete - Error deleting file " + filename + " for client " + clientId +" is open by clients: [ " + clients + " ]");
            }

            fileMetadata.Remove(filename);
            Console.WriteLine("#MDS: Deleted file: " + filename);
            makeCheckpoint();
        }

        public FileMetadata create(String clientID, string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            if ((writeQuorum > numberOfDataServers) || (readQuorum > numberOfDataServers))
                throw new CreateFileException("Invalid quorums values in create " + filename);

            if (fileMetadata.ContainsKey(filename))
            {
                throw new CreateFileException("#MDS.create - The file " + filename + " allready exists ");
            }

            List<ServerObjectWrapper> newFileDataServers = getFirstServers(numberOfDataServers);

            foreach(ServerObjectWrapper wrapper in newFileDataServers)
            {
                Console.WriteLine("#MDS.create - atribute file to dataserver " + wrapper.getObject<IDataServer>().GetHashCode());
            }

            FileMetadata newFileMetadata = new FileMetadata(filename, numberOfDataServers, readQuorum, writeQuorum, newFileDataServers);
            //FileInfo newFileInfo = new FileInfo(newFileMetadata, newFileDataServers);

            fileMetadata.Add(filename, newFileMetadata);
            fileMetadataLocks.Add(filename, new ManualResetEvent(false));
            Console.WriteLine("#MDS: Created " + filename);
            makeCheckpoint();

            //return open(clientID, filename);
            return fileMetadata[filename];
        }

        private List<ServerObjectWrapper> getFirstServers(int numDataServers)
        {
            List<ServerObjectWrapper> firstDataServers = new List<ServerObjectWrapper>();

            foreach (ServerObjectWrapper dataserverWrapper in dataServers.Values)
            {
                if (firstDataServers.Count < numDataServers)
                {
                    firstDataServers.Add(new ServerObjectWrapper(dataserverWrapper));
                }
            }
            return firstDataServers;
        }

        public void fail() 
        {
            Console.WriteLine("Fail not implemented in MD yet");
        }

        public void recover() 
        {
            Console.WriteLine("Recover not implemented in MD yet");
        }

        public void exit()
        {
            Console.WriteLine("#MDS: Exiting!");
            System.Environment.Exit(0);
        }


        public void makeCheckpoint()
        {
            lock (this)
            {
                String metadataServerId = Id;
                string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\" + metadataServerId;
                Util.createDir(dirName);

                System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(MetaDataServer));

                System.IO.StreamWriter fileWriter = new System.IO.StreamWriter(@dirName + "\\checkpoint.xml");
                writer.Serialize(fileWriter, this);
                fileWriter.Close();
            }
        }

        public static MetaDataServer getCheckpoint(String metadataServerId)
        {
            System.Xml.Serialization.XmlSerializer reader =
                      new System.Xml.Serialization.XmlSerializer(typeof(MetaDataServer));

            string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\" + metadataServerId + "\\checkpoint.xml";
            System.IO.StreamReader fileReader = new System.IO.StreamReader(dirName);

            MetaDataServer metadaServer = new MetaDataServer();
            metadaServer = (MetaDataServer)reader.Deserialize(fileReader);

            return metadaServer;
        }

        public FileMetadata updateReadMetadata(string clientId, string filename)
        {
            while (fileMetadata[filename].FileServers.Count < fileMetadata[filename].ReadQuorum) 
            {
                Console.WriteLine("updateReadMetadata - WAITING [filename: " + filename + ", #server: " + fileMetadata[filename].FileServers.Count + ", quorum: " + fileMetadata[filename].ReadQuorum);
                fileMetadataLocks[filename].WaitOne();
            }

            Console.WriteLine("updateReadMetadata - FOUND [filename: " + filename + ", #server: " + fileMetadata[filename].FileServers.Count + ", quorum: " + fileMetadata[filename].ReadQuorum);

            return fileMetadata[filename];
        }

        public void dump()
        {
            Console.WriteLine("#MDS: Dumping!\r\n");
            Console.WriteLine(" URL: " + Url);
            Console.WriteLine(" Registered Data Servers:");
            foreach (KeyValuePair<String, ServerObjectWrapper> dataServer in dataServers)
            {
                Console.WriteLine("\t" + dataServer.Key);
            }
            Console.WriteLine(" Opened Files: ");
            foreach (KeyValuePair<String, FileMetadata> files in fileMetadata)
            {
                Console.Write("\t" + files.Key + " - Clients[ ");
                foreach (String name in files.Value.Clients)
                {
                    Console.Write(name + " ");
                }
                Console.WriteLine("]");
                Console.WriteLine("nb " + files.Value.NumServers + " rq " + files.Value.ReadQuorum + " wq" + files.Value.WriteQuorum);
            }
            Console.WriteLine();

        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

    }

}
