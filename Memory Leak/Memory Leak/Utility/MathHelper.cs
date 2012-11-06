using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MemoryLeak.Utility
{
    class MathHelper //MY WHOLE LIFE IS A LIE
    {
        public static float Lerp(float value, float min, float max)
        {
            return min + value * (max - min);
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }
}
