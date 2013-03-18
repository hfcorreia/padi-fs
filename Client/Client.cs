using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using CommonTypes;

namespace Client
{
    public class Client : MarshalByRefObject, IClient
    {
        private int Port { get; set; }
        private string Name { get; set; }
        private string Url { get; set; }

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: port clientName");
                Console.ReadLine();
            }
            else
            {
                Client client = new Client();
                client.initialize(Int32.Parse(args[0]), args[1]);
                client.startConnection();
                 
                /*
                string dataserverURL = "tcp://localhost:" + port + "/" + id;
                client.getDataServer(dataserverURL);
                 * */

                Console.WriteLine("port: " + client.Port + " name: " + client.Name + " url: " + client.Url);
                Console.WriteLine("connection started");
                Console.ReadLine();
            }

        }

        public void initialize(int port, string name)
        {
            Port = port;
            Name = name;
            Url = "tcp://localhost:" + Port + "/" + Name;
        }

        void startConnection()
        {
            TcpChannel channel = new TcpChannel(Port);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(Client), Name,WellKnownObjectMode.Singleton);
        }

        /*
        private void getDataServer(string url)
        {
            IDataServer dataServer = (IDataServer)Activator.GetObject(typeof(IDataServer), dataserverURL);
        }
         * */

        public void write(string filename) { Console.WriteLine(filename); }

        public void read(string filename) { Console.WriteLine(filename); }

        public void open(string filename) 
        { 
            Console.WriteLine(filename); 
            
        }

        public void close(string filename) { Console.WriteLine(filename); }

        public void delete(string filename) { Console.WriteLine(filename); }

        public void create(string filename) { Console.WriteLine(filename); }
    }
}
