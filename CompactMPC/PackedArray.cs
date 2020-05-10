using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC
{
    public abstract class PackedArray<T> : IReadOnlyList<T>
    {
        protected byte[] Buffer { get; }
        public int Length { get; }

        protected PackedArray(int numberOfElements, int elementsPerByte)
        {
            Buffer = new byte[RequiredBytes(numberOfElements, elementsPerByte)];
            Length = numberOfElements;
        }

        protected PackedArray(byte[] bytes, int numberOfElements, int elementsPerByte)
        {
            int numberOfBytes = RequiredBytes(numberOfElements, elementsPerByte);

            if (bytes.Length < numberOfBytes)
                throw new ArgumentException("Not enough data provided.", nameof(bytes));

            Buffer = new byte[numberOfBytes];
            Length = numberOfElements;

            Array.Copy(bytes, Buffer, numberOfBytes);
        }

        public byte[] ToBytes()
        {
            byte[] bytes = new byte[Buffer.Length];
            Array.Copy(Buffer, bytes, Buffer.Length);
            return bytes;
        }

        public T[] ToArray()
        {
            T[] result = new T[Length];
            for (int i = 0; i < result.Length; ++i)
                result[i] = this[i];

            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Range(0, Length).Select(i => this[i]).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected static int RequiredBytes(int numberOfElements, int elementsPerByte)
        {
            if (numberOfElements > 0)
                return (numberOfElements - 1) / elementsPerByte + 1;

            return 0;
        }

        protected byte ReadBits(int index, int elementsPerByte, int bitMask)
        {
            int byteIndex = index / elementsPerByte;
            int bitsPerElement = 8 / elementsPerByte;
            int shift = bitsPerElement * (index % elementsPerByte);
            return (byte)((Buffer[byteIndex] >> shift) & bitMask);
        }

        protected void WriteBits(byte bits, int index, int elementsPerByte, int bitMask)
        {
            int byteIndex = index / elementsPerByte;
            int bitsPerElement = 8 / elementsPerByte;
            int shift = bitsPerElement * (index % elementsPerByte);
            Buffer[byteIndex] = (byte)((Buffer[byteIndex] & ~(bitMask << shift)) | ((bits & bitMask) << shift));
        }

        protected abstract T ReadElement(int index);
        protected abstract void WriteElement(T value, int index);

        public T this[int index]
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

        int IReadOnlyCollection<T>.Count
        {
            get
            {
                return Length;
            }
        }
    }
}
