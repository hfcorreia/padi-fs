using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Remoting.Channels;
using System.Collections;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters;

namespace Client.services
{
    class StartConnectionService : ClientService
    {
        private Client Client { get; set; }

        public StartConnectionService(ClientState clientState, Client client) : base(clientState) 
        {
            Client = client;
        }

        override public void execute()
        {
            Console.WriteLine("#Client: starting connection..");
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = State.Port;
            TcpChannel channel = new TcpChannel(props, null, provider);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(Client, State.Id, typeof(Client));
        }
    }
}
