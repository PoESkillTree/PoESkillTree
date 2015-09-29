using System;

namespace POESKillTree.Utils
{
    // Based on http://blogs.msdn.com/b/pfxteam/archive/2009/02/19/9434171.aspx
    /// <summary>
    /// Acts as a thread safe wrapper to <see cref="Random"/>.
    /// Each thread has its own <see cref="Random"/> instance.
    /// Each instance uses the same <see cref="Random"/> instance for each thread.
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
        [ThreadStatic]
        private static Random _local;
        
        private static Random Local
        {
            get
            {
                var inst = _local;
                if (inst == null)
                {
                    int seed;
                    lock (Global) seed = Global.Next();
                    _local = inst = new Random(seed);
                }
                return inst;
            }
        }

        public override int Next()
        {
            return Local.Next();
        }

        public override int Next(int maxValue)
        {
            return Local.Next(maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            return Local.Next(minValue, maxValue);
        }

        public override double NextDouble()
        {
            return Local.NextDouble();
        }

        public override void NextBytes(byte[] buffer)
        {
            Local.NextBytes(buffer);
        }
    }
}