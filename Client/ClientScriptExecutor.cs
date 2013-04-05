using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

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
            String newLine = parseLine(line);
    
            String[] input = newLine.Split(' ');

            String[] newInput = input.Skip(2).ToArray<String>();

           switch (input[0])
            {
                case "OPEN":
                    open(newInput);
                    break;
                case "CLOSE":
                    close(newInput);
                    break;
                case "CREATE":
                    create(newInput);
                    break;
                case "DELETE":
                    delete(newInput);
                    break;
                case "WRITE":
                    write(newInput);
                    break;
                case "READ":
                    read(newInput);
                    break;
                case "DUMP":
                    dump(newInput);
                    break;
                default:
                    Console.WriteLine("#Client: No such command: " + input[0] + "!");
                    break;
            }
        }

        private String parseLine(string line)
        {
            Match mp = Regex.Match(line, "\"(.*)\"");
            String newLine = line.Replace(",", "");
            if (mp.Success)
            {
                newLine = Regex.Replace(newLine, "\"(.*)\"", "\"" + mp.Groups[1].Value + "\"");
            }
            return newLine;
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
          Match mp = Regex.Match(input[1], "\"(.*)\"");
          if (mp.Success)
          {
              byte[] content = System.Text.Encoding.UTF8.GetBytes(mp.Groups[1].Value);
              ClientEntity.write(fileRegisterId, content);
          }
          else
          {
              ClientEntity.write(fileRegisterId, Int32.Parse(input[1]));
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
