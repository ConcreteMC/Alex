using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    
	public class WorldMap : IMap, ITicked, IDisposable
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(WorldMap));
        
        private readonly World _world;
        private readonly ConcurrentDictionary<ChunkCoordinates, RenderedMap> _textureContainers = new();

        private readonly ConcurrentHashSet<MapIcon> _markers;
        
        
        /// <inheritdoc />
        public int Width => (_world.ChunkManager.RenderDistance) * 16;

        /// <inheritdoc />
        public int Height => (_world.ChunkManager.RenderDistance) * 16;

        /// <inheritdoc />
        public float Scale { get; } = 1f;

        /// <inheritdoc />
        public Vector3 Center => _world?.Camera?.Position ?? Vector3.Zero;

        /// <inheritdoc />
        public float Rotation => 180f - _world.Player.KnownPosition.HeadYaw;

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
            if (TryGetContainer(e.Position, out var container))
                container.Invalidate();
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
               // RemoveContainer(coordinates);
                //return false;
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

        private IEnumerable<RenderedMap> GetContainers(ChunkCoordinates center, int radius)
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
            throw new NotImplementedException();
        }

        private Texture2D _texture = null;
        private ChunkCoordinates _previousCenter = ChunkCoordinates.Zero;
        /// <inheritdoc />
        public Texture2D GetTexture(GraphicsDevice device)
        {
            var elementSize = 16;
            var center = new ChunkCoordinates(Center);
            var forceRedraw = center != _previousCenter;
            
            if (_texture == null || _texture.IsDisposed || _texture.Width != Width || _texture.Height != Height)
            {
                var oldTexture = _texture;
                _texture = new Texture2D(device, Width, Height);
                forceRedraw = true;
                
                oldTexture?.Dispose();
            }

            if (forceRedraw)
            {
                _texture.SetData(ArrayOf<uint>.Create(Width * Height));
            }
            
            foreach (var container in GetContainers(center, _world.ChunkManager.RenderDistance))
            {
               // if (container.Invalidated)
              //      continue;
                
                if (!forceRedraw && !container.PendingChanges && !container.Invalidated) continue;

                var coordinates = container.Coordinates;
                var distance = coordinates - center;

                var renderPos = new Vector2(Width / 2f, Height / 2f);
                renderPos += new Vector2(distance.X * elementSize, distance.Z * elementSize);

                var pos = renderPos.ToPoint();
                
                var width = elementSize;
                var height = elementSize;

                var destination = new Rectangle(pos.X, pos.Y, width, height);

                if (!_texture.Bounds.Contains(destination))
                {
                    Log.Warn($"Texture position out of bounds.");
                }
                else
                {
                    var data = container.GetData();
                    _texture.SetData(0, destination, data, 0, data.Length);
                    //didChange = true;
                }
                
                if (container.Invalidated)
                    RemoveContainer(container.Coordinates);
            }

            _previousCenter = center;
            return _texture;
        }

        public IEnumerable<MapIcon> GetMarkers(ChunkCoordinates center, int radius)
        {
            var markers = _markers;
            if (markers == null || markers.IsEmpty)
                yield break;
           
            foreach (var icon in markers.Where(x => x.AlwaysShown || new ChunkCoordinates(x.Position).DistanceTo(center) <= radius).OrderBy(x => x.DrawOrder))
            {
                yield return icon;
            }
        }

        /// <inheritdoc />
        public void OnTick()
        {
            foreach (var container in GetContainers(new ChunkCoordinates(Center), _world.ChunkManager.RenderDistance))
            {
                if (container.IsDirty)
                {
                    _world.ChunkManager.TryGetChunk(container.Coordinates, out var chunk);
                    container.Update(_world, chunk);
                }
            }
        }

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