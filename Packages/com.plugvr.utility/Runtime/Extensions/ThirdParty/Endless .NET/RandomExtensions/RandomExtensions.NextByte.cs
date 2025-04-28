using System;

namespace Dive.Utility.CsharpExtensions
{
    public static partial class RandomExtensions
    {
        /// <summary>
        /// Returns one random byte.
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public static byte NextByte(this Random random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            var result = (byte)random.Next(byte.MinValue, byte.MaxValue + 1);
            return result;
        }
    }
}