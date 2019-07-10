using System.Linq;
using Alex.ResourcePackLib.Json.Models.Items;

namespace Alex.Graphics.Models.Items
{
    public class ItemModelRenderer : Model
    {
	    private ResourcePackItem _model { get; }
		
		public ItemModelRenderer(ResourcePackItem model)
		{
			_model = model;
			Cache();
		}

	    private void Cache()
	    {
		    var t = _model.Textures.FirstOrDefault();
			
		   // int verticesPerTool = TOOL_TEXTURE_SIZE * TOOL_TEXTURE_SIZE * 36;
		}
    }
}
