using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using CommonTypes;
using System.Collections;
using System.Runtime.Serialization.Formatters;
using CommonTypes.Exceptions;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace DataServer
{
    public class DataServer : MarshalByRefObject, IDataServer
    {
        private int CheckpointCounter { get; set; }

        public String Id { get; set; }

        public int Port { get; set; }

        public string Host { get; set; }

        public string Url { get { return "tcp://" + Host + ":" + Port + "/" + Id; } }

        public SerializableDictionary<string, File> Files { get; set; }

        private DSstate State { get; set; }

        internal Dictionary<string, ReaderWriterLockSlim> FileLocks { get; set; }

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
                DataServer dataServer = new DataServer();
                dataServer.initialize(Int32.Parse(args[0]), args[1], "localhost");
                dataServer.startConnection(dataServer);

                Console.WriteLine("#DS: Registered " + dataServer.Id + " at " + dataServer.Url);
                Console.ReadLine();
            }
        }

        public void initialize(int port, string id, string host)
        {
            Port = port;
            Id = id;
            Host = host;
            Files = new SerializableDictionary<string, File>();
            State = new DSstateNormal(this);
            FileLocks = new Dictionary<string, ReaderWriterLockSlim>();
            CheckpointCounter = 0;
        }

        public void startConnection(DataServer dataServer)
        {
            Console.WriteLine("#DS: starting connection " + Id);
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = Port;
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            RemotingServices.Marshal(dataServer, Id, typeof(DataServer));
            registInMetadataServers();
        }


        public void write(File file)
        {
            State.write(file);
        }

        public File read(string filename)
        {
            return State.read(filename);
        }


        public void registInMetadataServers()
        {
            Console.WriteLine("#DS: registering in MetadataServers");
            Task[] tasks = new Task[MetaInformationReader.Instance.MetaDataServers.Count];
            for (int md = 0; md < MetaInformationReader.Instance.MetaDataServers.Count; md++)
            {
                IMetaDataServer metadataServer = MetaInformationReader.Instance.MetaDataServers[md].getObject<IMetaDataServer>();
                tasks[md] = Task.Factory.StartNew(() => { metadataServer.registDataServer(Id, Host, Port); });
            }
        }

        public int readFileVersion(string filename)
        {
            return State.readFileVersion(filename);
        }

        public void exit()
        {
            Console.WriteLine("#DS: Exiting!");
            System.Environment.Exit(0);
        }

        public void makeCheckpoint()
        {
                String dataServerId = Id;
                Console.WriteLine("#DS: making checkpoint " + CheckpointCounter++ +  " from server " + Id);

                string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\" + dataServerId;
                Util.createDir(dirName);

                System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(DataServer));

            System.IO.StreamWriter fileWriter = new System.IO.StreamWriter(@dirName + "\\checkpoint.xml");
            writer.Serialize(fileWriter, this);

                // DataContractSerializer w = new DataContractSerializer(typeof(DataServer));
                // System.Xml.XmlDictionaryWriter xw = new System.Xml.DelegatingXmlDictionaryWriter(@dirName + "\\checkpoint.xml");
                // w.WriteObject(fileWriter, this);

            fileWriter.Close();
            Console.WriteLine("#DS: checkpoint " + CheckpointCounter + " from server " + Id + " done");
        }

        public static DataServer getCheckpoint(String dataServerId)
        {
            /*
            Console.WriteLine("#DS: getting checkpoint");
            System.Xml.Serialization.XmlSerializer reader =
                        new System.Xml.Serialization.XmlSerializer(typeof(DataServer));

            string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\" + dataServerId + "\\checkpoint.xml";
            System.IO.StreamReader fileReader = new System.IO.StreamReader(dirName);

            DataServer dataServer = new DataServer();
            dataServer = (DataServer)reader.Deserialize(fileReader);
            
            return dataServer;*/
            return new DataServer();
        }

        public void dump()
        {
            Console.WriteLine("#DS: Dumping!\r\n");
            Console.WriteLine(" URL: " + Url);
            Console.WriteLine(" Opened Files:");
            foreach (KeyValuePair<String, File> name in Files)
            {
                Console.WriteLine("\t " + name.Key);
            }
            Console.WriteLine();
        }

        public void fail()
        {
            State.fail();
        }

        public void recover()
        {
            State.recover();
        }

        public void freeze()
        {
            State.freeze();
        }

        public void unfreeze()
        {
            State.unfreeze();
        }

        public void setState(DSstate newState)
        {
            State = newState;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
