using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public struct BitQuadruple : IEnumerable<Bit>
    {
        public const int Length = 4;

        private byte _value;

        private BitQuadruple(byte value)
        {
            _value = (byte)(value & 15);
        }

        public BitQuadruple(Bit v0, Bit v1, Bit v2, Bit v3)
        {
            _value = (byte)((byte)v0 | ((byte)v1 << 1) | ((byte)v2 << 2) | ((byte)v3 << 3));
        }

        public BitQuadruple(Bit[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (values.Length != Length)
                throw new ArgumentException("Source array must contain exactly four bits.", nameof(values));

            _value = 0;
            for (int i = 0; i < Length; ++i)
                _value |= (byte)((byte)values[i] << i);
        }

        public BitQuadruple(BitArray values, int startIndex)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            if (startIndex + Length > values.Length)
                throw new ArgumentException("Source array does not contain at least four bits starting at the given index.", nameof(values));

            _value = 0;
            for (int i = 0; i < Length; ++i)
                _value |= (byte)((values[startIndex + i] ? 1 : 0) << i);
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

                return new Bit((byte)(_value >> index));
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

        public byte PackedValue
        {
            get
            {
                return _value;
            }
        }
    }
}
