using System;
using System.Net.Sockets;
using System.Text;

using Neo.Lux.VM;
using Neo.Lux.Utils;

namespace Neo.Lux.Debugger
{
    public class DebugClient: IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public bool IsConnected => _client != null;

        public DebugClient()
        {
            try
            {
                this._client = new TcpClient(DebugServer.SERVER_IP, DebugServer.PORT_NO);
                _stream = _client.GetStream();
            }
            catch
            {
                _client = null;
                _stream = null;
            }
        }

        public void WriteLine(string msg)
        {
            if (!IsConnected)
            {
                return;
            }

            string debuggerData = $"LOG,{msg}#\n";
            byte[] bytesToSend = UTF8Encoding.UTF8.GetBytes(debuggerData);

            _stream.Write(bytesToSend, 0, bytesToSend.Length);
        }

        public void SendScript(byte[] script)
        {
            if (!IsConnected)
            {
                return;
            }

            string debuggerData = $"CODE,{script.ByteToHex()}#\n";
            byte[] bytesToSend = UTF8Encoding.UTF8.GetBytes(debuggerData);

            _stream.Write(bytesToSend, 0, bytesToSend.Length);
        }

        public void SendEvent(string name, object[] args)
        {
            if (!IsConnected)
            {
                return;
            }

            string debuggerData = $"EVENT,{name}#\n";
            byte[] bytesToSend = UTF8Encoding.UTF8.GetBytes(debuggerData);

            _stream.Write(bytesToSend, 0, bytesToSend.Length);
        }

        public void Step(ExecutionEngine vm)
        {
            if (!IsConnected)
            {
                return;
            }

            string debuggerData = $"STEP,{vm.State},{vm.CurrentContext.ScriptHash.ToAddress()},{vm.CurrentContext.InstructionPointer}#\n";
            byte[] bytesToSend = UTF8Encoding.UTF8.GetBytes(debuggerData);

            _stream.Write(bytesToSend, 0, bytesToSend.Length);

            /*byte[] bytesToRead = new byte[_client.ReceiveBufferSize];
            int bytesRead = _stream.Read(bytesToRead, 0, _client.ReceiveBufferSize);*/
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Close();
                _client = null;
            }
        }
    }
}
