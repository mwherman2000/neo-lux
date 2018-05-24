using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public void Deposit(byte[] asset, BigInteger amount)
        {
            var balance = balances.ContainsKey(asset) ? balances[asset] : 0;
            balance += amount;
            balances[asset] = balance;
        }

        public void Withdraw(byte[] asset, BigInteger amount)
        {
            var balance = balances.ContainsKey(asset) ? balances[asset] : 0;
            balance -= amount;
            balances[asset] = balance;
        }
    }

    public class Chain
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
                account.Withdraw(output.assetID, output.value.ToBigInteger());
            }

            foreach (var output in tx.outputs)
            {
                var account = GetAccount(output.scriptHash);
                account.Deposit(output.assetID, output.value.ToBigInteger());
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
