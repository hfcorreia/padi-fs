using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using CommonTypes.Exceptions;

namespace DataServer
{
    class DSstateFail : DSstate
    {
        public DSstateFail(DataServer dataServer) : base(dataServer) { }

        public override void write(File file) 
        {
            failError(file.FileName);
        }
        public override File read(string filename) 
        {
            failError(filename);
            return null;
        }
        public override int readFileVersion(string filename) 
        {
            failError(filename);
            return -1;
        }

        private void failError(string filename)
        {
            Console.WriteLine("#DS read file version " + filename + " refused because the server is down!");
            throw new ServerDownException("The server " + Ds.Id + " is down");
        }
    }
}
