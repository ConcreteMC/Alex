using System;
using System.Collections.Generic;
using System.Text;

namespace RocketUI.Utilities
{
    public static class MathUtils
    {
        public static int RoundToNearestInterval(int value, int interval)
        {	
            var scale = (1f / interval);
            return (int)(Math.Round(value * scale) / scale);
        }

        public static double RoundToNearestInterval(double value, double interval)
        {	
            var scale = (1f / interval);
            return Math.Round(value * scale) / scale;
        }

        public static float RoundToNearestInterval(float value, float interval)
        {	
            var scale = (1f / interval);
            return (float)(Math.Round(value * scale) / scale);
        }
        public static int IntCeil(double value)
        {
            int i = (int) value;
            return value > (double) i ? i + 1 : i;
        }
    }
}
