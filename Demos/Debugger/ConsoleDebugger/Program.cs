using Neo.Lux.Cryptography;
using Neo.Lux.Debugger;
using Neo.Lux.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleDebugger
{
    public class Contract
    {
        public UInt160 scriptHash;

        public byte[] script;

        public AVMInstruction[] instructions;
        public NeoMapFile map;

        public Contract(byte[] script)
        {
            this.script = script;
            this.instructions = NeoTools.Disassemble(script).ToArray();
            this.scriptHash = script.ToScriptHash();
        }

        public bool LoadDebugInfo(string fileName)
        {
            var debugFile = fileName.Replace(".avm", ".debug.json");
            if (File.Exists(debugFile))
            {
                this.map = new NeoMapFile();
                this.map.LoadFromFile(debugFile);
                return true;
            }

            return false;
        }
    }

    class ConsoleServer : DebugServer
    {
        private Dictionary<UInt160, Contract> contracts = new Dictionary<UInt160, Contract>();

        private Dictionary<string, string> fileMap = new Dictionary<string, string>();
        private Dictionary<string, string[]> fileContent = new Dictionary<string, string[]>();

        private string lastFile = null;
        private int lastLine = -1;
        private int lastInstruction = -1;

        public ConsoleServer() {
            fileMap["AWnpZy4Yc4iyGsz8wpVX8fiWgg55w7xmi3"] = @"D:\code\Crypto\PhantasmaNeo\PhantasmaContract\bin\Debug\PhantasmaContract.avm";
        }

        private void WriteLine(ConsoleColor color, string msg)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(msg);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public override void OnClientConnect(uint ID)
        {
            WriteLine(ConsoleColor.Magenta, "Client attached: #" + ID);
        }

        public override void OnClientDisconnect(uint ID)
        {
            WriteLine(ConsoleColor.Magenta, "Client deattached: #" + ID);
        }

        public override void OnClientStep(uint ID, DebugMessage msg)
        {
            var contract = contracts.ContainsKey(msg.scriptHash) ? contracts[msg.scriptHash] : null;

            if (contract!= null)
            {
                int index = -1;
                for (int i=0; i<contract.instructions.Length; i++)
                {
                    var entry = contract.instructions[i];
                    if (msg.offset >= entry.startOfs && msg.offset <= entry.endOfs)
                    {
                        index = i;
                        break;
                    }
                }

                bool found = false;

                if (contract.map != null)
                {
                    string filePath;
                    try
                    {
                        int line = contract.map.ResolveLine(msg.offset, out filePath);

                        string[] lines;

                        if (fileContent.ContainsKey(filePath))
                        {
                            lines = fileContent[filePath];
                        }
                        else
                        {
                            lines = File.ReadAllLines(filePath);
                            fileContent[filePath] = lines;
                        }

                        if (lastFile != filePath || lastLine != line)
                        {
                            WriteLine(ConsoleColor.Gray, line + ", " + lines[line - 1]);
                            lastLine = line;
                            lastFile = filePath;
                        }

                        found = true;
                    }
                    catch
                    {
                        found = false;
                    }
                }

                if (index >= 0)
                {
                    if (lastInstruction>=0 && Math.Abs(lastInstruction - index) > 20)
                    {
                        WriteLine(ConsoleColor.DarkGray, new string('.', 70));
                    }
                    lastInstruction = index;
                }
                
                if (!found && index>=0)
                {
                    var entry = contract.instructions[index];
                    string extra = "";

                    if (entry.data != null)
                    {
                        extra = " => "+FormattingUtils.OutputData(entry.data, false);
                    }

                    WriteLine(ConsoleColor.Gray, msg.offset + ", " + entry.opcode+ extra);
                    found = true;
                }

                if (!found)
                {
                    contract = null;
                }
            }
            
            if (contract == null)
            {
                WriteLine(ConsoleColor.Gray, msg.offset+", [External code]");
            }
        }

        public override void OnClientScript(uint ID, byte[] script)
        {
            var contract = new Contract(script);
            contracts[contract.scriptHash] = contract;

            var address = contract.scriptHash.ToAddress();
            WriteLine(ConsoleColor.Green, "Loaded script: " + address);

            if (contracts.Count == 2)
            {
                fileMap[address] = fileMap[fileMap.Keys.First()];
            }

            if (fileMap.ContainsKey(address))
            {
                var fileName = fileMap[address];
                if (contract.LoadDebugInfo(fileName))
                {
                    WriteLine(ConsoleColor.Green, "Loaded debug info: " + fileName);
                }
            }
        }

        public override void OnClientLog(uint ID, string log)
        {
            WriteLine(ConsoleColor.DarkYellow, "// " + log);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var server = new ConsoleServer();
            Console.WriteLine("Starting the debugger...");

            Console.CancelKeyPress += delegate {
                Console.WriteLine("Stopping the debugger");
                server.Stop();
            };

            server.Start();
        }
    }
}
