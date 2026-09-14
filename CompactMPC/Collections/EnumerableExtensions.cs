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

    public static Dictionary<TKey, TValueTarget> ToDictionary<TKey, TValueSource, TValueTarget>(this IEnumerable<KeyValuePair<TKey, TValueSource>> source, Func<TValueSource, TValueTarget> valueSelector) where TKey : notnull =>
        source.ToDictionary(keyValuePair => keyValuePair.Key, keyValuePair => valueSelector(keyValuePair.Value));

    public static IReadOnlyDictionary<TSource, TTarget> Match<TSource, TTarget>(this IEnumerable<TSource> source, IEnumerable<TTarget> target, Func<TSource, TTarget, bool> matchPredicate)
        where TSource : notnull
    {
        var targetItems = target.ToHashSet();
        var matches = source.ToDictionary(
            item => item,
            item =>
            {
                var match = targetItems.FirstOrDefault(targetItem => matchPredicate(item, targetItem)) ??
                            throw new ArgumentException("No matching target item found for source item.", nameof(source));

                targetItems.Remove(match);
                return match;
            }
        );

        if (targetItems.Count > 0)
            throw new ArgumentException("Some target items were not matched to any source item.", nameof(target));

        return matches;
    }
}
