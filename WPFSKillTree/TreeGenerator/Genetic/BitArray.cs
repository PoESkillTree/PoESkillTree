using System;

namespace POESKillTree.TreeGenerator.Genetic
{
    /// <summary>
    /// An array of bits that allows efficient storage. Implements Equals and GetHashCode.
    /// Immutable after the first call to <see cref="GetHashCode"/>.
    /// </summary>
    /// <remarks>
    /// Based on System.Collections.BitArray, which is licensed under MIT license by the .NET Foundation.
    /// See https://github.com/dotnet/corefx/blob/d0dc5fc099946adc1035b34a8b1f6042eddb0c75/src/System.Collections/src/System/Collections/BitArray.cs
    /// for the original implementation.
    /// </remarks>
    public sealed class BitArray : IEquatable<BitArray>
    {
        /// <summary>
        /// The number of Bits which fit into one Int32.
        /// </summary>
        private const int BitsPerInt = 32;

        /// <summary>
        /// Stores the bits as an array of ints. Each int stores BitsPerInt bits.
        /// Bits are stored LSB first into ints.
        /// </summary>
        private readonly int[] _array;

        private int? _hash;

        /// <summary>
        /// Gets or sets the bit at the specified index.
        /// The setter can only be used until <see cref="GetHashCode"/> is called for the first time. To make sure
        /// that <see cref="GetHashCode"/> has not yet be called, treat a BitArray always as immutable after it has been initialized.
        /// </summary>
        public bool this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Must be >= 0 and < Length");
                }

                return (_array[index / BitsPerInt] & (1 << (index % BitsPerInt))) != 0;
            }
            set
            {
                if (index < 0 || index >= Length)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Must be >= 0 and < Length");
                }
                if (_hash.HasValue)
                {
                    throw new InvalidOperationException("Must not be changed after GetHashCode was called.");
                }

                if (value)
                {
                    _array[index / BitsPerInt] |= (1 << (index % BitsPerInt));
                }
                else
                {
                    _array[index / BitsPerInt] &= ~(1 << (index % BitsPerInt));
                }
            }
        }

        /// <summary>
        /// Gets the number of bits stored in this BitArray (constant for each instance).
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Creates an instance that can hold length bit values. All bits are initialized
        /// as false.
        /// </summary>
        public BitArray(int length)
        {
            Length = length;
            _array = new int[GetArrayLength(Length)];
        }

        /// <summary>
        /// Creates an instance which initially has the same values as the given instance.
        /// This instance is not yet immutable even if <see cref="GetHashCode"/> was called on
        /// the given instance.
        /// </summary>
        public BitArray(BitArray bits)
        {
            Length = bits.Length;
            _array = new int[GetArrayLength(Length)];
            bits._array.CopyTo(_array, 0);
        }

        /// <summary>
        /// Creates an instance that initially stores the given bit values.
        /// </summary>
        public BitArray(bool[] bits)
        {
            Length = bits.Length;
            _array = new int[GetArrayLength(Length)];
            for (var i = 0; i < Length; i++)
            {
                if (bits[i])
                {
                    _array[i / BitsPerInt] |= (1 << (i % BitsPerInt));
                }
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as BitArray;
            return other != null && Equals(other);
        }

        public bool Equals(BitArray other)
        {
            if (null == other) return false;
            if (this == other) return true;
            if (other.Length != Length) return false;
            if (_hash.HasValue && other._hash.HasValue && other.GetHashCode() != GetHashCode())
            {
                return false;
            }
            for (var i = 0; i < _array.Length; i++)
            {
                if (_array[i] != other._array[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            if (!_hash.HasValue)
            {
                var hash = 17;
                foreach (var i in _array)
                {
                    hash = unchecked(hash * 23 + i.GetHashCode());
                }
                _hash = hash;
            }
            return _hash.Value;
        }

        private static int GetArrayLength(int n)
        {
            return n > 0 ? (n - 1) / BitsPerInt + 1 : 0;
        }
    }
}