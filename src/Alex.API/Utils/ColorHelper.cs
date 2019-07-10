using System;
using Microsoft.Xna.Framework;

namespace Alex.API.Utils
{
    public static class ColorHelper
    {
        public static Color Darken(this Color color, float amount)
        {
            var hsl = (HslColor) color;
            hsl.Luminosity -= amount;

            return hsl.ToRgb();
        }

        public static void RgbToHsl(byte red, byte green, byte blue, out double h, out double s, out double l)
        {
            double r = red / 255.0f;
            double g = green / 255.0f;
            double b = blue / 255.0f;

            double v;
            double m;
            double vm;
            double r2, g2, b2;
            h = 0; // default to black
            s = 0;
            l = 0;
            v = Math.Max(r,g);
            v = Math.Max(v,b);
            m = Math.Min(r,g);
            m = Math.Min(m,b);
            l = (m + v) / 2.0;
            if (l <= 0.0)
            {
                return;
            }
            vm = v - m;
            s = vm;
            if (s > 0.0)
            {
                s /= (l <= 0.5) ? (v + m ) : (2.0 - v - m) ;
            }
            else
            {
                return;
            }
            r2 = (v - r) / vm;
            g2 = (v - g) / vm;
            b2 = (v - b) / vm;
            if (r == v)
            {
                h = (g == m ? 5.0 + b2 : 1.0 - g2);
            }
            else if (g == v)
            {
                h = (b == m ? 1.0 + r2 : 3.0 - b2);
            }
            else
            {
                h = (r == m ? 3.0 + g2 : 5.0 - r2);
            }
            if (h >= 6f) h -= 6f; 
            if (h < 0f) h += 6f;
            h /= 6.0;
        }
        
        public static void HslToRgb(double h, double s, double l, out byte r, out byte g, out byte b)
        {
            double v;
            double r2, g2, b2;
            
            r2 = l; // default to gray
            g2 = l;
            b2 = l;
            v = (l <= 0.5) ? (l * (1.0 + s)) : (l + s - l * s);

            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;
                
                m = l + l - v;
                sv = (v - m) / v;
                h *= 6.0;
                sextant = (int) h;
                fract = h - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                 {
                    case 0:
                        r2 = v;
                        g2 = mid1;
                        b2 = m;
                        break;
                    case 1:
                        r2 = mid2;
                        g2 = v;
                        b2 = m;
                        break;
                    case 2:
                        r2 = m;
                        g2 = v;
                        b2 = mid1;
                        break;
                    case 3:
                        r2 = m;
                        g2 = mid2;
                        b2 = v;
                        break;
                    case 4:
                        r2 = mid1;
                        g2 = m;
                        b2 = v;
                        break;
                    case 5:
                        r2 = v;
                        g2 = m;
                        b2 = mid2;
                        break;
                }
            }

            r = Convert.ToByte(r2 * 255.0f);
            g = Convert.ToByte(g2 * 255.0f);
            b = Convert.ToByte(b2 * 255.0f);
        }
    }
}