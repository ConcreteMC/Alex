namespace Alex.Utils.Skins
{
	/*public class Skin : ICloneable
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
	}*/
}