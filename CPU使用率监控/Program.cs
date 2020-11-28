using System;
using Microsoft.Extensions.Configuration;

namespace CPU使用率监控
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("config.json", true, true)
                .Build();
            var monitor = new CpuUsageMonitor(config["processName"]);
            monitor.UsageUpdate += Monitor_UsageUpdate;
            monitor.Start();

            Console.ReadLine();
        }

        private static void Monitor_UsageUpdate(System.Collections.Generic.List<(System.Diagnostics.Process Process, double Usage)> Usage)
        {
            Console.Clear();
            foreach (var u in Usage)
            {
                Console.WriteLine($"{u.Process.ProcessName}[{u.Process.Id}]:{u.Usage:P2}");
            }
        }
    }
}
