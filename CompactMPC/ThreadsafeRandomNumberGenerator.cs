using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CompactMPC
{
    public class ThreadsafeRandomNumberGenerator : RandomNumberGenerator
    {
        private RandomNumberGenerator _randomNumberGenerator;
        private object _randomNumberGeneratorLock;

        public ThreadsafeRandomNumberGenerator(RandomNumberGenerator randomNumberGenerator)
        {
            _randomNumberGenerator = randomNumberGenerator;
            _randomNumberGeneratorLock = new object();
        }

        public override void GetBytes(byte[] data)
        {
            lock (_randomNumberGeneratorLock)
            {
                _randomNumberGenerator.GetBytes(data);
            }
        }
    }
}
