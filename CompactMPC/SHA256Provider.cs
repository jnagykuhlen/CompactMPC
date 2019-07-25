using System;
using System.Collections.Generic;
using System.Text;

namespace CompactMPC
{
    public class SHA256Provider : IHashAlgorithmProvider
    {
        public ThreadsafeHashAlgorithm CreateHashAlgorithm()
        {
            return new ThreadsafeSHA256();
        }
    }
}
