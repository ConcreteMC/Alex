using System;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.API.Graphics.GpuResources
{
	public class ManagedTexture2D : Texture2D, IGpuResource
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ManagedTexture2D));

		/// <inheritdoc />
		public EventHandler<IGpuResource> ResourceDisposed { get; set; }
		public GpuResourceManager Parent             { get; }
		public long               PoolId             { get; }
		public object             Owner              { get; private set; }
		public DateTime           CreatedTime        { get; }
        
		public long MemoryUsage
		{
			get { return GetFormatSize(Format) * Width * Height * LevelCount; }
		}

		private long _references = 0;

		//private WeakList<object> _objectReferences = new WeakList<object>();
		public ManagedTexture2D(GpuResourceManager parent, long id, object owner, GraphicsDevice graphicsDevice, int width, int height) : base(graphicsDevice, width, height)
		{
			Parent = parent;
			PoolId = id;
			CreatedTime = DateTime.UtcNow;
			Owner = owner;
		}

		public ManagedTexture2D(GpuResourceManager parent, long id, object owner, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format) : base(graphicsDevice, width, height, mipmap, format)
		{
			Parent = parent;
			PoolId = id;
			CreatedTime = DateTime.UtcNow;
			Owner = owner;
		}

		public ManagedTexture2D(GpuResourceManager parent, long id, object owner, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, int arraySize) : base(graphicsDevice, width, height, mipmap, format, arraySize)
		{
			Parent = parent;
			PoolId = id;
			CreatedTime = DateTime.UtcNow;
			Owner = owner;
		}

		private static int GetFormatSize(SurfaceFormat format)
		{
			switch (format)
			{
				case SurfaceFormat.Dxt1:
					return 8;
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
					return 16;
				case SurfaceFormat.Alpha8:
					return 1;
				case SurfaceFormat.Bgr565:
				case SurfaceFormat.Bgra4444:
				case SurfaceFormat.Bgra5551:
				case SurfaceFormat.HalfSingle:
				case SurfaceFormat.NormalizedByte2:
					return 2;
				case SurfaceFormat.Color:
				case SurfaceFormat.Single:
				case SurfaceFormat.Rg32:
				case SurfaceFormat.HalfVector2:
				case SurfaceFormat.NormalizedByte4:
				case SurfaceFormat.Rgba1010102:
				case SurfaceFormat.Bgra32:
					return 4;
				case SurfaceFormat.HalfVector4:
				case SurfaceFormat.Rgba64:
				case SurfaceFormat.Vector2:
					return 8;
				case SurfaceFormat.Vector4:
					return 16;
				default:
					throw new ArgumentException("Should be a value defined in SurfaceFormat", "Format");
			}
		}

		public void Use(object caller)
		{
			// if (caller == Owner) return;
            
			if (Interlocked.Increment(ref _references) > 0)
			{
                
			}
		}

		public void Release(object caller)
		{
			// if (caller == Owner) return;
            
			if (Interlocked.Decrement(ref _references) == 0)
			{
                
			}
		}
        
		/*public PooledTexture2D(GpuResourceManager parent, long id, GraphicsDevice graphicsDevice, int width, int height, bool mipmap, SurfaceFormat format, SurfaceType type, bool shared, int arraySize) : base(graphicsDevice, width, height, mipmap, format, type, shared, arraySize)
        {
            Parent = parent;
            PoolId = id;
            CreatedTime = DateTime.UtcNow;
        }*/

		public bool MarkedForDisposal { get; private set; }
		public static bool ReportInvalidReturn { get; set; } = true;

		static ManagedTexture2D()
		{
			if (LogManager.Configuration.Variables.TryGetValue("textureDisposalWarning", out var v)
			    && int.TryParse(v.OriginalText, out int r))
			{
				ReportInvalidReturn = r != 0;
			}
		}
        
		public void ReturnResource(object caller)
		{
			if (MarkedForDisposal) return;

			if (Interlocked.Read(ref _references) > 0)
			{
				if (ReportInvalidReturn)
					Log.Debug(
						$"Cannot mark texture for disposal, has uncleared references. Owner={Owner.ToString()}, Id={PoolId}, References={_references}");

				return;
			}

			MarkedForDisposal = true;
			Parent?.QueueForDisposal(this);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Parent?.Disposed(this);
				ResourceDisposed?.Invoke(this, this);
			}

			base.Dispose(disposing);
            
		}
	}
}