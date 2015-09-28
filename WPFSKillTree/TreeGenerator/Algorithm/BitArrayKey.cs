using System;
using System.Collections;

namespace POESKillTree.TreeGenerator.Algorithm
{
    /// <summary>
    /// Class for using BitArrays as keys in dictonaries.
    /// Equality and HashCodes are based on the encapsulated bool[].
    /// </summary>
    public class BitArrayKey : IEquatable<BitArrayKey>
    {
        private readonly int _hash;

        public readonly BitArray Data;

        public BitArrayKey(BitArray data)
        {
            if (data == null) throw new ArgumentNullException("data");
            Data = data;
            _hash = GetHashCode(data);
        }

        public bool Equals(BitArrayKey other)
        {
            if (other == null) throw new ArgumentNullException("other");
            return Equals(Data, other.Data);
        }

        public override bool Equals(object obj)
        {
            var other = obj as BitArrayKey;
            return other != null && Equals(Data, other.Data);
        }

        public override int GetHashCode()
        {
            return _hash;
        }

        private static bool Equals(BitArray x, BitArray y)
        {
            for (var i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static int GetHashCode(BitArray obj)
        {
            var result = 29;
            for (var i = 0; i < obj.Length; i++)
            {
                if (obj.Get(i)) result++;
                result *= 23;
            }
            return result;
        }
    }
}