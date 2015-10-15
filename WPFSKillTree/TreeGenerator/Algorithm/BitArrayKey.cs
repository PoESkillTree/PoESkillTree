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
        /// <summary>
        /// Hash is calculated on construction and saved here.
        /// </summary>
        private readonly int _hash;

        /// <summary>
        /// The wrapped BitArray.
        /// </summary>
        public readonly BitArray Data;

        /// <summary>
        /// Constructs a new BitArrayKey that wraps the given <see cref="BitArray"/>.
        /// </summary>
        /// <param name="data">BitArray to wrap. Should not be changed once stored. (not null)</param>
        public BitArrayKey(BitArray data)
        {
            if (data == null) throw new ArgumentNullException("data");
            Data = data;
            _hash = GetHashCode(data);
        }

        public bool Equals(BitArrayKey other)
        {
            return other != null && Equals(Data, other.Data);
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