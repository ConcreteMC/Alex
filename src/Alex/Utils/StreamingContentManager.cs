using System;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace Alex.Utils
{
	public class StreamingContentManager : ContentManager
	{
		//private byte[] _data;

		internal StreamingContentManager(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider, rootDirectory)
		{
			//_data = data;
		}

		protected override Stream OpenStream(string assetName)
		{
			return new MemoryStream(ResourceManager.ReadResource(assetName), false);
		}
	}
}
