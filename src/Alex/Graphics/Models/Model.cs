using System;
using Alex.Blocks;
using Alex.Rendering;
using Alex.Utils;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models
{
    public class Model
    {
        public Model()
        {

        }

        public virtual VertexPositionNormalTextureColor[] GetVertices(World world, Vector3 position, Block baseBlock)
        {
            return new VertexPositionNormalTextureColor[0];
        }

	    public virtual BoundingBox GetBoundingBox(Vector3 position, Block requestingBlock)
	    {
			return new BoundingBox(position, position + Vector3.One);
	    }

	    protected byte GetLight(World world, Vector3 position)
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

	    protected bool CanRender(World world, Block me, Vector3 pos)
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
	}
}
