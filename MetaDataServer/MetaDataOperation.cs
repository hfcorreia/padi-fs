using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace MetaDataServer
{
    [Serializable()]
    public abstract class MetaDataOperation
    {
        public int OperationId { get; set; }

        public abstract void execute(MetaDataServer md);
        public override string ToString()
        {
            return "Operation - [ id: " + OperationId + " , type: " + GetType() + " ]";
        }

        /***
         * operations are equal if they have the same operation id
         * the operationId must be unique
         ***/ 
        public override bool Equals(object obj)
        {
            if (obj != null && typeof(object) == typeof(MetaDataOperation))
            {
                return ((MetaDataOperation)obj).OperationId == this.OperationId;
            }
            return false;
        }
    }
}
