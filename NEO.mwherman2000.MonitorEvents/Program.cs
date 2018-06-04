using Neo.Lux.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEO.mwherman2000.MonitorEvents
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = NeoRPC.ForPrivateNet();

            uint currentBlock = api.GetBlockHeight();
            Console.WriteLine($"currentBlock: {currentBlock}");

            var monitor = new NeoEventMonitor(api);

            monitor.Run(500);

            Console.WriteLine("Monitoring: press Enter to stop...");
            Console.ReadLine();

            monitor.Stop();

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
