using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace CommonTypes
{
    interface IPuppetMaster
    {
        void registerClient(IClient client);
    }
}
