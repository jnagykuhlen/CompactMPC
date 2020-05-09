using System;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.Expressions
{
    public static class AggregationHelper
    {
        public static T AggregateDepthEfficient<T>(this IReadOnlyCollection<T> source, Func<T, T, T> func)
        {
            return AggregateDepthEfficient(source, func, source.Count);
        }

        private static T AggregateDepthEfficient<T>(IEnumerable<T> source, Func<T, T, T> func, int count)
        {
            switch (count)
            {
                case 0:
                    throw new ArgumentException("Aggregation requires at least one operand.", nameof(source));
                case 1:
                    return source.First();
                default:
                    return AggregateDepthEfficient(Reduce(source, func), func, (count + 1) / 2);
            }
        }

        private static IEnumerable<T> Reduce<T>(IEnumerable<T> source, Func<T, T, T> func)
        {
            T previous = default!;
            bool parity = false;

            foreach (T current in source)
            {
                if (parity)
                    yield return func(previous, current);

                previous = current;
                parity = !parity;
            }

            if (parity)
                yield return previous;
        }
    }
}
