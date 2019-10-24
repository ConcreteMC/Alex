using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Alex.API.Graphics;
using Alex.Services;
using App.Metrics;
using App.Metrics.Counter;
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
        private ITimer ChunkSectionUpdatesTimer { get; set; }
        private ITimer ChunkSectionNeighborTimer { get; set; }
        private ITimer ChunkBufferUpdateTimer { get; set; }
        private ITimer ChunkBufferCreationTimer { get; set; }
        private ITimer NetworkChunkProcessing { get; set; }
        private IMeter ChunkBufferResizing { get; set; }
        private IGauge ChunkBufferSize { get; set; }
        private IGauge ChunkVertexCount { get; set; }
        private IGauge ChunkBlockCount { get; set; }
        
        private Alex.Alex Alex { get; }
        public ProfilerMetrics(Alex.Alex alex)
        {
            Alex = alex;

            ProfilerService = alex.Services.GetService<ProfilerService>();
            ProfilerService.OnProfilerStarted += ProfilerServiceOnOnProfilerStarted;
            ProfilerService.OnProfilerStopped += ProfilerServiceOnOnProfilerStopped;
            ProfilerService.OnCounter += ProfilerServiceOnOnCounter;
            ProfilerService.OnGenericProfilingEvent += ProfilerServiceOnOnGenericProfilingEvent;
        }

        private void ProfilerServiceOnOnGenericProfilingEvent(object sender, GenericProfilingEvent e)
        {
            switch (e.Id)
            {
                case "chunk.bufferSize":
                    ChunkBufferSize.SetValue(e.Value);
                    break;
                case "chunk.vertexCount":
                    ChunkVertexCount.SetValue(e.Value);
                    break;
                case "chunk.blockCount":
                    ChunkVertexCount.SetValue(e.Value);
                    break;
            }
        }

        private void ProfilerServiceOnOnCounter(object sender, CounterProfilingEvent e)
        {
            switch (e.Id)
            {
                case "chunk.bufferResize":
                    ChunkBufferResizing.Mark();
                    break;
            }
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
                ChunkUpdateTime.Record((long) e.Profiler.DurationMilliseconds, TimeUnit.Milliseconds);
                
                var timings = new Stack<Timing>();
                timings.Push(e.Profiler.Root);
                
                while (timings.Count > 0)
                {
                    var timing = timings.Pop();
                    switch (timing.Name)
                    {
                        case "chunk.sections":
                            ChunkSectionUpdatesTimer.Record((long) timing.DurationMilliseconds.Value, TimeUnit.Milliseconds);
                            break;
                        case "chunk.buffer":
                            ChunkBufferUpdateTimer.Record((long) timing.DurationMilliseconds.Value,
                                TimeUnit.Milliseconds);
                            break;
                        case "chunk.meshing":
                            ChunkMeshTimer.Record((long) timing.DurationMilliseconds.Value, TimeUnit.Milliseconds);
                            break;
                        case "chunk.buffer.check":
                            ChunkBufferCreationTimer.Record((long) timing.DurationMilliseconds.Value, TimeUnit.Milliseconds);
                            break;
                        case "chunk.neighboring":
                            ChunkSectionNeighborTimer.Record((long) timing.DurationMilliseconds.Value, TimeUnit.Milliseconds);
                            break;
                    }
                    
                    if (timing.HasChildren)
                    {
                        var children = timing.Children;
                        for (var i = children.Count - 1; i >= 0; i--) timings.Push(children[i]);
                    }
                }
                
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
           
           ChunkSectionUpdatesTimer = metrics.Provider.Timer.Instance(new TimerOptions()
           {
               Context = Context,
               Name = "Chunk Section Updates",
               MeasurementUnit = Unit.Custom("MS"),
               DurationUnit = TimeUnit.Milliseconds,
               RateUnit = TimeUnit.Milliseconds
           });
           
           ChunkSectionNeighborTimer = metrics.Provider.Timer.Instance(new TimerOptions()
           {
               Context = Context,
               Name = "Chunk Section Neighbor Checks",
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
           
           ChunkBufferUpdateTimer = metrics.Provider.Timer.Instance(new TimerOptions()
           {
               Context = Context,
               Name = "Chunk Buffer Updates",
               MeasurementUnit = Unit.Custom("MS"),
               DurationUnit = TimeUnit.Milliseconds,
               RateUnit = TimeUnit.Milliseconds
           });
           
           ChunkBufferCreationTimer = metrics.Provider.Timer.Instance(new TimerOptions()
           {
               Context = Context,
               Name = "Chunk Buffer Creation",
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

           ChunkBufferResizing = metrics.Provider.Meter.Instance(new MeterOptions()
           {
                Context = Context,
                Name = "Chunk Buffer Resize",
                MeasurementUnit = Unit.Calls
           });
           
           ChunkBufferSize = metrics.Provider.Gauge.Instance(new GaugeOptions()
           {
               Context = Context,
               Name = "Chunk Buffer Size",
               MeasurementUnit = Unit.Bytes
           });
           
           ChunkVertexCount = metrics.Provider.Gauge.Instance(new GaugeOptions()
           {
               Context = Context,
               Name = "Chunk Vertex Count",
               MeasurementUnit = Unit.Items
           });
           
           ChunkBlockCount = metrics.Provider.Gauge.Instance(new GaugeOptions()
           {
               Context = Context,
               Name = "Chunk Block Count",
               MeasurementUnit = Unit.Items
           });
        }

        public void Run()
        {
            FpsMeter.SetValue(Alex.FpsMonitor.Value);
        }
    }
}
