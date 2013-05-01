using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace MetaDataServer
{
    class MetaDataRegisterServerOperation : MetaDataOperation
    {

        String DataServerId { get; set; }
        String DataServerHost { get; set; }
        int DataServerPort { get; set; }

        public MetaDataRegisterServerOperation(String dataServerId, String dataServerHost, int dataServerPort)
        {
            DataServerId = dataServerId;
            DataServerHost = dataServerHost;
            DataServerPort = dataServerPort;
        }

         public override void execute(MetaDataServer md)
        {
            Console.WriteLine("#MDS: Registering DS " + DataServerId);
            ServerObjectWrapper remoteObjectWrapper = new ServerObjectWrapper(DataServerPort, DataServerId, DataServerHost);
            md.DataServers.Add(DataServerId, remoteObjectWrapper);
            md.addServerToUnbalancedFiles(DataServerId);
            md.makeCheckpoint();
        }      
    }
}
