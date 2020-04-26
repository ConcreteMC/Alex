using System.Collections.Generic;
using System.Linq;
using Alex.ResourcePackLib.Json.Converters;
using Alex.ResourcePackLib.Json.Models.Entities;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using NLog;

namespace Alex.Graphics.Models.Entity.Geometry
{
    public partial class BedrockGeometry
    {
        [JsonProperty("format_version")]
        public string FormatVersion { get; set; }

        [JsonProperty("minecraft:geometry")]
        public MinecraftGeometry[] MinecraftGeometry { get; set; }
    }

    public partial class MinecraftGeometry
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
        
        [JsonProperty("bones")]
        public List<Bone> Bones { get; set; }
        
        [JsonProperty("texturewidth", NullValueHandling = NullValueHandling.Ignore)]
        public int? TextureWidth { get; set; } = null;
        
        [JsonProperty("textureheight", NullValueHandling = NullValueHandling.Ignore)]
        public int? TextureHeight { get; set; } = null;
        
        [JsonProperty("visible_bounds_width", NullValueHandling = NullValueHandling.Ignore)]
        public float? VisibleBoundsWidth { get; set; } = null;
        
        [JsonProperty("visible_bounds_height", NullValueHandling = NullValueHandling.Ignore)]
        public float? VisibleBoundsHeight { get; set; } = null;

        [JsonProperty("visible_bounds_offset", NullValueHandling = NullValueHandling.Ignore)]
        public Vector3 VisibleBoundsOffset { get; set; } = Vector3.Zero;

        [JsonProperty("description")]
        public SkinDescription Description { get; set; }

        public EntityModel Convert()
        {
            if (Bones.All(x => x.Cubes == null))
                return null;
            
            EntityModel model = new EntityModel();
            model.Name = Description.Identifier;
            model.Textureheight = Description.TextureHeight;
            model.Texturewidth = Description.TextureWidth;

            var scale = Vector3.One;

            if (VisibleBoundsHeight.HasValue && VisibleBoundsWidth.HasValue)
            {
              /*  scale.X = 0.0625F * VisibleBoundsWidth.Value;
                scale.Z = 0.0625F * VisibleBoundsWidth.Value;
                
                scale.Y = 0.0625F * VisibleBoundsHeight.Value;*/
            }
            
            Dictionary<string, EntityModelBone> bones = new Dictionary<string, EntityModelBone>();
            foreach (var bone in Bones)
            {
                if (bone.Cubes == null)
                    continue;
                
                EntityModelBone newBone;
                if (!bones.TryGetValue(bone.Name, out newBone))
                {
                    newBone = new EntityModelBone();
                    bones.TryAdd(bone.Name, newBone);
                }
                else
                {
                    Log.Warn($"Overwriting bone...");
                }

                newBone.Parent = bone.Parent;
                newBone.Name = bone.Name;
                newBone.Pivot = bone.Pivot;
                newBone.Rotation = new Vector3(bone.Rotation.X, bone.Rotation.Y, bone.Rotation.Z);

                if (bone.Cubes == null)
                {
                    newBone.NeverRender = true;
                }
                else
                {
                    newBone.Cubes = bone.Cubes.Select(x => new EntityModelCube()
                    {
                        Mirror = x.Mirror,
                        Origin = x.Origin * scale,
                        Size = x.Size * scale,
                        Uv = x.Uv,
                        Inflate = x.Inflate
                    }).ToArray();
                }

                bones[bone.Name] = newBone;
                // newBone.
            }

            model.Bones = bones.Values.ToArray();
            return model;
        }
    }

    public partial class Bone
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("neverRender", NullValueHandling = NullValueHandling.Ignore)]
        public bool NeverRender { get; set; } = false;
        
        [JsonProperty("reset", NullValueHandling = NullValueHandling.Ignore)]
        public bool Reset { get; set; } = false;
        
        [JsonProperty("parent", NullValueHandling = NullValueHandling.Ignore)]
        public string Parent { get; set; }

        [JsonProperty("pivot", NullValueHandling = NullValueHandling.Ignore)]
        public Vector3 Pivot { get; set; } = new Vector3(0f, 0f, 0f);
        
        [JsonProperty("rotation", NullValueHandling = NullValueHandling.Ignore)]
        public Vector3 Rotation { get; set; } = new Vector3(0f, 0f, 0f);

        [JsonProperty("poly_mesh", NullValueHandling = NullValueHandling.Ignore)]
        public PolyMesh PolyMesh { get; set; } = null;

        [JsonProperty("locators", NullValueHandling = NullValueHandling.Ignore)]
        public Locators Locators { get; set; } = null;
        
        [JsonProperty("cubes", NullValueHandling = NullValueHandling.Ignore)]
        public Cube[] Cubes { get; set; }
    }

    public partial class Cube
    {
        [JsonProperty("inflate", NullValueHandling = NullValueHandling.Ignore)] public double Inflate { get; set; } = 1d;

        [JsonProperty("mirror")]
        public bool Mirror { get; set; }

        [JsonProperty("origin")]
        public Vector3 Origin { get; set; } = new Vector3(0f, 0f, 0f);

        [JsonProperty("size")]
        public Vector3 Size { get; set; } = new Vector3(0f, 0f, 0f);

        [JsonProperty("uv")]
        public Vector2 Uv { get; set; } = new Vector2(0f, 0f);
    }
    
    public partial class Locators
    {
        [JsonProperty("lead_hold")]
        public Vector3 LeadHold { get; set; }
    }

    public partial class PolyMesh
    {
        [JsonProperty("normalized_uvs")]
        public bool NormalizedUvs { get; set; }

        [JsonProperty("normals")]
        public Vector3[] Normals { get; set; }

        [JsonProperty("polys")]
        public long[][][] Polys { get; set; }

        [JsonProperty("positions")]
        public Vector3[] Positions { get; set; }

        [JsonProperty("uvs")]
        public Vector2[] Uvs { get; set; }
    }

    public partial class SkinDescription
    {
        [JsonProperty("identifier")]
        public string Identifier { get; set; }

        [JsonProperty("texture_height")]
        public long TextureHeight { get; set; }

        [JsonProperty("texture_width")]
        public long TextureWidth { get; set; }
    }
}