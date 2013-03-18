using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{

    public class MetaInformationReader
    {
        public static Dictionary<int, RemoteObjectWrapper> MetaDataServers;
        private static bool isInitialized = false;

        public static void initMetaData()
        {
            if (!isInitialized)
            {
                MetaDataServers = new Dictionary<int, RemoteObjectWrapper>();
                int startingPort = 10000;
                for (int id = 0; id < 3; ++id)
                {
                    string url = "tcp://localhost:" + startingPort + id + "/" + id;
                    RemoteObjectWrapper metadataServer = new RemoteObjectWrapper(startingPort + id, id, url);
                    MetaDataServers.Add(id, metadataServer);
                }
                isInitialized = true;
            }
        }
    }
}
