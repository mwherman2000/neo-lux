using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Neo.Lux.Cryptography;
using Neo.Lux.Core;
using Neo.Lux.Utils;

namespace NeoLux.WhiteList
{
    class AirDropper
    {
        static void Main()
        {
            var api = NeoDB.ForMainNet();
            
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

            do
            {
                Console.Write("Enter contract script hash or token symbol: ");
                var temp = Console.ReadLine();

                if (temp.StartsWith("0x"))
                {
                    temp = temp.Substring(2);
                }
                else
                {
                    scriptHash = NeoAPI.GetScriptHashFromSymbol(temp);
                }

                if (scriptHash == null && temp.Length == 40)
                {
                    scriptHash = NeoAPI.GetScriptHashFromString(temp);
                }

            } while (scriptHash == null);

            var keys = KeyPair.FromWIF(privateKey);

            var token = new NEP5(api, scriptHash);

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

            decimal amount;
            Console.WriteLine("Write amount to distribute to each address:");
            do
            {
                if (decimal.TryParse(Console.ReadLine(), out amount) && amount > 0)
                {
                    break;
                }
            } while (true);


            int skip = 0;
            int done = 0;

            var removed = new List<string>();

            Console.WriteLine("Initializng airdrop...");

            var balance = token.BalanceOf(keys);
            var minimum = lines.Count * amount;
            if (balance < minimum)
            {
                Console.WriteLine($"Error: For this Airdrop you need at least {minimum} {token.Symbol} at {keys.address}");
                Console.ReadLine();
                return;
            }

            while (lines.Count > 0)
            {
                var line = lines[0].Trim();
                if (line.Length != 34 || !line.StartsWith("A"))
                {
                    skip++;
                    lines.RemoveAt(0);
                    Console.WriteLine("Invalid address: "+line);
                    continue;
                }

                var hash = line.GetScriptHashFromAddress();

                try
                {
                    var oldBalance = token.BalanceOf(hash);

                    while (true)
                    {
                        var tx = token.Transfer(keys, line, amount);

                        if (tx != null)
                        {
                            Thread.Sleep(20000);
                            var newBalance = token.BalanceOf(hash);

                            if (newBalance > oldBalance)
                            {
                                Console.WriteLine($"{line} => {tx.Hash}");
                                File.AppendAllText("airdrop.txt", $"{line},{tx.Hash}\n");
                                lines.RemoveAt(0);
                                done++;
                                break;
                            }
                        }
                        else
                        {
                            Thread.Sleep(5000);
                        }
                    }
                } catch
                {
                    continue;
                }

            }

            Console.WriteLine($"Skipped {skip} invalid addresses.");
            Console.WriteLine($"Airdropped {amount} {token.Symbol} to {done} addresses.");

            Console.ReadLine();
        }
    }
}
