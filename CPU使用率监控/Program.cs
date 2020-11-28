using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace CPU使用率监控
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("config.json", true, true)
                .Build();
            var monitor = new CpuUsageMonitor(config["processName"]);
            monitor.UsageUpdate += Monitor_UsageUpdate;
            monitor.Start();

            await Task.Delay(10 * 1000);
        }

        private static void Monitor_UsageUpdate(System.Collections.Generic.List<(System.Diagnostics.Process Process, double Usage)> Usage)
        {
            foreach (var u in Usage)
            {
                Console.WriteLine($"{u.Process.ProcessName}[{u.Process.Id}]:{u.Usage:P2}");
            }
        }
    }
}
