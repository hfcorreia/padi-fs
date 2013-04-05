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


namespace PuppetForm
{
    class PuppetMaster
    {
        public Dictionary<String, ServerObjectWrapper> clients = new Dictionary<String, ServerObjectWrapper>();      //<clientId, clientWrapper>
        public Dictionary<String, ServerObjectWrapper> dataServers = new Dictionary<String, ServerObjectWrapper>();  //<dataServerId, dataServerWrapper>
        public PuppetScriptExecutor ScriptExecutor { get; set; }
        public void createClient(String clientId)
        {
            if (!clients.ContainsKey(clientId))
            {
                int clientPort = Util.getNewPort();
                Process.Start("Client.exe", clientPort + " " + clientId);
                ServerObjectWrapper clientWrapper = new ServerObjectWrapper(clientPort, clientId, "localhost");
                clients.Add(clientId, clientWrapper);
            }
        }

        public void createDataServer(String id)
        {
            if (!dataServers.ContainsKey(id))
            {
                int port = Util.getNewPort();
                Process.Start("DataServer.exe", port + " " + id);
                ServerObjectWrapper dataServerWrapper = new ServerObjectWrapper(port, id, "localhost");

                dataServers.Add(id, dataServerWrapper);
            }
        }

        public void startMetaDataServers(int numServers)
        {
            //MetadataServers are fixed and their proprieties are specified in CommonTypes project
            foreach (ServerObjectWrapper metaDataWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                Process.Start("MetaDataServer.exe", metaDataWrapper.Port + " " + metaDataWrapper.Id);
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
            catch (CommonTypes.Exceptions.OpenFileException e)
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
            catch (CommonTypes.Exceptions.CloseFileException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
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
            catch (CommonTypes.Exceptions.CreateFileException e)
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
            catch (CommonTypes.Exceptions.DeleteFileException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }

        }

        public void fail(string process)
        {
            startProcess(process);
            System.Windows.Forms.MessageBox.Show("FAIL: Not Done Yet"); 
        }

        public void recover(string process) {
            startProcess(process);
            System.Windows.Forms.MessageBox.Show("RECOVER: Not Done Yet"); }

        public void freeze(string process) 
        {
            startProcess(process);
            System.Windows.Forms.MessageBox.Show("FREEZE: Not Done Yet"); 
        }

        public void unfreeze(string process) 
        {
            startProcess(process);
            System.Windows.Forms.MessageBox.Show("UNFREEZE: Not Done Yet"); 
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
            catch (Exception e)
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
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
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
            foreach (ServerObjectWrapper metadataWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                try
                {
                    metadataWrapper.getObject<IMetaDataServer>().exit();
                }
                catch (Exception e) { Console.WriteLine("Error Closing."); }
            }
            foreach (ServerObjectWrapper dataServerWrapper in dataServers.Values)
            {
                try
                {
                    dataServerWrapper.getObject<IDataServer>().exit();
                }
                catch (Exception e) { Console.WriteLine("Error Closing."); }
            }
            foreach (ServerObjectWrapper clientWrapper in clients.Values)
            {
                try
                {
                    clientWrapper.getObject<IClient>().exit();
                }
                catch (Exception e) { Console.WriteLine("Error Closing."); }
            }
            try
            {
                System.Environment.Exit(0);
            }
            catch (Exception e) { Console.WriteLine("Error exiting."); }
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
