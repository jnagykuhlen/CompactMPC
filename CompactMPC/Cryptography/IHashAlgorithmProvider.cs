using System.Security.Cryptography;

namespace CompactMPC.Cryptography
{
    public interface IHashAlgorithmProvider
    {
        HashAlgorithm Create();
    }
}
