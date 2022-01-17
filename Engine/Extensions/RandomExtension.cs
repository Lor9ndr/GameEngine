using System;

namespace Engine.Extensions
{
    public static class RandomExtension
    {
        public static double NextDouble(this Random @this, double min, double max)
        {
            return @this.NextDouble() * (max - min) + min;
        }

        public static float NextFloat(this Random @this, float min, float max)
        {
            return (float)@this.NextDouble(min, max);
        }
    }
}
