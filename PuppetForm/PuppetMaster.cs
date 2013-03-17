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

        private Dictionary<String, IClient> clients = new Dictionary<string, IClient>();
        private Dictionary<int, IMetaDataServer> metadataServers = new Dictionary<int, IMetaDataServer>();
        private Dictionary<int, IDataServer> dataServers = new Dictionary<int, IDataServer>();

        public void createClient(String clientPort, String clientName)
        {
            Process.Start("Client.exe", clientPort + " " + clientName);

            string clientURL = "tcp://localhost:" + clientPort + "/" + clientName;
            IClient client = (IClient) Activator.GetObject(typeof(IClient), clientURL);
            clients.Add(clientName, client);
        }

        public void startMetaDataServers(int numServers)
        {
            int startingPort = 10000;
            for (int i = 0; i < numServers; ++i) 
            {
                createMetaDataServer(startingPort + i, i);
            }
        }

        public void createMetaDataServer(int port, int id)
        {
            Process.Start("MetaDataServer.exe", port + " " + id);

            string clientURL = "tcp://localhost:" + port + "/" + id;
            IMetaDataServer metadataServer = (IMetaDataServer)Activator.GetObject(typeof(IMetaDataServer), clientURL);
            metadataServers.Add(id, metadataServer);
        }

        public void createDataServer(int port, int id)
        {
            Process.Start("DataServer.exe", port + " " + id);

            string dataserverURL = "tcp://localhost:" + port + "/" + id;
            IDataServer dataServer = (IDataServer)Activator.GetObject(typeof(IDataServer), dataserverURL);
            dataServers.Add(id, dataServer);
        }

        public void fail(string process) { }

        public void recover(string process) { }

        public void freeze(string process) { }

        public void unfreeze(string process) { }

        public void create(string process, string filename, int numberDataServers, int readQuorum, int writeQuorum) { }

        public void open(string process, string filename) { }

        public void close(string process, string filename) { }

        public void read(string process, string fileRegister, string semantics, string stringRegister) { }

        public void write(string process, string fileRegister, byte[] byteArrayRegister) { }

        public void write(string process, string fileRegister, string contents) { }

        public void copy(string process, string fileRegister1, string semantics, string fileRegister2, string salt) { }

        public void dump(string process) { }

        public void exeScript(string process, string filename) { }


        internal void test()
        {
            for(int md=0; md<3; ++md)
            {
                metadataServers[md].open("HELLO WORLD");
            }
        }
    }
}
