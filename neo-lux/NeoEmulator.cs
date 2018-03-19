using System.Collections.Generic;
using Neo.Cryptography;
using Neo.Emulator.API;

namespace NeoLux
{
    public class NeoEmulator : NeoAPI
    {
        private Blockchain blockchain;

        public NeoEmulator()
        {
            this.blockchain = new Blockchain();
        }
        
        public override Dictionary<string, decimal> GetBalancesOf(string address, bool getTokens = false)
        {
            var acc = blockchain.FindAccountByAddress(address);
            var result = new Dictionary<string, decimal>();
            foreach (var entry in acc.balances)
            {
                result[entry.Key] = entry.Value;
            }
            return result;
        }

        public override byte[] GetStorage(string scriptHash, byte[] key)
        {
            var bytes = scriptHash.HexToBytes();
            var hash = new UInt160(bytes);
            var acc = blockchain.FindAccountByHash(hash);

            if (acc == null || acc.storage == null)
            {
                return null;
            }

            return acc.storage.Read(key);
        }

        public override Dictionary<string, List<UnspentEntry>> GetUnspent(string address)
        {
            var acc = blockchain.FindAccountByAddress(address);
            var result = new Dictionary<string, List<UnspentEntry>>();
            foreach (var entry in acc.balances)
            {
                var unspents = new List<UnspentEntry>();
                unspents.Add(new UnspentEntry() { index = 0, txid = "", value = entry.Value });
                result[entry.Key] = unspents;
            }

            return result;
        }

        public override bool SendRawTransaction(string hexTx)
        {
            throw new System.NotImplementedException();
        }

        public override InvokeResult TestInvokeScript(string scriptHash, object[] args)
        {
            throw new System.NotImplementedException();
        }
    }
}
