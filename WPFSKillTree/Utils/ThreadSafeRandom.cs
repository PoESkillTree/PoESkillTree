using System;
using System.Threading;

namespace PoESkillTree.Utils
{
    // Based on http://blogs.msdn.com/b/pfxteam/archive/2009/02/19/9434171.aspx
    /// <summary>
    /// Acts as a thread safe wrapper to <see cref="Random"/>.
    /// Each thread has its own <see cref="Random"/> instance.
    /// </summary>
    public class ThreadSafeRandom : Random
    {
        /// <summary>
        /// RNG for creating <see cref="Random"/> instances with different seeds for each thread.
        /// </summary>
        private static readonly Random Global = new Random();

        /// <summary>
        /// <see cref="Random"/> instance for each thread.
        /// </summary>
        private readonly ThreadLocal<Random> _local = new ThreadLocal<Random>(() =>
        {
            int seed;
            lock (Global) seed = Global.Next();
            return new Random(seed);
        });

        public override int Next()
        {
            return _local.Value.Next();
        }

        public override int Next(int maxValue)
        {
            return _local.Value.Next(maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            return _local.Value.Next(minValue, maxValue);
        }

        public override double NextDouble()
        {
            return _local.Value.NextDouble();
        }

        public override void NextBytes(byte[] buffer)
        {
            _local.Value.NextBytes(buffer);
        }
    }
}