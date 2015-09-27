using System;
using System.Security.Cryptography;

namespace POESKillTree.Utils
{
    // source: http://blogs.msdn.com/b/pfxteam/archive/2009/02/19/9434171.aspx
    public class ThreadSafeRandom
    {
        private static readonly RNGCryptoServiceProvider Global = new RNGCryptoServiceProvider();

        [ThreadStatic]
        private static Random _local;

        private Random Local
        {
            get
            {
                var inst = _local;
                if (inst == null)
                {
                    byte[] buffer = new byte[4];
                    Global.GetBytes(buffer);
                    _local = inst = new Random(
                        BitConverter.ToInt32(buffer, 0));
                }
                return inst;
            }
        }

        public int Next()
        {
            return Local.Next();
        }

        public int Next(int maxValue)
        {
            return Local.Next(maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            return Local.Next(minValue, maxValue);
        }

        public double NextDouble()
        {
            return Local.NextDouble();
        }
    }
}