namespace Alex.ResourcePackLib.Json.Models
{
    public sealed class EntityModel
    {
		public string Name { get; set; }
	    public int TextureWidth = 32;
	    public int TextureHeight = 16;
		public EntityModelBone[] Bones { get; set; }
	}
}
