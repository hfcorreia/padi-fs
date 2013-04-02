using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{

    public class MetaInformationReader
    {
        private List<ServerObjectWrapper> metadataServersList;
        public List<ServerObjectWrapper> MetaDataServers { get { return metadataServersList; } set { metadataServersList=value; } }
       
        private static MetaInformationReader instance;

        public static MetaInformationReader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MetaInformationReader();
                }
                return instance;
            }
        }

        private MetaInformationReader()
        {
                MetaDataServers = new List<ServerObjectWrapper>();
                ServerObjectWrapper mds1wrapper = new ServerObjectWrapper( Int32.Parse(Properties.Resources.MDS1_PORT), "m-1", Properties.Resources.MDS1_Host );
                ServerObjectWrapper mds2wrapper = new ServerObjectWrapper( Int32.Parse(Properties.Resources.MDS2_PORT), "m-2", Properties.Resources.MDS2_Host );
                ServerObjectWrapper mds3wrapper = new ServerObjectWrapper( Int32.Parse(Properties.Resources.MDS3_PORT), "m-3", Properties.Resources.MDS3_Host );

                MetaDataServers.Add( mds1wrapper );
                MetaDataServers.Add( mds2wrapper );
                MetaDataServers.Add( mds3wrapper );
        }

    }
}
