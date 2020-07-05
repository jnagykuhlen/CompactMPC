using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class QuadrupleIndexArrayTest
    {
        [TestMethod]
        public void TestByteConversion()
        {
            QuadrupleIndexArray indexArray = new QuadrupleIndexArray(new[] {3, 1, 2, 1, 3, 0, 1})
            {
                [2] = 0,
                [5] = 2
            };

            indexArray.Should().HaveCount(7);
            indexArray.ToBytes().Should().Equal(0b01000111, 0b00011011);
        }

        [TestMethod]
        public void TestRequiredBytes()
        {
            QuadrupleIndexArray.RequiredBytes(0).Should().Be(0);
            QuadrupleIndexArray.RequiredBytes(1).Should().Be(1);
            QuadrupleIndexArray.RequiredBytes(2).Should().Be(1);
            QuadrupleIndexArray.RequiredBytes(3).Should().Be(1);
            QuadrupleIndexArray.RequiredBytes(4).Should().Be(1);
            QuadrupleIndexArray.RequiredBytes(5).Should().Be(2);
        }
    }
}
