using System.Collections.Generic;

namespace CompactMPC.ExpressionsNew;

public interface IOutputExpression<out T> : IExpression
{
    T ReadValue(IReadOnlyList<Bit> source, int position);
}
