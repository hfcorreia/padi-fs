using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaDataServer
{
    [Serializable]
    public class MetaDataServerAliveMessage
    {
        public int MetadataServerId { get; set; }
        public Boolean IsMaster { get; set; }
        public List<MetaDataOperation> Operations { get; set; }

        public MetaDataServerAliveMessage(int metadataServerId, Boolean isMaster)
        {
            MetadataServerId = metadataServerId;
            IsMaster = isMaster;
            Operations = null;
        }

        public MetaDataServerAliveMessage(int metadataServerId, Boolean isMaster, List<MetaDataOperation> operations)
        {
            MetadataServerId = metadataServerId;
            IsMaster = isMaster;
            Operations = operations;
        }

        public override string ToString()
        {
            String result = "[ MetaDataServerAliveMessage - metadataServer: " + MetadataServerId + ", ismaster: " + IsMaster + ", operations: ";
            foreach (MetaDataOperation op in Operations)
            {
                result += op + ", ";
            }
            result += " ]";
            return result;
        }
    }
}
