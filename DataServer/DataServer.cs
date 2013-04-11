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

namespace DataServer
{
    public class DataServer : MarshalByRefObject, IDataServer
    {
        public int CheckpointCounter { get; set; }

        public String Id { get; set; }

        public int Port { get; set; }
        
        public string Host { get; set; }

        public string Url { get { return "tcp://" + Host +":" + Port + "/" + Id; } }

        public SerializableDictionary<string, File> Files { get; set; }

        
             
        private DSstate State { get; set; }

        //isto vai ter de levar um lock qualquer para quando esta a tratar dos pedidos na queue nao andar ng a mexer?
        internal List<BufferedRequest> requestsBuffer = new List<BufferedRequest>();
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

                Console.WriteLine("#DS: Registered "+ dataServer.Id + " at " + dataServer.Url);
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
            Console.WriteLine("#DS: writing file '" + file.FileName +"', version: " + file.Version + ", content: " + file.Content );

            State.write(file);

        }

        public File read(string filename)
        {
            Console.WriteLine("#DS: reading file '" + filename + "'");
			return State.read(filename);
        }


        public void registInMetadataServers()
        {
            Console.WriteLine("#DS: registering in MetadataServers");
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                metadataServerWrapper.getObject<IMetaDataServer>().registDataServer(Id, Host, Port);
            }
        }

        public int readFileVersion(string filename)
        {
            Console.WriteLine("#DS: reading file version for file'" + filename + "'");
            return State.readFileVersion(filename);
        }

        public void exit()
        {
            Console.WriteLine("#DS: Exiting!");
            System.Environment.Exit(0);
        }

        public void makeCheckpoint()
        {
            /*
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

            fileWriter.Close();*/
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

        public void queue(BufferedRequest request)
        {
            requestsBuffer.Add(request);

            Console.WriteLine("QUEUE DEBUG");
            foreach (BufferedRequest req in requestsBuffer)
            {
                Console.WriteLine("Buff: " + req.GetType());
            }
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

        //so existe porque nao consigo por state a public e a fazer checkpoints sem erros
        public void setState(DSstate newState)
        {
            State = newState;
        }

    }
}
