using System.Runtime.Serialization;

namespace Alex.Common.Data.Options
{
	[DataContract]
	public class SoundOptions : OptionsBase
	{
		[DataMember] public OptionsProperty<double> GlobalVolume { get; set; }

		[DataMember] public OptionsProperty<double> MusicVolume { get; set; }

		[DataMember] public OptionsProperty<double> SoundEffectsVolume { get; set; }

		[DataMember] public OptionsProperty<double> AmbientVolume { get; set; }

		[DataMember] public OptionsProperty<double> BlocksVolume { get; set; }

		[DataMember] public OptionsProperty<double> PlayerVolume { get; set; }

		[DataMember] public OptionsProperty<double> NeutralVolume { get; set; }

		[DataMember] public OptionsProperty<double> HostileVolume { get; set; }

		[DataMember] public OptionsProperty<double> WeatherVolume { get; set; }

		[DataMember] public OptionsProperty<double> RecordVolume { get; set; }

		public SoundOptions()
		{
			GlobalVolume = DefineRangedProperty(1.0, 0.0, 1.0);
			MusicVolume = DefineRangedProperty(1.0, 0.0, 1.0);
			SoundEffectsVolume = DefineRangedProperty(1.0, 0.0, 1.0);
			AmbientVolume = DefineRangedProperty(1.0, 0.0, 1.0);
			BlocksVolume = DefineRangedProperty(1.0, 0.0, 1.0);
			PlayerVolume = DefineRangedProperty(1.0, 0.0, 1.0);
			NeutralVolume = DefineRangedProperty(1.0, 0.0, 1.0);
			HostileVolume = DefineRangedProperty(1.0, 0.0, 1.0);
			WeatherVolume = DefineRangedProperty(1.0, 0.0, 1.0);
			RecordVolume = DefineRangedProperty(1.0, 0.0, 1.0);
		}
	}
}