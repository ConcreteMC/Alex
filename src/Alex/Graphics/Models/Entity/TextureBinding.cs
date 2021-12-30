using Alex.Graphics.Effect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.Graphics.Models.Entity;

public class TextureBinding
{
	public TextureBinding(Texture2D texture, Vector2 size)
	{
		Texture = texture;
		Size = size;
		Effect = new EntityEffect();
		Effect.Texture = texture;
		Effect.TextureScale = Vector2.One / size;
	}

	public Vector2 Size { get; }
	public Texture2D Texture { get; }
	public EntityEffect Effect { get; }
}