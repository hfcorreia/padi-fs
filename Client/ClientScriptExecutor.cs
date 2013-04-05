using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    class ClientScriptExecutor
    {
        private Client ClientEntity { get; set; }
        private String ScriptName { get; set; }
        private String ScriptLocation { get; set; }

        public ClientScriptExecutor(Client client, String scriptName)
        {
            ClientEntity = client;
            ScriptName = scriptName;
            ScriptLocation = Environment.CurrentDirectory + Properties.Resources.CLIENT_SCRIPT_DIR + ScriptName;
        }

        public void runScriptFile()
        {
            Console.WriteLine("\r\n#Client: Running Script " + ScriptName);
            System.IO.StreamReader fileReader = new System.IO.StreamReader(ScriptLocation);

            String line = fileReader.ReadLine();
            while (line != null)
            {
                if (line.StartsWith("#"))
                {
                    line = fileReader.ReadLine();
                    continue;
                }
                else
                {
                    runCommand(line);
                    line = fileReader.ReadLine();
                }
            }
            Console.WriteLine("#Client: End of Script " + ScriptName + "\r\n");
            fileReader.Close();
        }

        private void runCommand(string line)
        {
            exeScriptCommand(line);
        }

        private void exeScriptCommand(string line)
        {
            String[] input = line.Split(' ');
            String[] newInput = input.Skip(2).ToArray<String>();

           switch (input[0])
            {
                case "open":
                    open(newInput);
                    break;
                case "close":
                    close(newInput);
                    break;
                case "create":
                    create(newInput);
                    break;
                case "delete":
                    delete(newInput);
                    break;
                case "write":
                    write(newInput);
                    break;
                case "read":
                    read(newInput);
                    break;
                case "dump":
                    dump(newInput);
                    break;
                default:
                    Console.WriteLine("#Client: No such command: " + input[0] + "!");
                    break;
            }
        }

        private void dump(string[] input)
        {
            ClientEntity.dump();
        }

        private void read(string[] input)
        {
          ClientEntity.read(Int32.Parse(input[0]), input[1], Int32.Parse(input[2]));
        }

        private void write(string[] input)
        {
          int fileRegisterId = Int32.Parse(input[0]);
          int stringRegisterId = 0;
          if (Int32.TryParse(input[1], out stringRegisterId))
          {
              ClientEntity.write(fileRegisterId, stringRegisterId);
          }
          else
          {
              byte[] content = new byte[input[1].Length * sizeof(char)];
              System.Buffer.BlockCopy(input[1].ToCharArray(), 0, content, 0, content.Length);
              ClientEntity.write(fileRegisterId, content);
          }
        }

        private void delete(string[] input)
        {
            ClientEntity.delete(input[0]);
        }

        private void create(string[] input)
        {
     
           ClientEntity.create(input[0], Int32.Parse(input[1]), Int32.Parse(input[2]), Int32.Parse(input[3]));
        }

        private void close(string[] input)
        {
            ClientEntity.close(input[0]);
        }

        private void open(string[] input)
        {
            ClientEntity.open(input[0]);
        }
    }
}
