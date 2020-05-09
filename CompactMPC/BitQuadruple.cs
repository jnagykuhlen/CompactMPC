using System;
using System.Collections;
using System.Collections.Generic;

namespace CompactMPC
{
    public readonly struct BitQuadruple : IReadOnlyList<Bit>
    {
        public const int Length = 4;

        public byte PackedValue { get; }
        
        private BitQuadruple(byte value)
        {
            PackedValue = (byte)(value & 0xf);
        }

        public BitQuadruple(Bit v0, Bit v1, Bit v2, Bit v3)
        {
            PackedValue = (byte)((byte)v0 | ((byte)v1 << 1) | ((byte)v2 << 2) | ((byte)v3 << 3));
        }

        public static BitQuadruple FromPackedValue(byte packedValue)
        {
            return new BitQuadruple(packedValue);
        }
        
        public Bit this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return new Bit((byte)(PackedValue >> index));
            }
        }

        public IEnumerator<Bit> GetEnumerator()
        {
            for (int i = 0; i < Length; ++i)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int IReadOnlyCollection<Bit>.Count
        {
            get
            {
                return Length;
            }
        }
    }
}
