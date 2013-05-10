using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;
using System.Runtime.Serialization;

namespace MetaDataServer
{
    [Serializable]
    public class MetaDataLog 
    {
        [NonSerialized()]
        private MetaDataServer mdsField;
        private MetaDataServer MetadataServer { get { return mdsField; } set { mdsField = value; } }

        public List<MetaDataOperation> log { get; set; }

        public MetaDataLog() { }

        public int Status { get; set; }
        public int NextId { get; set; }

        public void init(MetaDataServer md)
        {
            log = new List<MetaDataOperation>();
            MetadataServer = md;
            NextId = 0;
            Status = 0;
        }

        public void registerOperation(MetaDataServer md, MetaDataOperation operation)
        {

            Console.WriteLine("registerOperation: " + operation + (operation == null ? "NULL" : "" + operation.OperationId));
            dump();
            if (md == null || operation == null)
            {
                throw new ArgumentNullException("RegisterOperation - Expected a MDS and an operation [ MDS:" + md + ", Operation: " + operation + " ]");
            }

            int operationId;

            lock (typeof(MetaDataLog))
            {
                operationId = NextId++;
            }

            if (md.getReplicationHandler().IsMaster)
            {
                operation.OperationId = operationId;
            }
            if (!log.Contains(operation))
            {
                log.Add(operation);
            }

            md.getReplicationHandler().syncOperation(operation);
        }

        public MetaDataOperation getOperation(int operationId)
        {
            try
            {
                return log.First(operation => operation.OperationId == operationId);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (ArgumentNullException)
            {
                return null;
            }
        }

        public void incrementStatus() 
        {
            lock (typeof(MetaDataLog))
            {
                Status++;
            }
        }

        public void dump()
        {
            Console.WriteLine("Log from MD " + MetadataServer.Id + " with " + log.Count + " operations");
            for(int i=0; i< NextId; ++i)
            {
                Console.WriteLine("\tOperation[" + i + "]: " + ((getOperation(i) ==null )? "NULL" : (""+getOperation(i))));
            }
        }


        public List<MetaDataOperation> getOperationsFrom(int fromStatus)
        {
            if (Status < fromStatus)
            {
                throw new ArgumentNullException("Log - trying to get operations from " + fromStatus + " but the actual status is " + Status);
            }

            List<MetaDataOperation> logCopy = null;
            lock (typeof(MetaDataLog))
            {
                logCopy = new List<MetaDataOperation>(log);
            }

            List<MetaDataOperation> operations = new List<MetaDataOperation>();
            operations.AddRange(logCopy.FindAll(operation => (operation.OperationId >= fromStatus) && !operations.Contains(operation)));


            return operations;
        }

        public void registerOperations(MetaDataServer md, List<MetaDataOperation> list)
        {
            foreach (MetaDataOperation operation in list)
            {
               
                registerOperation(md, operation);
            }
        }


    }
}
