using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client.services
{
    abstract class ClientService
    {
        public ClientState State { get; set; }

        public ClientService(ClientState clientState){
            State = clientState;
        }

        public abstract void execute();
    }
}
