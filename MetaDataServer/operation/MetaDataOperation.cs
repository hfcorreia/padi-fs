using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace MetaDataServer
{
    public abstract class MetaDataOperation
    {
        public abstract void execute(MetaDataServer md);
    }
}
