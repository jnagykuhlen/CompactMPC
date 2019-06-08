using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC.Buffers;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class BufferBuilderTest
    {
        [TestMethod]
        public void TestCombineBufferWithIds()
        {
            byte[] prefix = new byte[] { 1, 3, 5, 228, 113 };
            int firstId = 12312345;
            int secondId = 127;
            byte[] suffix = new byte[] { 43, 31, 8 };


            byte[] result = BufferBuilder.From(prefix).With(firstId).With(secondId).With(suffix).Create();

            Assert.AreEqual(prefix.Length + suffix.Length + 8, result.Length);
            Assert.IsTrue(Enumerable.SequenceEqual(prefix, result.Take(prefix.Length)));
            Assert.IsTrue(Enumerable.SequenceEqual(BitConverter.GetBytes(firstId), result.Skip(prefix.Length).Take(4)));
            Assert.IsTrue(Enumerable.SequenceEqual(BitConverter.GetBytes(secondId), result.Skip(prefix.Length + 4).Take(4)));
            Assert.IsTrue(Enumerable.SequenceEqual(suffix, result.Skip(prefix.Length + 8)));
        }
    }
}
