using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MetaDataServer
{
    [Serializable]
    public class OperationComparer : IComparer<MetaDataOperation>
    {
        public int Compare(MetaDataOperation x, MetaDataOperation y)
        {
            return (x == null) ? 1 :
                    ((y == null) ? -1 :
                         x.OperationId - y.OperationId);
        }
    }
}
