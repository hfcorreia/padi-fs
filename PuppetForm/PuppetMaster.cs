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

        private Dictionary<int, RemoteObjectWrapper> clients = new Dictionary<int, RemoteObjectWrapper>();      //<clientId, clientWrapper>
        private Dictionary<int, RemoteObjectWrapper> dataServers = new Dictionary<int, RemoteObjectWrapper>();  //<dataServerId, dataServerWrapper>

        public void createClient(int clientPort, int clientId)
        {
            Process.Start( "Client.exe", clientPort + " " + clientId );
            RemoteObjectWrapper clientWrapper = new RemoteObjectWrapper( clientPort, clientId, "localhost" );
            
            clients.Add( clientId, clientWrapper );
        }

        public void createDataServer( int port, int id )
        {
            Process.Start( "DataServer.exe", port + " " + id );
            RemoteObjectWrapper dataServerWrapper = new RemoteObjectWrapper( port, id, "localhost" );
            if (!dataServers.ContainsKey(id))
            {
                dataServers.Add(id, dataServerWrapper);
            }
        }
        
        public void startMetaDataServers( int numServers )
        {
            //MetadataServers are fixed and their proprieties are specified in CommonTypes project
            foreach ( RemoteObjectWrapper metaDataWrapper in MetaInformationReader.Instance.MetaDataServers )
            {
                Process.Start( "MetaDataServer.exe", metaDataWrapper.Port + " " + metaDataWrapper.Id );
            }
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
            startMetaDataServers(3);

            createDataServer(1000, 0);
            createDataServer(1000, 1);
            createDataServer(1000, 2);

            createClient(8085, 0);

            //send dummy message to all metadata servers:
            foreach( RemoteObjectWrapper metadataWrapper in MetaInformationReader.Instance.MetaDataServers )
            {
                //metadataWrapper.getObject<IMetaDataServer>().open( "PuppetMaster - HelloWorld!" );
            }

            //send dummy message to all data servers:
            foreach ( RemoteObjectWrapper dataServerWrapper in dataServers.Values )
            {
               // dataServerWrapper.getObject<IDataServer>().read( "PuppetMaster - HelloWorld!" );
            }

            //send dummy message to all clients:
            foreach ( RemoteObjectWrapper clientWrapper in clients.Values )
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
            foreach (RemoteObjectWrapper metadataWrapper in MetaInformationReader.Instance.MetaDataServers)
            {
                try
                {
                    metadataWrapper.getObject<IMetaDataServer>().exit();
                }
                catch (Exception e) { Console.WriteLine("Error Closing."); }
            }
            foreach (RemoteObjectWrapper dataServerWrapper in dataServers.Values)
            {
                try
                {
                    dataServerWrapper.getObject<IDataServer>().exit();
                }
                catch (Exception e) { Console.WriteLine("Error Closing."); }
            }
            foreach (RemoteObjectWrapper clientWrapper in clients.Values)
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
