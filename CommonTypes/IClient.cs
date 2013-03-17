using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonTypes
{
    public interface IClient
    {
         void write(string filename);

         void read(string filename);

         void open(string filename);

         void close(string filename);

         void delete(string filename);

         void create(string filename);
    }
}
