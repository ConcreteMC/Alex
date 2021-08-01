using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using Alex.Blocks.State;
using Alex.Common.Utils.Collections;
using Alex.Common.Utils.Vectors;
using Alex.Entities;
using Alex.Worlds;
using Alex.Worlds.Chunks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RocketUI;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Alex.Gui.Elements.Map
{
	public class WorldMap : IUpdateable, IDisposable
    {
        private World _world;
        private ConcurrentDictionary<ChunkCoordinates, RenderedMap> _textureContainers =
            new ConcurrentDictionary<ChunkCoordinates, RenderedMap>();

        //public ObservableCollection<MapIcon> Icons => _icons;
        private ThreadSafeList<MapIcon> _icons;
        public WorldMap(World world)
        {
            _world = world;
            _icons = new ThreadSafeList<MapIcon>();
            
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
            _icons.Add(icon);
        }

        private void EntityRemoved(object sender, Entity e)
        {
            _icons.Remove(e.MapIcon);
        }

        private void EntityAdded(object sender, Entity e)
        {
            _icons.Add(e.MapIcon);
          //  TrackEntity(e, MapMarker.SmallBlip);
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
            foreach (var icon in _icons.OrderBy(o => o.DrawOrder))
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
                foreach (var container in GetContainers(new ChunkCoordinates(_world.Camera.Position), _world.ChunkManager.RenderDistance))
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
            
            var elements = _textureContainers.ToArray();
            _textureContainers.Clear();

            foreach (var element in elements)
            {
                element.Value?.Dispose();
            }
        }
    }

    public class RenderedMap : Utils.Map, IDisposable
    {
        public bool IsDirty { get; private set; }
        public bool Invalidated { get; private set; } = false;

        public Texture2D Texture { get; private set; }

        public ChunkCoordinates Coordinates { get; }
        public RenderedMap(ChunkCoordinates coordinates) : base(16,16)
        {
            Coordinates = coordinates;
        }

        private void Init(GraphicsDevice device)
        {
            if (Texture != null)
                return;

            Texture = new Texture2D(device, 16, 16);
        }

        public void Update(World world, ChunkColumn target, GraphicsDevice device)
        {
            if (target == null)
            {
                Invalidated = true;

                return;
            }

            if (Texture == null)
                Init(device);

            var cx = target.X * 16;
            var cz = target.Z * 16;
            var maxHeight = 0;

            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    BlockState state;

                    var height = target.GetHeight(x, z);

                    do
                    {
                        height--;
                        state = target.GetBlockState(x, height, z);
                        maxHeight = Math.Max(height, maxHeight);
                    } while (height > 0 && state.Block.BlockMaterial.MapColor.BaseColor.A <= 0);

                    var blockNorth = world.GetHeight(new BlockCoordinates((x + cx), height, (z + cz) - 1)) - 1;

                    var offset = 1;

                    if (blockNorth > height)
                    {
                        offset = 0;
                    }
                    else if (blockNorth < height)
                    {
                        offset = 2;
                    }

                    var blockMaterial = state?.Block?.BlockMaterial;

                    if (blockMaterial != null)
                    {
                        this[x, z] = blockMaterial.MapColor.Index * 4 + offset;
                    }
                }
            }

            Texture.SetData(this.GetData());
            IsDirty = false;
        }

        public void MarkDirty()
        {
            IsDirty = true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            base.Dispose();
            //_map?.Dispose();
           // _map = null;
            Texture?.Dispose();
            Texture = null;
        }
    }
}