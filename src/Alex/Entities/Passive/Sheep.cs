using Alex.Networking.Java.Packets.Play;
using Alex.Worlds;
using Microsoft.Xna.Framework;
using NLog;

namespace Alex.Entities.Passive
{
	public class Sheep : PassiveMob
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Sheep));
		private static readonly Color[] SheepColors = new Color[]
		{
			new Color(1908001), 
			new Color(11546150), 
			new Color(6192150), 
			new Color(8606770), 
			new Color(3949738), 
			new Color(8991416), 
			new Color(1481884), 
			new Color(10329495), 
			new Color(4673362), 
			new Color(15961002), 
			new Color(8439583), 
			new Color(16701501), 
			new Color(3847130), 
			new Color(13061821), 
			new Color(16351261), 
			new Color(16383998), 
			new Color(1908001), 
			new Color(8606770), 
			new Color(3949738), 
			new Color(16383998) 
		};
		
		public Sheep(World level) : base((EntityType)13, level)
		{
			JavaEntityId = 91;
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
			
			ModelRenderer.EntityColor = SheepColors[value % SheepColors.Length].ToVector3();
		}

		public void SetSheared(bool value)
		{
			TryUpdateGeometry("minecraft:sheep", value ? "sheared" : "default");
			IsSheared = value;
		}
	}
}
