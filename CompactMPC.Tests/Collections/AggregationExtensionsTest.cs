using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Collections;

[TestClass]
public class AggregationExtensionsTest
{
    [TestMethod]
    public void TestAggregateDepthEfficientAggregatesSingleValue()
    {
        IReadOnlyList<int> source = [3];
        var result = source.AggregateDepthEfficient((x, y) => x + y);
        result.Should().Be(3);
    }

    [TestMethod]
    public void TestAggregateDepthEfficientAggregatesEvenNumberOfValues()
    {
        IReadOnlyList<int> source = [1, 3, 5, 4];
        var result = source.AggregateDepthEfficient((x, y) => x + y);
        result.Should().Be(13);
    }

    [TestMethod]
    public void TestAggregateDepthEfficientAggregatesOddNumberOfValues()
    {
        IReadOnlyList<int> source = [8, 3, 5, 4, 1];
        var result = source.AggregateDepthEfficient((x, y) => x + y);
        result.Should().Be(21);
    }

    [TestMethod]
    public void TestAggregateDepthEfficientDoesNotAggregateEmptyList()
    {
        IReadOnlyList<int> source = [];
        Action aggregate = () => source.AggregateDepthEfficient((x, y) => x + y);
        aggregate.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void TestAggregateDepthEfficientAggregatesWithMinimumDepth()
    {
        IReadOnlyList<int> source = [0, 0, 0, 0, 0, 0, 0];
        var depth = source.AggregateDepthEfficient((x, y) => Math.Max(x, y) + 1);
        depth.Should().Be(3);
    }
}
