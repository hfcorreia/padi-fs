using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{

    public class MetaInformationReader
    {
        private List<RemoteObjectWrapper> metadataServersList;
        public List<RemoteObjectWrapper> MetaDataServers { get { return metadataServersList; } set { metadataServersList=value; } }
       
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
                MetaDataServers = new List<RemoteObjectWrapper>();
                RemoteObjectWrapper mds1wrapper = new RemoteObjectWrapper( Int32.Parse( Properties.Resources.MDS1_PORT ), 1, Properties.Resources.MDS1_Host );
                RemoteObjectWrapper mds2wrapper = new RemoteObjectWrapper( Int32.Parse( Properties.Resources.MDS2_PORT ), 2, Properties.Resources.MDS2_Host );
                RemoteObjectWrapper mds3wrapper = new RemoteObjectWrapper( Int32.Parse( Properties.Resources.MDS3_PORT ), 3, Properties.Resources.MDS3_Host );

                MetaDataServers.Add( mds1wrapper );
                MetaDataServers.Add( mds2wrapper );
                MetaDataServers.Add( mds3wrapper );
        }

    }
}
