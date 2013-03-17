using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace DataServer
{
    class DataServer
    {
        public int Id { get; set; }

        public int Port { get; set; }

        public string Url { get { return "tcp://localhost:" + Port + "/" + Id; } } 

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: port metadataServerId");
                Console.ReadLine();
            }
            else
            {
                DataServer dataServer = new DataServer();
                dataServer.initialize(Int32.Parse(args[0]), Int32.Parse(args[1]));
                dataServer.startConnection();

                Console.WriteLine("port: " + dataServer.Port + " Id: " + dataServer.Id + " url: " + dataServer.Url);
                Console.WriteLine("connection started");
                Console.ReadLine();
            }
        }

        public void initialize(int port, int id)
        {
            Port = port;
            Id = id;
        }

        public void startConnection()
        {
            TcpChannel channel = new TcpChannel(Port);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(DataServer), "" + Id, WellKnownObjectMode.Singleton);
        }


    }
}
