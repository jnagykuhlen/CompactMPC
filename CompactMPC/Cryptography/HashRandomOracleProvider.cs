namespace CompactMPC.Cryptography
{
    public class HashRandomOracleProvider : IRandomOracleProvider
    {
        private readonly IHashAlgorithmProvider _hashAlgorithmProvider;

        public HashRandomOracleProvider(IHashAlgorithmProvider hashAlgorithmProvider)
        {
            _hashAlgorithmProvider = hashAlgorithmProvider;
        }

        public RandomOracle Create()
        {
            return new HashRandomOracle(_hashAlgorithmProvider);
        }
    }
}
