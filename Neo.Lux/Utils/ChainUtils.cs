using Neo.Lux.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neo.Lux.Utils
{
    class ChainUtils
    {
        public static byte[] Compress(byte[] data)
        {
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress))
            {
                compressor.Write(data, 0, data.Length);
                compressor.Close();
                return compressStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            var output = new MemoryStream();
            using (var compressedStream = new MemoryStream(data))
            {
                using (var zipStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
                {
                    var buffer = new byte[4096];
                    int read;
                    while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, read);
                    }
                    output.Position = 0;
                    return output.ToArray();
                }
            }
        }

        public static void ExportBlocks(int chunk, uint block, List<string> lines)
        {
            byte[] txData;

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    var curBlock = block;
                    foreach (var line in lines)
                    {
                        var bytes = line.HexToBytes();
                        writer.Write(curBlock);
                        writer.Write(bytes.Length);
                        writer.Write(bytes);
                        curBlock++;
                    }
                }

                txData = stream.ToArray();
            }

            var compressed = Compress(txData);

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {

                    writer.WriteVarInt(lines.Count);
                    writer.Write(compressed.Length);
                    writer.Write(compressed);
                }

                var data = stream.ToArray();
                var fileName = "chain/chunk" + chunk;
                File.WriteAllBytes(fileName, data);

                // LoadChunk(fileName);
            }

            lines.Clear();
        }

        public static List<Block> LoadChunk(string fileName)
        {
            var bytes = File.ReadAllBytes(fileName);

            byte[] txdata;
            int blockCount;
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    blockCount = (int)reader.ReadVarInt();
                    var len = reader.ReadInt32();
                    var compressed = reader.ReadBytes(len);

                    txdata = Decompress(compressed);
                }
            }

            uint currentBlock = 0;
            var blocks = new List<Block>();
            using (var stream = new MemoryStream(txdata))
            {
                using (var reader = new BinaryReader(stream))
                {
                    for (int i = 0; i < blockCount; i++)
                    {
                        currentBlock = reader.ReadUInt32();
                        var len = reader.ReadInt32();
                        var blockData = reader.ReadBytes(len);

                        var block = Block.Unserialize(blockData);
                        blocks.Add(block);
                    }
                }
            }

            return blocks;
        }

    }
}
