namespace CompactMPC.Cryptography
{
    public interface IRandomOracleProvider
    {
        RandomOracle Create();
    }
}
