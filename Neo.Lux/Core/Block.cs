using Neo.Lux.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Lux.Core
{
    public class Block
    {
        public uint Version;
        public uint Height;
        public byte[] Hash;
        public byte[] MerkleRoot;
        public byte[] PreviousHash;
        public DateTime Timestamp;
        public long Consensus;
        public byte[] NextConsensus;
        public Transaction[] transactions;
        public Witness witness;

        public static Block Unserialize(BinaryReader reader)
        {
            var block = new Block();
            block.Version = reader.ReadUInt32();
            block.Hash = reader.ReadBytes(32);
            block.MerkleRoot = reader.ReadBytes(32);
            block.Timestamp = reader.ReadUInt32().ToDateTime();
            block.Height = reader.ReadUInt32();
            block.Consensus = reader.ReadInt64();
            block.NextConsensus = reader.ReadBytes(20);
            var pad = reader.ReadByte(); // should be 1
            block.witness = Witness.Unserialize(reader);

            var txCount = (int)reader.ReadVarInt();
            block.transactions = new Transaction[txCount];
            for (int i = 0; i < txCount; i++)
            {
                block.transactions[i] = Transaction.Unserialize(reader);
            }

            return block;
        }
    }
}
