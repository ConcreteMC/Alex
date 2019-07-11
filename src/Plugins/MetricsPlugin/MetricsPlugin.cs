using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Alex.Plugins;
using Alex.Services;
using App.Metrics;
using App.Metrics.Formatters.InfluxDB;
using App.Metrics.Health;
using App.Metrics.Reporting.InfluxDB;
using MetricsPlugin.Metrics;
using MetricsPlugin.Tasks;
using NLog;

namespace MetricsPlugin
{
    public class MetricsPlugin : Plugin
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private IMetricsRoot Metrics { get; }
        private IHealthRoot Health { get; set; }
        private IList<IMetricTask> MetricTasks { get; set; }

        private bool _isRunning { get; set; }
        private Timer _metricTimer { get; set; }
        private CancellationTokenSource _threadCancellationTokenSource;
        private CancellationToken _threadCancellationToken;
        public MetricsPlugin()
        {
            Metrics = new MetricsBuilder().Report.ToInfluxDb(options =>
            {
                options.InfluxDb.BaseUri = new Uri("http://127.0.0.1:8086");
                options.InfluxDb.Database = "metrics";
            }).Build();

            Health = AppMetricsHealth.CreateDefaultBuilder().Configuration.Configure(c => { }).Build();

            MetricTasks = new List<IMetricTask>();
        }

        public override void Enabled(Alex.Alex alex)
        {
            if (_isRunning) return;
            _isRunning = true;

            ConfigureTasks(alex);

            foreach (var task in MetricTasks.ToArray())
            {
                task.Configure(Metrics);
            }

            _threadCancellationTokenSource = new CancellationTokenSource();

            _metricTimer = new Timer(async state => await Run(), null, 0, 1000);

            Log.Info($"Metrics plugin enabled!");
        }


        private void ConfigureTasks(Alex.Alex alex)
        {
            MetricTasks.Add(new SystemPerformanceCountersTask());
            MetricTasks.Add(new ProfilerMetrics(alex));
            MetricTasks.Add(new GPUMetricsTask());
            MetricTasks.Add(new WorldMetrics(alex));
        }

        public override void Disabled(Alex.Alex alex)
        {
            if (!_isRunning) return;
            _isRunning = !_isRunning;

            _metricTimer.Change(Timeout.Infinite, Timeout.Infinite);

            _threadCancellationTokenSource.Cancel();

            //_thread.Abort();
           // _thread.Join();

            Log.Info($"Metrics plugin disabled!");
        }

        private async Task Run()
        {
           // while (_isRunning)
           // {
                Parallel.ForEach(MetricTasks.ToArray(), task => { task.Run(); });

                var healthStatus = await Health.HealthCheckRunner.ReadAsync(_threadCancellationToken);

                using (var ms = new MemoryStream())
                {
                    await Health.DefaultOutputHealthFormatter.WriteAsync(ms, healthStatus);
                }

                await Task.WhenAll(Metrics.ReportRunner.RunAllAsync(_threadCancellationToken));
                // }

        }
    }
}
