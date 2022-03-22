using System;
using System.Threading;
using Alex.Entities.Events;
using Alex.Gamestates.InGame;
using Alex.Utils.Collections;
using Alex.Utils.Threading;
using Alex.Worlds;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Xna.Framework;
using RocketUI;

namespace Alex.DebugOverlay
{
    public class OverlayScreen : Screen
    {
        private Alex           _alex;
        private TextElement    _fpsElement;
        private InfluxDBClient _infucksClient;
        private WriteApiAsync  _writeApiAsync;
        private WriteApi  _writeApi;
        
        private Func<PointData> _frameTimeBuilder;
        private Func<PointData> _taskExecTimeBuilder;
        private Func<PointData> _chunkUpdatedBuilder;
        private Func<PointData> _entityUpdatedBuilder;

        private BackgroundWorker _backgroundWorker = new BackgroundWorker(CancellationToken.None);
        public OverlayScreen(Alex alex)
        {
            _infucksClient = InfluxDBClientFactory.Create(
                InfluxDBClientOptions.Builder.CreateNew().Url("http://localhost:8086")
                   .AuthenticateToken(
                        "5up3r-S3cr3t-auth-t0k3n")
                   .Org("influxdata-org").Bucket("alex").AddDefaultTag("host", Environment.MachineName).Build());

            _writeApiAsync = _infucksClient.GetWriteApiAsync();
            _writeApi = _infucksClient.GetWriteApi();
            
            _frameTimeBuilder = () => PointData.Measurement("rendering.frame_time")
               .Tag("host", Environment.MachineName).Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            _taskExecTimeBuilder = () => PointData.Measurement("tasks.completed")
               .Tag("host", Environment.MachineName).Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            _chunkUpdatedBuilder = () => PointData.Measurement("chunks.updated")
               .Tag("host", Environment.MachineName).Timestamp(DateTime.UtcNow, WritePrecision.Ns);
            
            _entityUpdatedBuilder = () => PointData.Measurement("entity.update")
               .Tag("host", Environment.MachineName).Timestamp(DateTime.UtcNow, WritePrecision.Ns);
            
            _alex = alex;
            _alex.OnEndDraw += OnEndDraw;

            SizeToWindow = true;

            AddChild(
                _fpsElement =
                    new TextElement("00 FPS", true) { Anchor = Alignment.TopLeft, TextAlignment = TextAlignment.Left });

            /*AddChild(_graph = new GuiGraph() { Anchor = Alignment.TopRight });
            _graph.AutoSizeMode = AutoSizeMode.None;
            _graph.Width = 210;
            _graph.Height = 180;*/

            _alex.UiTaskManager.TaskFinished += TaskFinished;
            EntityManager.EntityTicked += OnEntityTicked;
            EntityManager.EntityUpdated += OnEntityUpdated;
            _backgroundWorker.Start();
            //_alex.UiTaskManager.TaskCreated += TaskCreated;
        }

        private void OnEntityUpdated(object sender, EntityUpdateEventArgs e)
        {
            var entityType = (e.Entity?.Type);

            if (entityType == null)
                return;

            _backgroundWorker.Enqueue(
                () =>
                {
                    foreach (var item in e.TimingsReport)
                    {
                        _writeApi.WritePoint(
                            PointData.Measurement(Sanitize(item.Name)).Tag("host", Environment.MachineName)
                               .Timestamp(DateTime.UtcNow, WritePrecision.Ns).Tag("entityType", entityType.ToString())
                               .Field("duration", item.ElapsedTime.TotalMilliseconds));
                    }
                });
        }

        private string Sanitize(string input)
        {
            return input.Replace(" ", "_").ToLower();
        }

        private void OnEntityTicked(object sender, EntityUpdateEventArgs e)
        {
            var entityType = e.Entity?.Type;

            if (entityType == null)
                return;

            _backgroundWorker.Enqueue(
                () =>
                {
                    foreach (var item in e.TimingsReport)
                    {
                        _writeApi.WritePoint(
                            PointData.Measurement(Sanitize(item.Name)).Tag("host", Environment.MachineName)
                               .Timestamp(DateTime.UtcNow, WritePrecision.Ns).Tag("entityType", entityType.ToString())
                               .Field("duration", item.ElapsedTime.TotalMilliseconds));
                    }
                });
        }

        // private void TaskCreated(object? sender, TaskCreatedEventArgs e)
        // {
        //     e.Task.StateChanged += TaskStateChanged;
        // }
        //
        // private void TaskStateChanged(object? sender, TaskStateUpdatedEventArgs e)
        // {
        //     if (sender is ManagedTask manTask)
        //     {
        //         _writeApiAsync.WritePointAsync(_taskExecTimeBuilder()
        //             .Tag("state", e.NewState.ToString())
        //             .Field("queued", e.TimeTillExecution.TotalMilliseconds)
        //             .Field("duration", e.ExecutionTime.TotalMilliseconds)
        //         );
        //     }
        // }

        private void TaskFinished(object sender, TaskFinishedEventArgs e)
        {
            _writeApiAsync.WritePointAsync(_taskExecTimeBuilder()
                .Field("queued", e.TimeTillExecution.TotalMilliseconds)
                .Field("duration", e.ExecutionTime.TotalMilliseconds)
            );
        }

        private ulong        _frameCount   = 0;
        private bool         _setup        = false;
        private PlayingState _playingState = null;

        private void OnEndDraw(object sender, EventArgs e)
        {
            var frameTime = _alex.FpsMonitor.LastFrameTime;
            _writeApiAsync.WritePointAsync(_frameTimeBuilder().Field("value", frameTime));

         //   _graph?.Add(_frameCount++, frameTime, Color.Green);

            //_history.Push(new Record(_frameCount++, frameTime));
            _fpsElement.Text =
                $"{_alex.FpsMonitor.Value:00} FPS\nFrametime: {_alex.FpsMonitor.AverageFrameTime:F2}ms avg";
            //_streamWriter.WriteLine($"{_frameCounter++},{frameTime}");

            if (Alex.InGame && !_setup)
            {
                if (_alex.GameStateManager.GetActiveState() is PlayingState playingState)
                {
                    playingState.World.ChunkManager.OnChunkUpdate += OnChunkUpdate;
                    _playingState = playingState;
                    _setup = true;
                }
            }
            else if (!Alex.InGame && _setup)
            {
                _setup = false;
                _playingState.World.ChunkManager.OnChunkUpdate -= OnChunkUpdate;
                _playingState = null;
            }
        }

        private void OnChunkUpdate(object sender, ChunkUpdatedEventArgs e)
        {
            _writeApiAsync.WritePointAsync(_chunkUpdatedBuilder()
                .Tag("x", e.Position.X.ToString())
                .Tag("z", e.Position.Z.ToString())
                .Tag("is_new", e.Chunk.IsNew.ToString())
                .Field("duration", e.ExecutionTime.TotalMilliseconds)
            );
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _alex.OnEndDraw -= OnEndDraw;
            _infucksClient.Dispose();
            base.Dispose(disposing);
        }
    }
}