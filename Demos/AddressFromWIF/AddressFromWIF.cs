using System;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;

namespace AddressFromWIF
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("ERROR! expected a WIF key as argument");
                return;
            }

            var keyStr = args[0];
            var fromKey = keyStr.Length == 52 ? KeyPair.FromWIF(keyStr) : new KeyPair(keyStr.HexToBytes());

            Console.WriteLine(keyStr + "\t" +fromKey.address);
        }
    }
}
