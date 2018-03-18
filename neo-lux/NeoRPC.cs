using LunarParser;
using Neo.Emulator.Utils;
using NeoLux.Core;
using System;
using System.Collections.Generic;

namespace NeoLux
{
    public class NeoRPC : NeoAPI
    {
        public readonly string url;
        public readonly int port;

        public NeoRPC(string url, int port)
        {
            this.url = url;
            this.port = port;
        }

        public override Dictionary<string, decimal> GetBalancesOf(string address, bool getTokens = false)
        {
            var response = QueryRPC("getaccountstate", new object[] { address });
            var result = new Dictionary<string, decimal>();

            var resultNode = response.GetNode("result");
            var balances = resultNode.GetNode("balances");

            foreach (var entry in balances.Children)
            {
                var assetID = entry.GetString("asset");
                var amount = entry.GetDecimal("value");

                var symbol = SymbolFromAssetID(assetID);

                result[symbol] = amount;
            }

            return result;
        }

        public override byte[] GetStorage(string scriptHash, byte[] key)
        {
            var response = QueryRPC("getstorage", new object[] { key.ByteToHex() });
            var result = response.GetString("result");
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            return result.HexToByte();
        }

        public override Dictionary<string, List<UnspentEntry>> GetUnspent(string address)
        {
            throw new NotImplementedException();
        }

        public override bool SendRawTransaction(string hexTx)
        {
            var response = QueryRPC("sendrawtransaction", new object[] {hexTx });
            var result = response.GetBool("result");
            return result;
        }

        public override InvokeResult TestInvokeScript(string scriptHash, object[] args)
        {
            var invoke = new InvokeResult();
            invoke.state = null;

            var temp = new object[args.Length + 1];
            temp[0] = scriptHash;
            for (int i=0; i<args.Length; i++)
            {
                temp[i + 1] = args[i];
            }

            var response = QueryRPC("invokefunction", temp);

            if (response != null)
            {
                var root = response["result"];
                if (root != null)
                {
                    var stack = root["stack"];
                    invoke.result = ParseStack(stack);

                    invoke.gasSpent = root.GetDecimal("gas_consumed");
                    invoke.state = root.GetString("state");
                }
            }

            return invoke;
        }

        public DataNode QueryRPC(string method, object[] _params, int id = 1)
        {
            var paramData = DataNode.CreateArray("params");
            foreach (var entry in _params)
            {
                paramData.AddField(null, entry);
            }

            var jsonRpcData = DataNode.CreateObject(null);
            jsonRpcData.AddField("method", method);
            jsonRpcData.AddNode(paramData);
            jsonRpcData.AddField("id", id);
            jsonRpcData.AddField("jsonrpc", "2.0");

            var rpcEndpoint = url + ":" + port;

            var response = RequestUtils.Request(RequestType.POST, rpcEndpoint, jsonRpcData);

            return response;
        }

    }
}
