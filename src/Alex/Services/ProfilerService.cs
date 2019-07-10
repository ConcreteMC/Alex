using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using StackExchange.Profiling;
using StackExchange.Profiling.Helpers;
using StackExchange.Profiling.Internal;

namespace Alex.Services
{
    public class ProfilerService : DefaultProfilerProvider
    {
        private ConcurrentDictionary<Guid, MiniProfiler> _profilers = new ConcurrentDictionary<Guid, MiniProfiler>();
        public ProfilerService()
        {
            MiniProfiler.DefaultOptions.StopwatchProvider = GetStopwatch;
            MiniProfiler.DefaultOptions.ProfilerProvider = this;
        }

        public override MiniProfiler Start(string profilerName, MiniProfilerBaseOptions options)
        {
            var profiler = base.Start(profilerName, options);
            _profilers.TryAdd(profiler.Id, profiler);

            return profiler;
        }

        public override void Stopped(MiniProfiler profiler, bool discardResults)
        {
            base.Stopped(profiler, discardResults);
            ProfilerStopped(profiler, discardResults);
        }

        public override async Task StoppedAsync(MiniProfiler profiler, bool discardResults)
        {
            await base.StoppedAsync(profiler, discardResults);
            ProfilerStopped(profiler, discardResults);
        }

        private void ProfilerStopped(MiniProfiler profiler, bool discard)
        {
            if (_profilers.TryRemove(profiler.Id, out _))
            {

            }
        }

        public IStopwatch GetStopwatch()
        {
            return new Stopwatch(this);
        }

        private class Stopwatch : IStopwatch
        {
            public long ElapsedTicks => Watch.ElapsedTicks;
            public long Frequency => System.Diagnostics.Stopwatch.Frequency;
            public bool IsRunning => Watch.IsRunning;

            private ProfilerService Profiler { get; }
            private System.Diagnostics.Stopwatch Watch { get; }
            public Stopwatch(ProfilerService service)
            {
                Profiler = service;
                Watch = System.Diagnostics.Stopwatch.StartNew();
            }

            public void Stop()
            {
                Watch.Stop();
            }
        }
    }
}
