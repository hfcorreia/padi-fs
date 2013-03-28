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

        public void createClient(int clientPort, int clientId)
        {
            Process.Start( "Client.exe", clientPort + " " + clientId );
            ServerObjectWrapper clientWrapper = new ServerObjectWrapper( clientPort, clientId, "localhost" );

            if (!clients.ContainsKey(clientId))
            {
                clients.Add(clientId, clientWrapper);
            }
        }

        public void createDataServer( int port, int id )
        {
            Process.Start( "DataServer.exe", port + " " + id );
            ServerObjectWrapper dataServerWrapper = new ServerObjectWrapper( port, id, "localhost" );
            if (!dataServers.ContainsKey(id))
            {
                dataServers.Add(id, dataServerWrapper);
            }
        }
        
        public void startMetaDataServers( int numServers )
        {
            //MetadataServers are fixed and their proprieties are specified in CommonTypes project
            foreach ( ServerObjectWrapper metaDataWrapper in MetaInformationReader.Instance.MetaDataServers )
            {
                Process.Start( "MetaDataServer.exe", metaDataWrapper.Port + " " + metaDataWrapper.Id );
            }
        }
        public void open(int clientId, string filename) {
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

        public void close(int clientId, string filename) {
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

        public void fail(string process) { }

        public void recover(string process) { }

        public void freeze(string process) { }

        public void unfreeze(string process) { }

        public void read(string process, string fileRegister, string semantics, string stringRegister) { }

        public void write(string process, string fileRegister, byte[] byteArrayRegister) { }

        public void write(string process, string fileRegister, string contents) { }

        public void copy(string process, string fileRegister1, string semantics, string fileRegister2, string salt) { }

        public void dump(string process) { }

        public void exeScript(string process, string filename) { 
            System.IO.StreamReader file = new System.IO.StreamReader(filename);
            String line;
            while((line = file.ReadLine()) != null){
                System.Windows.Forms.MessageBox.Show(line);
            }

            file.Close();
        }
    


        internal void test()
        {
            startMetaDataServers(3);

            createDataServer(1000, 0);
            createDataServer(1000, 1);
            createDataServer(1000, 2);

            createClient(8085, 0);

            //send dummy message to all metadata servers:
            foreach( ServerObjectWrapper metadataWrapper in MetaInformationReader.Instance.MetaDataServers )
            {
                //metadataWrapper.getObject<IMetaDataServer>().open( "PuppetMaster - HelloWorld!" );
            }

            //send dummy message to all data servers:
            foreach ( ServerObjectWrapper dataServerWrapper in dataServers.Values )
            {
               // dataServerWrapper.getObject<IDataServer>().read( "PuppetMaster - HelloWorld!" );
            }

            //send dummy message to all clients:
            foreach ( ServerObjectWrapper clientWrapper in clients.Values )
            {
               // clientWrapper.getObject<IClient>().open( "PuppetMaster - HelloWorld!" );
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel( channel, true );

            Application.Run( new ControlBoard() );

        }

        private static bool processIsRunning( string processName )
        {
            return ( System.Diagnostics.Process.GetProcessesByName( processName ).Length != 0 );
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
