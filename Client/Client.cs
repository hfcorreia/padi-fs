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
    [Serializable]
    public class Client : MarshalByRefObject, IClient
    {
        private static int INITIAL_FILE_VERSION = 0;
        private static byte[] INITIAL_FILE_CONTENT = new byte[]{};

        private int Port { get; set; }
        private string Id { get; set; }
        private string Url { get { return "tcp://localhost:" + Port + "/" + Id; } }
        private Dictionary<String, List<ServerObjectWrapper>> fileServers = new Dictionary<string, List<ServerObjectWrapper>>();
        static void Main(string[] args)
        {
            Console.SetWindowSize(80, 15);
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: port clientName");
                Console.ReadLine();
            }
            else
            {
                Client client = new Client();
                client.initialize(Int32.Parse(args[0]), args[1]);

                client.startConnection(client);
                Console.WriteLine("#Client: Registered " + client.Id + " at " + client.Url);
                Console.ReadLine();
            }

        }

        public void initialize(int port, string id)
        {
            Port = port;
            Id = id;
        }

        void startConnection(Client client)
        {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = Port;
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            RemotingServices.Marshal(client, "" + Id, typeof(Client));

        }

        public void write(string filename, byte[] fileContent)
        {
            int fileVersion = readFileVersion(filename);
            File file = new File(filename, fileVersion, fileContent);
            write(file);
        }

        private int readFileVersion(string filename)
        {
            int fileVersion = 0;
            if (fileServers.ContainsKey(filename))
            {
                foreach (ServerObjectWrapper dataServerWrapper in fileServers[filename]){
                    fileVersion = dataServerWrapper.getObject<IDataServer>().readFileVersion(filename);
                }
            }
           
            return fileVersion;
        }

        public void write(File file)
        { 
            if (fileServers.ContainsKey(file.FileName))
            {
                foreach (ServerObjectWrapper dataServerWrapper in fileServers[file.FileName])
                {
                    dataServerWrapper.getObject<IDataServer>().write(file);
                }
            }
        }

        public void read(string filename) { Console.WriteLine("Not done"); }

        public void open(int clientId, string filename) {
            
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                List<ServerObjectWrapper> servers = metadataServerWrapper.getObject<IMetaDataServer>().open( clientId, filename);
                cacheServersForFile(filename, servers);
            }
        }

        public void close(int clientId, string filename) 
        { 
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                metadataServerWrapper.getObject<IMetaDataServer>().close(clientId, filename);
                removeCacheServersForFile(filename);
            }
        }

        private void removeCacheServersForFile(string filename)
        {
            if (fileServers.ContainsKey(filename))
            {
                fileServers.Remove(filename);
            }

        }

        public void delete(string filename) 
        {

            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                metadataServerWrapper.getObject<IMetaDataServer>().delete(filename);
            }
        }

        public void create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            Console.WriteLine("#Client: creating file " + filename );
            List<ServerObjectWrapper> dataserverForFile = null;
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                
                dataserverForFile = metadataServerWrapper.getObject<IMetaDataServer>().create(filename, numberOfDataServers, readQuorum, writeQuorum);
            }
            
            cacheServersForFile(filename, dataserverForFile);

            File emptyFile = new File(filename, INITIAL_FILE_VERSION, INITIAL_FILE_CONTENT);
            write(emptyFile); //writes an empty file
        }

        private void cacheServersForFile(string filename, List<ServerObjectWrapper> dataserverForFile)
        {
            if (!fileServers.ContainsKey(filename))
            {
                fileServers.Add(filename, dataserverForFile);
            }
        }

         public void exit()
        {
            System.Environment.Exit(0);
        }
    }
}
