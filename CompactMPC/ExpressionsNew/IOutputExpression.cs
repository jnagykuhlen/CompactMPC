using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew
{
    public interface IOutputExpression<T> : IExpression
    {
        T FromBits(IReadOnlyList<Bit> bits);
    }
}