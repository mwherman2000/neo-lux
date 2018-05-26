using LunarParser;
using System;
using System.Collections.Generic;
using System.Linq;
using Neo.Lux.Cryptography;
using System.Numerics;
using Neo.Lux.Utils;
using Neo.Lux.VM;
using System.Threading;

namespace Neo.Lux.Core
{
    public class NeoException : Exception
    {
        public NeoException(string msg) : base (msg)
        {

        }

        public NeoException(string msg, Exception cause) : base(msg, cause)
        {

        }
    }

    public class InvokeResult
    {
        public VMState state;
        public decimal gasSpent;
        public object[] stack;
        public Transaction transaction;
    }

    public abstract class NeoAPI
    {
        private static Dictionary<string, string> _systemAssets = null;

        private Action<string> _logger;
        public Action<string> Logger
        {
            get
            {
                return _logger != null ? _logger : DummyLogger;
            }
        }

        private uint oldBlock;

        public virtual void SetLogger(Action<string> logger = null)
        {
            this._logger = logger;
        }

        private void DummyLogger(string s)
        {

        }

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

        public static byte[] GetAssetID(string symbol)
        {
            var info = GetAssetsInfo();
            foreach (var entry in info)
            {
                if (entry.Key == symbol)
                {
                    return LuxUtils.ReverseHex(entry.Value).HexToBytes();
                }
            }

            return null;
        }

        public static IEnumerable<KeyValuePair<string, string>> Assets
        {
            get
            {
                var info = GetAssetsInfo();
                return info;
            }
        }

