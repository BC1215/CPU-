using System;

namespace CPU使用率监控
{
    class Program
    {
        static void Main(string[] args)
        {
            var monitor = new CpuUsageMonitor("Tobii.Eyex.Engine");
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
