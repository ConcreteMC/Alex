using Alex.Common.Utils;
using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using NLog;

namespace Alex.Entities.Passive
{
	public class Sheep : PassiveMob
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Sheep));
		private static readonly DyeColor[] SheepColors = new DyeColor[]
		{
			DyeColor.WhiteDye,
			DyeColor.OrangeDye,
			DyeColor.MagentaDye,
			DyeColor.LightBlueDye,
			DyeColor.YellowDye, 
			DyeColor.LimeDye, 
			DyeColor.PinkDye,
			DyeColor.GrayDye,
			DyeColor.LightGrayDye,
			DyeColor.CyanDye,
			DyeColor.PurpleDye,
			DyeColor.BlueDye, 
			DyeColor.BrownDye,
			DyeColor.GreenDye, 
			DyeColor.RedDye, 
			DyeColor.BlackDye
		};

		private DyeColor _color = DyeColor.WhiteDye;
		public DyeColor Color
		{
			get
			{
				return _color;
			}
			set
			{
				_color = value;
				ModelRenderer.EntityColor = value.Color.ToVector3();
			}
		}

		public Sheep(World level) : base(level)
		{
			Height = 1.3;
			Width = 0.9;
		}
		
		protected override void HandleJavaMeta(MetaDataEntry entry)
		{
			base.HandleJavaMeta(entry);

			if (entry.Index == 16 && entry is MetadataByte meta)
			{
				SetSheared((meta.Value & 0x10) != 0);
				SetColor(meta.Value & 0x0F);
			}
		}

		public void SetColor(int value)
		{
			if (value < 0 || value > 19)
			{
				Log.Warn($"Invalid sheep color: {value}");
				return;
			}
			
			Color = SheepColors[value % SheepColors.Length];
		}

		public void SetSheared(bool value)
		{
			//TryUpdateGeometry("minecraft:sheep", value ? "sheared" : "default");
			IsSheared = value;
		}
	}
}
