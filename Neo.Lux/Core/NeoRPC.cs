using LunarParser;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using System;
using System.Collections.Generic;

namespace Neo.Lux.Core
{
    public abstract class NeoRPC : NeoAPI
    {
        public readonly string neoscanUrl;
        public readonly int port;

        public NeoRPC(int port, string neoscanURL)
        {
            this.port = port;
            this.neoscanUrl = neoscanURL;
        }

        public static NeoRPC ForMainNet()
        {
            return new RemoteRPCNode(10332, "http://neoscan.io");
        }

        public static NeoRPC ForTestNet()
        {
            return new RemoteRPCNode(20332, "https://neoscan-testnet.io");
        }

        public static NeoRPC ForPrivateNet()
        {
            return new LocalRPCNode(30333, "http://localhost:4000");
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
            var url = this.neoscanUrl +"/api/main_net/v1/get_balance/" + address;
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

        public override Transaction GetTransaction(string hash)
        {
            var response = QueryRPC("getrawtransaction", new object[] { hash });
            if (response != null && response.HasNode("result"))
            {
                var result = response.GetString("result");
                var bytes = result.HexToBytes();
                return Transaction.Unserialize(bytes);
            }
            else
            {
                return null;
            }
        }

    }

    public class LocalRPCNode : NeoRPC
    {
        public LocalRPCNode(int port, string neoscanURL) : base(port, neoscanURL)
        {
        }

        protected override string GetRPCEndpoint()
        {
            return $"http://localhost:{port}";
        }
    }

    public class RemoteRPCNode : NeoRPC
    {
        private int rpcIndex = 0;

        public RemoteRPCNode(int port, string neoscanURL) : base(port, neoscanURL)
        {
        }

        protected override string GetRPCEndpoint()
        {
            if (rpcIndex == 0)
            {
                rpcIndex = 1 + (Environment.TickCount % 5);
            }

            var url = $"http://seed{rpcIndex}.neo.org:{port}";
            rpcIndex++;
            if (rpcIndex > 5)
            {
                rpcIndex = 1;
            }

            return url;
        }
    }
}
