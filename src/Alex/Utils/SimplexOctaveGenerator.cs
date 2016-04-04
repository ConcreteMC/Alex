namespace Alex.Utils
{
    public class SimplexOctaveGenerator
    {
        private readonly OpenSimplexNoise[] _generators;

        public SimplexOctaveGenerator(long seed, int octaves)
        {
            _generators = new OpenSimplexNoise[octaves];
            for (var i = 0; i < _generators.Length; i++)
            {
                _generators[i] = new OpenSimplexNoise(seed);
            }
        }

        public double XScale { get; set; }
        public double YScale { get; set; }
        public double ZScale { get; set; }
        public double WScale { get; set; }


        public double Noise(double x, double y, double frequency, double amplitude)
        {
            return Noise(x, y, 0, 0, frequency, amplitude, false);
        }

        public double Noise(double x, double y, double z, double frequency, double amplitude)
        {
            return Noise(x, y, z, 0, frequency, amplitude, false);
        }

        public double Noise(double x, double y, double z, double w, double frequency, double amplitude)
        {
            return Noise(x, y, z, w, frequency, amplitude, false);
        }

        public double Noise(double x, double y, double z, double w, double frequency, double amplitude, bool normalized)
        {
            double result = 0;
            double amp = 1;
            double freq = 1;
            double max = 0;

            x *= XScale;
            y *= YScale;
            z *= ZScale;
            w *= WScale;

            foreach (var octave in _generators)
            {
                result += octave.Evaluate((float)(x * freq), (float)(y * freq), (float)(z * freq), (float)(w * freq)) * amp;
                max += amp;
                freq *= frequency;
                amp *= amplitude;
            }

            if (normalized)
            {
                result /= max;
            }

            return result;
        }

        public void SetScale(double scale)
        {
            XScale = scale;
            YScale = scale;
            ZScale = scale;
            WScale = scale;
        }
    }
}
