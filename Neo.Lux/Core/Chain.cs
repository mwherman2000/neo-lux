using System;
using System.Collections.Generic;
using System.Linq;

namespace Neo.Lux.Core
{
    public class Chain
    {
        protected Dictionary<uint, Block> _blocks = new Dictionary<uint, Block>();

        public Chain(NeoAPI api)
        {
            var max = api.GetBlockHeight();

            for (uint i=0; i<=max; i++)
            {
                var block = api.GetBlock(i);
                _blocks[i] = block;
            }
        }

        public Block GetBlock(uint height)
        {
            return _blocks.ContainsKey(height) ? _blocks[height] : null;
        }
    }

    public class VirtualChain : Chain
    {
        public VirtualChain(NeoAPI api) : base(api)
        {

        }

        public void GenerateBlock(IEnumerable<Transaction> transactions)
        {
            var block = new Block();

            var rnd = new Random();
            block.Consensus = ((long)rnd.Next() << 32) + rnd.Next();
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

            _blocks[block.Height] = block;
        }
    }
}
