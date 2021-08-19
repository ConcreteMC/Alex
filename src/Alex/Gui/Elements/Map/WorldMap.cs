using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Alex.Common.Data.Options;
using Alex.Common.Utils.Vectors;
using Alex.Common.World;
using Alex.Entities;
using Alex.Utils;
using Alex.Worlds;
using ConcurrentCollections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MiNET.Worlds;
using NLog;
using NLog.Fluent;
using RocketUI;

namespace Alex.Gui.Elements.Map
{
    public class WorldMap : IMap, ITicked, IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(WorldMap));
        
        private World _world;
        private readonly ConcurrentDictionary<ChunkCoordinates, RenderedMap> _textureContainers = new();

        private readonly ConcurrentHashSet<MapIcon> _markers;

        public int ChunkSize { get; set; } = 16;
        private int RenderDistance => _world?.ChunkManager?.RenderDistance ?? 1;
        /// <inheritdoc />
        public int Width => RenderDistance * ChunkSize * 3;

        /// <inheritdoc />
        public int Height => RenderDistance * ChunkSize * 3;

        /// <inheritdoc />
        public float Scale { get; } = 1f;

        /// <inheritdoc />
        public Vector3 Center => _world?.Camera?.Position ?? Vector3.Zero;

        /// <inheritdoc />
        public float Rotation => 180f - (_world?.Player?.KnownPosition?.HeadYaw ?? 0);

        public bool AlphaBlending { get; set; } = true;
        private OptionsPropertyAccessor<bool> _alphaBlendingAccessor;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        public WorldMap(World world, AlexOptions options)
        {
            _world = world;
            _markers = new ConcurrentHashSet<MapIcon>();
            
            world.ChunkManager.OnChunkAdded += OnChunkAdded;
            world.ChunkManager.OnChunkRemoved += OnChunkRemoved;
            world.ChunkManager.OnChunkUpdate += OnChunkUpdate;

            world.EntityManager.EntityAdded += EntityAdded;
            world.EntityManager.EntityRemoved += EntityRemoved;

            ChunkSize = 16;// * Math.Max(1, lod);

            _alphaBlendingAccessor = options.UserInterfaceOptions.Minimap.AlphaBlending.Bind(AlphaBindSettingChanged);
            AlphaBlending = _alphaBlendingAccessor.Value;

            _spriteBatch = new SpriteBatch(Alex.Instance.GraphicsDevice);
        }

        private void AlphaBindSettingChanged(bool oldvalue, bool newvalue)
        {
            AlphaBlending = newvalue;
        }

        public MapIcon AddMarker(Vector3 position, MapMarker icon)
        {
            var res = new MapIcon(icon) {Position = position};
           // AddChild(res);
           Add(res);
           
            return res;
        }

        public void Add(MapIcon icon)
        {
            if (icon == null)
                return;
            
            _markers.Add(icon);
        }

        public void Remove(MapIcon icon)
        {
            if (icon == null)
                return;

            _markers.TryRemove(icon);
        }

        private void EntityRemoved(object sender, Entity e)
        {
            Remove(e.MapIcon);
        }

        private void EntityAdded(object sender, Entity e)
        {
            Add(e.MapIcon);
        }
        
        private void OnChunkUpdate(object sender, ChunkUpdatedEventArgs e)
        {
            if (TryGetContainer(e.Position, out var container))
            {
                container.MarkDirty();
            }
        }

        private void OnChunkRemoved(object sender, ChunkRemovedEventArgs e)
        {
            if (TryGetContainer(e.Position, out var container))
                container.Invalidate();
        }

        private void OnChunkAdded(object sender, ChunkAddedEventArgs e)
        {
            var container = _textureContainers.GetOrAdd(
                e.Position, coordinates => new RenderedMap(coordinates, ChunkSize));
            container.MarkDirty();
        }

        private bool TryAdd(ChunkCoordinates coordinates, RenderedMap container)
        {
            return _textureContainers.TryAdd(coordinates, container);
        }

        private bool TryGetContainer(ChunkCoordinates coordinates, out RenderedMap container)
        {
            if (!_textureContainers.TryGetValue(coordinates, out container)) return false;

            if (container.Invalidated)
            {
                RemoveContainer(coordinates);
                return false;
            }

            return true;
        }
        

        private void RemoveContainer(ChunkCoordinates coordinates)
        {
            if (_textureContainers.TryRemove(coordinates, out var container))
            {
                container.Dispose();
            }
        }

        public IEnumerable<IMapElement> GetSections(ChunkCoordinates center, int radius)
        {
            var containers = _textureContainers;
            if (containers == null || containers.IsEmpty)
                yield break;
            
            for (int x = center.X - radius; x < center.X + radius; x++)
            {
                for (int y = center.Z - radius; y < center.Z + radius; y++)
                {
                    if (TryGetContainer(new ChunkCoordinates(x, y), out var container))
                    {
                        yield return container;
                    }
                }
            }
        }

        /// <inheritdoc />
        public uint[] GetData()
        {
            uint[] data = new uint[Width * Height];
            
            _renderTarget.GetData(data);
            
            return data;
        }

        /// <inheritdoc />
        public Texture2D GetTexture(GraphicsDevice device)
        {
            if (Disposed) return null;
            
            Texture2D oldTexture = null;
            var texture = _renderTarget;

            try
            {
                if (texture == null || texture.IsDisposed || texture.Width != Width || texture.Height != Height)
                {
                    oldTexture = texture;

                    texture = new RenderTarget2D(device, Width, Height);
                }
                
                using (device.PushRenderTarget(_renderTarget))
                {
                    _spriteBatch.Begin();

                    foreach (var element in GetSections(new ChunkCoordinates(Center), RenderDistance))
                    {
                        var t = element.GetTexture(device);
                        if (t == null)
                            continue;

                        var distance = element.Position - Center;
                        
                        _spriteBatch.Draw(t, new Vector2(Width / 2f, Height / 2f) + new Vector2(distance.X, distance.Z), Color.White);
                    }
                    
                    _spriteBatch.End();
                }
            }
            finally
            {
                _renderTarget = texture;
                
                if (oldTexture != null && oldTexture != _renderTarget)
                    oldTexture.Dispose();
            }

            return texture;
        }

        public IEnumerable<MapIcon> GetMarkers(ChunkCoordinates center, int radius)
        {
            var markers = _markers;
            if (markers == null || markers.IsEmpty)
                yield break;

            foreach (var icon in markers
               .Where(
                    x => x.AlwaysShown || (Math.Abs(new ChunkCoordinates(x.Position).DistanceTo(center)) <= radius
                                           )).OrderBy(x => x.DrawOrder))
            {
                yield return icon;
            }
        }

        /// <inheritdoc />
        public void OnTick()
        {
            if (Disposed) return;

            foreach (var container in GetSections(new ChunkCoordinates(Center), RenderDistance))
            {
                if (container is RenderedMap rm)
                {
                    if (rm.IsDirty)
                    {
                        rm.Tick(_world, AlphaBlending);
                    }
                }
            }
        }

        public bool Disposed { get; private set; } = false;
        /// <inheritdoc />
        public void Dispose()
        {
            if (Disposed)
                return;
            
            Disposed = true;
          //  _trackedEntities.CollectionChanged -= TrackedEntitiesOnCollectionChanged;
         //   _trackedEntities.Clear();
            
         //  _trackedEntities = null;
            
            _world.EntityManager.EntityAdded -= EntityAdded;
            _world.EntityManager.EntityRemoved -= EntityRemoved;
            
            _world.ChunkManager.OnChunkAdded -= OnChunkAdded;
            _world.ChunkManager.OnChunkRemoved -= OnChunkRemoved;
            _world.ChunkManager.OnChunkUpdate -= OnChunkUpdate;

            _markers.Clear();

            var elements = _textureContainers.ToArray();
            _textureContainers.Clear();

            foreach (var element in elements)
            {
                element.Value?.Dispose();
            }

            _world = null;
            
            _alphaBlendingAccessor?.Dispose();
            _alphaBlendingAccessor = null;
        }
    }
}