using RocketUI.Audio;

namespace Alex.Audio
{
	public class RocketSoundEffect : ISoundEffect
	{
		private string _sound;
		private AudioEngine _audioEngine;

		public RocketSoundEffect(AudioEngine audioEngine, string sound)
		{
			_audioEngine = audioEngine;
			_sound = sound;
		}

		/// <inheritdoc />
		public void Play()
		{
			_audioEngine.PlaySound(_sound);
		}
	}
}