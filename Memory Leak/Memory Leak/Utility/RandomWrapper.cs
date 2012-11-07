using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MemoryLeak.Utility
{
    class RandomWrapper
    {
        private static Random _random = new Random();

        public static float Range(float min, float max)
        {
            return min + Range() * (max - min);
        }

        public static float Range(float max)
        {
            return Range(0f, max);
        }

        public static float Range()
        {
            return (float)_random.NextDouble();
        }

        public static int Range(int max)
        {
            return Range(0, max);
        }

        public static int Range(int min, int max)
        {
            return _random.Next(min, max);
        }

        public static float Gaussian(float mean, float stdDev) //Taken from http://stackoverflow.com/questions/218060/random-gaussian-variables
        {
            double u1 = _random.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = _random.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            return (float)(mean + stdDev * randStdNormal); //random normal(mean,stdDev^2)
        }

        public static T FromList<T>(params T[] list)
        {
            return list[Range(list.Length)];
        }

        public static Vector2 RangeVector(float min, float max)
        {
            return new Vector2(Range(min, max), Range(min, max));
        }

        public static Vector2 GaussianVector(float mean, float stdDev)
        {
            return new Vector2(Gaussian(mean, stdDev), Gaussian(mean, stdDev));
        }

        public static Vector2 RangeVectorCircle(float minRadius, float maxRadius)
        {
            float angle = Range(0f, (float)Math.PI * 2);
            return new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Range(minRadius, maxRadius);
        }

        public static Color RangeColor()
        {
            return RangeColor(0, 255);
        }

        public static Color RangeColor(int min, int max, bool same = false)
        {
            byte r = (byte)(Range(min, max));
            byte g = same ? r : (byte)(Range(min, max));
            byte b = same ? r : (byte)(Range(min, max));
            byte a = same ? r : (byte)(Range(min, max));
            return new Color(r, g, b, a);
        }

        public static T[] Scramble<T>(T[] list)
        {
            List<T> tempList = new List<T>();
            foreach (var v in list)
            {
                if (Range() > 0.5)
                    tempList.Add(v);
                else
                    tempList.Insert(0, v);
            }

            return tempList.ToArray();
        }
    }
}
