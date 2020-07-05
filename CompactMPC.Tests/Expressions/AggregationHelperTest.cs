using System;
using FluentAssertions;
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
            result.Should().Be(3);
        }
        
        [TestMethod]
        public void TestAggregateDepthEfficientAggregatesEvenNumberOfValues()
        {
            int result = new[] { 1, 3, 5, 4 }.AggregateDepthEfficient((x, y) => x + y);
            result.Should().Be(13);
        }
        
        [TestMethod]
        public void TestAggregateDepthEfficientAggregatesOddNumberOfValues()
        {
            int result = new[] { 8, 3, 5, 4, 1 }.AggregateDepthEfficient((x, y) => x + y);
            result.Should().Be(21);
        }
        
        [TestMethod]
        public void TestAggregateDepthEfficientDoesNotAggregateZeroValues()
        {
           Action aggregate = () => new int[] { }.AggregateDepthEfficient((x, y) => x + y);
           aggregate.Should().Throw<ArgumentException>();
        }
        
        [TestMethod]
        public void TestAggregateDepthEfficientAggregatesWithMinimumDepth()
        {
            int depth = new[] { 0, 0, 0, 0, 0, 0, 0 }.AggregateDepthEfficient((x, y) => Math.Max(x, y) + 1);
            depth.Should().Be(3);
        }
    }
}
