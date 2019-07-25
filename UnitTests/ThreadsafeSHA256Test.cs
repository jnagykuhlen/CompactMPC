using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CompactMPC.UnitTests
{
    [TestClass]
    public class ThreadsafeSHA256Test
    {
        [TestMethod]
        public void TestConcurrency()
        {
            ThreadsafeHashAlgorithm hash = new ThreadsafeSHA256();
            byte[] data = new[] { (byte)0x34, (byte)0x2f, (byte)0xab, (byte)0x25, (byte)0x33 };
            byte[] expected = hash.ComputeHash(data);

            int count = 100;
            Parallel.For(0, 2, i =>
            {
                for (int j = 0; j < count; ++j)
                {
                    byte[] result = hash.ComputeHash(data);
                    CollectionAssert.AreEqual(expected, result);
                }
            });
        }
    }
}
