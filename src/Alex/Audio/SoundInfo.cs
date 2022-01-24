using Alex.Common.Utils;
using Alex.ResourcePackLib.Json.Bedrock.Sound;

namespace Alex.Audio
{
	public class SoundInfo
	{
		private static FastRandom _fastRandom = new FastRandom();
		public string Name { get; set; }
		public SoundCategory Category { get; set; }

		public WrappedSound Sound
		{
			get
			{
				if (_sounds == null || _sounds.Length == 0)
					return null;

				return _sounds[_fastRandom.Next() % _sounds.Length];
			}
		}

		private WrappedSound[] _sounds;

		public SoundInfo(string name, SoundCategory category, WrappedSound[] sounds)
		{
			Name = name;
			Category = category;
			_sounds = sounds;
		}
	}
}