        public static string SymbolFromAssetID(byte[] assetID)
        {
            var str = assetID.ByteToHex();
            var result = SymbolFromAssetID(str);
            if (result == null)
            {
                result = SymbolFromAssetID(LuxUtils.ReverseHex(str));
            }

            return result;
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


        // TODO NEP5 should be refactored to be a data object without the embedded api

        public struct TokenInfo {
            public string symbol;
            public string hash;
            public string name;
            public int decimals;
        }

        private static Dictionary<string, TokenInfo> _tokenScripts = null;

        internal static Dictionary<string, TokenInfo> GetTokenInfo()
        {
            if (_tokenScripts == null)
            {
                _tokenScripts = new Dictionary<string, TokenInfo>();
                AddToken("RPX", "ecc6b20d3ccac1ee9ef109af5a7cdb85706b1df9", "RedPulse", 8);
                AddToken("DBC", "b951ecbbc5fe37a9c280a76cb0ce0014827294cf", "DeepBrain", 8);
                AddToken("QLC", "0d821bd7b6d53f5c2b40e217c6defc8bbe896cf5", "Qlink", 8);
                AddToken("APH", "a0777c3ce2b169d4a23bcba4565e3225a0122d95", "Aphelion", 8);
                AddToken("ZPT", "ac116d4b8d4ca55e6b6d4ecce2192039b51cccc5", "Zeepin", 8);
                AddToken("TKY", "132947096727c84c7f9e076c90f08fec3bc17f18", "TheKey", 8);
                AddToken("TNC", "08e8c4400f1af2c20c28e0018f29535eb85d15b6", "Trinity", 8);
                AddToken("CPX", "45d493a6f73fa5f404244a5fb8472fc014ca5885", "APEX", 8);
                AddToken("ACAT","7f86d61ff377f1b12e589a5907152b57e2ad9a7a", "ACAT", 8);
                AddToken("NRV", "a721d5893480260bd28ca1f395f2c465d0b5b1c2", "Narrative", 8);
                AddToken("THOR","67a5086bac196b67d5fd20745b0dc9db4d2930ed", "Thor", 8);
                AddToken("RHT", "2328008e6f6c7bd157a342e789389eb034d9cbc4", "HashPuppy", 0);
                AddToken("IAM", "891daf0e1750a1031ebe23030828ad7781d874d6", "BridgeProtocol", 8);
                AddToken("SHW", "78e6d16b914fe15bc16150aeb11d0c2a8e532bdd", "Switcheo", 8);
                AddToken("OBT", "0e86a40588f715fcaf7acd1812d50af478e6e917", "Orbis", 8);
                AddToken("SOUL", "ed07cffad18f1308db51920d99a2af60ac66a7b3", "Phantasma", 8); //OLD 4b4f63919b9ecfd2483f0c72ff46ed31b5bbb7a4
            }

            return _tokenScripts;
        }

        private static void AddToken(string symbol, string hash, string name, int decimals)
        {
            _tokenScripts[symbol] = new TokenInfo { symbol = symbol, hash = hash, name = name, decimals = decimals };
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
                    return GetScriptHashFromString(entry.Value.hash);
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

        public abstract InvokeResult InvokeScript(byte[] script);

        public InvokeResult InvokeScript(UInt160 scriptHash, string operation, object[] args)
        {
            return InvokeScript(scriptHash, new object[] { operation, args });
        }

        public InvokeResult InvokeScript(UInt160 scriptHash, object[] args)
        {
            var script = GenerateScript(scriptHash, args);

            return InvokeScript(script);
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
            if (item is int || item is sbyte || item is short)
            {
                var n = (int)item;
                sb.EmitPush((BigInteger)n);
            }
            else
            if (item is uint || item is byte || item is ushort)
            {
                var n = (uint)item;
                sb.EmitPush((BigInteger)n);
            }
            else
            {
                throw new NeoException("Unsupported contract parameter: " + item.ToString());
            }
        }

        public static byte[] GenerateScript(UInt160 scriptHash, object[] args)
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

        private Dictionary<string, Transaction> lastTransactions = new Dictionary<string, Transaction>();

        public void GenerateInputsOutputs(KeyPair key, string symbol, IEnumerable<Transaction.Output> targets, out List<Transaction.Input> inputs, out List<Transaction.Output> outputs, decimal system_fee = 0)
        {
            var from_script_hash = new UInt160(key.signatureHash.ToArray());
            GenerateInputsOutputs(from_script_hash, symbol, targets, out inputs, out outputs, system_fee);
        }

        public void GenerateInputsOutputs(UInt160 from_script_hash, string symbol, IEnumerable<Transaction.Output> targets, out List<Transaction.Input> inputs, out List<Transaction.Output> outputs, decimal system_fee = 0)
        {           
            var unspent = GetUnspent(from_script_hash);
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

            var from_address = from_script_hash.ToAddress();

            if (!unspent.ContainsKey(symbol))
            {
                throw new NeoException($"Not enough {symbol} in address {from_address}");
            }

            decimal cost = 0;

            if (targets != null)
            {
                foreach (var target in targets)
                {
                    if (target.scriptHash.Equals(from_script_hash))
                    {
                        throw new NeoException("Target can't be same as input");
                    }

                    cost += target.value;
                }
            }

            var targetAssetID = LuxUtils.ReverseHex(assetID).HexToBytes();
            
            var sources = unspent[symbol];
            decimal selected = 0;

            if (lastTransactions.ContainsKey(from_address))
            {
                var lastTx = lastTransactions[from_address];

                uint index = 0;
                foreach (var output in lastTx.outputs)
                {
                    if (output.assetID.SequenceEqual(targetAssetID) && output.scriptHash.Equals(from_script_hash))
                    {
                        selected += output.value;

                        var input = new Transaction.Input()
                        {
                            prevHash = lastTx.Hash,
                            prevIndex = index,
                        };

                        inputs.Add(input);

                        break;
                    }

                    index++;
                }
            }

            foreach (var src in sources)
            {
                if (selected >= cost && inputs.Count > 0)
                {
                    break;
                }

                selected += src.value;

                var input = new Transaction.Input()
                {
                    prevHash = src.hash,
                    prevIndex = src.index,
                };

                inputs.Add(input);
            }

            if (selected < cost)
            {
                throw new NeoException($"Not enough {symbol}");
            }

            if(cost > 0 && targets != null)
            {
                foreach (var target in targets)
                {
                    var output = new Transaction.Output()
                    {
                        assetID = targetAssetID,
                        scriptHash = target.scriptHash,
                        value = target.value
                    };
                    outputs.Add(output);
                }
            }

            if (selected > cost || cost == 0)
            {
                var left = selected - cost;

                var change = new Transaction.Output()
                {
                    assetID = targetAssetID,
                    scriptHash = from_script_hash,
                    value = left
                };
                outputs.Add(change);
            }
        }

        public Transaction CallContract(KeyPair key, UInt160 scriptHash, object[] args, string attachSymbol = null, IEnumerable<Transaction.Output> attachTargets = null)
        {
            var bytes = GenerateScript(scriptHash, args);
            return CallContract(key, scriptHash, bytes, attachSymbol, attachTargets);
        }

        public Transaction CallContract(KeyPair key, UInt160 scriptHash, string operation, object[] args, string attachSymbol = null, IEnumerable<Transaction.Output> attachTargets = null)
        {
            return CallContract(key, scriptHash, new object[] { operation, args }, attachSymbol, attachTargets);
        }

        public Transaction CallContract(KeyPair key, UInt160 scriptHash, byte[] bytes, string attachSymbol = null, IEnumerable<Transaction.Output> attachTargets = null)
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
                attachTargets = new List<Transaction.Output>();                
            }

            GenerateInputsOutputs(key, attachSymbol, attachTargets, out inputs, out outputs);

            if (inputs.Count == 0)
            {
                throw new NeoException($"Not enough inputs for transaction");
            }

            var transaction = new Transaction()
            {
                type = TransactionType.InvocationTransaction,
                version = 0,
                script = bytes,
                gas = 0,
                inputs = inputs.ToArray(),
                outputs = outputs.ToArray()
            };

            transaction.Sign(key);

            if (SendTransaction(key, transaction))
            {
                return transaction;
            }

            return null;
        }

