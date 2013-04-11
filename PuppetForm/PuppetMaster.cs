using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading.Tasks;
using System.Windows.Forms;
using CommonTypes;
using CommonTypes.Exceptions;



namespace PuppetForm
{
    class PuppetMaster
    {
        public Dictionary<String, ServerObjectWrapper> clients = new Dictionary<String, ServerObjectWrapper>();      //<clientId, clientWrapper>
        public Dictionary<String, ServerObjectWrapper> dataServers = new Dictionary<String, ServerObjectWrapper>();  //<dataServerId, dataServerWrapper>
        public PuppetScriptExecutor ScriptExecutor { get; set; }
        public ControlBoard ControlBoard { get; set; }
       
        public PuppetMaster(ControlBoard cb) {
            ControlBoard = cb;
        }

        public void createClient(String clientId)
        {
            if (!clients.ContainsKey(clientId))
            {
                int clientPort = Util.getNewPort();

                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                
                try
                {
                    Process.Start(path + "\\Client.exe", clientPort + " " + clientId);
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show("Error creating client:" + e.Message + " :\n" + e.StackTrace);
                }
                ServerObjectWrapper clientWrapper = new ServerObjectWrapper(clientPort, clientId, "localhost");
                clients.Add(clientId, clientWrapper);

                ControlBoard.printCommand("CREATE " + clientId);
            }
        }

        public void createDataServer(String id)
        {
            if (!dataServers.ContainsKey(id))
            {
                int port = Util.getNewPort();
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                Process.Start(path + "\\DataServer.exe", port + " " + id);
                ServerObjectWrapper dataServerWrapper = new ServerObjectWrapper(port, id, "localhost");

                dataServers.Add(id, dataServerWrapper);
                ControlBoard.printCommand("CREATE " + id);
            }
        }

        public void startMetaDataServers(int numServers)
        {
            //MetadataServers are fixed and their proprieties are specified in CommonTypes project
            foreach (ServerObjectWrapper metaDataWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
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
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error opening file " + filename + " : " + e.Message + " :\n" + e.StackTrace);
            }

            ControlBoard.printCommand("OPEN " + clientId + " " + filename);
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
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error closing file " + filename + " : " + e.Message + " :\n" + e.StackTrace);
            }

            ControlBoard.printCommand("CLOSE " + clientId + " " + filename);
        }

        public void create(String clientId, string filename, int numberDataServers, int readQuorum, int writeQuorum)
        {
            startProcess(clientId);
            ServerObjectWrapper sow = clients[clientId];
            IClient client = sow.getObject<IClient>();
            try
            {
                FileMetadata metadata = client.create(filename, numberDataServers, readQuorum, writeQuorum);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error creating file " + filename + ":" + e.Message + " :\n" + e.StackTrace);
            }

            ControlBoard.printCommand("CREATE " + clientId + " " + filename + " " + numberDataServers + " " + readQuorum + " " + writeQuorum);
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
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error deleting file " + filename + ":" + e.Message + " :\n" + e.StackTrace);
            }

