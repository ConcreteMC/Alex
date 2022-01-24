using System.Globalization;
using Alex.Gui;

namespace Alex.Gamestates.MainMenu.Options
{
	public class SoundOptionsState : OptionsStateBase
	{
		public SoundOptionsState(GuiPanoramaSkyBox skyBox) : base(skyBox)
		{
			TitleTranslationKey = "options.sounds.title";

			var masterSlider = CreateSlider(
				v =>
					$"{GuiRenderer.GetTranslation("soundCategory.master")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}",
				options => options.SoundOptions.GlobalVolume, 0, 1D, 0.01D);

			var musicSlider = CreateSlider(
				v =>
					$"{GuiRenderer.GetTranslation("soundCategory.music")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}",
				options => options.SoundOptions.MusicVolume, 0, 1D, 0.01);

			var ambientVolume = CreateSlider(
				v =>
					$"{GuiRenderer.GetTranslation("soundCategory.ambient")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}",
				options => options.SoundOptions.AmbientVolume, 0, 1D, 0.01);

			var record = CreateSlider(
				v =>
					$"{GuiRenderer.GetTranslation("soundCategory.record")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}",
				options => options.SoundOptions.RecordVolume, 0, 1D, 0.01);

			var weather = CreateSlider(
				v =>
					$"{GuiRenderer.GetTranslation("soundCategory.weather")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}",
				options => options.SoundOptions.WeatherVolume, 0, 1D, 0.01);

			var hostile = CreateSlider(
				v =>
					$"{GuiRenderer.GetTranslation("soundCategory.hostile")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}",
				options => options.SoundOptions.HostileVolume, 0, 1D, 0.01);

			var neutral = CreateSlider(
				v =>
					$"{GuiRenderer.GetTranslation("soundCategory.neutral")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}",
				options => options.SoundOptions.NeutralVolume, 0, 1D, 0.01);

			var player = CreateSlider(
				v =>
					$"{GuiRenderer.GetTranslation("soundCategory.player")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}",
				options => options.SoundOptions.PlayerVolume, 0, 1D, 0.01);

			var blocks = CreateSlider(
				v =>
					$"{GuiRenderer.GetTranslation("soundCategory.block")}: {((int)(v * 100)).ToString(CultureInfo.InvariantCulture)}",
				options => options.SoundOptions.BlocksVolume, 0, 1D, 0.01);

			AddGuiRow(masterSlider);
			AddGuiRow(musicSlider, record);
			AddGuiRow(weather, blocks);
			AddGuiRow(hostile, neutral);
			AddGuiRow(player, ambientVolume);
		}

		public string Format(double value)
		{
			return (value * 100).ToString(CultureInfo.InvariantCulture);
		}
	}
}