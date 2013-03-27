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
    public class MetaDataServer : MarshalByRefObject, IMetaDataServer
    {
        public  int Port { get; set; }
        public  int Id { get; set; }
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
                metadataServer.startConnection();

                Console.WriteLine("#MDS: Registered " + metadataServer.Id + " at " + metadataServer.Url);
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
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = Port;
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(MetaDataServer), "" + Id, WellKnownObjectMode.Singleton);
        }

        public void registDataServer(int id, string host, int port)
        {
            Console.WriteLine("#MDS: Registering DS " + id);
            ServerObjectWrapper remoteObjectWrapper = new ServerObjectWrapper(port, id, host);
            dataServers.Add(id, remoteObjectWrapper);
        }

        public List<ServerObjectWrapper> open(string filename)
        {
            if (filesInfo.ContainsKey(filename))
            {
                Console.WriteLine("#MDS: opened " + filename);
                filesInfo[filename].NumberOfClients++;
                return filesInfo[filename].DataServers;
            }
            else
            {
                Console.WriteLine("#MDS: No such file " + filename);
                return null;
            }
         }

        public void close(string filename)
        {
            if (filesInfo.ContainsKey(filename))
            {
                Console.WriteLine("#MDS: closed " + filename);
                filesInfo[filename].NumberOfClients--;
            }
            else
            {
                Console.WriteLine("#MDS: " + filename + " does not exist");
            }
        }

        public void delete(string filename)
        {
            if (filesInfo.ContainsKey(filename) && filesInfo[filename].NumberOfClients == 0)
            {
                filesInfo.Remove(filename);
                Console.WriteLine("#MDS: Deleted " + filename);
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
    }

}
