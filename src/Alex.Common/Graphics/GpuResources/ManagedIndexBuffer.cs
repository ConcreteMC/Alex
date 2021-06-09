using System;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using NLog;

namespace Alex.Common.Graphics.GpuResources
{
	public class ManagedIndexBuffer : IndexBuffer, IGpuResource
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(ManagedIndexBuffer));

		/// <inheritdoc />
		public EventHandler<IGpuResource> ResourceDisposed { get; set; }
        
		public GpuResourceManager Parent      { get; }
		public long               PoolId      { get; }
		public object             Owner       { get; private set; }
		public DateTime           CreatedTime { get; }

		public long MemoryUsage
		{
			get { return (IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4) * IndexCount; }
		}

		public ManagedIndexBuffer(GpuResourceManager parent, long id, object owner, GraphicsDevice graphicsDevice, IndexElementSize indexElementSize, int indexCount, BufferUsage bufferUsage) : base(graphicsDevice, indexElementSize, indexCount, bufferUsage)
		{
			Parent = parent;
			PoolId = id;
			CreatedTime = DateTime.UtcNow;
			Owner = owner;
		}
        
		private long _references = 0;
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

		public bool MarkedForDisposal { get; private set; }
		public void ReturnResource(object caller)
		{
			if (Interlocked.Read(ref _references) > 0)
			{
				if (ManagedTexture2D.ReportInvalidReturn)
					Log.Debug(
						$"Cannot mark indexbuffer for disposal, has uncleared references. Owner={Owner.ToString()}, Id={PoolId}, References={_references}");

				return;
			}
            
			if (!MarkedForDisposal)
			{
				MarkedForDisposal = true;
				Parent?.QueueForDisposal(this);
			}
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