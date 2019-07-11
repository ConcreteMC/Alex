using System;
using System.Collections.Generic;
using System.Text;
using App.Metrics;

namespace MetricsPlugin.Tasks
{
    public interface IMetricTask
    {
        void Configure(IMetricsRoot metrics);
        void Run();
    }
}
