using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alex.API.Graphics;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Gauge;

namespace MetricsPlugin.Tasks
{
    public class GPUMetricsTask : IMetricTask
    {
        private const string Context = "GPU";

        private IGauge ResourceCounter;
        private IGauge Memory;
        public void Configure(IMetricsRoot metrics)
        {
            ResourceCounter = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = Context,
                Name = "Resources",
                MeasurementUnit = Unit.Items,
               // Tags = new MetricTags()
            });

            Memory = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = Context,
                Name = "Memory Usage",
                MeasurementUnit = Unit.Bytes,
                // Tags = new MetricTags()
            });
        }

        public void Run()
        {
            var resources = GpuResourceManager.Instance.GetResources().ToArray();
            ResourceCounter.SetValue(resources.Length);

            Memory.SetValue(GpuResourceManager.GetMemoryUsage);
        }
    }
}
