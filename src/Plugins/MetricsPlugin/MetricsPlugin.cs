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
using MiNET.Plugins.Attributes;
using NLog;

namespace MetricsPlugin
{
    [PluginInfo(Name = "Alex - InfluxDB Metrics", Author = "Kenny van Vulpen",
        Description = "Reports game metrics to an influxdb instance", Version = "1.0")]
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

        private ProfilerService Profiler { get; }
        private Alex.Alex GameInstance { get; }

        public MetricsPlugin(Alex.Alex alex, ProfilerService profilerService)
        {
            GameInstance = alex;
            Profiler = profilerService;
            Metrics = new MetricsBuilder().Report.ToInfluxDb(options =>
            {
                options.InfluxDb.BaseUri = new Uri("http://localhost:8086");
                options.InfluxDb.Database = "alex";
            }).Build();

            Health = AppMetricsHealth.CreateDefaultBuilder().Configuration.Configure(c => { }).Build();

            MetricTasks = new List<IMetricTask>();
        }

        public override void Enabled()
        {
            if (_isRunning) return;
            _isRunning = true;

            ConfigureTasks();

            foreach (var task in MetricTasks.ToArray())
            {
                task.Configure(Metrics);
            }

            _threadCancellationTokenSource = new CancellationTokenSource();

            _metricTimer = new Timer(async state => await Run(), null, 0, 1000);
        }


        private void ConfigureTasks()
        {
            MetricTasks.Add(new SystemPerformanceCountersTask());
            MetricTasks.Add(new ProfilerMetrics(GameInstance, Profiler));
            MetricTasks.Add(new GPUMetricsTask());
            MetricTasks.Add(new WorldMetrics(GameInstance));
        }

        public override void Disabled()
        {
            if (!_isRunning) return;
            _isRunning = !_isRunning;

            _metricTimer.Change(Timeout.Infinite, Timeout.Infinite);

            _threadCancellationTokenSource.Cancel();

            //_thread.Abort();
            // _thread.Join();
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
