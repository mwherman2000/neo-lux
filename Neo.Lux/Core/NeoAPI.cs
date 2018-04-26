using LunarParser;
using System;
using System.Collections.Generic;
using System.Linq;
using Neo.Lux.Cryptography;
using System.Numerics;
using Neo.Lux.Utils;

namespace Neo.Lux.Core
{
    public class NeoException : Exception
    {
        public NeoException(string msg) : base (msg)
        {

        }
    }

    public struct InvokeResult
    {
        public string state;
        public decimal gasSpent;
        public object[] result;
    }

    public struct TransactionOutput
    {
        public byte[] addressHash;
        public decimal amount;
    }

    public abstract class NeoAPI
    {
        private static Dictionary<string, string> _systemAssets = null;

        internal static Dictionary<string, string> GetAssetsInfo()
        {
            if (_systemAssets == null)
            {
                _systemAssets = new Dictionary<string, string>();
                AddAsset("NEO", "c56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b");
                AddAsset("GAS", "602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7");
            }

            return _systemAssets;
        }

        private static void AddAsset(string symbol, string hash)
        {
            _systemAssets[symbol] = hash;
        }

        public static IEnumerable<string> AssetSymbols
        {
            get
            {
                var info = GetAssetsInfo();
                return info.Keys;
            }
        }

        public static string SymbolFromAssetID(string assetID)
        {
            if (assetID == null)
            {
                return null;
            }

            if (assetID.StartsWith("0x"))
            {
                assetID = assetID.Substring(2);
            }

            var info = GetAssetsInfo();
            foreach (var entry in info)
            {
                if (entry.Value == assetID)
                {
                    return entry.Key;
                }
            }

            return null;
        }

        private static Dictionary<string, string> _tokenScripts = null;

        internal static Dictionary<string, string> GetTokenInfo()
        {
            if (_tokenScripts == null)
            {
                _tokenScripts = new Dictionary<string, string>();
                AddToken("RPX", "ecc6b20d3ccac1ee9ef109af5a7cdb85706b1df9");
                AddToken("DBC", "b951ecbbc5fe37a9c280a76cb0ce0014827294cf");
                AddToken("QLC", "0d821bd7b6d53f5c2b40e217c6defc8bbe896cf5");
                AddToken("APH", "a0777c3ce2b169d4a23bcba4565e3225a0122d95");
                AddToken("ZPT", "ac116d4b8d4ca55e6b6d4ecce2192039b51cccc5");
                AddToken("TKY", "132947096727c84c7f9e076c90f08fec3bc17f18");
                AddToken("TNC", "08e8c4400f1af2c20c28e0018f29535eb85d15b6");
                AddToken("CPX", "45d493a6f73fa5f404244a5fb8472fc014ca5885");
                AddToken("ACAT", "7f86d61ff377f1b12e589a5907152b57e2ad9a7a");
                AddToken("NRV", "a721d5893480260bd28ca1f395f2c465d0b5b1c2");
                AddToken("THOR", "67a5086bac196b67d5fd20745b0dc9db4d2930ed");
                AddToken("RHT", "2328008e6f6c7bd157a342e789389eb034d9cbc4");
                AddToken("IAM", "891daf0e1750a1031ebe23030828ad7781d874d6");
                AddToken("SHW", "78e6d16b914fe15bc16150aeb11d0c2a8e532bdd");
                AddToken("OBT", "0e86a40588f715fcaf7acd1812d50af478e6e917");
            }

            return _tokenScripts;
        }

        private static void AddToken(string symbol, string hash)
        {
            GetTokenInfo();
            _tokenScripts[symbol] = hash;
        }

        public static IEnumerable<string> TokenSymbols
        {
            get
            {
                var info = GetTokenInfo();
                return info.Keys;
            }
        }

        public static byte[] GetScriptHashFromString(string hash)
        {
            return hash.HexToBytes().Reverse().ToArray();
        }

        public static byte[] GetScriptHashFromSymbol(string symbol)
        {
            GetTokenInfo();
            foreach (var entry in _tokenScripts)
            {
                if (entry.Key == symbol)
                {
                    return GetScriptHashFromString(entry.Value);
                }
            }

            return null;
        }

        public static string GetStringFromScriptHash(byte[] hash)
        {
            return LuxUtils.ReverseHex(hash.ToHexString());
        }

        protected static object[] ParseStack(DataNode stack)
        {
            if (stack != null)
            {
                var items = new List<object>();

                if (stack.Children.Count() > 0 && stack.Name == "stack")
                {
                    foreach (var child in stack.Children)
                    {
                        var item = ParseStackItems(child);
                        items.Add(item);
                    }
                }

                return items.ToArray();
            }