        protected abstract bool SendTransaction(Transaction tx);

        public bool SendTransaction(KeyPair keys, Transaction tx)
        {
            if (oldBlock == 0)
            {
                oldBlock = this.GetBlockHeight();
            }

            return SendTransaction(tx);
        }

        public abstract byte[] GetStorage(string scriptHash, byte[] key);

        public abstract Transaction GetTransaction(UInt256 hash);

        public Transaction GetTransaction(string hash)
        {
            var val = new UInt256(LuxUtils.ReverseHex(hash).HexToBytes());
            return GetTransaction(val);
        }

        public Transaction SendAsset(KeyPair fromKey, string toAddress, string symbol, decimal amount)
        {
            if (String.Equals(fromKey.address, toAddress, StringComparison.OrdinalIgnoreCase))
            {
                throw new NeoException("Source and dest addresses are the same");
            }

            var toScriptHash = toAddress.GetScriptHashFromAddress();
            var target = new Transaction.Output() { scriptHash = new UInt160(toScriptHash), value = amount };
            var targets = new List<Transaction.Output>() { target };
            return SendAsset(fromKey, symbol, targets);
        }

        public Transaction SendAsset(KeyPair fromKey, string symbol, IEnumerable<Transaction.Output> targets)
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

            var ok = SendTransaction(tx);
            return ok ? tx : null;
        }

        public Transaction WithdrawAsset(KeyPair toKey, string fromAddress, string symbol, decimal amount, byte[] verificationScript)
        {
            var fromScriptHash = new UInt160(fromAddress.GetScriptHashFromAddress());
            var target = new Transaction.Output() { scriptHash = new UInt160(toKey.address.GetScriptHashFromAddress()), value = amount };
            var targets = new List<Transaction.Output>() { target };
            return WithdrawAsset(toKey, fromScriptHash, symbol, targets, verificationScript);
        }

