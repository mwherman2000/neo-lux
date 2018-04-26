using System;
using System.Security.Cryptography;
using Neo.Lux.Cryptography;

namespace WalletGenerator
{
    class Generator
    {
        static void Main(string[] args)
        {
            int nwallets;

            if (args.Length != 1)
            {
                Console.Error.WriteLine("[WARN] Expected argument with number of wallets. Defaulting to 100");
                nwallets = 100;
            }
            else
                nwallets = int.Parse(args[0]);

            Console.WriteLine("Generating " + nwallets + " NEO wallets:\n");

            byte[] privateKey = new byte[32];

            for (int n = 0; n < nwallets; n++)
            {
                // generate a new private key
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(privateKey);
                }

                // generate a key pair
                KeyPair keys = new KeyPair(privateKey);

                // Console.WriteLine ("address: " + keys.address);
                // Console.WriteLine ("raw wif: " + keys.WIF);
                Console.WriteLine(keys.address + "\t" + keys.WIF);
                if (n > 0 && n % 10 == 0)
                    Console.WriteLine("");
            }

            Console.ReadLine(); // pause
        }
    }
}
