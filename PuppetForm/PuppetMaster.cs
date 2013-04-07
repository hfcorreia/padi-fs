using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Diagnostics;
using System.ComponentModel;
using CommonTypes;
using CommonTypes.Exceptions;
using System.Threading.Tasks;
using System.IO;


namespace PuppetForm
{
    class PuppetMaster
    {
        public Dictionary<String, ServerObjectWrapper> clients = new Dictionary<String, ServerObjectWrapper>();      //<clientId, clientWrapper>
        public Dictionary<String, ServerObjectWrapper> dataServers = new Dictionary<String, ServerObjectWrapper>();  //<dataServerId, dataServerWrapper>
        public PuppetScriptExecutor ScriptExecutor { get; set; }
       
        public void createClient(String clientId)
        {
            System.Windows.Forms.MessageBox.Show("Create Client: entrei");
            if (!clients.ContainsKey(clientId))
            {
                System.Windows.Forms.MessageBox.Show("Create Client: nao existe client " + clientId);
                int clientPort = Util.getNewPort();
                string path = Environment.CurrentDirectory;
                Process.Start(path + "\\Client.exe", clientPort + " " + clientId);
                System.Windows.Forms.MessageBox.Show("Create Client: process arrancado");
                ServerObjectWrapper clientWrapper = new ServerObjectWrapper(clientPort, clientId, "localhost");
                System.Windows.Forms.MessageBox.Show("Create Client: client wrapper");
                clients.Add(clientId, clientWrapper);
                System.Windows.Forms.MessageBox.Show("Create Client: client added");
            }
        }

        public void createDataServer(String id)
        {
            if (!dataServers.ContainsKey(id))
            {
                int port = Util.getNewPort();
                string path = Environment.CurrentDirectory;
                Process.Start(path + "\\DataServer.exe", port + " " + id);
                ServerObjectWrapper dataServerWrapper = new ServerObjectWrapper(port, id, "localhost");

                dataServers.Add(id, dataServerWrapper);
            }
        }

        public void startMetaDataServers(int numServers)
        {
            //MetadataServers are fixed and their proprieties are specified in CommonTypes project
            foreach (ServerObjectWrapper metaDataWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                string path = Environment.CurrentDirectory;
                Process.Start(path + "\\MetaDataServer.exe", metaDataWrapper.Port + " " + metaDataWrapper.Id);
            }
        }

