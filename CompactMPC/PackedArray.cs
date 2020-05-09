using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC
{
    public abstract class PackedArray<T> : IReadOnlyList<T>, ICollection<T>
    {
        protected byte[] Buffer { get; }
        
        public int Length { get; }

        protected PackedArray(int numberOfBytes, int numberOfElements)
        {
            if (numberOfElements < 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfElements));

            Buffer = new byte[numberOfBytes];
            Length = numberOfElements;
        }

        protected PackedArray(byte[] bytes, int numberOfBytes, int numberOfElements)
        {
            if (numberOfElements < 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfElements));

            if (bytes.Length < numberOfBytes)
                throw new ArgumentException("Not enough data provided.", nameof(bytes));

            Buffer = new byte[numberOfBytes];
            Length = numberOfElements;

            Array.Copy(bytes, Buffer, numberOfBytes);
        }

        protected PackedArray(int numberOfBytes, T[] elements)
            : this(numberOfBytes, elements.Length)
        {
            for (int i = 0; i < Length; ++i)
                WriteElement(elements[i], i);
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

        protected static int RequiredBytes(int numberOfElements, int elementsPerByte)
        {
            if (numberOfElements > 0)
                return (numberOfElements - 1) / elementsPerByte + 1;

            return 0;
        }

        protected byte ReadBit(int index)
        {
            return ReadBits(index, 8, 0x1);
        }

        protected void WriteBit(byte bit, int index)
        {
            WriteBits(bit, index, 8, 0x1);
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

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerable.Range(0, Length).Select(i => this[i]).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException("Cannot add element to array.");
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException("Cannot remove element from array.");
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException("Cannot clear array.");
        }

        public bool Contains(T item)
        {
            return Enumerable.Contains(this, item);
        }

        void ICollection<T>.CopyTo(T[] array, int startIndex)
        {
            for (int i = 0; i < Length; ++i)
                array.SetValue(this[i], startIndex + i);
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        int ICollection<T>.Count
        {
            get
            {
                return Length;
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
