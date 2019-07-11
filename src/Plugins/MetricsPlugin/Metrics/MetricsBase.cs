using System;
using System.Collections.Generic;
using System.Text;
using App.Metrics;

namespace MetricsPlugin.Metrics
{
    public class MetricsBase
    {
        protected IMetricsRoot Metrics { get; }
        public MetricsBase(IMetricsRoot metrics)
        {
            Metrics = metrics;
        }
    }
}
