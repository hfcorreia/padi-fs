﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using CommonTypes;

namespace MetaDataServer
{
    public class MetaDataServer : MarshalByRefObject, IMetaDataServer
    {
        public  int Port { get; set; }
        public  int Id { get; set; }
        public string Url { get { return "tcp://localhost:" + Port + "/" + Id; } }
        private Dictionary<int, string> dataServers = new Dictionary<int, string>(); // <serverID, url>

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: port metadataServerId");
                Console.ReadLine();
            }
            else
            {
                MetaDataServer metadataServer = new MetaDataServer();
                metadataServer.initialize(Int32.Parse(args[0]), Int32.Parse(args[1]));
                metadataServer.startConnection();

                Console.WriteLine("port: " + metadataServer.Port + " Id: " + metadataServer.Id + " url: " + metadataServer.Url);
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

            RemotingConfiguration.RegisterWellKnownServiceType(typeof(MetaDataServer), "" + Id, WellKnownObjectMode.Singleton);
        }

        public void registDataServer(int id, string url)
        {
            dataServers.Add(id, url);
            Console.WriteLine("DS registed " + id + " w/ url: " + url);
        }

        public void open(string filename)
        {
            Console.WriteLine("#MDS " + Id + " open " + filename);
        }

        public void close(string filename)
        {
            Console.WriteLine("#MDS " + Id + " close " + filename);
        }

        public void delete(string filename)
        {
            Console.WriteLine("#MDS " + Id + " delete " + filename);
        }

        public void create(string filename)
        {
            Console.WriteLine("#MDS " + Id + " create " + filename);
        }
        public void exit() 
        {
            System.Environment.Exit(0);
        }
    }

}