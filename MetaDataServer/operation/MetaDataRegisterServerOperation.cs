using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace MetaDataServer
{
    [Serializable]
    public class MetaDataRegisterServerOperation : MetaDataOperation
    {
        public String DataServerId { get; set; }
        public String DataServerHost { get; set; }
        public int DataServerPort { get; set; }

        public MetaDataRegisterServerOperation() { }
        public MetaDataRegisterServerOperation(String dataServerId, String dataServerHost, int dataServerPort)
        {
            DataServerId = dataServerId;
            DataServerHost = dataServerHost;
            DataServerPort = dataServerPort;
        }

         public override void execute(MetaDataServer md)
        {
            
            Console.WriteLine("#MDS: Registering DS " + DataServerId);
            if ( md.DataServers.ContainsKey(DataServerId) ) 
            {
                md.DataServers.Remove(DataServerId);
            }
            ServerObjectWrapper remoteObjectWrapper = new ServerObjectWrapper(DataServerPort, DataServerId, DataServerHost);
            md.DataServers.Add(DataServerId, remoteObjectWrapper);
            md.addServerToUnbalancedFiles(DataServerId);

            HeartbeatMessage heartbeat = new HeartbeatMessage(DataServerId, 0 , 0, 0, 0, new Dictionary<string,FileAccessCounter>());
            md.receiveHeartbeat(heartbeat);
            md.makeCheckpoint();
        }      
    }
}
