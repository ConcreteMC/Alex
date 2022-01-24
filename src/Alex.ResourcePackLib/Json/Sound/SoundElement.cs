namespace Alex.ResourcePackLib.Json.Sound
{
	public partial struct SoundElement
	{
		public SoundMetadata SoundMetadata;
		public string Path;

		public static implicit operator SoundElement(SoundMetadata soundMetadata) =>
			new SoundElement { SoundMetadata = soundMetadata };

		public static implicit operator SoundElement(string path) => new SoundElement { Path = path };
	}
}