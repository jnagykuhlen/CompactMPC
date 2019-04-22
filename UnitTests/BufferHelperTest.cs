using System;
using System.Text;
using System.Linq;
using System.Collections;
using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using CompactMPC;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class BufferHelperTest
    {
        [TestMethod]
        public void TestCombineBufferWithIds()
        {
            byte[] buffer = new byte[] { 1, 3, 5, 132, 228 };
            int firstId = 12312345;
            int secondId = 127;
            
            byte[] result = BufferHelper.Combine(buffer, firstId, secondId);

            Assert.AreEqual(buffer.Length + 8, result.Length);
            Assert.IsTrue(Enumerable.SequenceEqual(buffer, result.Take(buffer.Length)));
            Assert.IsTrue(Enumerable.SequenceEqual(BitConverter.GetBytes(firstId), result.Skip(buffer.Length).Take(4)));
            Assert.IsTrue(Enumerable.SequenceEqual(BitConverter.GetBytes(secondId), result.Skip(buffer.Length + 4).Take(4)));
        }
    }
}