            return null;
        }

        protected static object ParseStackItems(DataNode stackItem)
        {
            var type = stackItem.GetString("type");
            var value = stackItem.GetString("value");

            switch (type)
            {
                case "ByteArray":
                    {
                        return value.HexToBytes();
                    }

                case "Boolean":
                    {
                        return (value.ToLower() == "true");
                    }

                case "Integer":
                    {
                        BigInteger intVal;
                        BigInteger.TryParse(value, out intVal);
                        return intVal;
                    }
                case "Array": // Type
                    {
                        var items = new List<object>();
                        foreach (var child in stackItem.Children)
                        {
                            var item = ParseStackItems(child);
                            items.Add(item);
                        }
                        return items.ToArray();
                    }
                default:
                    {
                        //Console.WriteLine("ParseStack:unknown DataNode stack type: '" + type + "'");
                        break;
                    }
            }

            return null;
        }

        public abstract InvokeResult TestInvokeScript(byte[] scriptHash, object[] args);

        public InvokeResult TestInvokeScript(byte[] scriptHash, string operation, object[] args)
        {
            return TestInvokeScript(scriptHash, new object[] { operation, args });
        }

        public static void EmitObject(ScriptBuilder sb, object item)
        {
            if (item is IEnumerable<byte>)
            {
                var arr = ((IEnumerable<byte>)item).ToArray();

                sb.EmitPush(arr);
            }
            else
            if (item is IEnumerable<object>)
            {
                var arr = ((IEnumerable<object>)item).ToArray();

                for (int index = arr.Length - 1; index >= 0; index--)
                {
                    EmitObject(sb, arr[index]);
                }

                sb.EmitPush(arr.Length);
                sb.Emit(OpCode.PACK);
            }
            else
            if (item == null)
            {
                sb.EmitPush("");
            }
            else
            if (item is string)
            {
                sb.EmitPush((string)item);
            }
            else
            if (item is bool)
            {
                sb.EmitPush((bool)item);
            }
            else
            if (item is BigInteger)
            {
                sb.EmitPush((BigInteger)item);
            }
            else
            {
                throw new Exception("Unsupported contract parameter: " + item.ToString());
            }
        }

        public static byte[] GenerateScript(byte[] scriptHash, object[] args)
        {
            using (var sb = new ScriptBuilder())
            {
                var items = new Stack<object>();

                if (args != null)
                {
                    foreach (var item in args)
                    {
                        items.Push(item);
                    }
                }

                while (items.Count > 0)
                {
                    var item = items.Pop();
                    EmitObject(sb, item);
                }
                
                sb.EmitAppCall(scriptHash, false);

                var timestamp = DateTime.UtcNow.ToTimestamp();
                var nonce = BitConverter.GetBytes(timestamp);

                //sb.Emit(OpCode.THROWIFNOT);
                sb.Emit(OpCode.RET);
                sb.EmitPush(nonce);

                var bytes = sb.ToArray();

                string hex = bytes.ByteToHex();
                //System.IO.File.WriteAllBytes(@"D:\code\Crypto\neo-debugger-tools\ICO-Template\bin\Debug\inputs.avm", bytes);

                return bytes;
            }
        }

