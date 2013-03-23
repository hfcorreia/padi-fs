using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public interface IMetaDataServer
    {
        /**
         * Create
         * Creates a new file if it does not exist. 
         * Selects the dataservers for the file and assign a unique filename on each dataserver.
         * In case of success returns the metadata of that file, otherwise throws an exception.
         **/
        List<RemoteObjectWrapper> create(string filename, int numberOfDataServers, int readQuorum, int writeQuorum);

        /**
         * Open
         * Returns the metadata content for a given file.
         * In case the file does not exist throws an exception.
         **/
        File open(string filename);

        /**
         * Close
         * Informs the metadataserver that the client is no longer using a given file
         **/
        void close(string filename);

        /**
         * Delete
         * if the file is not open by any client it deletes a that file from the dataservers and returns true.
         * otherwise throws an exception.
         **/
        void delete(string filename);

        /**
         * Fail
         * The server starts ignoring all requests from Clients and DataServers.
         **/
        void fail();

        /**
         * Recover
         * The server replys to the client and metada servers.
         **/
        void recover();

        void registDataServer(int id, string host, int port);
        void exit();

    }
}
