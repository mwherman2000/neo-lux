using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using Neo.Lux.VM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Neo.Lux.Core
{
    public class Asset
    {
        public UInt256 hash;
        public string name;
    }

    public class Account
    {
        public UInt160 hash;

        public Dictionary<byte[], BigInteger> balances = new Dictionary<byte[], BigInteger>(new ByteArrayComparer());

        public List<Transaction.Input> unspent = new List<Transaction.Input>();

        public Dictionary<byte[], byte[]> storage = new Dictionary<byte[], byte[]>(new ByteArrayComparer());

        public Contract contract;

        public void Deposit(byte[] asset, BigInteger amount, Transaction.Input input)
        {
            var balance = balances.ContainsKey(asset) ? balances[asset] : 0;
            balance += amount;
            balances[asset] = balance;

            unspent.Add(input);
        }

        public void Withdraw(byte[] asset, BigInteger amount, Transaction.Input input)
        {
            var balance = balances.ContainsKey(asset) ? balances[asset] : 0;
            balance -= amount;
            balances[asset] = balance;
            unspent.RemoveAll( x => x.prevHash == input.prevHash && x.prevIndex == input.prevIndex);
        }
    }

    public class Chain: InteropService, IScriptTable
    {
        protected Dictionary<uint, Block> _blocks = new Dictionary<uint, Block>();

        protected Dictionary<UInt256, Block> _blockMap = new Dictionary<UInt256, Block>();
        protected Dictionary<UInt256, Transaction> _txMap = new Dictionary<UInt256, Transaction>();
        protected Dictionary<byte[], Asset> _assetMap = new Dictionary<byte[], Asset>(new ByteArrayComparer());
        protected Dictionary<UInt160, Account> _accountMap = new Dictionary<UInt160, Account>();

        public void SyncWithNode(NeoAPI api)
        {
            var max = api.GetBlockHeight();

            var min = (uint)_blocks.Count;

            for (uint i = min; i<=max; i++)
            {
                var block = api.GetBlock(i);
                AddBlock(block);
            }
        }

        protected void AddBlock(Block block)
        {
            _blocks[block.Height] = block;
            _blockMap[block.Hash] = block;

            foreach (var tx in block.transactions)
            {
                ExecuteTransaction(tx);
            }

            foreach (var tx in block.transactions)
            {
                _txMap[tx.Hash] = tx;
            }
        }

        private void ExecuteTransaction(Transaction tx)
        {
            foreach (var input in tx.inputs)
            {
                var input_tx = GetTransaction(input.prevHash);
                var output = input_tx.outputs[input.prevIndex];

                var account = GetAccount(output.scriptHash);
                account.Withdraw(output.assetID, output.value.ToBigInteger(), input);
            }

            uint index = 0;
            foreach (var output in tx.outputs)
            {
                var account = GetAccount(output.scriptHash);
                var unspent = new Transaction.Input() { prevIndex = index, prevHash = tx.Hash };
                account.Deposit(output.assetID, output.value.ToBigInteger(), unspent);
                index++;
            }

            switch (tx.type)
            {
                case TransactionType.PublishTransaction:
                    {
                        var contract_hash = tx.contractRegistration.script.ToScriptHash();
                        var account = GetAccount(contract_hash);
                        account.contract = tx.contractRegistration;
                        break;
                    }

                case TransactionType.ContractTransaction:
                case TransactionType.InvocationTransaction:
                    {
                        var vm = new ExecutionEngine(tx, this, this);
                        vm.LoadScript(tx.script);
                        vm.Execute();
                        break;
                    }
            }
        }

        public Block GetBlock(uint height)
        {
            return _blocks.ContainsKey(height) ? _blocks[height] : null;
        }

        public Block GetBlock(UInt256 hash)
        {
            return _blockMap.ContainsKey(hash) ? _blockMap[hash] : null;
        }

        public Account GetAccount(UInt160 hash)
        {
            if (_accountMap.ContainsKey(hash))
            {
                return _accountMap[hash];
            }

            var account = new Account();
            account.hash = hash;
            _accountMap[hash] = account;
            return account;
        }

        public Asset GetAsset(byte[] id)
        {
            return _assetMap.ContainsKey(id) ? _assetMap[id] : null;
        }

        public Transaction GetTransaction(UInt256 hash)
        {
            return _txMap.ContainsKey(hash) ? _txMap[hash] : null;
        }

        public byte[] GetScript(byte[] script_hash)
        {
            var hash = new UInt160(script_hash);
            var account = GetAccount(hash);
            return account != null && account.contract != null ? account.contract.script : null;
        }
    }

    public class VirtualChain : Chain
    {
        public VirtualChain(NeoAPI api, KeyPair owner) 
        {
            var scripthash = new UInt160(owner.signatureHash.ToArray());

            var txs = new List<Transaction>();
            foreach (var symbol in NeoAPI.AssetSymbols)
            {
                var neo = NeoAPI.GetAssetID(symbol);
                var tx = new Transaction();
                tx.outputs = new Transaction.Output[] { new Transaction.Output() { assetID = neo, scriptHash = scripthash, value = 1000000000 } };
                tx.inputs = new Transaction.Input[] { };
                txs.Add(tx);
            }
            GenerateBlock(txs);
        }

        public void GenerateBlock(IEnumerable<Transaction> transactions)
        {
            var block = new Block();

            var rnd = new Random();
            block.ConsensusData = ((long)rnd.Next() << 32) + rnd.Next();
            block.Height = (uint) _blocks.Count;
            block.PreviousHash = _blocks.Count > 0 ? _blocks[(uint)(_blocks.Count - 1)].Hash : null;
            //block.MerkleRoot = 
            block.Timestamp = DateTime.UtcNow;
            //block.Validator = 0;
            block.Version = 0;
            block.transactions = new Transaction[transactions.Count()];

            int index = 0;
            foreach (var tx in transactions)
            {
                block.transactions[index] = tx;
                index++;
            }

            AddBlock(block);
        }
    }
}
