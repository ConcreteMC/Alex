using System;
using System.Collections.Concurrent;
using System.Linq;
using Alex.API.Graphics;
using Alex.Services;
using App.Metrics;
using App.Metrics.Gauge;
using App.Metrics.Meter;
using App.Metrics.Timer;
using MetricsPlugin.Tasks;
using NLog;
using StackExchange.Profiling;

namespace MetricsPlugin.Metrics
{
    public class ProfilerMetrics : IMetricTask
    {
        private const string Context = "Profiler";

        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        private ConcurrentDictionary<Guid, MiniProfiler> _chunkProfilers = new ConcurrentDictionary<Guid, MiniProfiler>();
        private ConcurrentDictionary<Guid, MiniProfiler> _networkChunkProfilers = new ConcurrentDictionary<Guid, MiniProfiler>();
        private ProfilerService ProfilerService { get; }
        private IMetricsRoot Metrics { get; set; }
        private IGauge FpsMeter { get; set; }
        private ITimer ChunkUpdateTime { get; set; }
        private ITimer ChunkMeshTimer { get; set; }
        private ITimer NetworkChunkProcessing { get; set; }
        private Alex.Alex Alex { get; }
        public ProfilerMetrics(Alex.Alex alex)
        {
            Alex = alex;

            ProfilerService = alex.Services.GetService<ProfilerService>();
            ProfilerService.OnProfilerStarted += ProfilerServiceOnOnProfilerStarted;
            ProfilerService.OnProfilerStopped += ProfilerServiceOnOnProfilerStopped;
        }


        private void ProfilerServiceOnOnProfilerStarted(object sender, ProfilerStartedEvent e)
        {
            if (e.Profiler.Name.Equals("BEToJavaColumn", StringComparison.InvariantCultureIgnoreCase))
            {
                _networkChunkProfilers.TryAdd(e.Id, e.Profiler);
            }
            else if (e.Profiler.Name.Contains("chunk", StringComparison.InvariantCultureIgnoreCase))
            {
                _chunkProfilers.TryAdd(e.Id, e.Profiler);
            }
        }

        private void ProfilerServiceOnOnProfilerStopped(object sender, ProfilerStoppedEvent e)
        {
            if (_networkChunkProfilers.TryRemove(e.Id, out _))
            {
                NetworkChunkProcessing.Record((long) e.ElapsedTime.TotalMilliseconds, TimeUnit.Milliseconds);
            }
            else if (_chunkProfilers.TryRemove(e.Id, out _))
            {
                ChunkUpdateTime.Record((long) e.ElapsedTime.TotalMilliseconds, TimeUnit.Milliseconds);
            }
            else
            {
                Log.Warn($"Could not remove timer!");
            }
        }

        public void Configure(IMetricsRoot metrics)
        {
            Metrics = metrics;
            FpsMeter = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = Context,
                Name = "FPS",
                MeasurementUnit = Unit.None
            });

           ChunkUpdateTime = metrics.Provider.Timer.Instance(new TimerOptions()
           {
               Context = Context,
               Name = "Chunk Update Time",
               MeasurementUnit = Unit.Custom("MS"),
               DurationUnit = TimeUnit.Milliseconds,
               RateUnit = TimeUnit.Milliseconds
           });
           
           ChunkMeshTimer = metrics.Provider.Timer.Instance(new TimerOptions()
           {
               Context = Context,
               Name = "Chunk Meshing",
               MeasurementUnit = Unit.Custom("MS"),
               DurationUnit = TimeUnit.Milliseconds,
               RateUnit = TimeUnit.Milliseconds
           });
           
           NetworkChunkProcessing = metrics.Provider.Timer.Instance(new TimerOptions()
           {
               Context = Context,
               Name = "Chunk Processing",
               MeasurementUnit = Unit.Custom("MS"),
               DurationUnit = TimeUnit.Milliseconds,
               RateUnit = TimeUnit.Milliseconds
           });
        }

        public void Run()
        {
            FpsMeter.SetValue(Alex.FpsMonitor.Value);
        }
    }
}
