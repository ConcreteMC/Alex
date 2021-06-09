using Alex.Common.Blocks;
using Microsoft.Xna.Framework;

namespace Alex.Graphics.Models
{
    public abstract class Model
    {
	    public float Scale { get; set; } = 1f;
	    
	    static Model()
	    {
		 
	    }

	    /// <summary>
	    /// The per-face brightness modifier for lighting.
	    /// </summary>
	    private static readonly float[] FaceBrightness =
		    new float[]
		    {
			    0.6f, 0.6f, // North / South
			    0.8f, 0.8f, // East / West
			    1.0f, 0.5f // MinY / MaxY
		    };

	    public static Color AdjustColor(Color color, BlockFace face, bool shade = true)
	    {
		    float brightness = 1f;
		    if (shade)
		    {
			    switch (face)
			    {
				    case BlockFace.Down:
					    brightness = FaceBrightness[5];
					    break;
				    case BlockFace.Up:
					    brightness = FaceBrightness[4];
					    break;
				    case BlockFace.East:
					    brightness = FaceBrightness[2];
					    break;
				    case BlockFace.West:
					    brightness = FaceBrightness[3];
					    break;
				    case BlockFace.North:
					    brightness = FaceBrightness[0];
					    break;
				    case BlockFace.South:
					    brightness = FaceBrightness[1];
					    break;
				    case BlockFace.None:

					    break;
			    }
		    }
		    
		    return new Color(brightness * color.ToVector3());
	    }
    }
}
