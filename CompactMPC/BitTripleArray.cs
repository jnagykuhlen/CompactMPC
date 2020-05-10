using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC
{
    public class BitTripleArray : IReadOnlyList<BitTriple>
    {
        private readonly BitArray _bits;
        
        public int Length { get; }

        public BitTripleArray(int numberOfElements)
            : this(new BitArray(3 * numberOfElements), numberOfElements) { }

        public BitTripleArray(IReadOnlyList<BitTriple> elements)
            : this(elements.Count)
        {
            for (int i = 0; i < elements.Count; ++i)
                WriteElement(elements[i], i);
        }

        private BitTripleArray(BitArray bits, int numberOfElements)
        {
            _bits = bits;
            Length = numberOfElements;
        }

        public static BitTripleArray FromBytes(byte[] bytes, int numberOfElements)
        {
            BitArray bits = BitArray.FromBytes(bytes, 3 * numberOfElements);
            return new BitTripleArray(bits, numberOfElements);
        }
        
        public byte[] ToBytes()
        {
            return _bits.ToBytes();
        }

        public static int RequiredBytes(int numberOfElements)
        {
            if (numberOfElements > 0)
                return (3 * numberOfElements - 1) / 8 + 1;

            return 0;
        }
        
        public IEnumerator<BitTriple> GetEnumerator()
        {
            return Enumerable.Range(0, Length).Select(i => this[i]).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private BitTriple ReadElement(int index)
        {
            int startBitIndex = 3 * index;
            return new BitTriple(
                _bits[startBitIndex + 0],
                _bits[startBitIndex + 1],
                _bits[startBitIndex + 2]
            );
        }

        private void WriteElement(BitTriple value, int index)
        {
            int startBitIndex = 3 * index;
            _bits[startBitIndex + 0] = value.X;
            _bits[startBitIndex + 1] = value.Y;
            _bits[startBitIndex + 2] = value.Z;
        }

        public BitTriple this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return ReadElement(index);
            }
            set
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                WriteElement(value, index);
            }
        }

        int IReadOnlyCollection<BitTriple>.Count
        {
            get
            {
                return Length;
            }
        }

        public BitTripleArray Clone()
        {
            return new BitTripleArray(_bits.Clone(), Length);
        }
    }
}
