using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using System;
using System.IO;

namespace Neo.Lux.Core
{
    public class Block
    {
        public uint Version;
        public uint Height;
        public UInt256 Hash;
        public byte[] MerkleRoot;
        public UInt256 PreviousHash;
        public DateTime Timestamp;
        public long ConsensusData;
        public UInt160 Validator;
        public Transaction[] transactions;
        public Witness witness;

        public override string ToString()
        {
            return Hash.ToString();
        }

        public static Block Unserialize(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    return Unserialize(reader);
                }
            }
        }

        public byte[] Serialize()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(Version);
                    writer.Write(PreviousHash.ToArray());
                    writer.Write(MerkleRoot);
                    writer.Write((uint)Timestamp.ToTimestamp());
                    writer.Write(Height);
                    writer.Write(ConsensusData);
                    writer.Write(Validator.ToArray());
                    return stream.ToArray();
                }
            }
        }

        public static Block Unserialize(BinaryReader reader)
        {
            var block = new Block();

            block.Version = reader.ReadUInt32();
            block.PreviousHash = new UInt256(reader.ReadBytes(32));
            block.MerkleRoot = reader.ReadBytes(32);
            block.Timestamp = reader.ReadUInt32().ToDateTime();
            block.Height = reader.ReadUInt32();
            block.ConsensusData = reader.ReadInt64();

            var nextConsensus = reader.ReadBytes(20);
            block.Validator = new UInt160(nextConsensus);

            var pad = reader.ReadByte(); // should be 1
            block.witness = Witness.Unserialize(reader);

            var txCount = (int)reader.ReadVarInt();
            block.transactions = new Transaction[txCount];
            for (int i = 0; i < txCount; i++)
            {
                block.transactions[i] = Transaction.Unserialize(reader);
            }

            var lastPos = reader.BaseStream.Position;

            var data = block.Serialize();

            var hash = CryptoUtils.Hash256(data);
            block.Hash = new UInt256(hash);

            return block;
        }
    }
}
