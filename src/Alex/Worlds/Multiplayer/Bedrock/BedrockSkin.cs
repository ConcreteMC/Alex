using System;
using System.Collections.Generic;
using MiNET.Utils.Skins;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Alex.Worlds.Multiplayer.Bedrock
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

	public class PersonaPiece
	{
		[JsonProperty("IsDefault")]
		public bool Default { get; set; } = true; //indicates if it is a persona skin part which is the default for skins
		
		[JsonProperty("PackId")]
		public string PackId { get; set; } = ""; //which is a UUID of the pack the piece belongs to
		
		[JsonProperty("PieceId")]
		public string PieceId { get; set; } = ""; //a UUID unique to the piece
		
		[JsonProperty("PieceType")]
		public string PieceType { get; set; } = "persona_body"; 
		// PieceType holds the type of the piece. Several types I was able to find immediately are listed below.
		// - persona_skeleton
		// - persona_body
		// - persona_skin
		// - persona_bottom
		// - persona_feet
		// - persona_top
		// - persona_mouth
		// - persona_hair
		// - persona_eyes
		// - persona_facial_hair
		
		[JsonProperty("ProductId")]
		public string ProductId { get; set; } = ""; //a UUID identifying the piece for purchases  (empty for free pieces)
	}

	public class PieceTint
	{
		//holds a list of hex colours relevant to the piece
		// Some pieces have only one colour, for which the rest will be filled with #0
		//Some pieces like the eyes have different colours for the iris and the eyebrows for example
		
		[JsonProperty("Colors")]
		public string[] Colors { get; set; } = new string[4]
		{
			"#0",
			"#0",
			"#0",
			"#0"
		};
		
		[JsonProperty("PieceType")]
		public string PieceType { get; set; } //is the piece type that it refers to, which is present in the previous list
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

		public string SkinColor { get; set; } = "#0";
		public string ArmSize { get; set; } = "slim";

		public List<PersonaPiece> PersonaPieces { get; set; } = new List<PersonaPiece>();
		
		[JsonProperty("PieceTintColors")]
		public List<PieceTint> PieceTintColours { get; set; } = new List<PieceTint>();
		
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