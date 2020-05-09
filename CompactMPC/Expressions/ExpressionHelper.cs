using System;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.Expressions
{
    public static class ExpressionHelper
    {
        public static T AggregateDepthEfficient<T>(this IReadOnlyCollection<T> values, Func<T, T, T> func)
        {
            return AggregateDepthEfficient(values, func, values.Count);
        }

        private static T AggregateDepthEfficient<T>(IEnumerable<T> values, Func<T, T, T> func, int count)
        {
            if (count == 0)
                throw new ArgumentException("Aggregation requires at least one operand.", nameof(values));

            if (count == 1)
                return values.First();
            
            return AggregateDepthEfficient(Reduce(values, func), func, (count + 1) / 2);
        }

        private static IEnumerable<T> Reduce<T>(IEnumerable<T> values, Func<T, T, T> func)
        {
            T previous = default!;
            bool parity = false;

            foreach (T current in values)
            {
                if (parity)
                {
                    yield return func(previous, current);
                }

                previous = current;
                parity = !parity;
            }

            if (parity)
                yield return previous;
        }
    }
}
