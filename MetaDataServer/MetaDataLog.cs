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

        //public SortedSet<MetaDataOperation> log { get; set; }
        public List<MetaDataOperation> log { get; set; }

        public MetaDataLog() { }

        public int Status { get; set; }
        public int NextId { get; set; }

        //private static readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(MetaDataLog));

        public void init(MetaDataServer md)
        {
            //log = new SortedSet<MetaDataOperation>(new OperationComparer());
            log = new List<MetaDataOperation>();
            MetadataServer = md;
            NextId = 0;
            Status = 0;
        }

        public void registerOperation(MetaDataServer md, MetaDataOperation operation)
        {
            Console.WriteLine("#MD " + md + "Log - registerOperation - " + operation);
            if (md == null || operation == null)
            {
                throw new ArgumentNullException("registerOperation - expected a MDS and an operation, given [ MDS:" + md + "Operation: " + operation + "]");
            }

            int operationId;

            lock (typeof(MetaDataLog))
            {
                operationId = NextId++;
            }

            if (md.ReplicationHandler.IsMaster)
            {
                operation.OperationId = operationId;
            }
            if (!log.Contains(operation))
            {
                log.Add(operation);
            }

            md.ReplicationHandler.syncOperation(operation);
            saveLog(this);
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

        public void loadLog(String filename)
        {
            //TODO - load the log from a xml file and deserialize
        }

        private static void saveLog(MetaDataLog obj) 
        {

            

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
            Console.WriteLine("Log from MD " + MetadataServer.Id + " com " + log.Count + " operacoes registadas");
            foreach(MetaDataOperation op in log)
            {
                Console.WriteLine("Operation: " + op);
            }
        }


        public List<MetaDataOperation> getOperationsFrom(int fromStatus)
        {
            if (Status < fromStatus)
            {
                throw new ArgumentNullException("Log.getOperationsFrom - trying to get operations from " + fromStatus + "but the actual status is " + Status);
            }
            List<MetaDataOperation> logCopy = new List<MetaDataOperation>(log);
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
