using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace MetaDataServer
{
    public abstract class MetaDataOperation
    {
        public int OperationId { get; set; }

        public abstract void execute(MetaDataServer md);
    }
}
