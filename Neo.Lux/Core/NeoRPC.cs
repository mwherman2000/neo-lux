using LunarParser;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using System;
using System.Collections.Generic;

namespace Neo.Lux.Core
{
    public class NeoRPC : NeoAPI
    {
        public readonly string url;
        public readonly int port;

        public NeoRPC(string url, int port)
        {
            if (!url.StartsWith("http://"))
            {
                url = "http://" + url;
            }

            this.url = url;
            this.port = port;
        }

        public override Dictionary<string, decimal> GetAssetBalancesOf(string address)
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
            return result.HexToBytes();
        }

        // Note: This current implementation requires NeoScan running at port 4000
        public override Dictionary<string, List<UnspentEntry>> GetUnspent(string address)
        {
            var url = this.url+ ":4000/api/main_net/v1/get_balance/" + address;
            var json = RequestUtils.GetWebRequest(url);

            var root = LunarParser.JSON.JSONReader.ReadFromString(json);
            var unspents = new Dictionary<string, List<UnspentEntry>>();

            root = root["balance"];

            foreach (var child in root.Children)
            {
                var symbol = child.GetString("asset");

                List<UnspentEntry> list = new List<UnspentEntry>();
                unspents[symbol] = list;

                var unspentNode = child.GetNode("unspent");
                foreach (var entry in unspentNode.Children)
                {
                    var temp = new UnspentEntry() { txid = entry.GetString("txid"), value = entry.GetDecimal("value"), index = entry.GetUInt32("n") };
                    list.Add(temp);
                }
            }

            return unspents;
        }

        public override bool SendRawTransaction(string hexTx)
        {
            var response = QueryRPC("sendrawtransaction", new object[] {hexTx });
            var result = response.GetBool("result");
            return result;
        }

        public override InvokeResult TestInvokeScript(byte[] scriptHash, object[] args)
        {
            var invoke = new InvokeResult();
            invoke.state = null;

            var temp = new object[args.Length + 1];
            temp[0] = NeoAPI.GetStringFromScriptHash(scriptHash);
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