        private void GenerateInputsOutputs(KeyPair key, string symbol, IEnumerable<TransactionOutput> targets, out List<Transaction.Input> inputs, out List<Transaction.Output> outputs)
        {
            if (targets == null)
            {
                throw new NeoException("Invalid amount list");
            }
            
            var unspent = GetUnspent(key.address);
            // filter any asset lists with zero unspent inputs
            unspent = unspent.Where(pair => pair.Value.Count > 0).ToDictionary(pair => pair.Key, pair => pair.Value);

            inputs = new List<Transaction.Input>();
            outputs = new List<Transaction.Output>();

            string assetID;

            var info = GetAssetsInfo();
            if (info.ContainsKey(symbol))
            {
                assetID = info[symbol];
            }
            else
            {
                throw new NeoException($"{symbol} is not a valid blockchain asset.");
            }

            if (!unspent.ContainsKey(symbol))
            {
                throw new NeoException($"Not enough {symbol} in address {key.address}");
            }

            decimal cost = 0;

            var fromHash = key.PublicKeyHash.ToArray();
            foreach (var target in targets)
            {
                if (target.addressHash.SequenceEqual(fromHash))
                {
                    throw new NeoException("Target can't be same as input");
                }

                cost += target.amount;
            }

            var sources = unspent[symbol];
            decimal selected = 0;
            foreach (var src in sources)
            {
                selected += src.value;

                var input = new Transaction.Input()
                {
                    prevHash = LuxUtils.ReverseHex(src.txid).HexToBytes(),
                    prevIndex = src.index,
                };

                inputs.Add(input);

                if (selected >= cost)
                {
                    break;
                }
            }

            if (selected < cost)
            {
                throw new NeoException($"Not enough {symbol}");
            }

            if(cost > 0)
            {
                foreach (var target in targets)
                {
                    var output = new Transaction.Output()
                    {
                        assetID = LuxUtils.ReverseHex(assetID).HexToBytes(),
                        scriptHash = LuxUtils.ReverseHex(GetStringFromScriptHash(target.addressHash)).HexToBytes(),
                        value = target.amount
                    };
                    outputs.Add(output);
                }
            }

            if (selected > cost || cost == 0)
            {
                var left = selected - cost;

                var change = new Transaction.Output()
                {
                    assetID = LuxUtils.ReverseHex(assetID).HexToBytes(),
                    scriptHash = key.signatureHash.ToArray(),
                    value = left
                };
                outputs.Add(change);
            }
        }

        public Transaction CallContract(KeyPair key, byte[] scriptHash, object[] args, string attachSymbol = null, IEnumerable<TransactionOutput> attachTargets = null)
        {
            var bytes = GenerateScript(scriptHash, args);
            return CallContract(key, scriptHash, bytes, attachSymbol, attachTargets);
        }

        public Transaction CallContract(KeyPair key, byte[] scriptHash, string operation, object[] args, string attachSymbol = null, IEnumerable<TransactionOutput> attachTargets = null)
        {
            return CallContract(key, scriptHash, new object[] { operation, args }, attachSymbol, attachTargets);
        }

        public Transaction CallContract(KeyPair key, byte[] scriptHash, byte[] bytes, string attachSymbol = null, IEnumerable<TransactionOutput> attachTargets = null)
        {
            /*var invoke = TestInvokeScript(net, bytes);
            if (invoke.state == null)
            {
                throw new Exception("Invalid script invoke");
            }

            decimal gasCost = invoke.gasSpent;*/

            List<Transaction.Input> inputs;
            List<Transaction.Output> outputs;

            if (string.IsNullOrEmpty(attachSymbol))
            {
                attachSymbol = "GAS";
            }

            if (attachTargets == null)
            {
                attachTargets = new List<TransactionOutput>();                
            }

            GenerateInputsOutputs(key, attachSymbol, attachTargets, out inputs, out outputs);

            if (inputs.Count == 0)
            {
                throw new NeoException($"Not enough inputs for transaction");
            }

            Transaction tx = new Transaction()
            {
                type = TransactionType.InvocationTransaction,
                version = 0,
                script = bytes,
                gas = 0,
                inputs = inputs.ToArray(),
                outputs = outputs.ToArray()
            };

            tx.Sign(key);

            var rawTx = tx.Serialize(true);
            var hexTx = rawTx.ByteToHex();

            var ok = SendRawTransaction(hexTx);
            return ok ? tx : null;
        }

        public abstract bool SendRawTransaction(string hexTx);

        public abstract byte[] GetStorage(string scriptHash, byte[] key);

        public abstract Transaction GetTransaction(string hash);

        public Transaction GetTransaction(UInt256 hash)
        {
            return GetTransaction(hash.ToString());
        }

        public Transaction SendAsset(KeyPair fromKey, string toAddress, string symbol, decimal amount)
        {
            if (String.Equals(fromKey.address, toAddress, StringComparison.OrdinalIgnoreCase))
            {
                throw new NeoException("Source and dest addresses are the same");
            }

            var toScriptHash = toAddress.GetScriptHashFromAddress();
            var target = new TransactionOutput() { addressHash = toScriptHash, amount = amount };
            var targets = new List<TransactionOutput>() { target };
            return SendAsset(fromKey, symbol, targets);
        }

        public Transaction SendAsset(KeyPair fromKey, string symbol, IEnumerable<TransactionOutput> targets)
        {
            List<Transaction.Input> inputs;
            List<Transaction.Output> outputs;

            GenerateInputsOutputs(fromKey, symbol, targets, out inputs, out outputs);

            Transaction tx = new Transaction()
            {
                type = TransactionType.ContractTransaction,
                version = 0,
                script = null,
                gas = -1,
                inputs = inputs.ToArray(),
                outputs = outputs.ToArray()
            };

            tx.Sign(fromKey);

            var rawTx = tx.Serialize(true);
            var hexTx = rawTx.ByteToHex();

            var ok = SendRawTransaction(hexTx);
            return ok ? tx : null;
        }

