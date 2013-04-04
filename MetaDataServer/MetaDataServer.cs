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
        private Dictionary<String, FileMetadata> fileMetadata = new Dictionary<string, FileMetadata>();
       
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
            makeCheckpoint();
        }

        public FileMetadata open(String clientID, string filename)
        {
            if (fileMetadata.ContainsKey(filename) && !fileMetadata[filename].Clients.Contains(clientID))
            {
                Console.WriteLine("#MDS: opened file: " + filename);
                fileMetadata[filename].Clients.Add(clientID);
                makeCheckpoint();
                return fileMetadata[filename];
            }
            else
            {
                if (!fileMetadata.ContainsKey(filename))
                    throw new CommonTypes.Exceptions.OpenFileException("File " + filename + " does not exist");
                else throw new CommonTypes.Exceptions.OpenFileException("File " + filename + " is already opend.");
            }
        }

        public void close(String clientID, string filename)
        {
            if (fileMetadata.ContainsKey(filename) && fileMetadata[filename].Clients.Contains(clientID))
            {
                Console.WriteLine("#MDS: closed file: " + filename);
                fileMetadata[filename].Clients.Remove(clientID);
                makeCheckpoint();
            }
            else
            {
                if (!fileMetadata.ContainsKey(filename))
                    throw new CommonTypes.Exceptions.CloseFileException("File " + filename + " does not exist");
                else throw new CommonTypes.Exceptions.CloseFileException("File " + filename + " is already closed.");
            }
        }

        public void delete(string clientId, string filename)
        {
            if (fileMetadata.ContainsKey(filename) && fileMetadata[filename].Clients.Count == 1 && fileMetadata[filename].Clients.Contains(clientId))
            {
                fileMetadata[filename].Clients.Remove(clientId);
                fileMetadata.Remove(filename);
                Console.WriteLine("#MDS: Deleted file: " + filename);
                makeCheckpoint();
            }
            else
            {
                if (!fileMetadata.ContainsKey(filename))
                    throw new CommonTypes.Exceptions.DeleteFileException("File " + filename + " does not exist");
                else throw new CommonTypes.Exceptions.DeleteFileException("File " + filename + " is open.");
            }
        }

        public FileMetadata create(String clientID, string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            if (!(readQuorum <= numberOfDataServers) || !(writeQuorum <= numberOfDataServers))
                throw new CommonTypes.Exceptions.CreateFileException("Invalid quorums values in create " + filename);

            if (!fileMetadata.ContainsKey(filename) && numberOfDataServers <= dataServers.Count)
            {
                List<ServerObjectWrapper> newFileDataServers = getFirstServers(numberOfDataServers);
                FileMetadata newFileMetadata = new FileMetadata(filename, numberOfDataServers, readQuorum, writeQuorum, newFileDataServers);
                //FileInfo newFileInfo = new FileInfo(newFileMetadata, newFileDataServers);

                fileMetadata.Add(filename, newFileMetadata);
                Console.WriteLine("#MDS: Created " + filename);
                makeCheckpoint();

                return open(clientID, filename);
            }
            else
            {
                throw new CommonTypes.Exceptions.CreateFileException("Not enough data servers for create " + filename);
            }

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

        public void fail() { }

        public void recover() { }

        public void exit()
        {
            Console.WriteLine("#MDS: Exiting!");
            System.Environment.Exit(0);
        }


        public void makeCheckpoint()
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

        public void dump()
        {
            Console.WriteLine("#MDS: Dumping!");
        }
    }

}
