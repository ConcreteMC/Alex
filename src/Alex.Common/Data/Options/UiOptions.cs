using System;
using System.Runtime.Serialization;
using Alex.Common.Gui;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Alex.Common.Data.Options
{
	[DataContract]
	public class UiOptions : OptionsBase
	{
		[DataMember]
		public MinimapOptions Minimap { get; set; }
		
		[DataMember]
		public ScoreboardOptions Scoreboard { get; set; }
		
		[DataMember]
		public ChatOptions Chat { get; set; }
		
		public UiOptions()
		{
			Minimap = DefineBranch<MinimapOptions>();
			Scoreboard = DefineBranch<ScoreboardOptions>();
			Chat = DefineBranch<ChatOptions>();
		}
	}

	public class HudOptions : OptionsBase
	{
		
	}

	public class ChatOptions : OptionsBase
	{
		[DataMember]
		public OptionsProperty<bool> Enabled { get; set; }
		
		[DataMember]
		public OptionsProperty<int> MessageHistory { get; set; }

		public ChatOptions()
		{
			Enabled = DefineProperty(true);
			MessageHistory = DefineProperty(10, (value, newValue) =>
			{
				return Math.Clamp(value, 0, 100);
			});
		}
	}
	
	[DataContract]
	public class ScoreboardOptions : OptionsBase
	{
		[DataMember]
		public OptionsProperty<bool> Enabled { get; set; }
		
		[DataMember]
		public OptionsProperty<ElementPosition> Position { get; set; }
		
		[DataMember]
		public OptionsProperty<bool> ShowScore { get; set; }

		public ScoreboardOptions()
		{
			Enabled = DefineProperty(true);
			Position = DefineProperty(ElementPosition.Default, (value, newValue) =>
			{
				if (Enum.IsDefined(newValue))
					return newValue;

				return value;
			});

			ShowScore = DefineProperty(true);
		}
	}

	[DataContract]
	public class MinimapOptions : OptionsBase
	{
		[DataMember]
		public OptionsProperty<bool> Enabled { get; set; }
        
		[DataMember]
		public OptionsProperty<double> Size { get; set; }
		
		[DataMember]
		public OptionsProperty<ZoomLevel> DefaultZoomLevel { get; set; }

		[DataMember]
		public OptionsProperty<bool> AlphaBlending { get; set; }
		
		[DataMember]
		public OptionsProperty<ElementPosition> Position { get; set; }

		public MinimapOptions()
		{
			Enabled = DefineProperty(false);
			Size = DefineRangedProperty(1d, 0.125d, 2d);
			DefaultZoomLevel = DefineProperty(ZoomLevel.Default, ZoomValidator);
			AlphaBlending = DefineProperty(true);
			Position = DefineProperty(ElementPosition.RightTop, ElementValidator);
		}
		
		private ElementPosition ElementValidator(ElementPosition currentValue, ElementPosition newValue)
		{
			if (Enum.IsDefined(newValue))
				return newValue;

			return currentValue;
		}

		private ZoomLevel ZoomValidator(ZoomLevel currentValue, ZoomLevel newValue)
		{
			if (Enum.IsDefined(newValue))
				return newValue;

			return currentValue;
		}
	}
}