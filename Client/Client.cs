using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using CommonTypes;
using System.Runtime.Serialization.Formatters;
using System.Collections;

namespace Client
{
    public class Client : MarshalByRefObject, IClient
    {
        private int Port { get; set; }
        private string Id { get; set; }
        private string Url { get { return "tcp://localhost:" + Port + "/" + Id; } }
        private Dictionary<String, List<RemoteObjectWrapper>> fileServers = new Dictionary<string, List<RemoteObjectWrapper>>();
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: port clientName");
                Console.ReadLine();
            }
            else
            {
                Client client = new Client();
                client.initialize(Int32.Parse(args[0]), args[1]);
                File newFile = new File("hugo", 1, new byte[] { 0110, 111, 0001, 011 });
                Util.writeToDisk(newFile, client.Id);
                Util.readFromDisk(client.Id, "hugo", 1);
                client.startConnection();
                 
               Console.WriteLine("port: " + client.Port + " name: " + client.Id + " url: " + client.Url);
               Console.WriteLine("connection started");
               Console.ReadLine();
            }

        }

        public void initialize(int port, string id)
        {
            Port = port;
            Id = id;
        }

        void startConnection()
        {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = Port;
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(Client), Id, WellKnownObjectMode.Singleton);
            create("RIJO FILE.txt",2,1,1);
            open("RIJO FILE.txt");
        }

        public void write(string filename) { Console.WriteLine("#CLIENT " + Id + " write " + filename); }

        public void read(string filename) { Console.WriteLine("#CLIENT " + Id + " read " + filename); }

        public void open(string filename) { 
            foreach (RemoteObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                Console.WriteLine("#CLIENT " + Id + " open " + filename);
                List<RemoteObjectWrapper> servers = metadataServerWrapper.getObject<IMetaDataServer>().open(filename);
                cacheServersForFile(filename, servers);
            }
        }

        public void close(string filename) { Console.WriteLine("#CLIENT " + Id + " close " + filename); }

        public void delete(string filename) { Console.WriteLine("#CLIENT " + Id + " delete " + filename); }

        public void create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum) 
        {
            List<RemoteObjectWrapper> dataserverForFile = null;
            foreach (RemoteObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                Console.WriteLine("#CLIENT " + Id + " create " + filename);
                dataserverForFile = metadataServerWrapper.getObject<IMetaDataServer>().create(filename, numberOfDataServers, readQuorum, writeQuorum);
            }
            cacheServersForFile(filename, dataserverForFile);
            Console.WriteLine("#CLIENT " + Id + " created on" + dataserverForFile.Count + " servers");
            foreach (RemoteObjectWrapper dataserverWrapper in dataserverForFile) {
                dataserverWrapper.getObject<IDataServer>().write(new File());
            }
        }

        private void cacheServersForFile(string filename, List<RemoteObjectWrapper> dataserverForFile)
        {
            if (!fileServers.ContainsKey(filename))
            {
                fileServers.Add(filename, dataserverForFile);
            }
        }

        public void sendToMetadataServer(string message) 
        {
            //we need to test this!
            foreach (RemoteObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                metadataServerWrapper.getObject<IMetaDataServer>().open("Client");
            }
        }

        public void exit()
        {
            System.Environment.Exit(0);
        }
    }
}
