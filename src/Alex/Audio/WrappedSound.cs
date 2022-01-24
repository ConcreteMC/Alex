using FmodAudio;

namespace Alex.Audio
{
	public class WrappedSound
	{
		public Sound Value { get; }
		public float Pitch { get; }
		public float Volume { get; }
		public bool Is3D { get; }

		public WrappedSound(Sound sound, float pitch = 1f, float volume = 1f, bool is3D = true)
		{
			Value = sound;
			Pitch = pitch;
			Volume = volume;
			Is3D = is3D;
		}
	}
}