using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPU使用率监控
{

    delegate void UsageUpdateEventHandler(List<(Process Process, double Usage)> Usage);
    enum MonitorState
    {
        Stopped = 1,
        Running = 2,
        CancelPending = 3
    }

    class CpuUsageMonitor
    {
        private const int DefaultRefreshInterval = 1000;
        public string ProcessName { get; }
        private readonly Dictionary<Process, double> _dicProcess2CpuUsage = new Dictionary<Process, double>();
        private readonly Dictionary<Process, TimeSpan> _dicProcess2LastTotalCpuTime = new Dictionary<Process, TimeSpan>();
        private bool _firstLoop = true;
        private MonitorState _monitorState = MonitorState.Stopped;
        private int _refreshInterval;
        private TimeSpan _intervalCpuTime;
        private readonly Process[] _processes;

        public event UsageUpdateEventHandler UsageUpdate;

        public List<(Process Process, double Usage)> Usage
        {
            get
            {
                return _processes.Select(p => (p, _dicProcess2CpuUsage[p])).ToList();
            }
        }

        public int RefreshInterval
        {
            get => _refreshInterval;
            set
            {
                _refreshInterval = value;
                _intervalCpuTime = TimeSpan.FromMilliseconds(_refreshInterval);
            }
        }

        public MonitorState MonitorState => _monitorState;

        public CpuUsageMonitor(string processName)
        {
            RefreshInterval = DefaultRefreshInterval;

            ProcessName = processName;

            _processes = Process.GetProcessesByName(processName);
            _dicProcess2CpuUsage = _processes.ToDictionary(k => k, v => 0d);
            _dicProcess2LastTotalCpuTime = _processes.ToDictionary(k => k, v => TimeSpan.Zero);
        }

        void Loop()
        {
            Task.Factory.StartNew(() =>
            {
                _monitorState = MonitorState.Running;
                var sw = new Stopwatch();
                while (true)
                {
                    sw.Restart();
                    if (_monitorState == MonitorState.CancelPending)
                    {
                        _monitorState = MonitorState.Stopped;
                        _firstLoop = true;
                        return;
                    }

                    foreach (var process in _processes)
                    {
                        if (process.HasExited)
                        {
                            continue;
                        }
                        if (_firstLoop)
                        {
                            _dicProcess2LastTotalCpuTime[process] = process.TotalProcessorTime;
                            _firstLoop = false;
                        }
                        else
                        {
                            var cpuTime = process.TotalProcessorTime - _dicProcess2LastTotalCpuTime[process];
                            var percent = (cpuTime / _intervalCpuTime) / Environment.ProcessorCount;
                            _dicProcess2LastTotalCpuTime[process] = process.TotalProcessorTime;
                            _dicProcess2CpuUsage[process] = percent;
                        }
                    }

                    Task.Factory.StartNew(() =>
                    {
                        UsageUpdate?.Invoke(Usage);
                    });
                    sw.Stop();

                    Task.Delay(Math.Max(0, RefreshInterval - (int)sw.Elapsed.TotalMilliseconds)).Wait();
                }
            });
        }

        public void Start()
        {
            if (_monitorState == MonitorState.Stopped)
            {
                Loop();
            }
        }

        public void Stop()
        {
            if (_monitorState == MonitorState.Running)
            {
                _monitorState = MonitorState.CancelPending;
            }
        }
    }
}
