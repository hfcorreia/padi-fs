using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;

namespace DataServer
{
    [Serializable]
    class DSstateNormal : DSstate
    {

        public DSstateNormal(DataServer dataServer) : base(dataServer) { }

        public override void write(File file)
        {
            Console.WriteLine("#DS write( fileName: " + file.FileName + ", version: " + file.Version + ")");
            if (file == null)
            {
                return;
            }

            if (Ds.Files.ContainsKey(file.FileName))
            {
                //updates the file
                Ds.Files.Remove(file.FileName);
            }
            //creates a new file
            Ds.Files.Add(file.FileName, file);

            Util.writeFileToDisk(file, "" + "DS" + Ds.Id);
            Ds.makeCheckpoint();
        }

        public override File read(string filename)
        {
            if (filename == null || !Ds.Files.ContainsKey(filename))
            {
                return null; //throw exception because the file does not exist
            }

            return Util.readFileFromDisk("DS" + Ds.Id, filename, Ds.Files[filename].Version);
        }

        public override int readFileVersion(string filename)
        {
            return Ds.Files.ContainsKey(filename) ? Ds.Files[filename].Version : -1;
        }
    }
}
