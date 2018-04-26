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
    public class Target
    {
        public string address;
        public byte[] hash;

        public decimal balance;

        public Transaction transaction;

        public bool finished;
    }

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

                scriptHash = NeoAPI.GetScriptHashFromSymbol(temp);

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

            var balance = token.BalanceOf(keys);
            var minimum = lines.Count * amount;
            if (balance < minimum)
            {
                Console.WriteLine($"Error: For this Airdrop you need at least {minimum} {token.Symbol} at {keys.address}");
                Console.ReadLine();
                return;
            }

            var targets = new List<Target>();
            
            foreach (var temp in lines)
            {
                var line = temp.Trim();
                if (line.Length != 34 || !line.StartsWith("A"))
                {
                    skip++;
                    lines.RemoveAt(0);
                    Console.WriteLine("Invalid address: " + line);
                    continue;
                }

                var target = new Target();
                target.address = line;
                target.hash = line.GetScriptHashFromAddress();
                target.balance = token.BalanceOf(target.hash);
                target.transaction = null;

                targets.Add(target);

                Console.WriteLine($"Found {target.address}: {target.balance} {token.Symbol}");
            }

            while (targets.Count > 0)
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    var target = targets[i];
                    try
                    {
                        Console.WriteLine($"Sending {token.Symbol} to  {target.address}");
                        target.transaction = token.Transfer(keys, target.address, amount);

                        if (target.transaction != null)
                        {
                            Console.WriteLine("Unconfirmed transaction: "+target.transaction.Hash);
                        }
                        else
                        {
                            Console.WriteLine("Transaction failed");
                        }

                        Thread.Sleep(15000);
                    }
                    catch
                    {
                        continue;
                    }
                }

                for (int i = 0; i < targets.Count; i++)
                {
                    var target = targets[i];

                    if (target.transaction == null)
                    {
                        continue;
                    }

                    try
                    {
                        Console.WriteLine($"Confirming balance: {target.address}. Should have {target.balance + amount} {token.Symbol} or more.");
                        var newBalance = token.BalanceOf(target.hash);

                        Console.WriteLine($"Got {newBalance} {token.Symbol}.");

                        if (newBalance > target.balance)
                        {
                            Console.WriteLine($"Confirming transaction: {target.transaction.Hash}");
                            var tx = api.GetTransaction(target.transaction.Hash);

                            if (tx != null)
                            {
                                Console.WriteLine($"Confirmed {target.address} => {tx.Hash}");
                                File.AppendAllText("airdrop.txt", $"{target.address},{tx.Hash}\n");
                                target.finished = true;
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }

                targets.RemoveAll(x => x.finished);
            }

            Console.WriteLine($"Skipped {skip} invalid addresses.");
            Console.WriteLine($"Airdropped {amount} {token.Symbol} to {done} addresses.");

            Console.WriteLine("Finished.");
            Console.ReadLine();
        }
    }
}
