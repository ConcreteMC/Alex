using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using Alex.Common.Utils.Collections;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.Worlds;
using ConcurrentCollections;
using Microsoft.Xna.Framework;
using RocketUI;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.Gui.Elements.Map
{
    public enum ZoomLevel : byte
    {
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4,
        Level5 = 5,
        Level6 = 6,
        Level7 = 7,
        Level8 = 8,
        Level9 = 9,
        Level10 = 10,
        
        Minimum = Level1,
        Maximum = Level10,
        Default = Level6,
    }
    
	public class WorldMap : IUpdateable, IDisposable
    {
        private readonly World _world;
        private readonly ConcurrentDictionary<ChunkCoordinates, RenderedMap> _textureContainers = new();

        private readonly ConcurrentHashSet<MapIcon> _markers;
        public Vector3 CenterPosition => _world.Camera.Position;
        
        public WorldMap(World world)
        {
            _world = world;
            _markers = new ConcurrentHashSet<MapIcon>();
            
            world.ChunkManager.OnChunkAdded += OnChunkAdded;
            world.ChunkManager.OnChunkRemoved += OnChunkRemoved;
            world.ChunkManager.OnChunkUpdate += OnChunkUpdate;

            world.EntityManager.EntityAdded += EntityAdded;
            world.EntityManager.EntityRemoved += EntityRemoved;
        }
        
        public MapIcon AddMarker(Vector3 position, MapMarker icon)
        {
            var res = new MapIcon(icon) {Position = position};
           // AddChild(res);

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
            RemoveContainer(e.Position);
        }

        private void OnChunkAdded(object sender, ChunkAddedEventArgs e)
        {
            var container = new RenderedMap(e.Position);
            if (TryAdd(e.Position, container))
            {
                container.MarkDirty();
            }
            else
            {
                if (TryGetContainer(e.Position, out var c))
                {
                    c.MarkDirty();
                }
            }
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

        public IEnumerable<RenderedMap> GetContainers(ChunkCoordinates center, int radius)
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
        
        public IEnumerable<MapIcon> GetMarkers(ChunkCoordinates center, int radius)
        {
            var markers = _markers;
            if (markers == null || markers.IsEmpty)
                yield break;
            
            foreach (var icon in markers.Where(x => new ChunkCoordinates(x.Position).DistanceTo(center) <= radius).OrderBy(x => x.DrawOrder))
            {
                yield return icon;
            }
        }

        /// <inheritdoc />
        public void Update(GameTime gameTime)
        {
            var device = Alex.Instance.GraphicsDevice;
            
            using (new GraphicsContext(device))
            {
                foreach (var container in GetContainers(new ChunkCoordinates(CenterPosition), _world.ChunkManager.RenderDistance))
                {
                    if (container.IsDirty)
                    {
                        _world.ChunkManager.TryGetChunk(container.Coordinates, out var chunk);
                        container.Update(_world, chunk, device);
                    }
                }
            }
        }

        /// <inheritdoc />
        public bool Enabled { get; } = true;

        /// <inheritdoc />
        public int UpdateOrder { get; } = 0;

        /// <inheritdoc />
        public event EventHandler<EventArgs> EnabledChanged;

        /// <inheritdoc />
        public event EventHandler<EventArgs> UpdateOrderChanged;

        /// <inheritdoc />
        public void Dispose()
        {
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
        }
    }
}