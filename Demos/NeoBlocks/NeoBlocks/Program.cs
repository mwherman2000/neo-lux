using Neo.Lux.Core;
using Neo.Lux.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Numerics;

namespace NeoBlocks
{
    class Program
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


        static void ExportBlocks(int chunk, uint block, List<string> lines)
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

        static uint LoadChunk(string fileName)
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
                    }
                }
            }

            return currentBlock;
        }

        static void Main(string[] args)
        {
            var files = Directory.EnumerateFiles("chain").OrderBy(c =>
            {
                var temp = c.Replace("chain\\chunk", "");
                return int.Parse(temp);
            }).ToList();

            uint startBlock = 0;

            var chunk = 0;

            var startT = Environment.TickCount;
            foreach (var file in files)
            {
                Console.WriteLine("Loading " + file);
                startBlock = LoadChunk(file) + 1;
                chunk++;
            }

            var endT = Environment.TickCount;
            var delta = (endT - startT) / 1000;
            Console.WriteLine("Finished in "+delta+" seconds");
            Console.ReadLine();
            return;

            var lines = new List<string>();

            var api = new LocalRPCNode(10332, "http://neoscan.io");
            var blockCount = api.GetBlockHeight();

            BigInteger lastP = 0;

            for (uint i=0; i<=blockCount; i++)
            {
                if (lines.Count == 2500)
                {
                    ExportBlocks(chunk, startBlock, lines);

                    chunk++;
                    startBlock = i;
                }

                var response = api.QueryRPC("getblock", new object[] { i });
                var blockData = response.GetString("result");
                lines.Add(blockData);

                if (i == 2477)
                {
                    Console.WriteLine(blockData);
                }

                BigInteger p = (i * 100) / blockCount;
                if (p != lastP)
                {
                    lastP = p;
                    Console.WriteLine(p + "%");
                }
            }

            ExportBlocks(chunk, startBlock, lines);

            /*
            var block = GetBlock(api, 2193680);

            Console.WriteLine("Block hash: " + block.Hash.ByteToHex());
            foreach(var tx in block.transactions)
            {
                Console.WriteLine($"{tx.Hash} => {tx.type}");
            }*/

            Console.WriteLine("Finished");
            Console.ReadLine();
        }
    }
}
