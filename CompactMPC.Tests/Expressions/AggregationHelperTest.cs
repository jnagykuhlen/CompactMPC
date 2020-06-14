using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Expressions
{
    [TestClass]
    public class AggregationHelperTest
    {
        [TestMethod]
        public void TestAggregateDepthEfficientAggregatesSingleValue()
        {
            int result = new[] { 3 }.AggregateDepthEfficient((x, y) => x + y);

            Assert.AreEqual(3, result);
        }
        
        [TestMethod]
        public void TestAggregateDepthEfficientAggregatesEvenNumberOfValues()
        {
            int result = new[] { 1, 3, 5, 4 }.AggregateDepthEfficient((x, y) => x + y);
            
            Assert.AreEqual(13, result);
        }
        
        [TestMethod]
        public void TestAggregateDepthEfficientAggregatesOddNumberOfValues()
        {
            int result = new[] { 8, 3, 5, 4, 1 }.AggregateDepthEfficient((x, y) => x + y);
            
            Assert.AreEqual(21, result);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAggregateDepthEfficientDoesNotAggregateZeroValues()
        {
            new int[] { }.AggregateDepthEfficient((x, y) => x + y);
        }
        
        [TestMethod]
        public void TestAggregateDepthEfficientAggregatesWithMinimumDepth()
        {
            int depth = new[] { 0, 0, 0, 0, 0, 0, 0 }.AggregateDepthEfficient((x, y) => Math.Max(x, y) + 1);

            Assert.AreEqual(3, depth);
        }
    }
}
