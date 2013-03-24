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
        public int Id { get; set; }

        public int Port { get; set; }
        
        public string Host { get; set; }

        public string Url { get { return "tcp://" + Host +":" + Port + "/" + Id; } }

        private Dictionary<string, File> files = new Dictionary<string, File>();

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: port metadataServerId");
                Console.ReadLine();
            }
            else
            {
                DataServer dataServer = new DataServer();
                dataServer.initialize(Int32.Parse(args[0]), Int32.Parse(args[1]), "localhost");
                dataServer.startConnection();

                ///File newFile = new File("dataServerSession", 1, new byte[] { 0110 });
                ///Util.writeToDisk(newFile, "" + "DS" + dataServer.Id);
                ///Util.readFromDisk("DS" + dataServer.Id, "dataServerSession", 1);

                Console.WriteLine("port: " + dataServer.Port + " Id: " + dataServer.Id + " url: " + dataServer.Url);
                Console.WriteLine("connection started");
                Console.ReadLine();
            }
        }

        public void initialize(int port, int id, string host)
        {
            Port = port;
            Id = id;
            Host = host;
        }

        public void startConnection()
        {

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = Port;
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(DataServer), "" + Id, WellKnownObjectMode.Singleton);
            registInMetadataServers();
            
        }


        public void write(File file)
        {
            Console.WriteLine("#DS " + Id + " write [filename: " + file.FileName + ", version:" + file.Version + "content: " + file.Content + "] - START");
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

            Util.writeToDisk(file, "" + "DS" + Id);

            Console.WriteLine("#DS " + Id + " write [filename: " + file.FileName + ", version:" + file.Version + "content: " + file.Content + "] - DONE!");
        }



        public File read(string filename)
        {
            Console.WriteLine("#DS " + Id + " read " + filename);
            if (filename == null || !files.ContainsKey(filename))
            {
                return null; //throw exception because the file does not exist
            }

            return Util.readFromDisk("DS" + Id, filename, files[filename].Version);
        }


        public void registInMetadataServers()
        {
            //we need to test this!
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                metadataServerWrapper.getObject<IMetaDataServer>().registDataServer(Id, Host, Port);
            }
        }

        public int readFileVersion(string filename)
        {
            return files.ContainsKey(filename) ? files[filename].Version : -1;
        }

        public void exit()
        {
            System.Environment.Exit(0);
        }

    }
}