        public Transaction WithdrawAsset(KeyPair fromKey, string toAddress, string symbol, decimal amount)
        {
            var toScriptHash = toAddress.GetScriptHashFromAddress();
            var target = new TransactionOutput() { addressHash = toScriptHash, amount = amount };
            var targets = new List<TransactionOutput>() { target };
            return WithdrawAsset(fromKey, symbol, targets);
        }

        public Transaction WithdrawAsset(KeyPair toKey, string symbol, IEnumerable<TransactionOutput> targets)
        {
            List<Transaction.Input> inputs;
            List<Transaction.Output> outputs;

            GenerateInputsOutputs(toKey, symbol, targets, out inputs, out outputs);

            Transaction tx = new Transaction()
            {
                type = TransactionType.ContractTransaction,
                version = 0,
                script = null,
                gas = -1,
                inputs = inputs.ToArray(),
                outputs = outputs.ToArray()
            };

            tx.Sign(toKey);

            var rawTx = tx.Serialize(true);
            var hexTx = rawTx.ByteToHex();

            var ok = SendRawTransaction(hexTx);
            return ok ? tx : null;
        }

        public Dictionary<string, decimal> GetBalancesOf(KeyPair key)
        {
            return GetBalancesOf(key.address);
        }

        public Dictionary<string, decimal> GetBalancesOf(string address)
        {
            var assets = GetAssetBalancesOf(address);
            var tokens = GetTokenBalancesOf(address);

            var result = new Dictionary<string, decimal>();

            foreach (var entry in assets)
            {
                result[entry.Key] = entry.Value;
            }

            foreach (var entry in tokens)
            {
                result[entry.Key] = entry.Value;
            }

            return result;
        }

        public Dictionary<string, decimal> GetTokenBalancesOf(KeyPair key)
        {
            return GetTokenBalancesOf(key.address);
        }

        public Dictionary<string, decimal> GetTokenBalancesOf(string address)
        {
            var result = new Dictionary<string, decimal>();
            foreach (var symbol in TokenSymbols)
            {
                var token = GetToken(symbol);
                try
                {
                    var amount = token.BalanceOf(address);
                    if (amount > 0)
                    {
                        result[symbol] = amount;
                    }
                }
                catch
                {
                    continue;
                }
            }
            return result;
        }

        public Dictionary<string, decimal> GetAssetBalancesOf(KeyPair key)
        {
            return GetAssetBalancesOf(key.address);
        }

        public abstract Dictionary<string, decimal> GetAssetBalancesOf(string address);

        public bool IsAsset(string symbol)
        {
            var info = GetAssetsInfo();
            return info.ContainsKey(symbol);
        }

        public bool IsToken(string symbol)
        {
            var info = GetTokenInfo();
            return info.ContainsKey(symbol);
        }

        public NEP5 GetToken(string symbol)
        {
            var info = GetTokenInfo();
            if (info.ContainsKey(symbol))
            {
                var tokenScriptHash = info[symbol];
                return new NEP5(this, tokenScriptHash);
            }

            throw new NeoException("Invalid token symbol");
        }

        public struct UnspentEntry
        {
            public string txid;
            public uint index;
            public decimal value;
        }

        public abstract Dictionary<string, List<UnspentEntry>> GetUnspent(string address);

        /*public static decimal getClaimAmounts(Net net, string address)
        {
            var apiEndpoint = getAPIEndpoint(net);
            var response = RequestUtils.Request(RequestType.GET, apiEndpoint + "/v2/address/claims/" + address);
            return response.GetDecimal("total_claim");
            return (available: parseInt(res.data.total_claim), unavailable: parseInt(res.data.total_unspent_claim));
        }
        */

        /*public static DataNode getTransactionHistory(Net net, string address)
        {
            var apiEndpoint = getAPIEndpoint(net);
            var response = RequestUtils.Request(RequestType.GET, apiEndpoint + "/v2/address/history/" + address);
            return response["history"];
        }*/

        /*public static int getWalletDBHeight(Net net)
        {
            var apiEndpoint = getAPIEndpoint(net);
            var response = RequestUtils.Request(RequestType.GET, apiEndpoint + "/v2/block/height");
            return response.GetInt32("block_height");
        }*/


    }

}
