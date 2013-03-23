using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using CommonTypes;

namespace Client
{
    public class Client : MarshalByRefObject, IClient
    {
        private int Port { get; set; }
        private string Id { get; set; }
        private string Url { get { return "tcp://localhost:" + Port + "/" + Id; } }

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
            TcpChannel channel = new TcpChannel(Port);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(Client), Id, WellKnownObjectMode.Singleton);
            create("RIJO FILE.txt");
            open("RIJO FILE.txt");
        }

        public void write(string filename) { Console.WriteLine("#CLIENT " + Id + " write " + filename); }

        public void read(string filename) { Console.WriteLine("#CLIENT " + Id + " read " + filename); }

        public void open(string filename) { 
            foreach (RemoteObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                Console.WriteLine("#CLIENT " + Id + " open " + filename);
                metadataServerWrapper.getObject<IMetaDataServer>().open(filename);
            }
        }

        public void close(string filename) { Console.WriteLine("#CLIENT " + Id + " close " + filename); }

        public void delete(string filename) { Console.WriteLine("#CLIENT " + Id + " delete " + filename); }

        public void create(string filename) 
        {
            int numberOfDataServers = 1;
            int readQuorum = 1;
            int writeQuorum = 1;
            List<RemoteObjectWrapper> dataserverForFile = null;
            foreach (RemoteObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                Console.WriteLine("#CLIENT " + Id + " create " + filename);
                dataserverForFile = metadataServerWrapper.getObject<IMetaDataServer>().create(filename, numberOfDataServers, readQuorum, writeQuorum);
            }
            Console.WriteLine("#CLIENT " + Id + " created " + dataserverForFile);
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
