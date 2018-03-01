using System;
using Alex.CoreRT.API.Graphics;
using Alex.CoreRT.API.World;
using Alex.CoreRT.Blocks;
using Alex.CoreRT.Utils;
using Microsoft.Xna.Framework;
using ResourcePackLib.CoreRT.Json;

namespace Alex.CoreRT.Graphics.Models
{
    public class BlockModel
    {
        public BlockModel()
        {

        }

        public virtual VertexPositionNormalTextureColor[] GetVertices(IWorld world, Vector3 position, Block baseBlock)
        {
            return new VertexPositionNormalTextureColor[0];
        }

	    public virtual BoundingBox GetBoundingBox(Vector3 position, Block requestingBlock)
	    {
			return new BoundingBox(position, position + Vector3.One);
	    }

		protected VertexPositionNormalTextureColor[] GetFaceVertices(BlockFace blockFace, Vector3 startPosition, Vector3 endPosition, UVMap uvmap, int rotation = 0)
		{
			Color faceColor = Color.White;
			Vector3 normal = Vector3.Zero;
			Vector3 textureTopLeft = Vector3.Zero, textureBottomLeft = Vector3.Zero, textureBottomRight = Vector3.Zero, textureTopRight = Vector3.Zero;
			switch (blockFace)
			{
				case BlockFace.Up:
					textureTopLeft = VectorExtension.From(startPosition, endPosition, endPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, endPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, endPosition, startPosition);

					normal = Vector3.Up;
					faceColor = uvmap.ColorTop; //new Color(0x00, 0x00, 0xFF);
					break;
				case BlockFace.Down:
					textureTopLeft = VectorExtension.From(startPosition, startPosition, endPosition);
					textureTopRight = VectorExtension.From(endPosition, startPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, startPosition);

					normal = Vector3.Down;
					faceColor = uvmap.ColorBottom; //new Color(0xFF, 0xFF, 0x00);
					break;
				case BlockFace.West: //Left side
					textureTopLeft = VectorExtension.From(startPosition, endPosition, startPosition);
					textureTopRight = VectorExtension.From(startPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(startPosition, startPosition, endPosition);

					normal = Vector3.Left;
					faceColor = uvmap.ColorLeft; // new Color(0xFF, 0x00, 0xFF);
					break;
				case BlockFace.East: //Right side
					textureTopLeft = VectorExtension.From(endPosition, endPosition, startPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(endPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, endPosition);

					normal = Vector3.Right;
					faceColor = uvmap.ColorRight; //new Color(0x00, 0xFF, 0xFF);
					break;
				case BlockFace.South: //Front
					textureTopLeft = VectorExtension.From(startPosition, endPosition, startPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, startPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, startPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, startPosition);

					normal = Vector3.Forward;
					faceColor = uvmap.ColorFront; // ew Color(0x00, 0xFF, 0x00);
					break;
				case BlockFace.North: //Back
					textureTopLeft = VectorExtension.From(startPosition, endPosition, endPosition);
					textureTopRight = VectorExtension.From(endPosition, endPosition, endPosition);

					textureBottomLeft = VectorExtension.From(startPosition, startPosition, endPosition);
					textureBottomRight = VectorExtension.From(endPosition, startPosition, endPosition);

					normal = Vector3.Backward;
					faceColor = uvmap.ColorBack; // new Color(0xFF, 0x00, 0x00);
					break;
				case BlockFace.None:
					break;
			}

			Microsoft.Xna.Framework.Vector2 uvTopLeft = uvmap.TopLeft;
			Microsoft.Xna.Framework.Vector2 uvTopRight = uvmap.TopRight;
			Microsoft.Xna.Framework.Vector2 uvBottomLeft = uvmap.BottomLeft;
			Microsoft.Xna.Framework.Vector2 uvBottomRight = uvmap.BottomRight;

			var faceRotation = rotation;

			if (faceRotation == 90)
			{
				uvTopLeft = uvmap.BottomLeft;
				uvTopRight = uvmap.TopLeft;

				uvBottomLeft = uvmap.BottomRight;
				uvBottomRight = uvmap.TopRight;
			}
			else if (faceRotation == 180)
			{
				uvTopLeft = uvmap.BottomRight;
				uvTopRight = uvmap.BottomLeft;

				uvBottomLeft = uvmap.TopRight;
				uvBottomRight = uvmap.TopLeft;
			}
			else if (faceRotation == 270)
			{
				uvTopLeft = uvmap.BottomLeft;
				uvTopRight = uvmap.TopLeft;

				uvBottomLeft = uvmap.BottomRight;
				uvBottomRight = uvmap.TopRight;
			}


			var topLeft = new VertexPositionNormalTextureColor(textureTopLeft, normal, uvTopLeft, faceColor);
			var topRight = new VertexPositionNormalTextureColor(textureTopRight, normal, uvTopRight, faceColor);
			var bottomLeft = new VertexPositionNormalTextureColor(textureBottomLeft, normal, uvBottomLeft,
				faceColor);
			var bottomRight = new VertexPositionNormalTextureColor(textureBottomRight, normal, uvBottomRight,
				faceColor);

			switch (blockFace)
			{
				case BlockFace.Up:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.Down:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
				case BlockFace.North:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
				case BlockFace.East:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.South:
					return (new[]
					{
						bottomLeft, topLeft, topRight,
						bottomRight, bottomLeft, topRight
					});
				case BlockFace.West:
					return (new[]
					{
						topLeft, bottomLeft, topRight,
						bottomLeft, bottomRight, topRight
					});
			}

			return new VertexPositionNormalTextureColor[0];
		}

		protected byte GetLight(IWorld world, Vector3 position)
	    {
			Vector3 lightOffset = Vector3.Zero;

		    bool initial = true;
		    byte blockLight = 0;
		    byte skyLight = 0;

		    byte highestBlocklight = 0;
		    byte highestSkylight = 0;
		    bool lightFound = false;
		    for(int i = 0; i < 6; i++)
		    {
			    switch (i)
			    {
					case 0:
						lightOffset = Vector3.Zero;
						break;
					case 1:
						initial = false;
						lightOffset = Vector3.Up;
						break;
					case 2:
						lightOffset = Vector3.Forward;
						break;
					case 3:
						lightOffset = Vector3.Backward;
						break;
					case 4:
						lightOffset = Vector3.Left;
						break;
					case 5:
						lightOffset = Vector3.Right;
						break;
					case 6:
						lightOffset = Vector3.Down;
						break;
			    }

			    skyLight = world.GetSkyLight(position + lightOffset);
			    blockLight = world.GetBlockLight(position + lightOffset);
			    if (initial && (blockLight > 0 || skyLight > 0))
			    {
				    lightFound = true;

					break;
			    }
				else if (skyLight > 0 || blockLight > 0)
			    {
				    if (skyLight > 0)
				    {
					    lightFound = true;
						break;
				    }
				    else if (blockLight > highestBlocklight)
				    {
					    highestBlocklight = blockLight;
					    highestSkylight = skyLight;
				    }
			    }
			}

		    if (!lightFound)
		    {
			    skyLight = highestSkylight;
			    if (highestBlocklight > 0)
			    {
				    blockLight = (byte)(highestBlocklight - 1);
				}
			    else
			    {
				    blockLight = 0;
			    }
		    }

		    var result = (byte)Math.Min(blockLight + skyLight, 15);

		    return result;
	    }

	    protected bool CanRender(IWorld world, Block me, Vector3 pos)
	    {
		    if (pos.Y >= 256) return true;

		    var cX = (int)pos.X & 0xf;
		    var cZ = (int)pos.Z & 0xf;

		    if (cX < 0 || cX > 16)
			    return false;

		    if (cZ < 0 || cZ > 16)
			    return false;

		    var block = world.GetBlock(pos);

		    if (me.Solid && block.Transparent) return true;
		    if (me.Transparent && block.Transparent && !block.Solid) return false;
		    if (me.Transparent) return true;
		    if (!me.Transparent && block.Transparent) return true;
		    if (block.Solid && !block.Transparent) return false;

		    return true;
	    }

	    protected UVMap GetTextureUVMap(ResourceManager resources, string texture, float x1, float x2, float y1, float y2)
	    {
		    var textureInfo = resources.Atlas.GetAtlasLocation(texture.Replace("blocks/", ""));
		    var textureLocation = textureInfo.Position;

		    var uvSize = resources.Atlas.AtlasSize;

		    var pixelSizeX = (textureInfo.Width / uvSize.X) / 16f; //0.0625
		    var pixelSizeY = (textureInfo.Height / uvSize.Y) / 16f;

		    textureLocation.X /= uvSize.X;
		    textureLocation.Y /= uvSize.Y;

		    x1 = textureLocation.X + (x1 * pixelSizeX);
		    x2 = textureLocation.X + (x2 * pixelSizeX);
		    y1 = textureLocation.Y + (y1 * pixelSizeY);
		    y2 = textureLocation.Y + (y2 * pixelSizeY);


		    return new UVMap(new Microsoft.Xna.Framework.Vector2(x1, y1),
			    new Microsoft.Xna.Framework.Vector2(x2, y1), new Microsoft.Xna.Framework.Vector2(x1, y2),
			    new Microsoft.Xna.Framework.Vector2(x2, y2), Color.White, Color.White, Color.White);
	    }
    }
}
