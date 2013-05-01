using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaDataServer
{
    class MetaDataLog
    {
        private List<MetaDataOperation> log = new List<MetaDataOperation>();
        private MetaDataServer Md { get; set; }

        public MetaDataLog(MetaDataServer md)
        {
            Md = md;
        }

        public void registerOperation(MetaDataOperation op)
        {
            log.Add(op);
        }

        public void printLog()
        {
            Console.WriteLine("Log from MD " + Md.Id + " com " + log.Count + " operacoes registadas");
            foreach(MetaDataOperation op in log)
            {
                Console.WriteLine("Operation: " + op);
            }
        }

    }
}
