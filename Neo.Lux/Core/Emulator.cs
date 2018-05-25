using System;
using System.Collections.Generic;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;

namespace Neo.Lux.Core
{
    public class NeoEmulator: NeoAPI
    {
        public VirtualChain Chain { get; private set; }

        public NeoEmulator(KeyPair owner)
        {
            this.Chain = new VirtualChain(this, owner);
        }

        public override InvokeResult InvokeScript(byte[] script)
        {
            return Chain.InvokeScript(script);
        }

        protected override bool SendTransaction(Transaction tx)
        {
            Chain.GenerateBlock(new Transaction[] { tx });
            return true;
        }

        public override byte[] GetStorage(string scriptHash, byte[] key)
        {
            var hash = new UInt160(scriptHash.HexToBytes());

            var account = Chain.GetAccount(hash);

            if (account.storage != null && account.storage.entries.ContainsKey(key))
            {
                return account.storage.entries[key];
            }

            return null;
        }

        public override Transaction GetTransaction(UInt256 hash)
        {
            return Chain.GetTransaction(hash);
        }

        public override uint GetBlockHeight()
        {
            return Chain.BlockHeight;
        }

        public override Block GetBlock(UInt256 hash)
        {
            return Chain.GetBlock(hash);
        }

        public override Block GetBlock(uint height)
        {
            return Chain.GetBlock(height);
        }

        public override Dictionary<string, decimal> GetAssetBalancesOf(string address)
        {
            var hash = address.AddressToScriptHash();
            var account = Chain.GetAccount(new UInt160(hash));

            var result = new Dictionary<string, decimal>();

            foreach (var asset in NeoAPI.Assets)
            {
                var symbol = asset.Key;
                var assetID = GetAssetID(symbol);

                if (account.balances.ContainsKey(assetID))
                {
                    result[symbol] = account.balances[assetID].ToDecimal();
                }
            }

            return result;
        }

        public override Dictionary<string, List<UnspentEntry>> GetUnspent(UInt160 hash)
        {
            var result = new Dictionary<string, List<UnspentEntry>>();

            var account = Chain.GetAccount(hash);

            foreach (var entry in account.unspent)
            {
                var tx = Chain.GetTransaction(entry.prevHash);
                var output = tx.outputs[entry.prevIndex];

                var unspent = new UnspentEntry() { index = entry.prevIndex, txid = entry.prevHash.ToString().Replace("0x",""), value = output.value };

                var symbol = NeoAPI.SymbolFromAssetID(output.assetID);

                List<UnspentEntry> list;

                if (result.ContainsKey(symbol))
                {
                    list = result[symbol];
                }
                else
                {
                    list = new List<UnspentEntry>();
                    result[symbol] = list;
                }

                list.Add(unspent);
            }

            return result;
        }

        protected override string GetRPCEndpoint()
        {
            return "/emulator";
        }

        public override void SetLogger(Action<string> logger = null)
        {
            base.SetLogger(logger);
            this.Chain.SetLogger(logger);
        }
    }
}
