using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using App.Metrics;
using App.Metrics.Gauge;

namespace MetricsPlugin.Tasks
{
    internal class SystemPerformanceCountersTask : IMetricTask
    {
        private static readonly string ContextName = "Process";
        private static int _processorTotal = Environment.ProcessorCount;

        protected IGauge ProcessPhysicalMemoryGauge { get; private set; }
        protected IGauge ProcessTotalProcessorTime { get; private set; }
        protected IGauge ProcessUserProcessorTime { get; private set; }
        protected IGauge CpuUsage { get; private set; }
        protected IGauge Threads { get; private set; }
        protected IGauge CompletionPorts { get; private set; }
        private Process _process;

        private int _maxThreads, _maxCompletionPorts;
        internal SystemPerformanceCountersTask()
        {
            _process = Process.GetCurrentProcess();

            ThreadPool.GetMaxThreads(out int threads, out int complPorts);
            _maxThreads = threads;
            _maxCompletionPorts = complPorts;
        }

        public void Configure(IMetricsRoot metrics)
        {
            ProcessPhysicalMemoryGauge = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = ContextName,
                Name = "Environment Working Set",
                MeasurementUnit = Unit.Bytes
            });

            ProcessTotalProcessorTime = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = ContextName,
                Name = "CPU Time (total)",
                MeasurementUnit = Unit.None
            });

            ProcessUserProcessorTime = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = ContextName,
                Name = "CPU Time (user)",
                MeasurementUnit = Unit.None
            });

            CpuUsage = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = ContextName,
                Name = "CPU Usage",
                MeasurementUnit = Unit.Percent
            });

            Threads = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = ContextName,
                Name = "Threads",
                MeasurementUnit = Unit.Threads
            });

            CompletionPorts = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = ContextName,
                Name = "Completion Ports",
                MeasurementUnit = Unit.None
            });
        }

        private static double _oldTime = 0;
        private static DateTime _oldStamp = DateTime.Now;
        private static double _newTime = 0;
        private static DateTime _newStamp = DateTime.Now;
        private static double _change = 0;
        private static double _period = 0;

        public void Run()
        {
            _process.Refresh();

            ProcessPhysicalMemoryGauge.SetValue(Environment.WorkingSet);
            ProcessTotalProcessorTime.SetValue(_process.TotalProcessorTime.Ticks);
            ProcessTotalProcessorTime.SetValue(_process.UserProcessorTime.Ticks);

            _newTime = _process.TotalProcessorTime.TotalMilliseconds;
            _newStamp = DateTime.Now;
            // calculates CPU usage since last measurement
            _change = _newTime - _oldTime;
            // calculates time between CPU measurements
            _period = _newStamp.Subtract(_oldStamp).TotalMilliseconds;
            _oldTime = _newTime;
            _oldStamp = _newStamp;

            double use = (_change / (_period * _processorTotal) * 100.0);
            // if sampling error causes CPU to read over 100, set to 100
            if (use > 100.0)
            {
                use = 100.0;
            }

            CpuUsage.SetValue(use);

            ThreadPool.GetAvailableThreads(out int threads, out int complPorts);
            Threads.SetValue(_maxThreads - threads);
            CompletionPorts.SetValue(_maxCompletionPorts - complPorts);
        }

    }
}
