using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MemoryLeak.Utility
{
    public static class Utility
    {
		public static float Direction(this Vector2 p1, Vector2 p2)
		{
			float r = (float)Math.Atan2(p1.Y - p2.Y, p2.X - p1.X);
			return r < 0 ? r + (2 * (float)Math.PI) : r;
		}

		public static Vector2 LengthDir(float dir, float len)
		{
			return new Vector2((float)Math.Cos(dir) * len, (float)-Math.Sin(dir) * len);
		}

        public static float GetSmallest(List<float> values)
        {
            float[] smallest = {float.MaxValue};
            foreach (var i in values.Where(i => Math.Abs(i) < Math.Abs(smallest[0]))) smallest[0] = i;
            return smallest[0];
        }
    }
}
