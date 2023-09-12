using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew
{
    public interface IInputExpression<in T> : IExpression
    {
        IReadOnlyList<Bit> ToBits(T value);
    }
}