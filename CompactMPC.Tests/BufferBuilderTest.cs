using System;
using System.Linq;
using CompactMPC.Buffers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC
{
    [TestClass]
    public class BufferBuilderTest
    {
        [TestMethod]
        public void TestCombineBufferWithIds()
        {
            byte[] prefix = { 1, 3, 5, 228, 113 };
            int firstId = 12312345;
            int secondId = 127;
            byte[] suffix = { 43, 31, 8 };

            byte[] result = BufferBuilder.From(prefix).With(firstId).With(secondId).With(suffix).Create();

            Assert.AreEqual(prefix.Length + suffix.Length + 8, result.Length);
            Assert.IsTrue(prefix.SequenceEqual(result.Take(prefix.Length)));
            Assert.IsTrue(BitConverter.GetBytes(firstId).SequenceEqual(result.Skip(prefix.Length).Take(4)));
            Assert.IsTrue(BitConverter.GetBytes(secondId).SequenceEqual(result.Skip(prefix.Length + 4).Take(4)));
            Assert.IsTrue(suffix.SequenceEqual(result.Skip(prefix.Length + 8)));
        }
    }
}
