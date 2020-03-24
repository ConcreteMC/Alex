using System;
using System.IO;
using Alex.API.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;

namespace Alex.API.Utils
{
	public class Skin
	{
		public bool Slim { get; set; } = false;
		public PooledTexture2D Texture { get; set; } = null;
	}
}
