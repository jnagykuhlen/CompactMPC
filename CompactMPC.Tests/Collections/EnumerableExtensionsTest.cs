using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.Collections;

[TestClass]
public class EnumerableExtensionsTest
{
    [TestMethod]
    public void TestFlatten()
    {
        IEnumerable<IEnumerable<int>> source =
        [
            [1, 3, 7],
            [2, 4]
        ];

        source.Flatten().Should().Equal(1, 3, 7, 2, 4);
    }

    [TestMethod]
    public void TestJoin()
    {
        IEnumerable<string> inner = ["rat", "rabbit", "mouse"];
        IEnumerable<string> outer = ["apple", "pie", "cake"];

        inner.Join(outer, value => value.Length, (first, second) => first + second)
            .Should().Equal("ratpie", "mouseapple");
    }

    [TestMethod]
    public void TestWithout()
    {
        IEnumerable<int> source = [1, 3, 7, 2, 3];
        source.Without(3).Should().Equal(1, 7, 2);
    }

    [TestMethod]
    public void TestMatchWithSuccessfulAssignment()
    {
        IEnumerable<int> source = [1, 2, 3];
        IEnumerable<string> target = ["c", "bb", "aaa"];

        var matches = source.Match(
            target,
            (sourceItem, targetItem) => sourceItem == targetItem.Length
        );

        matches.Should().Equal(
            new Dictionary<int, string>
            {
                { 1, "c" },
                { 2, "bb" },
                { 3, "aaa" }
            }
        );
    }

    [TestMethod] public void TestMatchWithMissingTarget()
    {
        IEnumerable<int> source = [1, 2, 3];
        IEnumerable<string> target = ["c", "bb", "aaaa"];

        var matchAction = () => source.Match(
            target,
            (sourceItem, targetItem) => sourceItem == targetItem.Length
        );

        matchAction.Should().Throw<ArgumentException>();
    }
    
    [TestMethod] public void TestMatchWithExtraSource()
    {
        IEnumerable<int> source = [1, 2, 3, 4];
        IEnumerable<string> target = ["c", "bb", "aaaa"];

        var matchAction = () => source.Match(
            target,
            (sourceItem, targetItem) => sourceItem == targetItem.Length
        );

        matchAction.Should().Throw<ArgumentException>();
    }
    
    [TestMethod] public void TestMatchWithExtraTarget()
    {
        IEnumerable<int> source = [1, 2, 3];
        IEnumerable<string> target = ["c", "bb", "aaa", "dddd"];

        var matchAction = () => source.Match(
            target,
            (sourceItem, targetItem) => sourceItem == targetItem.Length
        );

        matchAction.Should().Throw<ArgumentException>();
    }
}
