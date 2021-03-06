﻿using System;
using System.Collections.Generic;

namespace CompactMPC
{
    public class BitArray : PackedArray<Bit>
    {
        private const int ElementsPerByte = 8;
        private const int BitMask = 0x1;

        public BitArray(int numberOfElements)
            : base(numberOfElements, ElementsPerByte) { }

        public BitArray(IReadOnlyList<Bit> elements)
            : base(elements.Count, ElementsPerByte)
        {
            for (int i = 0; i < elements.Count; ++i)
                WriteElement(elements[i], i);
        }
        
        private BitArray(byte[] bytes, int numberOfElements, int elementsPerByte)
            : base(bytes, numberOfElements, elementsPerByte) { }

        public static BitArray FromBytes(byte[] bytes, int numberOfElements)
        {
            return new BitArray(bytes, numberOfElements, ElementsPerByte);
        }

        public static int RequiredBytes(int numberOfBits)
        {
            return RequiredBytes(numberOfBits, ElementsPerByte);
        }

        public BitArray Clone()
        {
            return new BitArray(Buffer, Length, ElementsPerByte);
        }

        public void Or(BitArray other)
        {
            if (other.Length != Length)
                throw new ArgumentException("Bit array length does not match.", nameof(other));

            for (int i = 0; i < Buffer.Length; ++i)
                Buffer[i] |= other.Buffer[i];
        }

        public void Xor(BitArray other)
        {
            if (other.Length != Length)
                throw new ArgumentException("Bit array length does not match.", nameof(other));

            for (int i = 0; i < Buffer.Length; ++i)
                Buffer[i] ^= other.Buffer[i];
        }

        public void And(BitArray other)
        {
            if (other.Length != Length)
                throw new ArgumentException("Bit array length does not match.", nameof(other));

            for (int i = 0; i < Buffer.Length; ++i)
                Buffer[i] &= other.Buffer[i];
        }

        public void Not()
        {
            for (int i = 0; i < Buffer.Length; ++i)
                Buffer[i] = (byte)~Buffer[i];
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
                characters[i] = ReadElement(i).IsSet ? '1' : '0';

            return new string(characters);
        }

        public override string ToString()
        {
            return ToBinaryString();
        }

        protected override Bit ReadElement(int index)
        {
            return new Bit(ReadBits(index, ElementsPerByte, BitMask));
        }

        protected sealed override void WriteElement(Bit value, int index)
        {
            WriteBits((byte)value, index, ElementsPerByte, BitMask);
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
    }
}
