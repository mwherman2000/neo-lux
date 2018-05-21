using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Neo.Lux.Cryptography;
using Neo.Lux.Core;
using Neo.Lux.Utils;

namespace Neo.Lux.Airdropper
{
    class CustomRPCNode: NeoDB
    {
        private int n = 0;

        public CustomRPCNode() : base("http://api.wallet.cityofzion.io")
        {
            this.rpcEndpoint = null;
        }

        protected override string GetRPCEndpoint()
        {
            n++;
            var result =  "https://seed"+n+".redpulse.com:10331";
            if (n > 4) n = 0;
            return result;
        }
    }

    class AirDropper
    {
        static void ColorPrint(ConsoleColor color, string text) {
            var ctemp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ctemp;
        }

        const string result_fileName = "airdrop_result.txt";

        static void Main()
        {
            string fileName;

            do
            {
                Console.Write("Enter whitelist file name or NEO address: ");
                fileName = Console.ReadLine();

                if (!fileName.Contains("."))
                {
                    break;
                }

                if (File.Exists(fileName))
                {
                    break;
                }
            } while (true);

            List<string> lines;

            if (fileName.Contains("."))
            {
                lines = File.ReadAllLines(fileName).ToList();
            }
            else
            {
                lines = new List<string>() { fileName };
            }

            if (File.Exists(result_fileName))
            {
                var finishedLines = File.ReadAllLines(result_fileName);
                var finishedAddresses = new HashSet<string>();
                foreach (var entry in finishedLines)
                {
                    var temp = entry.Split(',');
                    finishedAddresses.Add(temp[0]);
                }

                var previousTotal = lines.Count;

                lines = lines.Where(x => !finishedAddresses.Contains(x)).ToList();

                var skippedTotal = previousTotal - lines.Count;

                Console.WriteLine($"Skipping {skippedTotal} addresses...");

            }

            //var api = NeoDB.ForMainNet();            
            //var api = new LocalRPCNode(10332, "http://neoscan.io");
            var api = new CustomRPCNode();

            api.SetLogger(x =>
            {
                ColorPrint(ConsoleColor.DarkGray, x);
            });
            
            string privateKey;
            byte[] scriptHash = null;
            
            do
            {
                Console.Write("Enter WIF private key: ");
                privateKey = Console.ReadLine();

                if (privateKey.Length == 52)
                {
                    break;
                }

            } while (true);

            var keys = KeyPair.FromWIF(privateKey);
            Console.WriteLine("Public address: " + keys.address);

            do
            {
                Console.Write("Enter contract script hash or token symbol: ");
                var temp = Console.ReadLine();

                scriptHash = NeoAPI.GetScriptHashFromSymbol(temp);

                if (scriptHash == null && temp.Length == 40)
                {
                    scriptHash = NeoAPI.GetScriptHashFromString(temp);
                }

            } while (scriptHash == null);


            var token = new NEP5(api, scriptHash);

            decimal amount;
            Console.WriteLine($"Write amount of {token.Symbol} to distribute to each address:");
            do
            {
                if (decimal.TryParse(Console.ReadLine(), out amount) && amount > 0)
                {
                    break;
                }
            } while (true);

            int skip = 0;
            int done = 0;

            Console.WriteLine($"Initializing {token.Name} airdrop...");

            var srcBalance = token.BalanceOf(keys);
            Console.WriteLine($"Balance of {keys.address} is {srcBalance} {token.Symbol}");

            var minimum = lines.Count * amount;
            if (srcBalance < minimum)
            {
                ColorPrint(ConsoleColor.Red, $"Error: For this Airdrop you need at least {minimum} {token.Symbol} at {keys.address}");
                Console.ReadLine();
                return;
            }

            foreach (var temp in lines)
            {
                var address = temp.Trim();
                if (!address.IsValidAddress())
                {
                    skip++;
                    ColorPrint(ConsoleColor.Yellow, "Invalid address: " + address);
                    continue;
                }

                var hash = address.GetScriptHashFromAddress();
                var balance = token.BalanceOf(hash);
                
                Console.WriteLine($"Found {address}: {balance} {token.Symbol}");

                Console.WriteLine($"Sending {token.Symbol} to  {address}");
                Transaction tx = null;

                int failCount = 0;
                int failLimit = 20;
                do
                {
                    int tryCount = 0;
                    int tryLimit = 3;
                    do
                    {
                        var result = token.Transfer(keys, address, amount);
                        Thread.Sleep(1000);

                        if (result != null)
                        {
                            tx = result.transaction;
                            break;
                        }

                        Console.WriteLine("Tx failed, retrying...");

                        tryCount++;
                    } while (tryCount < tryLimit);


                    if (tx != null)
                    {
                        break;
                    }
                    else
                    {

                        Console.WriteLine("Changing RPC server...");
                        Thread.Sleep(2000);
                        api.rpcEndpoint = null;
                        failCount++;
                    }
                } while (failCount< failLimit);

                if (failCount >= failLimit || tx == null)
                {
                    ColorPrint(ConsoleColor.Red, "Try limit reached, internal problem maybe?");
                    break;
                }

                Console.WriteLine("Unconfirmed transaction: " + tx.Hash);

                api.WaitForTransaction(keys, tx);

                ColorPrint(ConsoleColor.Green, "Confirmed transaction: " + tx.Hash);
                
                File.AppendAllText(result_fileName, $"{address},{tx.Hash}\n");

                done++;
            }

            Console.WriteLine($"Skipped {skip} invalid addresses.");
            Console.WriteLine($"Airdropped {amount} {token.Symbol} to {done} addresses.");

            Console.WriteLine("Finished.");
            Console.ReadLine();
        }
    }
}
