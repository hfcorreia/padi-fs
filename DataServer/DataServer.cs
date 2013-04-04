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

namespace DataServer
{
    public class DataServer : MarshalByRefObject, IDataServer
    {
        public String Id { get; set; }

        public int Port { get; set; }
        
        public string Host { get; set; }

        public string Url { get { return "tcp://" + Host +":" + Port + "/" + Id; } }

        private Dictionary<string, File> files = new Dictionary<string, File>();


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
            if (file == null)
            {
                return;
            }

            if (files.ContainsKey(file.FileName))
            {
                //updates the file
                files.Remove(file.FileName);
            }
            //creates a new file
            files.Add(file.FileName, file);

            Util.writeFileToDisk(file, Id);
            makeCheckpoint();
        }

        public File read(string filename)
        {
            Console.WriteLine("#DS: reading file '" + filename + "'");
            if (filename == null || !files.ContainsKey(filename))
            {
                return null; //throw exception because the file does not exist
            }

            return Util.readFileFromDisk(Id, filename, files[filename].Version);
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
            return files.ContainsKey(filename) ? files[filename].Version : -1;
        }

        public void exit()
        {
            Console.WriteLine("#DS: Exiting!");
            System.Environment.Exit(0);
        }

        public void makeCheckpoint()
        {
            
            String dataServerId = Id;
            Console.WriteLine("#DS: making checkpoint " + Id);

            string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\" + dataServerId;
            Util.createDir(dirName);

            System.Xml.Serialization.XmlSerializer writer =
            new System.Xml.Serialization.XmlSerializer(typeof(DataServer));

            System.IO.StreamWriter fileWriter = new System.IO.StreamWriter(@dirName + "\\checkpoint.xml");
            writer.Serialize(fileWriter, this);
            fileWriter.Close();
        }

        public static DataServer getCheckpoint(String dataServerId)
        {
            Console.WriteLine("#DS: getting checkpoint");
            System.Xml.Serialization.XmlSerializer reader =
                      new System.Xml.Serialization.XmlSerializer(typeof(DataServer));

            string dirName = CommonTypes.Properties.Resources.TEMP_DIR + "\\" + dataServerId + "\\checkpoint.xml"; 
            System.IO.StreamReader fileReader = new System.IO.StreamReader(dirName);

            DataServer dataServer = new DataServer();
            dataServer = (DataServer)reader.Deserialize(fileReader);

            return dataServer;
        }

        public void dump()
        {
            Console.WriteLine("#DS: Dumping!");
        }
    }
}
