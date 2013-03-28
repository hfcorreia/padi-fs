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
        public int Id { get; set; }
        public string Url { get { return "tcp://localhost:" + Port + "/" + Id; } }
        private Dictionary<int, ServerObjectWrapper> dataServers = new Dictionary<int, ServerObjectWrapper>(); // <serverID, DataServerWrapper>
        private Dictionary<string, FileInfo> filesInfo = new Dictionary<string, FileInfo>();

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
                metadataServer.initialize(Int32.Parse(args[0]), Int32.Parse(args[1]));
                Util.createDir(CommonTypes.Properties.Resources.TEMP_DIR);
                metadataServer.startConnection(metadataServer);

                Console.WriteLine("#MDS: Registered " + metadataServer.Id + " at " + metadataServer.Url);
                Console.ReadLine();
            }
        }

        public void initialize(int port, int id)
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

            RemotingServices.Marshal(metadataServer, "" + Id, typeof(MetaDataServer));
        }

        public void registDataServer(int id, string host, int port)
        {
            Console.WriteLine("#MDS: Registering DS " + id);
            ServerObjectWrapper remoteObjectWrapper = new ServerObjectWrapper(port, id, host);
            dataServers.Add(id, remoteObjectWrapper);
            makeCheckpoint();
        }

        public List<ServerObjectWrapper> open(int clientID, string filename)
        {
            if (filesInfo.ContainsKey(filename))
            {
                if (!filesInfo[filename].Clients.Contains(clientID))
                {
                    Console.WriteLine("#MDS: opened file: " + filename);
                    filesInfo[filename].Clients.Add(clientID);
                    makeCheckpoint();
                    return filesInfo[filename].DataServers;
                }
                Console.WriteLine("#MDS: " + filename + " is already opened");
                return null;
            }
            else
            {
                Console.WriteLine("#MDS: No such file: " + filename);
                return null;
            }
        }

        public void close(int clientID, string filename)
        {
            if (filesInfo.ContainsKey(filename) && filesInfo[filename].Clients.Contains(clientID))
            {
                Console.WriteLine("#MDS: closed file: " + filename);
                filesInfo[filename].Clients.Remove(clientID);
                makeCheckpoint();
            }
            else
            {
                Console.WriteLine("#MDS: " + filename + " isn't open");
            }
        }

        public void delete(string filename)
        {
            if (filesInfo.ContainsKey(filename) && filesInfo[filename].Clients.Count == 0)
            {
                filesInfo.Remove(filename);
                Console.WriteLine("#MDS: Deleted file: " + filename);
                makeCheckpoint();
            }
            else
            {
                //throw eception - because the file is open or does not exist
            }
        }

        public List<ServerObjectWrapper> create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            if (!filesInfo.ContainsKey(filename) && numberOfDataServers <= dataServers.Count)
            {
                FileMetadata newFileMetadata = new FileMetadata(filename, numberOfDataServers, readQuorum, writeQuorum);
                List<ServerObjectWrapper> newFileDataServers = getFirstServers(numberOfDataServers);

                FileInfo newFileInfo = new FileInfo(newFileMetadata, newFileDataServers);

                filesInfo.Add(filename, newFileInfo);
                Console.WriteLine("#MDS: Created " + filename);
                makeCheckpoint();
                return newFileDataServers;
            }
            else
            {
                return null; // throws exception
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
            System.Environment.Exit(0);
        }


        public void makeCheckpoint()
        {

            int metadataServerId = Id;
            Console.WriteLine("#MDS: making checkpoint " + Id);

            string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\MDS" + metadataServerId;
            Util.createDir(dirName);

            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(MetaDataServer));

            System.IO.StreamWriter fileWriter = new System.IO.StreamWriter(@dirName + "\\checkpoint.xml");
            writer.Serialize(fileWriter, this);
            fileWriter.Close();
        }

        public static MetaDataServer getCheckpoint(int metadataServerId)
        {
            System.Xml.Serialization.XmlSerializer reader =
                      new System.Xml.Serialization.XmlSerializer(typeof(MetaDataServer));

            string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\MDS" + metadataServerId + "\\checkpoint.xml";
            System.IO.StreamReader fileReader = new System.IO.StreamReader(dirName);

            MetaDataServer metadaServer = new MetaDataServer();
            metadaServer = (MetaDataServer)reader.Deserialize(fileReader);

            return metadaServer;
        }
    }

}