        public Transaction WithdrawAsset(KeyPair toKey, UInt160 fromScripthash, string symbol, IEnumerable<Transaction.Output> targets, byte[] verificationScript)
        {

            var check = verificationScript.ToScriptHash();
            if (check != fromScripthash)
            {
                throw new ArgumentException("Invalid verification script");
            }

            List<Transaction.Input> inputs;
            List<Transaction.Output> outputs;
            GenerateInputsOutputs(fromScripthash, symbol, targets, out inputs, out outputs);


            List<Transaction.Input> gas_inputs;
            List<Transaction.Output> gas_outputs;
            GenerateInputsOutputs(toKey, "GAS", null, out gas_inputs, out gas_outputs);

            foreach (var entry in gas_inputs)
            {
                inputs.Add(entry);
            }

            foreach (var entry in gas_outputs)
            {
                outputs.Add(entry);
            }

            Transaction tx = new Transaction()
            {
                type = TransactionType.ContractTransaction,
                version = 0,
                script = null,
                gas = -1,
                inputs = inputs.ToArray(),
                outputs = outputs.ToArray()
            };

            var witness = new Witness { invocationScript = ("0014" + toKey.address.AddressToScriptHash().ByteToHex()).HexToBytes(), verificationScript = verificationScript };
            tx.Sign(toKey, new Witness[] { witness });

            var ok = SendTransaction(tx);
            return ok ? tx : null;
        }

        public Transaction ClaimGas(KeyPair ownerKey, string fromAddress, byte[] verificationScript)
        {
            var fromScriptHash = new UInt160(fromAddress.GetScriptHashFromAddress());
            return ClaimGas(ownerKey, fromScriptHash, verificationScript);
        }

        public Transaction ClaimGas(KeyPair ownerKey, UInt160 fromScripthash, byte[] verificationScript)
        {

            var check = verificationScript.ToScriptHash();
            if (check != fromScripthash)
            {
                throw new ArgumentException("Invalid verification script");
            }

            decimal amount;
            var claimable = GetClaimable(fromScripthash, out amount);

            var references = new List<Transaction.Input>();
            foreach (var entry in claimable)
            {
                references.Add(new Transaction.Input() { prevHash = entry.hash, prevIndex = entry.index });
            }

            if (amount <=0)
            {
                throw new ArgumentException("No GAS to claim at this address");
            }

            List<Transaction.Input> inputs;
            List<Transaction.Output> outputs;
            GenerateInputsOutputs(ownerKey, "GAS", null, out inputs, out outputs);

            outputs.Add(
            new Transaction.Output()
            {
                scriptHash = fromScripthash,
                assetID = NeoAPI.GetAssetID("GAS"),
                value = amount
            });

            Transaction tx = new Transaction()
            {
                type = TransactionType.ClaimTransaction,
                version = 0,
                script = null,
                gas = -1,
                references = references.ToArray(),
                inputs = inputs.ToArray(),
                outputs =outputs.ToArray(),
            };

            var witness = new Witness { invocationScript = ("0014" + ownerKey.address.AddressToScriptHash().ByteToHex()).HexToBytes(), verificationScript = verificationScript };
            tx.Sign(ownerKey, new Witness[] { witness });

            var ok = SendTransaction(tx);
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

        public abstract Dictionary<string, decimal> GetAssetBalancesOf(UInt160 hash);

        public Dictionary<string, decimal> GetAssetBalancesOf(KeyPair key)
        {
            return GetAssetBalancesOf(key.address);
        }

        public Dictionary<string, decimal> GetAssetBalancesOf(string address)
        {
            var hash = new UInt160(address.AddressToScriptHash());
            return GetAssetBalancesOf(hash);
        }

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
                var token = info[symbol];
                return new NEP5(this, token.hash, token.name, new BigInteger(token.decimals));
            }

