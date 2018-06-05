using Neo.Lux.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Neo.Lux.Utils;
using Neo.Lux.Cryptography;
using System.Threading;

namespace NEO.mwherman2000.MonitorEvents
{
    class Program
    {
        public static NeoRPC _api;
        public static uint _currentBlock;
        public static Transaction _deployTx;
        public static uint _deployTxBlock;

        static void Main(string[] args)
        {
            const string demoPrivateKeyHex = "a9e2b5436cab6ff74be2d5c91b8a67053494ab5b454ac2851f872fb0fd30ba5e";

            const string my0Address = "AaLLC7svH3D117BUbCejPDSVmQqWsPDx3P";
            const string my0PublicKey = "02ad750a0a9d0ff45909de5f4ec7ee7eb679572f401f6b876ced386d763d28865d";
            const string my0PrivateKeyHex = "a16dc73091555ead9bf495611662411e6c4921e439c326f81d054b2cf85d64f5";
            const string my0PrivateKeyWIF = "L2dWPCDeD2uPfTaHw5urGeYUCugkPETCWweyKmdvwggixKG9Qqf3";

            string deployScriptHashHex = "0xf036ad1b59f21c09e3247fd5acb3bc73669547d2";
            string deployTxHashHex = "0xb6fc8de574c2d6b2e77f592efe4bd7c82ec0682bf255b91f61754543b9730375";
            string invokeTxHashHex = "0xf983108d2581ca7499118c219fde3c90f55cbb114200b8e3e31e39d1ce6614d9";

            //const string my0Address = "AN3LHQcpvwAfJAHk6DvzHyFxHr4KcY5mdA";
            //const string my0PrivateKeyHex = "13c5ae3e47d1e1421c2adb620ee134457590e9ba19f5f891d19c183b0d06d521";
            //const string my0PrivateKeyWIF = "Kwt9PVkXBPA1ZPtTAArBhmtQy2AkcYhs9MZU4YuLdW56Uoy3ddWN";
            //const string my0PublicKey = "02bc19691ee7253b1b48efc189c89d02b8e231ef5d39f45a8eda2d6536853dcded";
            //string deployScriptHashHex = "0xf036ad1b59f21c09e3247fd5acb3bc73669547d2";
            //string deployTxHashHex = "0x416a12bc0eac0b126e258a198702d5637dbe0d7401c62f17fa01f766dbf3f9f8";
            //string invokeTxHashHex = "0x13db6f23f2fc9d582f5a8e03bec0cb9c65c0828372547ba4263aa15906e46c29";

            const string my1Address = "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y";
            const string my1PrivateKeyHex = "1dd37fba80fec4e6a6f13fd708d8dcb3b29def768017052f6c930fa1c5d90bbb";
            const string my1PrivateKeyWIF = "KxDgvEKzgSBPPfuVfw67oPQBSjidEiqTHURKSDL1R7yGaGYAeYnr";
            const string my1PublicKey = "031a6c6fbbdf02ca351745fa86b9ba5a9452d785ac4f7fc2b7548ca2a46c4fcf4a";
            // 0xb7743f023b2cbb367f21b26d8da837929f1309f3
            const string avmFilename = @"D:\repos\neo-lux\NEO.mwherman2000.DeployTestSC1\bin\Debug\NEO.mwherman2000.DeployTestSC1.avm";

            _api = NeoRPC.ForPrivateNet();
            _currentBlock = _api.GetBlockHeight();
            Console.WriteLine($"currentBlock: {_currentBlock}");
            _deployTxBlock = 31163;

            Console.WriteLine($"deployScriptHash: {deployScriptHashHex}");
            Console.WriteLine($"deployTxHash: {deployTxHashHex}");
            _deployTx = _api.GetTransaction(deployTxHashHex.Replace("0x", ""));
            Console.WriteLine($"deployTx: {JsonConvert.SerializeObject(_deployTx)}");
            byte[] deployTxScript = _deployTx.script;
            int deployTxScriptLength = deployTxScript.Length;
            Console.WriteLine($"deployTxScriptLength: {deployTxScriptLength}");

            UInt160 deployTxScriptHash = deployTxScript.ToScriptHash();
            string deployTxSciptToScriptHashString = deployTxScriptHash.ToString();
            Console.WriteLine($"deployTxSciptToScriptHashString: {deployTxSciptToScriptHashString}");

            {
                Console.WriteLine($"invokeTxHashHex: {invokeTxHashHex}");
                Transaction invokeTx = _api.GetTransaction(invokeTxHashHex.Replace("0x", ""));
                Console.WriteLine($"invokeTx: {JsonConvert.SerializeObject(invokeTx)}");
                byte[] invokeTxScript = invokeTx.script;
                int invokeTxScriptLength = invokeTxScript.Length;
                Console.WriteLine($"invokeTxScriptLength: {invokeTxScriptLength}");

                UInt160 invokeTxScriptHash = invokeTxScript.ToScriptHash();
                string invokeTxSciptToScriptHashString = invokeTxScriptHash.ToString();
                Console.WriteLine($"invokeTxSciptToScriptHashString: {invokeTxSciptToScriptHashString}");
            }

            Console.WriteLine("Monitoring: press Enter to start monitoring...");
            Console.ReadLine();

            //var monitor = new NeoEventMonitor1(_api, deployTx, deployTxBlock-5);
            //monitor.Run();
            //Console.WriteLine("Monitoring: press Enter to stop monitoring...");
            //Console.ReadLine();
            //monitor.Stop();

            //Task<bool> monitor = NeoEventMonitor();
            //Console.WriteLine("Monitoring started...");

            var my0Keys = new KeyPair(my0PrivateKeyHex.HexToBytes());
            Console.WriteLine($"my0PrivateKeyHex.keys:\t{JsonConvert.SerializeObject(my0Keys)}");
            var my1Keys = new KeyPair(my1PrivateKeyHex.HexToBytes());
            Console.WriteLine($"my1PrivateKeyHex.keys:\t{JsonConvert.SerializeObject(my1Keys)}");

            Console.WriteLine();
            Console.WriteLine("Balances:");
            var balances = _api.GetAssetBalancesOf(my0Keys.address);
            foreach (var entry in balances)
            {
                Console.WriteLine(entry.Key + "\t" + entry.Value);
            }
            balances = _api.GetAssetBalancesOf(my1Keys.address);
            foreach (var entry in balances)
            {
                Console.WriteLine(entry.Key + "\t" + entry.Value);
            }

            //Console.WriteLine();
            //Console.WriteLine("Unspent Balances:");
            //var unspentBalances = _api.GetUnspent(my0Keys.address);
            //foreach (var entry in unspentBalances)
            //{
            //    Console.WriteLine(entry.Key + "\t" + JsonConvert.SerializeObject(entry.Value));
            //}
            //unspentBalances = _api.GetUnspent(my1Keys.address);
            //foreach (var entry in unspentBalances)
            //{
            //    Console.WriteLine(entry.Key + "\t" + JsonConvert.SerializeObject(entry.Value));
            //}

            //Console.WriteLine();
            //Console.WriteLine("GetClaimable Balances:");
            //decimal amount = 0;
            //var claimableGas = _api.GetClaimable(my0Keys.PublicKeyHash, out amount);
            //Console.WriteLine($"GetClaimable:\t{amount}");
            //foreach (var entry in claimableGas)
            //{
            //    Console.WriteLine(JsonConvert.SerializeObject(entry));
            //}
            //claimableGas = _api.GetClaimable(my1Keys.PublicKeyHash, out amount);
            //Console.WriteLine($"GetClaimable:\t{amount}");
            //foreach (var entry in claimableGas)
            //{
            //    Console.WriteLine(JsonConvert.SerializeObject(entry));
            //}

            var oldLogger = _api.Logger;
            _api.SetLogger(LocalLogger);
            for (int time = 0; time < 10; time++)
            {
                uint blockheight = _api.GetBlockHeight();
                Console.WriteLine($"blockheight: {blockheight}");
                while (_api.GetBlock(blockheight) == null) { Console.WriteLine("...waiting..."); Thread.Sleep(10000); }
                Thread.Sleep(10000);
                Console.WriteLine($"blockheight: {blockheight} found");

                Console.WriteLine($"#### START {time} calltx");
                string operation = "testcallcontract";
                object[] callargs = { 1024+time, 1024+time+1, 1024+time+2 };
                Transaction calltx = _api.CallContract(my0Keys, UInt160.Parse(deployScriptHashHex), operation, callargs);
                Console.WriteLine($"#### END   {time} calltx\t{JsonConvert.SerializeObject(calltx)}");
            }
            _api.SetLogger(oldLogger);

            Console.WriteLine("Waiting for task...");
            //monitor.Wait();

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }

        private static async Task<bool> NeoEventMonitor()
        {
            var monitor = new NeoEventMonitor1(_api, _deployTx, _deployTxBlock - 5);
            bool result = await Task.Run(() => monitor.Run());
            return result;
        }

        private static void LocalLogger(string s)
        {
            Console.WriteLine($"LOCALLOGGER: '{s}'");
        }
    }
}
