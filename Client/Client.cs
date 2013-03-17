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
    class Client : MarshalByRefObject, IClient
    {
        private int port;
        private string name;
        private string url;

        public Client(int port, string name)
        {
            this.port = port;
            this.name = name;
            this.url = "tcp://localhost:" + this.port + "/" + this.name;
        }

        void startConnection()
        {
            TcpChannel channel = new TcpChannel(this.port);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Client),
                "Client-" + name,
                WellKnownObjectMode.Singleton);
        }
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: port clientName");
            }
            else
            {
                Client client = new Client(Int32.Parse(args[0]), args[1]);
            }

        }
    }
}