            throw new NeoException("Invalid token symbol");
        }

        public struct UnspentEntry
        {
            public UInt256 hash;
            public uint index;
            public decimal value;
        }

        public abstract List<UnspentEntry> GetClaimable(UInt160 hash, out decimal amount);

        public abstract Dictionary<string, List<UnspentEntry>> GetUnspent(UInt160 scripthash);        

        public Dictionary<string, List<UnspentEntry>> GetUnspent(string address)
        {
            return GetUnspent(new UInt160(address.AddressToScriptHash()));
        }

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

        public string rpcEndpoint { get; set; }


        protected abstract string GetRPCEndpoint();

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

            if (Logger != DummyLogger)
            {
                Logger("NeoDB QueryRPC: " + method);
                LogData(jsonRpcData);
            }

            int retryCount = 0;
            do
            {
                if (rpcEndpoint == null)
                {
                    rpcEndpoint = GetRPCEndpoint();
                    Logger("Update RPC Endpoint: " + rpcEndpoint);
                }

                var response = RequestUtils.Request(RequestType.POST, rpcEndpoint, jsonRpcData);

                if (response != null && response.HasNode("result"))
                {
                    return response;
                }
                else
                {
                    if (response != null && response.HasNode("error"))
                    {
                        var error = response["error"];
                        Logger("RPC Error: " + error.GetString("message"));
                    }
                    else
                    {
                        Logger("No answer");
                    }
                    rpcEndpoint = null;
                    retryCount++;
                }

            } while (retryCount < 10);

            return null;
        }

        private void LogData(DataNode node, int ident = 0)
        {
            var tabs = new string('\t', ident);
            Logger($"{tabs}{node}");
            foreach (DataNode child in node.Children)
                LogData(child, ident + 1);
        }

        #region BLOCKS
        public abstract uint GetBlockHeight();
        public abstract Block GetBlock(UInt256 hash);
        public abstract Block GetBlock(uint height);

        #endregion

        public Transaction DeployContract(KeyPair keys, byte[] script, byte[] parameter_list, byte return_type, ContractPropertyState properties, string name, string version, string author, string email, string description)
        {
            if (script.Length > 1024 * 1024) return null;

            byte[] gen_script;
            using (var sb = new ScriptBuilder())
            {
                sb.EmitPush(description);
                sb.EmitPush(email);
                sb.EmitPush(author);
                sb.EmitPush(version);
                sb.EmitPush(name);
                sb.EmitPush((byte)properties);
                sb.EmitPush(return_type);
                sb.EmitPush(parameter_list);
                sb.EmitPush(script);

                sb.EmitSysCall("Neo.Contract.Create");

                gen_script = sb.ToArray();

                //string hex = bytes.ByteToHex();
                //System.IO.File.WriteAllBytes(@"D:\code\Crypto\neo-debugger-tools\ICO-Template\bin\Debug\inputs.avm", bytes);                
            }

            decimal fee = 100;

            if (properties.HasFlag(ContractPropertyState.HasStorage))
            {
                fee += 400;
            }

            if (properties.HasFlag(ContractPropertyState.HasDynamicInvoke))
            {
                fee += 500;
            }

            fee -= 10; // first 10 GAS is free

            List<Transaction.Input> inputs;
            List<Transaction.Output> outputs;

            GenerateInputsOutputs(keys, "GAS", null, out inputs, out outputs, fee);

            Transaction tx = new Transaction()
            {
                type = TransactionType.InvocationTransaction,
                version = 0,
                script = gen_script,
                gas = fee,
                inputs = inputs.ToArray(),
                outputs = outputs.ToArray()
            };

            tx.Sign(keys);

            var ok = SendTransaction(tx);
            return ok ? tx : null;
        }

        public void WaitForTransaction(KeyPair keys, Transaction tx)
        {
            if (tx == null)
            {
                throw new ArgumentNullException();
            }

            uint newBlock;

            do
            {
                Thread.Sleep(5000);
                newBlock = GetBlockHeight();
            } while (newBlock == oldBlock);

            while (oldBlock < newBlock)
            {
                var other = GetBlock(oldBlock);

                if (other != null)
                {
                    foreach (var entry in other.transactions)
                    {
                        if (entry.Hash == tx.Hash)
                        {
                            oldBlock = newBlock;
                            break;
                        }
                    }

                    oldBlock++;
                }
                else
                {
                    Thread.Sleep(5000);
                }

            }

            lastTransactions[keys.address] = tx;
        }


    }

}
