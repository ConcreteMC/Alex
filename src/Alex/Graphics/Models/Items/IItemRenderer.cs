using System;
using Alex.Graphics.Models.Entity;
using Alex.ResourcePackLib;
using Alex.ResourcePackLib.Json.Models;
using Alex.ResourcePackLib.Json.Models.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Items
{
	public interface IItemRenderer : IAttached, IDisposable
	{
		ResourcePackModelBase ResourcePackModel { get; }

		DisplayPosition DisplayPosition { get; set; }

		bool Cache(ResourceManager pack);

		IItemRenderer CloneItemRenderer();
	}
}