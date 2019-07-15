using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompactMPC
{
    public class BitArray : PackedArray<Bit>
    {
        private const int ElementsPerByte = 8;

        public BitArray(int numberOfElements)
            : base(RequiredBytes(numberOfElements), numberOfElements) { }

        public BitArray(Bit[] elements)
            : base(RequiredBytes(elements.Length), elements) { }

        protected BitArray(byte[] bytes, int numberOfElements)
            : base(bytes, RequiredBytes(numberOfElements), numberOfElements) { }

        public static BitArray FromBytes(byte[] bytes, int numberOfElements)
        {
            return new BitArray(bytes, numberOfElements);
        }

        public static int RequiredBytes(int numberOfBits)
        {
            return RequiredBytes(numberOfBits, ElementsPerByte);
        }

        public void Or(BitArray other)
        {
            if (other.Length != Length)
                throw new ArgumentException("Bit array length does not match.", nameof(other));

            ApplyOr(Buffer, other.Buffer);
        }

        public void Xor(BitArray other)
        {
            if (other.Length != Length)
                throw new ArgumentException("Bit array length does not match.", nameof(other));

            ApplyXor(Buffer, other.Buffer);
        }

        public void And(BitArray other)
        {
            if (other.Length != Length)
                throw new ArgumentException("Bit array length does not match.", nameof(other));

            ApplyAnd(Buffer, other.Buffer);
        }

        public void Not()
        {
            ApplyNot(Buffer);
        }

        public static BitArray FromBinaryString(string bitString)
        {
            BitArray result = new BitArray(bitString.Length);
            for (int i = 0; i < bitString.Length; ++i)
            {
                if (bitString[i] != '0' && bitString[i] != '1')
                    throw new ArgumentException("Binary string is only allowed to contain characters 0 and 1.", nameof(bitString));

                result[i] = new Bit(bitString[i] == '1');
            }

            return result;
        }

        public string ToBinaryString()
        {
            char[] characters = new char[Length];
            for (int i = 0; i < Length; ++i)
                characters[i] = ReadElement(i).Value ? '1' : '0';

            return new string(characters);
        }

        public override string ToString()
        {
            return ToBinaryString();
        }

        protected override Bit ReadElement(int index)
        {
            return (Bit)ReadBit(index);
        }

        protected override void WriteElement(Bit value, int index)
        {
            WriteBit((byte)value, index);
        }

        public BitArray Clone()
        {
            return new BitArray(Buffer, Length);
        }

        public static BitArray operator |(BitArray left, BitArray right)
        {
            BitArray clone = left.Clone();
            clone.Or(right);
            return clone;
        }

        public static BitArray operator ^(BitArray left, BitArray right)
        {
            BitArray clone = left.Clone();
            clone.Xor(right);
            return clone;
        }

        public static BitArray operator &(BitArray left, BitArray right)
        {
            BitArray clone = left.Clone();
            clone.And(right);
            return clone;
        }

        public static BitArray operator ~(BitArray right)
        {
            BitArray clone = right.Clone();
            clone.Not();
            return clone;
        }

        public static BitArray FromPairIndexArray(PairIndexArray array)
        {
            return new BitArray(array.ToBytes(), array.Length);
        }

        public PairIndexArray ToPairIndexArray()
        {
            return PairIndexArray.FromBytes(ToBytes(), Length);
        }

        // note(lumip): these turned out to be handy in extended OT where messages are given as byte[]
        //  but often need xoring (and converting to and from BitArray every time was tedious (and
        //  probably not that performant)).. this was the closest existing place to put them
        //  but I guess we should find a better one.
        public static void ApplyOr(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
                throw new ArgumentException("Byte arrays length does not match");

            for (int i = 0; i < left.Length; ++i)
                left[i] |= right[i];
        }

        public static void ApplyXor(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
                throw new ArgumentException("Byte arrays length does not match");

            for (int i = 0; i < left.Length; ++i)
                left[i] ^= right[i];
        }

        public static void ApplyAnd(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
                throw new ArgumentException("Byte arrays length does not match");

            for (int i = 0; i < left.Length; ++i)
                left[i] &= right[i];
        }

        public static void ApplyNot(byte[] value)
        {
            for (int i = 0; i < value.Length; ++i)
                value[i] = (byte)~value[i];
        }

        public static byte[] Or(byte[] left, byte[] right)
        {
            byte[] result = (byte[])left.Clone();
            ApplyOr(result, right);
            return result;
        }

        public static byte[] Xor(byte[] left, byte[] right)
        {
            byte[] result = (byte[])left.Clone();
            ApplyXor(result, right);
            return result;
        }

        public static byte[] And(byte[] left, byte[] right)
        {
            byte[] result = (byte[])left.Clone();
            ApplyAnd(result, right);
            return result;
        }

        public static byte[] Not(byte[] value)
        {
            byte[] result = (byte[])value.Clone();
            ApplyNot(result);
            return result;
        }
    }
}
