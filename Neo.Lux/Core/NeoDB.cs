using LunarParser;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using System.Collections.Generic;
using System;

namespace Neo.Lux.Core
{
    public class NeoDB : NeoAPI
    {
        public readonly string apiEndpoint;

        public NeoDB(string apiEndpoint) 
        {
            this.apiEndpoint = apiEndpoint;
        }

        public static NeoDB ForMainNet()
        {
            return new NeoDB("http://api.wallet.cityofzion.io");
        }

        public static NeoDB ForTestNet()
        {
            return new NeoDB("http://testnet-api.wallet.cityofzion.io");
        }

        public override InvokeResult TestInvokeScript(byte[] scriptHash, object[] args)
        {
            var script = GenerateScript(scriptHash, args);
            return TestInvokeScript(script);
        }

        public InvokeResult TestInvokeScript(byte[] script)
        {
            var response = QueryRPC("invokescript", new object[] { script.ByteToHex() });
            if (response != null)
            {
                var root = response["result"];
                if (root != null)
                {
                    var invoke = new InvokeResult();

                    var stack = root["stack"];
                    invoke.result = ParseStack(stack);

                    invoke.gasSpent = root.GetDecimal("gas_consumed");
                    invoke.state = root.GetString("state");

                    return invoke;
                }
            }

            return null;
        }

        public override bool SendRawTransaction(string hexTx)
        {
            var response = QueryRPC("sendrawtransaction", new object[] { hexTx });
            return response.GetBool("result");
        }

        public override byte[] GetStorage(string scriptHash, byte[] key)
        {
            var result = QueryRPC("getstorage", new object[] { scriptHash, key.ByteToHex() });
            if (result == null)
            {
                return null;
            }

            var hex = result.GetString("result");
            return hex.HexToBytes();
        }

        public override Dictionary<string, decimal> GetAssetBalancesOf(string address)
        {
            var url = apiEndpoint + "/v2/address/balance/" + address;
            var response = RequestUtils.Request(RequestType.GET, url);

            var result = new Dictionary<string, decimal>();
            foreach (var node in response.Children)
            {
                if (node.HasNode("balance"))
                {
                    var balance = node.GetDecimal("balance");
                    if (balance > 0)
                    {
                        result[node.Name] = balance;
                    }
                }
            }

            return result;
        }

        public override Dictionary<string, List<UnspentEntry>> GetUnspent(string address)
        {
            var url = apiEndpoint + "/v2/address/balance/" + address;
            var response = RequestUtils.Request(RequestType.GET, url);

            var result = new Dictionary<string, List<UnspentEntry>>();
            foreach (var node in response.Children)
            {
                var child = node.GetNode("unspent");
                if (child != null)
                {
                    List<UnspentEntry> list;
                    if (result.ContainsKey(node.Name))
                    {
                        list = result[node.Name];
                    }
                    else
                    {
                        list = new List<UnspentEntry>();
                        result[node.Name] = list;
                    }

                    foreach (var data in child.Children)
                    {
                        var input = new UnspentEntry()
                        {
                            txid = data.GetString("txid"),
                            index = data.GetUInt32("index"),
                            value = data.GetDecimal("value")
                        };

                        list.Add(input);
                    }
                }
            }
            return result;
        }

        public override Transaction GetTransaction(string hash)
        {
            var response = QueryRPC("getrawtransaction", new object[] { hash});
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

        protected override string GetRPCEndpoint()
        {
            var response = RequestUtils.Request(RequestType.GET, apiEndpoint + "/v2/network/best_node");
            return response.GetString("node");
        }
    }
}