            ControlBoard.printCommand("DELETE " + clientId + " " + filename);
        }

        public void failDS(string process)
        {
            try
            {
                startProcess(process);

                ServerObjectWrapper sow = dataServers[process];

                IDataServer dataServer = sow.getObject<IDataServer>();

                dataServer.fail();
            }
            catch (Exception e) 
            {
                System.Windows.Forms.MessageBox.Show("Error failing DS" + process + ":" + e.Message + " :\n" + e.StackTrace);
            }
            ControlBoard.printCommand("FAIL " + process);

        }

        public void failMD(string process)
        {
            try
            {
                //not tested
                startProcess(process);

                ServerObjectWrapper sow = MetaInformationReader.Instance.getMetadataById(process);

                IMetaDataServer metaData = sow.getObject<IMetaDataServer>();

                metaData.fail();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error failing MD" + process + ":" + e.Message + " :\n" + e.StackTrace);
            }
            ControlBoard.printCommand("FAIL " + process);

        }

        public void recoverDS(string process)
        {
            try
            {
                startProcess(process);

                ServerObjectWrapper sow = dataServers[process];

                IDataServer dataServer = sow.getObject<IDataServer>();

                dataServer.recover();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error failing MD" + process + ":" + e.Message + " :\n" + e.StackTrace);
            }
            ControlBoard.printCommand("RECOVER " + process);
        }

        public void recoverMD(string process)
        {
            try
            {
                //not tested
                startProcess(process);

                ServerObjectWrapper sow = MetaInformationReader.Instance.getMetadataById(process);

                IMetaDataServer metaData = sow.getObject<IMetaDataServer>();

                metaData.recover();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error recovering MD" + process + ":" + e.Message + " :\n" + e.StackTrace);
            }
            ControlBoard.printCommand("RECOVER " + process);
        }

        public void freeze(string process) 
        {
            try
            {
                startProcess(process);

                ServerObjectWrapper sow = dataServers[process];

                IDataServer dataServer = sow.getObject<IDataServer>();

                dataServer.freeze();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error freezing MD" + process + ":" + e.Message + " :\n" + e.StackTrace);
            }
            ControlBoard.printCommand("FREEZE " + process);
        }

        public void unfreeze(string process) 
        {
            try
            {
                startProcess(process);

                ServerObjectWrapper sow = dataServers[process];

                IDataServer dataServer = sow.getObject<IDataServer>();

                dataServer.unfreeze();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error unfreezing MD" + process + ":" + e.Message + " :\n" + e.StackTrace);
            }
            ControlBoard.printCommand("UNFREEZE " + process);
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
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error read file" + fileRegisterId+ ":" + e.Message + " :\n" + e.StackTrace);
            }

            ControlBoard.printCommand("READ " + process + " " + fileRegisterId + " " + semantics + " " + stringRegisterId);
        }

        public void read(string process, int fileRegisterId, string semantics)
        {
            startProcess(process);

            ServerObjectWrapper sow = clients[process];

            IClient client = sow.getObject<IClient>();

            try
            {
                client.read(fileRegisterId, semantics);
            }
            catch (PadiFsException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }

            ControlBoard.printCommand("READ " + process + " " + fileRegisterId + " " + semantics );
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
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error write file" + fileRegisterId + ":" + e.Message + " :\n" + e.StackTrace);
            }

            ControlBoard.printCommand("WRITE " + process + " " + fileRegisterId + " " + contents);
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
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error write file" + fileRegisterId + ":" + e.Message + " :\n" + e.StackTrace);
            }

            ControlBoard.printCommand("WRITE " + process + " " + fileRegisterId + " " + stringRegisterId);
        }


        public void copy(string process, int fileRegister1, string semantics, int fileRegister2, string salt)
        {
            try
            {
                startProcess(process);

                ServerObjectWrapper sow = getRemoteObjectWrapper(process);

                IClient client = sow.getObject<IClient>();

                client.copy(fileRegister1, semantics, fileRegister2, salt);
            }
            catch (Exception e) 
            {
                System.Windows.Forms.MessageBox.Show("Error copying file" + fileRegister1 + " to " +fileRegister2 + ":" + e.Message + " :\n" + e.StackTrace);
            }
            ControlBoard.printCommand("COPY " + process + " " + fileRegister1 + " " + semantics + " " + fileRegister2 + " " + salt);
        }

        public void dump(string process)
        {
            startProcess(process);

            ServerObjectWrapper sow = getRemoteObjectWrapper(process);

            IRemote obj = sow.getObject<IClient>();
            try
            {
                obj.dump();
            }
            catch (Exception e) 
            {
                System.Windows.Forms.MessageBox.Show("Error dumping process " + process + ":" + e.Message + " :\n" + e.StackTrace);
            }
            ControlBoard.printCommand("DUMP " + process);
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
                System.Windows.Forms.MessageBox.Show("Error executing client script for client " + process + ":" + e.Message + " :\n" + e.StackTrace);
            }

            ControlBoard.printCommand("EXESCRIPT " + process + " " + filename);
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
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Error getting string Registers For Client " + clientId + ":" + e.Message + " :\n" + e.StackTrace);
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
           catch (Exception e)
           {
               System.Windows.Forms.MessageBox.Show("Error getting file Registers For Client " + clientId + ":" + e.Message + " :\n" + e.StackTrace);
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
