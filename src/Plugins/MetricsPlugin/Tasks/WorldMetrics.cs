using System;
using System.Collections.Generic;
using System.Text;
using Alex.GameStates.Playing;
using App.Metrics;
using App.Metrics.Gauge;

namespace MetricsPlugin.Tasks
{
    public class WorldMetrics : IMetricTask
    {
        private const string Context = "World";

        private Alex.Alex Alex { get; }
        protected IGauge Chunks { get; private set; }
        protected IGauge RenderedChunks { get; private set; }
        protected IGauge ChunkUpdatesQueued { get; private set; }
        protected IGauge ChunkUpdateActive { get; private set; }
        protected IGauge Entities { get; private set; }
        protected IGauge RenderedEntities { get; private set; }
        protected IGauge RenderedVertices { get; private set; }

        public WorldMetrics(Alex.Alex alex)
        {
            Alex = alex;
        }

        public void Configure(IMetricsRoot metrics)
        {
            Chunks = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = Context,
                Name = "Chunks",
                MeasurementUnit = Unit.None
            });

            RenderedChunks = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = Context,
                Name = "Rendered Chunks",
                MeasurementUnit = Unit.None
            });

            ChunkUpdatesQueued = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = Context,
                Name = "Chunk Updates (Queued)",
                MeasurementUnit = Unit.None
            });

            ChunkUpdateActive = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = Context,
                Name = "Chunk Updates (Active)",
                MeasurementUnit = Unit.None
            });

            Entities = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = Context,
                Name = "Entities",
                MeasurementUnit = Unit.None
            });

            RenderedEntities = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = Context,
                Name = "Rendered Entities",
                MeasurementUnit = Unit.None
            });

            RenderedVertices = metrics.Provider.Gauge.Instance(new GaugeOptions()
            {
                Context = Context,
                Name = "Rendered Vertices",
                MeasurementUnit = Unit.None
            });
        }

        public void Run()
        {
            if (Alex.GameStateManager.GetActiveState() is PlayingState state)
            {
                var world = state.World;
                if (world == null)
                    return;

                Chunks.SetValue(world.ChunkManager.ChunkCount);
                RenderedChunks.SetValue(world.ChunkManager.RenderedChunks);
                ChunkUpdatesQueued.SetValue(world.ChunkManager.EnqueuedChunkUpdates);
                ChunkUpdateActive.SetValue(world.ChunkManager.ConcurrentChunkUpdates);

                Entities.SetValue(world.EntityManager.EntityCount);
                RenderedEntities.SetValue(world.EntityManager.EntitiesRendered);

                RenderedVertices.SetValue(world.ChunkManager.Vertices);
            }
        }
    }
}
