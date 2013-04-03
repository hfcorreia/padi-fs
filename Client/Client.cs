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
using CommonTypes.Exceptions;

namespace Client
{
    [Serializable]
    public class Client : MarshalByRefObject, IClient
    {
        private static int INITIAL_FILE_VERSION = 0;
        private static byte[] INITIAL_FILE_CONTENT = new byte[] { };

        private int Port { get; set; }
        private String Id { get; set; }
        private String Url { get { return "tcp://localhost:" + Port + "/" + Id; } }
        //private Dictionary<String, List<ServerObjectWrapper>> fileServers = new Dictionary<string, List<ServerObjectWrapper>>();
        FileMetadataContainer fileMetadataContainer = new FileMetadataContainer(Int32.Parse(Properties.Resources.FILE_REGISTER_CAPACITY));
        FileContentContainer fileContentContainer = new FileContentContainer(Int32.Parse(Properties.Resources.FILE_STRING_CAPACITY));
        
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
            Console.WriteLine("#Client: starting connection..");
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = Port;
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(client, Id, typeof(Client));
        }

        public void write(string filename, byte[] fileContent)
        {
            Console.WriteLine("#Client: writing file '" + filename + "' with content: '" + fileContent + "'");
            int fileVersion = readFileVersion(filename);
            File file = new File(filename, fileVersion, fileContent);
            if (fileMetadataContainer.containsFileMetadata(file.FileName))
            {
                foreach (ServerObjectWrapper dataServerWrapper in fileMetadataContainer.getFileMetadata(file.FileName).FileServers)
                {
                    dataServerWrapper.getObject<IDataServer>().write(file);
                }
            }
        }

        private int readFileVersion(string filename)
        {
            Console.WriteLine("#Client: reading file version for file '" + filename + "'");
            int fileVersion = 0;
            if (fileMetadataContainer.containsFileMetadata(filename))
            {
                foreach (ServerObjectWrapper dataServerWrapper in fileMetadataContainer.getFileMetadata(filename).FileServers)
                {
                    fileVersion = dataServerWrapper.getObject<IDataServer>().readFileVersion(filename);
                }
            }

            return fileVersion;
        }


        public void read(string filename) {
            Console.WriteLine("#Client: reading file '" + filename + "' -> NOT DONE!");
        }

        public void open(String clientId, string filename)
        {
            Console.WriteLine("#Client: opening file '" + filename + "'");
            FileMetadata fileMetadata = null;
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                fileMetadata = metadataServerWrapper.getObject<IMetaDataServer>().open(clientId, filename);
                //cacheServersForFile(filename, servers);
               
            }
            fileMetadataContainer.addFileMetadata(fileMetadata);
        }

        public void close(String clientId, string filename)
        {
            Console.WriteLine("#Client: closing file " + filename);
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                metadataServerWrapper.getObject<IMetaDataServer>().close(clientId, filename);
                //removeCacheServersForFile(filename);
                fileMetadataContainer.removeFileMetadata(filename);
                fileContentContainer.removeFileContent(filename);
            }
        }



        public void delete(string filename)
        {
            Console.WriteLine("#Client: Deleting file " + filename);
            if (fileMetadataContainer.containsFileMetadata(filename))
            {
                foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
                {
                    metadataServerWrapper.getObject<IMetaDataServer>().delete(Id, filename);
                }
                fileMetadataContainer.removeFileMetadata(filename);
                fileContentContainer.removeFileContent(filename);
            }
            else {
                throw new DeleteFileException("Trying to delete a file that is not in the file-register.");
            }
        }

        public FileMetadata create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            Console.WriteLine("#Client: creating file '" + filename + "' in " + numberOfDataServers + " servers. ReadQ: " + readQuorum + ", WriteQ:" + writeQuorum);
            //List<ServerObjectWrapper> dataserverForFile = null;
            FileMetadata fileMetadata = null;
            foreach (ServerObjectWrapper metadataServerWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                fileMetadata = metadataServerWrapper.getObject<IMetaDataServer>().create(Id, filename, numberOfDataServers, readQuorum, writeQuorum);
            }

            //cacheServersForFile(filename, fileMetadata);
            fileMetadataContainer.addFileMetadata(fileMetadata);

            //File emptyFile = new File(filename, INITIAL_FILE_VERSION, INITIAL_FILE_CONTENT);
            write(filename, INITIAL_FILE_CONTENT); //writes an empty file

            return fileMetadata;
        }

        public List<string> getAllFileRegisters() 
        {
            return fileMetadataContainer.getAllFileNames();
        }
        public List<string> getAllStringRegisters() { 
            return fileContentContainer.getAllFileNames(); 
        }

        public void exit()
        {
            Console.WriteLine("#Client: bye ='( ");
            System.Environment.Exit(0);
        }
    }
}
