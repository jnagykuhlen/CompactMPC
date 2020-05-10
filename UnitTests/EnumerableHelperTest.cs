using System.Collections.Generic;
using CompactMPC.UnitTests.Assertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.UnitTests
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

            IEnumerable<int> flattened = source.Flatten();
            
            EnumerableAssert.AreEqual(new[] { 1, 3, 7, 2, 4 }, flattened);
        }
    }
}
