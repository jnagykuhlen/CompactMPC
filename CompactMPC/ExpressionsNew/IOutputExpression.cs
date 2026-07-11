using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew;

public interface IOutputExpression<out T> : IExpression
{
    T ReadFrom(IReadOnlyList<Bit> source);
}
