using System;
using System.Collections.Generic;

namespace CompactMPC;

public class QuadrupleIndexArray : PackedArray<int>
{
    private const int ElementsPerByte = 4;
    private const int BitMask = 0x3;

    public QuadrupleIndexArray(int numberOfElements)
        : base(numberOfElements, ElementsPerByte) { }

    public QuadrupleIndexArray(IReadOnlyList<int> elements)
        : base(elements.Count, ElementsPerByte)
    {
        for (var i = 0; i < elements.Count; ++i)
            WriteElement(elements[i], i);
    }

    private QuadrupleIndexArray(byte[] bytes, int numberOfElements, int elementsPerByte)
        : base(bytes, numberOfElements, elementsPerByte) { }
        
    public static QuadrupleIndexArray FromBytes(byte[] bytes, int numberOfElements) =>
        new(bytes, numberOfElements, ElementsPerByte);

    public static int RequiredBytes(int numberOfElements) => RequiredBytes(numberOfElements, ElementsPerByte);

    protected override int ReadElement(int index) => ReadBits(index, ElementsPerByte, BitMask);

    protected sealed override void WriteElement(int value, int index)
    {
        if (value is < 0 or >= 4)
            throw new ArgumentOutOfRangeException(nameof(value), "Quadruple index must be in the range from 0 to 3.");

        WriteBits((byte)value, index, ElementsPerByte, BitMask);
    }
}
