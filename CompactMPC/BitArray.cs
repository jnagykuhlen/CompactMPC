using System;
using System.Collections.Generic;

namespace CompactMPC;

public class BitArray : PackedArray<Bit>
{
    private const int ElementsPerByte = 8;
    private const int BitMask = 0x1;

    public BitArray(int numberOfElements)
        : base(numberOfElements, ElementsPerByte) { }

    public BitArray(IReadOnlyList<Bit> elements)
        : base(elements.Count, ElementsPerByte)
    {
        for (var i = 0; i < elements.Count; ++i)
            WriteElement(elements[i], i);
    }

    private BitArray(byte[] bytes, int numberOfElements, int elementsPerByte)
        : base(bytes, numberOfElements, elementsPerByte) { }

    public static BitArray FromBytes(byte[] bytes, int numberOfElements) =>
        new(bytes, numberOfElements, ElementsPerByte);

    public static int RequiredBytes(int numberOfBits) => RequiredBytes(numberOfBits, ElementsPerByte);

    public void Or(BitArray other)
    {
        if (other.Length != Length)
            throw new ArgumentException("Bit array length does not match.", nameof(other));

        for (var i = 0; i < Buffer.Length; ++i)
            Buffer[i] |= other.Buffer[i];
    }

    public BitArray Xor(BitArray other)
    {
        if (other.Length != Length)
            throw new ArgumentException("Bit array length does not match.", nameof(other));

        for (var i = 0; i < Buffer.Length; ++i)
            Buffer[i] ^= other.Buffer[i];

        return this;
    }

    public BitArray And(BitArray other)
    {
        if (other.Length != Length)
            throw new ArgumentException("Bit array length does not match.", nameof(other));

        for (var i = 0; i < Buffer.Length; ++i)
            Buffer[i] &= other.Buffer[i];

        return this;
    }

    public BitArray Not()
    {
        for (var i = 0; i < Buffer.Length; ++i)
            Buffer[i] = (byte)~Buffer[i];

        return this;
    }

    public BitArray Xor(IReadOnlyList<BitArray> bitArrays)
    {
        foreach (var bitArray in bitArrays)
            Xor(bitArray);

        return this;
    }

    public static BitArray FromXor(IReadOnlyList<BitArray> bitArrays)
    {
        if (bitArrays.Count == 0)
            throw new ArgumentException("Bit array list is empty.", nameof(bitArrays));

        return new BitArray(bitArrays[0].Length).Xor(bitArrays);
    }

    public static BitArray FromBinaryString(string bitString)
    {
        var result = new BitArray(bitString.Length);
        for (var i = 0; i < bitString.Length; ++i)
        {
            if (bitString[i] != '0' && bitString[i] != '1')
                throw new ArgumentException("Binary string is only allowed to contain characters 0 and 1.", nameof(bitString));

            result[i] = new Bit(bitString[i] == '1');
        }

        return result;
    }

    public string ToBinaryString()
    {
        var characters = new char[Length];
        for (var i = 0; i < Length; ++i)
            characters[i] = ReadElement(i).IsSet ? '1' : '0';

        return new string(characters);
    }

    public override string ToString() => ToBinaryString();

    protected override Bit ReadElement(int index) => new(ReadBits(index, ElementsPerByte, BitMask));

    protected sealed override void WriteElement(Bit value, int index) => WriteBits((byte)value, index, ElementsPerByte, BitMask);
}
