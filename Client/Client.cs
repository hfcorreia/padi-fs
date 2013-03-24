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
        private static string FILENAME = "RIJO FILE.txt";
        private static int INITIAL_FILE_VERSION = 0;
        private static byte[] INITIAL_FILE_CONTENT = new byte[]{};

        private int Port { get; set; }
        private string Id { get; set; }
        private string Url { get { return "tcp://localhost:" + Port + "/" + Id; } }
        private Dictionary<String, List<ServerObjectWrapper>> fileServers = new Dictionary<string, List<ServerObjectWrapper>>();
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

                Console.WriteLine("port: " + client.Port + " name: " + client.Id + " url: " + client.Url);
                Console.WriteLine("connection started");
                Console.ReadLine();

                client.startConnection();

                Console.ReadLine();

                client.create(FILENAME, 1, 1, 1);

                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                try
                {
                    Byte[] fileAsBytes = encoding.GetBytes("EI EI");
                    client.write(FILENAME, fileAsBytes);
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR getting file content");
                }

                client.close(FILENAME);

                //delete(FILENAME);
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
            Console.WriteLine("#CLIENT " + Id + " write " + file.FileName);
            if (fileServers.ContainsKey(file.FileName))
            {
                foreach (ServerObjectWrapper dataServerWrapper in fileServers[file.FileName])
                {
                    Console.WriteLine("#CLIENT " + Id + " write " + file.FileName + "fileVersion" + file.Version + "fileContent" + file.Content);
                    dataServerWrapper.getObject<IDataServer>().write(file);
                }
            }
        }

        public void read(string filename) { Console.WriteLine("#CLIENT " + Id + " read " + filename); }

        public void open(string filename) { 
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                List<ServerObjectWrapper> servers = metadataServerWrapper.getObject<IMetaDataServer>().open(filename);
                Console.WriteLine("Servers for file - " + filename + " - " + servers);
                cacheServersForFile(filename, servers);
            }
        }

        public void close(string filename) 
        { 
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                Console.WriteLine("#CLIENT " + Id + " close " + filename);
                metadataServerWrapper.getObject<IMetaDataServer>().close(filename);
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
                Console.WriteLine("#CLIENT " + Id + " delete " + filename);
                metadataServerWrapper.getObject<IMetaDataServer>().delete(filename);
            }
        }

        public void create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum) 
        {
            Console.WriteLine("#CLIENT " + Id + " - CREATE - [filename: " + filename + ", NumberOfservers: " + numberOfDataServers + ", readQ: " + readQuorum + ", writeQ: " + writeQuorum + "] - START ");
            List<ServerObjectWrapper> dataserverForFile = null;
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                Console.WriteLine("#CLIENT " + Id + " create " + filename);
                dataserverForFile = metadataServerWrapper.getObject<IMetaDataServer>().create(filename, numberOfDataServers, readQuorum, writeQuorum);
            }
            
            cacheServersForFile(filename, dataserverForFile);

            File emptyFile = new File(filename, INITIAL_FILE_VERSION, INITIAL_FILE_CONTENT);
            write(emptyFile); //writes an empty file

            Console.WriteLine("#CLIENT " + Id + " - CREATE - " + filename + " created on " + dataserverForFile.Count + " servers - DONE!");
        }

        private void cacheServersForFile(string filename, List<ServerObjectWrapper> dataserverForFile)
        {
            if (!fileServers.ContainsKey(filename))
            {
                fileServers.Add(filename, dataserverForFile);
            }
        }

        public void sendToMetadataServer(string message) 
        {
            //we need to test this!
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
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
