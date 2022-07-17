using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew
{
    public interface IInputExpression<T> : IExpression
    {
        IReadOnlyList<Bit> ToBits(T value);
    }
}