using System;
using System.Collections.Generic;
using System.Text;

namespace CompactMPC.Cryptography
{
    public interface IRandomOracleProvider
    {
        RandomOracle Create();
    }
}
