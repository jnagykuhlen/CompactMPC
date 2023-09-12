using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class EnumerableHelperTest
    {
        [TestMethod]
        public void TestFlatten()
        {
            IEnumerable<IEnumerable<int>> source = new[]
            {
                new[] { 1, 3, 7},
                new[] { 2, 4 }
            };

            source.Flatten().Should().Equal(1, 3, 7, 2, 4);
        }

        [TestMethod]
        public void TestJoin()
        {
            IEnumerable<string> inner = new[] { "rat", "rabbit", "mouse" };
            IEnumerable<string> outer = new[] { "apple", "pie", "cake" };

            inner.Join(outer, value => value.Length, (first, second) => first + second).Should().Equal("ratpie", "mouseapple");
        }

        [TestMethod]
        public void TestWithout()
        {
            IEnumerable<int> source = new[] { 1, 3, 7, 2, 3};

            source.Without(3).Should().Equal(1, 7, 2);
        }
    }
}
