//#define MANUAL
//#define FILTER

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Neo.Cryptography;
using Neo.Emulator;

namespace NeoLux.WhiteList
{
    class WhitelistActivator
    {
        static void Main()
        {
            var api = NeoDB.ForMainNet();

            string privateKey;
            string scriptHash;

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
                Console.Write("Enter contract script hash: ");
                scriptHash = Console.ReadLine();

                if (scriptHash.StartsWith("0x"))
                {
                    scriptHash = scriptHash.Substring(2);
                }

                if (scriptHash.Length == 40)
                {
                    break;
                }
            } while (true);

            var keys = KeyPair.FromWIF(privateKey);

            string fileName;

#if !MANUAL
            do
            {
                Console.Write("Enter whitelist file name: ");
                fileName = Console.ReadLine();

                if (File.Exists(fileName))
                {
                    break;
                }
            } while (true);

            var lines = File.ReadAllLines(fileName);
#endif

#if FILTER
            var filtered = new HashSet<string>();
#endif

            int skip = 0;
            int add = 0;
            int errors = 0;
#if MANUAL
            while (true)
#else
            foreach (var temp in lines)
#endif
            {
                

#if MANUAL
                Console.Write("Enter address: ");
                var line = Console.ReadLine();
#else
                var line = temp.Trim();
#endif

                if (line.Length != 34 || !line.StartsWith("A"))
                {
                    skip++;
                    //Console.WriteLine("Invalid address");
                    continue;
                }

                try
                {
                    var hash = line.GetScriptHashFromAddress();

                    Console.WriteLine(line+ "," + hash.ByteToHex());

                    for (int i=1; i<=100; i++)
                    {
                        var p = api.TestInvokeScript(scriptHash, new object[] { "checkWhitelist", new object[] { hash } });
                        var state = System.Text.Encoding.UTF8.GetString((byte[])p.result);
                        Console.WriteLine(state=="on"?"Done": "Retrying...");

                        if (state == "on")
                        {
                            break;
                        }

#if FILTER
                        filtered.Add(line);
                        break;
#else
                        api.CallContract(keys, scriptHash, "enableWhitelist", new object[] { hash });
                        Console.WriteLine("Waiting for next block... 15seconds");
                        Thread.Sleep(15000);
#endif
                    }
                } catch
                {
                    errors++;
                }

                add++;
            }

#if FILTER

            File.WriteAllLines("filtered.csv", filtered.ToArray());
            Console.WriteLine($"{filtered.Count} addresses still to need to be added the whitelist.");
#else
            Console.WriteLine($"Skipped {skip} invalid addresses.");
            Console.WriteLine($"Failed {errors} addresses due to network errors.");
            Console.WriteLine($"Finished adding {add} addresses to the whitelist.");
#endif

            Console.ReadLine();
        }
    }
}
