using System.Collections.Generic;

namespace CompactMPC;

public class BitQuadrupleArray : PackedArray<BitQuadruple>
{
    private const int ElementsPerByte = 2;
    private const int BitMask = 0xf;

    public BitQuadrupleArray(int numberOfElements)
        : base(numberOfElements, ElementsPerByte) { }

    public BitQuadrupleArray(IReadOnlyList<BitQuadruple> elements)
        : base(elements.Count, ElementsPerByte)
    {
        for (var i = 0; i < elements.Count; ++i)
            WriteElement(elements[i], i);
    }
        
    private BitQuadrupleArray(byte[] bytes, int numberOfElements, int elementsPerByte)
        : base(bytes, numberOfElements, elementsPerByte) { }

    public static BitQuadrupleArray FromBytes(byte[] bytes, int numberOfElements) =>
        new(bytes, numberOfElements, ElementsPerByte);

    public static int RequiredBytes(int numberOfElements) =>
        RequiredBytes(numberOfElements, ElementsPerByte);

    protected override BitQuadruple ReadElement(int index) =>
        BitQuadruple.FromPackedValue(ReadBits(index, ElementsPerByte, BitMask));

    protected sealed override void WriteElement(BitQuadruple value, int index) =>
        WriteBits(value.PackedValue, index, ElementsPerByte, BitMask);
}
