using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew
{
    public interface IBitConverter<T>
    {
        IReadOnlyList<Bit> ToBits(T value, int numberOfBits);
        T FromBits(IReadOnlyList<Bit> bits);
    }
}
