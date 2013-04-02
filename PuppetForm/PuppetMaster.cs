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


namespace PuppetForm
{
    class PuppetMaster
    {

        private Dictionary<int, ServerObjectWrapper> clients = new Dictionary<int, ServerObjectWrapper>();      //<clientId, clientWrapper>
        private Dictionary<int, ServerObjectWrapper> dataServers = new Dictionary<int, ServerObjectWrapper>();  //<dataServerId, dataServerWrapper>
        public System.IO.StreamReader LoadedScriptReader { get; set; }

        public void createClient(int clientPort, int clientId)
        {
            Process.Start("Client.exe", clientPort + " " + clientId);
            ServerObjectWrapper clientWrapper = new ServerObjectWrapper(clientPort, clientId, "localhost");

            if (!clients.ContainsKey(clientId))
            {
                clients.Add(clientId, clientWrapper);
            }
        }

        public void createDataServer(int port, int id)
        {
            Process.Start("DataServer.exe", port + " " + id);
            ServerObjectWrapper dataServerWrapper = new ServerObjectWrapper(port, id, "localhost");
            if (!dataServers.ContainsKey(id))
            {
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
        public void open(int clientId, string filename)
        {
            ServerObjectWrapper sow = clients[clientId];

            IClient client = sow.getObject<IClient>();
            try
            {
                client.open(clientId, filename);
            }
            catch (CommonTypes.Exceptions.OpenFileException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void close(int clientId, string filename)
        {
            ServerObjectWrapper sow = clients[clientId];

            IClient client = sow.getObject<IClient>();
            try
            {
                client.close(clientId, filename);
            }
            catch (CommonTypes.Exceptions.CloseFileException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void create(int clientId, string filename, int numberDataServers, int readQuorum, int writeQuorum)
        {
            ServerObjectWrapper sow = clients[clientId];

            IClient client = sow.getObject<IClient>();

            try
            {
                client.create(filename, numberDataServers, readQuorum, writeQuorum);
            }
            catch (CommonTypes.Exceptions.CreateFileException e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }
        }

        public void delete(int clientId, string filename)
        {
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

        public void fail(string process) { System.Windows.Forms.MessageBox.Show("FAIL: Not Done Yet"); }

        public void recover(string process) { System.Windows.Forms.MessageBox.Show("RECOVER: Not Done Yet"); }

        public void freeze(string process) { System.Windows.Forms.MessageBox.Show("FREEZE: Not Done Yet"); }

        public void unfreeze(string process) { System.Windows.Forms.MessageBox.Show("UNFREEZE: Not Done Yet"); }

        public void read(string process, string fileRegister, string semantics, string stringRegister) {
            System.Windows.Forms.MessageBox.Show("READ: Not Done Yet");
        }

        public void write(string process, string fileRegister, byte[] byteArrayRegister) {
            System.Windows.Forms.MessageBox.Show("WRITE: Not Done Yet");
        }

        public void write(string process, string fileRegister, string contents) {
            System.Windows.Forms.MessageBox.Show("WRITE: Not Done Yet");
        }

        public void copy(string process, string fileRegister1, string semantics, string fileRegister2, string salt) {
            System.Windows.Forms.MessageBox.Show("COPY: Not Done Yet");
        }

        public void dump(string process) {
            System.Windows.Forms.MessageBox.Show("DUMP: Not Done Yet");
        }

        public void exeScriptCommand(String line)
        {
                String[] input = line.Split(' ');
                switch(input[0])
                {
                    case "open":
                        open(Int32.Parse(input[1]), input[2]);
                        break;
                    case "close":
                        close(Int32.Parse(input[1]), input[2]);
                        break;
                    case "create":
                        create(Int32.Parse(input[1]), input[2], Int32.Parse(input[3]), Int32.Parse(input[4]), Int32.Parse(input[5]));
                        break;
                    case "delete":
                        delete(Int32.Parse(input[1]), input[2]);
                        break;
                    case "write":
                        write(input[1], input[2], input[3]);
                        break;
                    case "read":
                        read(input[1], input[2], input[3], input[4]);
                        break;
                    case "copy":
                        copy(input[1], input[2], input[3], input[4], input[5]);
                        break;
                    case "dump":
                        dump(input[1]);
                        break;
                    case "fail":
                        fail(input[1]);
                        break;
                    case "recover":
                        recover(input[1]);
                        break;
                    case "freeze":
                        freeze(input[1]);
                        break;
                    case "unfreeze":
                        break;
                    case "#":
                        break;
                    default:
                        System.Windows.Forms.MessageBox.Show("No such command: " + input[0] + "!");
                        break;
                }
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
    }
}
