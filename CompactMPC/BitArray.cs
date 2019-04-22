using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InternalBitArray = System.Collections.BitArray;

namespace CompactMPC
{
    public class BitArray : IReadOnlyList<Bit>
    {
        private InternalBitArray _bits;
        
        public BitArray(InternalBitArray bits)
        {
            _bits = bits;
        }

        public BitArray(int numberOfBits)
        {
            _bits = new InternalBitArray(numberOfBits);
        }

        public BitArray(IEnumerable<Bit> bits)
        {
            _bits = new InternalBitArray(bits.Select(bit => bit.Value).ToArray());
        }
        
        public void Or(BitArray other)
        {
            _bits.Or(other._bits);
        }

        public void Xor(BitArray other)
        {
            _bits.Xor(other._bits);
        }

        public void And(BitArray other)
        {
            _bits.And(other._bits);
        }

        public void Not()
        {
            _bits.Not();
        }

        public static BitArray FromBinaryString(string bitString)
        {
            InternalBitArray bits = new InternalBitArray(bitString.Length);
            for (int i = 0; i < bitString.Length; ++i)
            {
                if (bitString[i] != '0' && bitString[i] != '1')
                    throw new ArgumentException("Binary string is only allowed to contain characters 0 and 1.", nameof(bitString));

                bits[i] = bitString[i] == '1';
            }

            return new BitArray(bits);
        }

        public string ToBinaryString()
        {
            char[] characters = new char[Length];
            for (int i = 0; i < characters.Length; ++i)
                characters[i] = _bits[i] ? '1' : '0';

            return new string(characters);
        }
        
        public byte[] ToBytes()
        {
            byte[] result = new byte[RequiredBytes(_bits.Length)];
            for (int bitIndex = 0; bitIndex < _bits.Length; ++bitIndex)
            {
                if (_bits[bitIndex])
                {
                    int byteIndex = bitIndex / 8;
                    result[byteIndex] |= (byte)(1 << (bitIndex % 8));
                }
            }

            return result;
        }

        public static BitArray FromBytes(byte[] data, int numberOfBits)
        {
            return new BitArray(new InternalBitArray(data) { Length = numberOfBits });
        }
        
        public static int RequiredBytes(int numberOfBits)
        {
            return (numberOfBits - 1) / 8 + 1;
        }

        public Bit[] ToArray()
        {
            Bit[] result = new Bit[Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = this[i];

            return result;
        }

        public override string ToString()
        {
            return ToBinaryString();
        }

        public IEnumerator<Bit> GetEnumerator()
        {
            return _bits.Cast<bool>().Select(flag => new Bit(flag)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Bit this[int index]
        {
            get
            {
                return new Bit(_bits[index]);
            }
            set
            {
                _bits[index] = value.Value;
            }
        }

        int IReadOnlyCollection<Bit>.Count
        {
            get
            {
                return _bits.Length;
            }
        }
        
        public int Length
        {
            get
            {
                return _bits.Length;
            }
        }

        public static BitArray operator |(BitArray left, BitArray right)
        {
            InternalBitArray copy = new InternalBitArray(left._bits);
            return new BitArray(copy.Or(right._bits));
        }

        public static BitArray operator ^(BitArray left, BitArray right)
        {
            InternalBitArray copy = new InternalBitArray(left._bits);
            return new BitArray(copy.Xor(right._bits));
        }

        public static BitArray operator &(BitArray left, BitArray right)
        {
            InternalBitArray copy = new InternalBitArray(left._bits);
            return new BitArray(copy.And(right._bits));
        }

        public static BitArray operator ~(BitArray right)
        {
            InternalBitArray copy = new InternalBitArray(right._bits);
            return new BitArray(copy.Not());
        }
    }
}
