using System;
using System.Collections.Generic;
using System.Text;

namespace CompactMPC
{
    public interface IHashAlgorithmProvider
    {
        ThreadsafeHashAlgorithm CreateHashAlgorithm();
    }
}
