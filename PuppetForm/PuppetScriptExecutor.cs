using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonTypes.Exceptions;

namespace PuppetForm
{
    class PuppetScriptExecutor
    {
        private PuppetMaster PuppetMasterEntity { get; set; }
        private String ScriptName { get; set; }
        private System.IO.StreamReader ScriptReader { get; set; }

        public PuppetScriptExecutor(PuppetMaster puppetMaster, String scriptName)
        {
            PuppetMasterEntity = puppetMaster;
            ScriptName = scriptName;
            ScriptReader = new System.IO.StreamReader(scriptName);
        }

        public void runScript(Boolean oneStep)
        {
            String line = ScriptReader.ReadLine();
            while (line != null)
            {
                if (line.StartsWith("#"))
                {
                    line = ScriptReader.ReadLine();
                    continue;
                }
                else
                {
                    runCommand(line);
                    if (oneStep) return;
                    line = ScriptReader.ReadLine();
                }
            }
            ScriptReader.Close();
            PuppetMasterEntity.ScriptExecutor = null;
            throw new PadiFsException("End of file");
        }

        private void runCommand(String line)
        {
            String[] input = line.Split(' ');
            String[] newInput = input.Skip(1).ToArray<String>();
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
                case "copy":
                    copy(newInput);
                    break;
                case "dump":
                    dump(newInput);
                    break;
                case "fail":
                    fail(newInput);
                    break;
                case "recover":
                    recover(newInput);
                    break;
                case "freeze":
                    freeze(newInput);
                    break;
                case "unfreeze":
                    break;
                case "exeScript":
                    exeClientScript(newInput);
                    break;
                default:
                    throw new PadiFsException("No such command: " + input[0] + "!");
            }
        }

        private void exeClientScript(string[] input)
        {
            PuppetMasterEntity.exeClientScript(input[0], input[1]);
        }

        private void freeze(string[] input)
        {
            PuppetMasterEntity.freeze(input[0]);
        }

        private void recover(string[] input)
        {
            PuppetMasterEntity.recover(input[0]);
        }

        private void fail(string[] input)
        {
            PuppetMasterEntity.fail(input[0]);
        }

        private void dump(string[] input)
        {
            PuppetMasterEntity.dump(input[0]);
        }

        private void copy(string[] input)
        {
            PuppetMasterEntity.copy(input[0], input[1], input[2], input[3], input[4]);
        }

        private void read(string[] input)
        {
            PuppetMasterEntity.read(input[0], Int32.Parse(input[1]), input[2], Int32.Parse(input[3]));
        }

        private void write(string[] input)
        {
            PuppetMasterEntity.write(input[0], Int32.Parse(input[1]), Int32.Parse(input[2]));
        }

        private void delete(string[] input)
        {
            PuppetMasterEntity.delete(input[0],input[1]);
        }

        private void create(string[] input)
        {
            PuppetMasterEntity.create(input[0], input[1], Int32.Parse(input[2]), Int32.Parse(input[3]), Int32.Parse(input[4]));
        }

        private void close(string[] input)
        {
            PuppetMasterEntity.close(input[0], input[1]);
        }

        private void open(string[] input)
        {
            PuppetMasterEntity.open(input[0], input[1]);
        }


    }
}
