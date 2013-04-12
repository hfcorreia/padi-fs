using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes;
using CommonTypes.Exceptions;

namespace DataServer
{
    [Serializable]
    class DSstateNormal : DSstate
    {

        public DSstateNormal(DataServer dataServer) : base(dataServer) { }

        public override void write(File file)
        {
            
            if (file == null)
            {
                return;
            }

            if (Ds.Files.ContainsKey(file.FileName))
            {
                //updates the file
                Ds.Files.Remove(file.FileName);
            }
            if (!Ds.FileLocks.ContainsKey(file.FileName))
            {
                Ds.FileLocks.Add(file.FileName, new System.Threading.ReaderWriterLockSlim());
            }
            //creates a new file
            Ds.Files.Add(file.FileName, file);

            Ds.FileLocks[file.FileName].EnterWriteLock();
            try
            {
                Console.WriteLine("#DS: write fileName: " + file.FileName + ", version: " + file.Version);
                Util.writeFileToDisk(file, "" + "DS" + Ds.Id);
            }
            finally
            {
                Ds.FileLocks[file.FileName].ExitWriteLock();
            }
            Ds.makeCheckpoint();
        }

        public override File read(string filename)
        {
            File file = null;
            if (filename == null || !Ds.Files.ContainsKey(filename) || !Ds.FileLocks.ContainsKey(filename))
            {
                throw new ReadFileException("The server does not contain the file " + filename);
            }
            Ds.FileLocks[filename].EnterWriteLock();
            try
            {
                Console.WriteLine("#DS: read fileName: " + filename);
                file = Util.readFileFromDisk("DS" + Ds.Id, filename, Ds.Files[filename].Version);
            }
            finally
            {
                Ds.FileLocks[filename].ExitWriteLock();
            }
            return file;
        }

        public override int readFileVersion(string filename)
        {
            int fileVersion = Ds.Files.ContainsKey(filename) ? Ds.Files[filename].Version : -1;
            Console.WriteLine("#DS: readFileVersion " + filename + " has version " + fileVersion);
            return fileVersion; 
        }
    }
}
