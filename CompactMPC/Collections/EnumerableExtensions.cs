using System;
using System.Collections.Generic;
using System.Linq;

namespace CompactMPC.Collections;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source) =>
        source.SelectMany(item => item);

    public static IEnumerable<TResult> Join<T, TKey, TResult>(this IEnumerable<T> outer, IEnumerable<T> inner, Func<T, TKey> keySelector, Func<T, T, TResult> resultSelector) =>
        outer.Join(inner, keySelector, keySelector, resultSelector);

    public static IEnumerable<T> Without<T>(this IEnumerable<T> source, T elementToExclude) =>
        source.Where(element => !Equals(element, elementToExclude));
}
