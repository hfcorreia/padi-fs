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
        
        /*
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
        */
        
        public override File read(string filename)
        {
            File file = null;
            if (filename == null || !Ds.Files.ContainsKey(filename) || !Ds.FileLocks.ContainsKey(filename))
            {
                Console.WriteLine("#DS: read error - the server does not have the file " + filename);
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

        public override void write(File file)
        {
            
            if (file == null)
            {
                return;
            }


            if (!Ds.FileLocks.ContainsKey(file.FileName))
            {
                Ds.FileLocks.Add(file.FileName, new System.Threading.ReaderWriterLockSlim());
            }

            Ds.FileLocks[file.FileName].EnterWriteLock();
            
            if (Ds.Files.ContainsKey(file.FileName))
            {

                if (Ds.Files[file.FileName].Version > file.Version)
                {
                    //trying to write an old version -> discard the write
                    Ds.FileLocks[file.FileName].ExitWriteLock();
                    return;
                }
                else if (Ds.Files[file.FileName].Version == file.Version)
                {
                    int actualClientId = getClientId(Ds.Files[file.FileName].clientId);
                    int newClientId = getClientId(file.clientId);

                    Console.WriteLine("DS write - actual: " + actualClientId + ", new: " + newClientId);
                    
                    if (actualClientId < newClientId)
                    {
                        //in case we have a draw (concurrent writes from diferent clients)
                        //we save the version of the client with smallest id and so we discard the
                        //request if the new user has a bigger id than the actual
                        Ds.FileLocks[file.FileName].ExitWriteLock();
                        return;
                    }
                }

                //the write will be persisted!!
                
                //if we allready have an old version of the file we delete it
                Ds.Files.Remove(file.FileName);
            }

            //creates a new file
            Ds.Files.Add(file.FileName, file);
            
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

        private int getClientId(string userString)
        {
            string temp = String.Copy(userString);
            temp.Replace("c-", "");
            int result;
            return Int32.TryParse(temp, out result) ? result : -1;
        }

        public override int readFileVersion(string filename)
        {
            int fileVersion = Ds.Files.ContainsKey(filename) ? Ds.Files[filename].Version : -1;
            Console.WriteLine("#DS: readFileVersion " + filename + " has version " + fileVersion);
            return fileVersion; 
        }
    }
}
