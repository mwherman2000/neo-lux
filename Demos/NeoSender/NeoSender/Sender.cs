using Neo.Lux.Core;
using Neo.Lux.Cryptography;
using Neo.Lux.Utils;
using System;
using System.Globalization;

namespace Neo.Sender
{
    class Sender
    {
        static void Main(string[] args)
        {
            if (args.Length<5)
            {
                Console.WriteLine("neo-sender <Net> <PrivateKey> <DestAddress> <Symbol> <Amount>");
                Console.WriteLine("Net          Can be Main, Test or custom URL");
                Console.WriteLine("PrivateKey   Can be a full hex private key or a WIF private key");
                Console.WriteLine("DestAddress  Must be a valid Neo address");
                Console.WriteLine("Symbol       Must be Neo, Gas or any support token (eg: RPX, DBC)");
                return;
            }

            var keyStr = args[1];
            var outputAddress = args[2];

            var symbol = args[3].ToUpper();   //"GAS"

            string amountStr = args[4];
            amountStr = amountStr.Replace(",", ".");
            var amount = Decimal.Parse(amountStr, CultureInfo.InvariantCulture);

            if (amount <= 0 || amount > 10000000) {
                Console.WriteLine("Invalid amount: " + amount);
                return;
            }

            var fromKey = keyStr.Length == 52 ? KeyPair.FromWIF(keyStr) : new KeyPair(keyStr.HexToBytes());

            Console.WriteLine($"Sending {amount} {symbol} from {fromKey.address} to {outputAddress}");

            var net = args[0].ToLowerInvariant();
            NeoAPI api;

            switch (net)
            {

                case "main":
                    api = NeoDB.ForMainNet();
                    // api = NeoDB.NewCustomRPC("http://api.wallet.cityofzion.io", "http://seed2.cityofzion.io:8080");
                    break;
                case "test":
				    api = NeoDB.ForTestNet();
                    break;
                default:
                    Console.Error.WriteLine("invalid net");
                    return;
            }

            Transaction tx = null;

            try
            {
                if (api.IsToken(symbol))
                {
                    var token = api.GetToken(symbol);
                    tx = token.Transfer(fromKey, outputAddress, amount);
                }
                else
                if (api.IsAsset(symbol))
                {
                    tx = api.SendAsset(fromKey, outputAddress, symbol, amount);
                }
                else
                {
                    Console.WriteLine("Unknown symbol.");
                    Environment.Exit(-1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error executing transaction. "+e.Message);
                Environment.Exit(-1);
            }

            if (tx != null)
            {
                Console.WriteLine("Transaction hash: " + tx.Hash);
            }
            else
            {
                Console.WriteLine("Transaction not accepted by mempool");
            }
        }
    }
}
