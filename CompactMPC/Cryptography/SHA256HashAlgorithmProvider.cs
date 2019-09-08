using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CompactMPC.Cryptography
{
    public class SHA256HashAlgorithmProvider : IHashAlgorithmProvider
    {
        public HashAlgorithm Create()
        {
            return SHA256.Create();
        }
    }
}
