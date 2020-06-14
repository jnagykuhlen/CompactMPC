using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Assertions
{
    public static class EnumerableAssert
    {
        public static void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            CollectionAssert.AreEqual(expected.ToList(), actual.ToList());
        }

        public static void AreNotEqual<T>(IEnumerable<T> notExpected, IEnumerable<T> actual)
        {
            CollectionAssert.AreNotEqual(notExpected.ToList(), actual.ToList());
        }

        public static void AreEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            CollectionAssert.AreEquivalent(expected.ToList(), actual.ToList());
        }
    }
}
