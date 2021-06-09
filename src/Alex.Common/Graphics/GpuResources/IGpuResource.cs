using System;

namespace Alex.Common.Graphics.GpuResources
{
	public interface IGpuResource : IDisposable
	{
		EventHandler<IGpuResource> ResourceDisposed { get; set; }
        
		GpuResourceManager Parent { get; }
		DateTime CreatedTime { get; }
		long PoolId { get; }
        
		object Owner { get; }
        
		long MemoryUsage { get; }

		bool MarkedForDisposal { get; }
		void ReturnResource(object caller);
	}
}