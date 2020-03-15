using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using MiNET.Utils.Skins;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Alex.Worlds.Bedrock
{
    public class SkinResourcePatch : ICloneable
	{
		public GeometryIdentifier Geometry { get; set; }

		public object Clone()
		{
			var cloned = new SkinResourcePatch();
			cloned.Geometry = (GeometryIdentifier) Geometry?.Clone();

			return cloned;
		}
	}

	public class GeometryIdentifier : ICloneable
	{
		[JsonProperty("default")]
		public string Default { get; set; }
		
		[JsonProperty("animated_face")]
		public string AnimatedFace { get; set; }
		public object Clone()
		{
			return MemberwiseClone();
		}
	}

	public class Skin : ICloneable
	{
		public bool Slim { get; set; }
		public bool IsPersonaSkin { get; set; }
		public bool IsPremiumSkin { get; set; }

		public Cape Cape { get; set; }
		public string SkinId { get; set; }

		public string ResourcePatch { get; set; }

		public SkinResourcePatch SkinResourcePatch { get; set; } // contains GeometryName
		public int Height { get; set; }
		public int Width { get; set; }
		public byte[] Data { get; set; }
		public string GeometryName { get; set; }
		public string GeometryData { get; set; }
		public string AnimationData { get; set; }
		public List<Animation> Animations { get; set; }

		public Skin()
		{
			Cape = new Cape();
			Animations = new List<Animation>();
		}

		public static GeometryModel Parse(string json)
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			settings.MissingMemberHandling = MissingMemberHandling.Error;
			settings.Formatting = Formatting.Indented;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

			return JsonConvert.DeserializeObject<GeometryModel>(json, settings);
		}

		public static string ToJson(GeometryModel geometryModel)
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			settings.MissingMemberHandling = MissingMemberHandling.Error;
			//settings.Formatting = Formatting.Indented;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			settings.Converters.Add(new StringEnumConverter {NamingStrategy = new CamelCaseNamingStrategy()});

			return JsonConvert.SerializeObject(geometryModel, settings);
		}


		public static string ToJson(SkinResourcePatch model)
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			settings.MissingMemberHandling = MissingMemberHandling.Error;
			//settings.Formatting = Formatting.Indented;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			settings.Converters.Add(new StringEnumConverter {CamelCaseText = true});

			string json = JsonConvert.SerializeObject(model, settings);

			return json;
		}

		public static SkinResourcePatch ToJSkinResourcePatch(string json)
		{
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
			settings.MissingMemberHandling = MissingMemberHandling.Error;
			//settings.Formatting = Formatting.Indented;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			settings.Converters.Add(new StringEnumConverter {CamelCaseText = true});

			var obj = JsonConvert.DeserializeObject<SkinResourcePatch>(json, settings);

			return obj;
		}

		public object Clone()
		{
			var clonedSkin = (Skin) MemberwiseClone();
			clonedSkin.Data = Data?.Clone() as byte[];
			clonedSkin.Cape = Cape?.Clone() as Cape;
			clonedSkin.SkinResourcePatch = SkinResourcePatch?.Clone() as SkinResourcePatch;

			foreach (Animation animation in Animations)
			{
				clonedSkin.Animations.Add((Animation) animation.Clone());
			}

			return clonedSkin;
		}
	}
}