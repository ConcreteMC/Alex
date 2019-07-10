using System;
using Microsoft.Xna.Framework;

namespace Alex.API.Utils
{
    public struct HslColor
    {
        private double _hue;

        public double Hue
        {
            get => _hue;
            set => _hue = CheckRange(value);
        }

        private double _saturation;

        public double Saturation
        {
            get => _saturation;
            set => _saturation = CheckRange(value);
        }

        private double _luminosity;

        public double Luminosity
        {
            get => _luminosity;
            set => _luminosity = CheckRange(value);
        }

        private double _alpha;

        public double Alpha
        {
            get => _alpha;
            set => _alpha = CheckRange(value);
        }

        private double CheckRange(double val) => Math.Max(0.0f, Math.Min(1.0f, val));
        
        public HslColor(Color color) : this()
        {
            ColorHelper.RgbToHsl(color.R, color.G, color.B, out var h, out var s, out var l);

            Hue = h;
            Saturation = s;
            Luminosity = l;
            Alpha = color.A / 255.0f;
        }
        public HslColor(double hue, double saturation, double luminosity, double alpha) : this()
        {
            Hue        = hue;
            Saturation = saturation;
            Luminosity = luminosity;
            Alpha      = alpha;
        }

        public Color ToRgb()
        {
            ColorHelper.HslToRgb(Hue, Saturation, Luminosity, out var r, out var g, out var b);
            return new Color(r, g, b, Convert.ToByte(Alpha * 255.0f));
        }

        public static implicit operator Color(HslColor hslColor)
        {
            return hslColor.ToRgb();
        }
        

        public static implicit operator HslColor(Color color)
        {
            return new HslColor(color);
        }
    }
}
