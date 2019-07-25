using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CompactMPC
{
    public class ThreadsafeSHA256 : ThreadsafeHashAlgorithm
    {
        protected override HashAlgorithm CreateHashAlgorithm()
        {
            return SHA256.Create();
        }
    }
}
