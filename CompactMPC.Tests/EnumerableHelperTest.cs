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
    }
}
