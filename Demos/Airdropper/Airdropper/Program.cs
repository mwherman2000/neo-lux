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
    class AirDropper
    {
        static void ColorPrint(ConsoleColor color, string text) {
            var ctemp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = ctemp;
        }

        static void Main()
        {
            var api = NeoDB.ForMainNet();

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

            var oldBlock = api.GetBlockHeight();

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
                Transaction tx;

                int failCount = 0;
                int failLimit = 20;
                do
                {
                    int tryCount = 0;
                    int tryLimit = 3;
                    do
                    {
                        tx = token.Transfer(keys, address, amount);
                        Thread.Sleep(1000);

                        if (tx != null)
                        {
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

                uint newBlock;

                do
                {
                    Thread.Sleep(5000);
                    newBlock = api.GetBlockHeight();
                } while (newBlock == oldBlock);

                oldBlock++;
                while (oldBlock < newBlock)
                {
                    var other = api.GetBlock(oldBlock);

                    if (other != null)
                    {
                        foreach (var entry in other.transactions)
                        {
                            if (entry.Hash == tx.Hash)
                            {
                                oldBlock = newBlock;
                                break;
                            }
                        }

                        oldBlock++;
                    }
                    else
                    {
                        Thread.Sleep(5000);
                    }

                }


                ColorPrint(ConsoleColor.Green, "Confirmed transaction: " + tx.Hash);
 
                File.AppendAllText("airdrop_result.txt", $"{address},{tx.Hash}\n");

                done++;
            }

            Console.WriteLine($"Skipped {skip} invalid addresses.");
            Console.WriteLine($"Airdropped {amount} {token.Symbol} to {done} addresses.");

            Console.WriteLine("Finished.");
            Console.ReadLine();
        }
    }
}
