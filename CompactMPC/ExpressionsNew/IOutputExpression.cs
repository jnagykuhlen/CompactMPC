using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew
{
    public interface IOutputExpression<out T> : IExpression
    {
        T FromBits(IReadOnlyList<Bit> bits);
    }
}