        public void open(String clientId, string filename)
        {
            startProcess(clientId);
            
            ServerObjectWrapper sow = clients[clientId];

            IClient client = sow.getObject<IClient>();
            try
            {
                client.open(filename);
            }
            catch (OpenFileException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void close(String clientId, string filename)
        {
            startProcess(clientId);

            ServerObjectWrapper sow = clients[clientId];

            IClient client = sow.getObject<IClient>();
            try
            {
                client.close(filename);
            }
            catch (CloseFileException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void create(String clientId, string filename, int numberDataServers, int readQuorum, int writeQuorum)
        {
            System.Windows.Forms.MessageBox.Show("Create: entrei");
            startProcess(clientId);
            System.Windows.Forms.MessageBox.Show("Create: process started");
            ServerObjectWrapper sow = clients[clientId];
            System.Windows.Forms.MessageBox.Show("Create: tenho o wrapper");
            IClient client = sow.getObject<IClient>();
            System.Windows.Forms.MessageBox.Show("Create: tenho o client");
            try
            {
                FileMetadata metadata = client.create(filename, numberDataServers, readQuorum, writeQuorum);
                System.Windows.Forms.MessageBox.Show("Create: file created at MD");
            }
            catch (CreateFileException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void delete(String clientId, string filename)
        {
            startProcess(clientId);

            ServerObjectWrapper sow = clients[clientId];

            IClient client = sow.getObject<IClient>();

            try
            {
                client.delete(filename);
            }
            catch (DeleteFileException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }

        }

        public void failDS(string process)
        {

            startProcess(process);

            ServerObjectWrapper sow = dataServers[process];

            IDataServer dataServer = sow.getObject<IDataServer>();

            dataServer.fail();

        }

        public void failMD(string process)
        {

            //not tested
            startProcess(process);

            ServerObjectWrapper sow = MetaInformationReader.Instance.getMetadataById(process);

            IMetaDataServer metaData = sow.getObject<IMetaDataServer>();

            metaData.fail();

        }

        public void recoverDS(string process)
        {
            startProcess(process);

            ServerObjectWrapper sow = dataServers[process];

            IDataServer dataServer = sow.getObject<IDataServer>();

            dataServer.recover();
        }

        public void recoverMD(string process)
        {
            //not tested
            startProcess(process);

            ServerObjectWrapper sow = MetaInformationReader.Instance.getMetadataById(process);

            IMetaDataServer metaData = sow.getObject<IMetaDataServer>();

            metaData.recover();
        }

        public void freeze(string process) 
        {
            startProcess(process);

            ServerObjectWrapper sow = dataServers[process];

            IDataServer dataServer = sow.getObject<IDataServer>();

            dataServer.freeze();
        }

        public void unfreeze(string process) 
        {
            startProcess(process);

            ServerObjectWrapper sow = dataServers[process];

            IDataServer dataServer = sow.getObject<IDataServer>();

            dataServer.unfreeze();
        }

        public void read(string process, int fileRegisterId, string semantics, int stringRegisterId)
        {
            startProcess(process);

            ServerObjectWrapper sow = clients[process];

            IClient client = sow.getObject<IClient>();

            try
            {
                client.read(fileRegisterId, semantics, stringRegisterId);
            }
            catch (PadiFsException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void write(string process, int fileRegisterId, string contents)
        {
            byte[] byteArrayRegisterContent = System.Text.Encoding.UTF8.GetBytes(contents);

            startProcess(process);

            ServerObjectWrapper sow = clients[process];

            IClient client = sow.getObject<IClient>();

            try
            {
                client.write(fileRegisterId, byteArrayRegisterContent);
            }
            catch (PadiFsException e)
            {

                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void write(string process, int fileRegisterId, int stringRegisterId)
        {
            startProcess(process);

            ServerObjectWrapper sow = clients[process];

            IClient client = sow.getObject<IClient>();

            try
            {
                client.write(fileRegisterId, stringRegisterId);
            }
            catch (PadiFsException e)
            {
                System.Windows.Forms.MessageBox.Show(e.StackTrace);
            }
        }
        

        public void copy(string process, string fileRegister1, string semantics, string fileRegister2, string salt)
        {
            startProcess(process);

            System.Windows.Forms.MessageBox.Show("COPY: Not Done Yet");
        }

        public void dump(string process)
        {
            startProcess(process);

            ServerObjectWrapper sow = getRemoteObjectWrapper(process);

            IRemote obj = sow.getObject<IClient>();

            obj.dump();
        }

        private ServerObjectWrapper getRemoteObjectWrapper(string process)
        {
            if (process.StartsWith("c-"))
            {
                return clients[process];
            }
            else if (process.StartsWith("d-"))
            {
                return dataServers[process];
            }
            else if (process.StartsWith("m-"))
            {
                return  MetaInformationReader.Instance.getMetadataById(process);
            }
            //change this
            return null;
        }

        private void startProcess(string process)
        {
            if (process.StartsWith("c-"))
            {
                createClient(process);
            }
            else if (process.StartsWith("d-"))
            {
                createDataServer(process);
            }
            else if (process.StartsWith("m-"))
            {
                /** not implemented only need when mds fail **/
            }
        }
        public void exeClientScript(string process, string filename)
        {
            startProcess(process);

            ServerObjectWrapper sow = clients[process];

            IClient client = sow.getObject<IClient>();
            try
            {
                client.exeScript(filename);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void exeScript(Boolean oneStep)
        {
            if (ScriptExecutor != null)
            {
                ScriptExecutor.runScript(oneStep);
            }
            else throw new PadiFsException("Load Script First");
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

            Application.Run(new ControlBoard());
            

        }

        private static bool processIsRunning(string processName)
        {
            return (System.Diagnostics.Process.GetProcessesByName(processName).Length != 0);
        }

        public void exitAll()
        {
            List<ServerObjectWrapper> allRemoteObjects = new List<ServerObjectWrapper>();
            allRemoteObjects.AddRange(MetaInformationReader.Instance.MetaDataServers);
            allRemoteObjects.AddRange(dataServers.Values);
            allRemoteObjects.AddRange(clients.Values);

            Task[] tasks = new Task[allRemoteObjects.Count];

            for (int id = 0; id < allRemoteObjects.Count; id++)
            {

                    IRemote remoteObject = allRemoteObjects[id].getObject<IRemote>();
                    tasks[id] = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            remoteObject.exit();
                        }
                        catch (IOException e) { /*DO NOTHING*/ }
                        catch (Exception e) { /*DO NOTHING*/ }
                    });
            }
            
            try
            {
                Task.WaitAll(tasks);
            }
            catch (Exception e) {}
            
            //Application.Exit();
        }
        
       public List<string> stringRegistersForClient(string clientId)
        {
            createClient(clientId);

            ServerObjectWrapper sow = clients[clientId];

            IClient client = sow.getObject<IClient>();

            try
            {
                return client.getAllStringRegisters();
            }
            catch (CommonTypes.Exceptions.DeleteFileException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }

            return new List<string>();
        }

       public List<string> fileRegistersForClient(string clientId)
       {
           createClient(clientId);

           ServerObjectWrapper sow = clients[clientId];

           IClient client = sow.getObject<IClient>();

           try
           {
               return client.getAllFileRegisters();
           }
           catch (CommonTypes.Exceptions.DeleteFileException e)
           {
               System.Windows.Forms.MessageBox.Show(e.Message);
           }

           return new List<string>();
       }

       public void dumpAllMds()
       {
           foreach (ServerObjectWrapper metaDataWrapper in MetaInformationReader.Instance.MetaDataServers)
           {
               dump(metaDataWrapper.Id);
           }
       }
    }
}
