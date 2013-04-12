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
using System.Threading.Tasks;
using Client.services;
using System.Diagnostics;

namespace Client
{
    [Serializable]
    public class Client : MarshalByRefObject, IClient
    {
        public ClientState ClientState { get; set; }

        #region INITIALIZATION
      

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
                Console.WriteLine("#Client: Registered " + client.ClientState.Id + " at " + client.ClientState.Url);
                Console.ReadLine();
            }
        }

        public void initialize(int port, string id)
        {
            ClientState = new ClientState(port, id);
            
            //atach a debugger - we should add some parameter to enable/disable this!
            if (Boolean.Parse(Properties.Resources.RUN_IN_DEBUG_MODE) && !Debugger.IsAttached)
            {
                Debugger.Launch();
            }

        }

        public void startConnection(Client client)
        {
            StartConnectionService startConnectionService = new StartConnectionService(ClientState, client);
            startConnectionService.execute();
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
        #endregion INITIALIZATION
        
        #region WRITE

        public void write(int fileRegisterId, byte[] content) 
        {
            if (ClientState.FileMetadataContainer.hasNullContent(fileRegisterId))
            {
                throw new WriteFileException("Client - The file is not open " + fileRegisterId);
            }

            FileMetadata fileMetadata = ClientState.FileMetadataContainer.getFileMetadata(fileRegisterId);

            File file = new File(fileMetadata.FileName, -1, content);

            write(file);
        }

        public void write(int fileRegisterId, int stringRegisterId)
        {
            if (ClientState.FileMetadataContainer.hasNullContent(fileRegisterId))
            {
                throw new WriteFileException("Client - the file is not open " + fileRegisterId);
            }

            if (ClientState.FileContentContainer.hasNullContent(stringRegisterId))
            {
                throw new WriteFileException("Client - We have no content at StringRegister " + stringRegisterId);
            }

            FileMetadata fileMetadata = ClientState.FileMetadataContainer.getFileMetadata(fileRegisterId);

            byte[] fileContent = ClientState.FileContentContainer.getFileContent(stringRegisterId).Content;

            File file = new File(fileMetadata.FileName, -1, fileContent);

            write(file);
        }

        private void write(File file)
        {
            file.Version = readFileVersion(file.FileName) + 1;
            file.clientId = ClientState.Id;

            WriteFileService writeFileService = new WriteFileService(ClientState, file);
            writeFileService.execute();

            ClientState.FileContentContainer.addFileContent(writeFileService.NewFile);
            ClientState.saveMostRecentVersion(writeFileService.NewFile.FileName, writeFileService.NewFile.Version);
        }
        
        #endregion WRITE

        private int readFileVersion(string filename)
        {
            ReadFileVersionService readFileVersionService = new ReadFileVersionService(ClientState, filename);
            readFileVersionService.execute();
            return readFileVersionService.FileVersion;
        }

        public File read(int fileRegisterId, string semantics, int stringRegisterId)
        {
            Console.WriteLine("client read in registers (file, string) : (" + fileRegisterId + " , " + stringRegisterId + ")");
            ReadFileService readFileService = new ReadFileService(ClientState, semantics, fileRegisterId);
            readFileService.execute();

            ClientState.FileContentContainer.setFileContent(stringRegisterId, readFileService.ReadedFile);
            ClientState.saveMostRecentVersion(readFileService.ReadedFile.FileName, readFileService.ReadedFile.Version);
            return readFileService.ReadedFile;
        }

        public File read(int fileRegisterId, string semantics)
        {
            ReadFileService readFileService = new ReadFileService(ClientState, semantics, fileRegisterId);
            readFileService.execute();

            ClientState.FileContentContainer.addFileContent(readFileService.ReadedFile);
            ClientState.saveMostRecentVersion(readFileService.ReadedFile.FileName, readFileService.ReadedFile.Version);
            return readFileService.ReadedFile;
        }

        public void open(string filename)
        {
            OpenFileService openFileService = new OpenFileService(ClientState, filename);
            openFileService.execute();
        }

        public void close(string filename)
        {
            CloseFileService closeFileService = new CloseFileService(ClientState, filename);
            closeFileService.execute();
        }

        public void delete(string filename)
        {
            if (ClientState.FileMetadataContainer.containsFileMetadata(filename)) 
            {
                close(filename);
            }

            DeleteFileService deleteFileService = new DeleteFileService(ClientState, filename);
            deleteFileService.execute();
        }

        public FileMetadata create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum)
        {
            CreateFileService createFileService = new CreateFileService(ClientState, filename, numberOfDataServers, readQuorum, writeQuorum);
            createFileService.execute();

            return createFileService.CreatedFileMetadata;
        }
        
        #region SCRIPT
        
        public void exeScript(string filename)
        {
            new ClientScriptExecutor(this, filename).runScriptFile();
        }

        public List<string> getAllFileRegisters()
        {
            return ClientState.FileMetadataContainer.getAllFileNames();
        }
        
        #endregion SCRIPT

        public List<string> getAllStringRegisters() {
            return ClientState.FileContentContainer.getAllFileContentAsString();
        }

        public void exit()
        {
            Console.WriteLine("#Client: Exiting!");
            System.Environment.Exit(0);
        }

        public void copy(int sourceFileRegisterId, string semantics, int targetFileRegisterId, string salt) 
        { 
            Console.WriteLine("#Client: copy from " + sourceFileRegisterId + " to " + targetFileRegisterId + " with salt " + salt);

            ReadFileService readFileService = new ReadFileService(ClientState, semantics, sourceFileRegisterId);
            readFileService.execute();

            byte[] newBytes = appendBytes(readFileService.ReadedFile.Content, salt);
            
            write(targetFileRegisterId, newBytes);
        }

        public byte[] appendBytes(byte[] original, string salt)
        {
            byte[] saltBytes = System.Text.Encoding.UTF8.GetBytes(salt);
            var contentVar = new System.IO.MemoryStream();
            contentVar.Write(original, 0, original.Length);
            contentVar.Write(saltBytes, 0, saltBytes.Length);
            return contentVar.ToArray();
        }

        public void dump()
        {
            Console.WriteLine("#Client: Dumping!\r\n");
            Console.WriteLine(" URL: " + ClientState.Url);
            Console.WriteLine(" Opened Files:");
            foreach (String name in ClientState.FileMetadataContainer.getAllFileNames())
            {
                Console.WriteLine("\t" + name);
            }
            Console.WriteLine();
        }
    
    }
}
