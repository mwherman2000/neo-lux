using Neo.Lux.Cryptography;
using Neo.Lux.VM;
using Neo.Lux.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Neo.Lux.Debugger
{
    public struct DebugMessage
    {
        public VMState state;
        public UInt160 scriptHash;
        public int offset;
    }

    public abstract class DebugServer
    {
        public const int PORT_NO = 4130;
        public const string SERVER_IP = "127.0.0.1";

        public DebugServer()
        {
        }

        private bool running;

        public void Start() {
            //---listen at the specified IP and port no.---
            IPAddress localAdd = IPAddress.Parse(SERVER_IP);
            TcpListener listener = new TcpListener(localAdd, PORT_NO);
            listener.Start();
            running = true;

            do
            {
                //---incoming client connected---
                TcpClient client = listener.AcceptTcpClient();

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    //---get the incoming data through a network stream---
                    NetworkStream nwStream = client.GetStream();
                    byte[] buffer = new byte[client.ReceiveBufferSize];

                    var ID = (uint)Environment.TickCount;

                    OnClientConnect(ID);
                    //---read incoming stream---

                    string queue = "";

                    do
                    {
                        int bytesRead;
                        try
                        {
                            bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                        }
                        catch
                        {
                            break;
                        }

                        //---convert the data received into a string---
                        string dataReceived = queue + Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        queue = "";

                        var commands = dataReceived.Split('\n');
                        foreach (var cmd in commands)
                        {
                            if (!cmd.EndsWith("#"))
                            {
                                queue += cmd;
                                break;
                            }

                            var sss = cmd.Substring(0, cmd.Length - 1);

                            var temp = sss.Split(',');

                            switch (temp[0])
                            {
                                case "STEP":
                                    {
                                        var msg = new DebugMessage()
                                        {
                                            state = (VMState)Enum.Parse(typeof(VMState), temp[1]),
                                            scriptHash = new UInt160(temp[2].AddressToScriptHash()),
                                            offset = int.Parse(temp[3])
                                        };

                                        OnClientStep(ID, msg);
                                        break;
                                    }

                                case "LOG":
                                    {
                                        OnClientLog(ID, temp[1]);
                                        break;
                                    }

                                case "CODE":
                                    {
                                        var script = temp[1].HexToBytes();
                                        OnClientScript(ID, script);
                                        break;
                                    }

                            }
                        }


                    } while (true);
                    OnClientDisconnect(ID);

                    client.Close();

                }).Start();
            } while (running);
            listener.Stop();
        }

        public void Stop()
        {
            running = false;
        }

        public abstract void OnClientConnect(uint ID);
        public abstract void OnClientDisconnect(uint ID);
        public abstract void OnClientStep(uint ID, DebugMessage message);
        public abstract void OnClientLog(uint ID, string log);
        public abstract void OnClientScript(uint ID, byte[] script);
    }
}
