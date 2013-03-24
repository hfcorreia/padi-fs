using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using CommonTypes;

namespace MetaDataServer
{
    public class MetaDataServer : MarshalByRefObject, IMetaDataServer
    {
        public  int Port { get; set; }
        public  int Id { get; set; }
        public string Url { get { return "tcp://localhost:" + Port + "/" + Id; } }
        private Dictionary<int, RemoteObjectWrapper> dataServers = new Dictionary<int, RemoteObjectWrapper>(); // <serverID, DataServerWrapper>
        private Dictionary<string, List<int>> fileServers = new Dictionary<string, List<int>>(); //<filename List<ServerId>> 
        static void Main(string[] args)
        {
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
                metadataServer.startConnection();

                Console.WriteLine("port: " + metadataServer.Port + " Id: " + metadataServer.Id + " url: " + metadataServer.Url);
                Console.WriteLine("connection started");
                Console.ReadLine();
            }
        }

        

        public void initialize(int port, int id)
        {
            Port = port;
            Id = id;
        }

        public void startConnection()
        {
            TcpChannel channel = new TcpChannel(Port);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(MetaDataServer), "" + Id, WellKnownObjectMode.Singleton);
        }

        public void registDataServer(int id, string host, int port)
        {
            RemoteObjectWrapper remoteObjectWrapper = new RemoteObjectWrapper(port, id, host);
            dataServers.Add(id, remoteObjectWrapper);
            Console.WriteLine("DS registed " + remoteObjectWrapper.Id + " w/ url: " + remoteObjectWrapper.URL);
        }

        public File open(string filename)
        {
            Console.WriteLine("#MDS " + Id + " open " + filename);
            return null;
        }

        public void close(string filename)
        {
            Console.WriteLine("#MDS " + Id + " close " + filename);
        }

        public void delete(string filename)
        {
            Console.WriteLine("#MDS " + Id + " delete " + filename);
        }

        public List<RemoteObjectWrapper> create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            Console.WriteLine("#MDS " + Id + " create " + filename);

            if (numberOfDataServers < dataServers.Count)
            {
                return getFirstServers(numberOfDataServers);
            }


            return null;
        }

        private List<RemoteObjectWrapper> getFirstServers(int numDataServers)
        {
            List<RemoteObjectWrapper> firstDataServers = new List<RemoteObjectWrapper>();
            foreach (RemoteObjectWrapper dataserverWrapper in dataServers.Values)
            {
                if (firstDataServers.Count < numDataServers)
                {
                    firstDataServers.Add(new RemoteObjectWrapper(dataserverWrapper));
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
    }

}
