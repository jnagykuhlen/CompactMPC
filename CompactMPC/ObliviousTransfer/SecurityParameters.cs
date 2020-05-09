using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace CompactMPC.ObliviousTransfer
{
    public class SecurityParameters
    {
        public BigInteger P { get; }
        public BigInteger Q { get; }
        public BigInteger G { get; }
        public int GroupElementSize { get; }
        public int ExponentSize { get; }
        
        public SecurityParameters(BigInteger p, BigInteger q, BigInteger g, int groupElementSize, int exponentSize)
        {
            P = p;
            Q = q;
            G = g;
            GroupElementSize = groupElementSize;
            ExponentSize = exponentSize;
        }

        public static SecurityParameters CreateDefault2048Bit()
        {
            // Recommendation from RFC 3526, 2048-bit MODP group, id 14
            const string primeHex = @"0
                FFFFFFFF FFFFFFFF C90FDAA2 2168C234 C4C6628B 80DC1CD1
                29024E08 8A67CC74 020BBEA6 3B139B22 514A0879 8E3404DD
                EF9519B3 CD3A431B 302B0A6D F25F1437 4FE1356D 6D51C245
                E485B576 625E7EC6 F44C42E9 A637ED6B 0BFF5CB6 F406B7ED
                EE386BFB 5A899FA5 AE9F2411 7C4B1FE6 49286651 ECE45B3D
                C2007CB8 A163BF05 98DA4836 1C55D39A 69163FA8 FD24CF5F
                83655D23 DCA3AD96 1C62F356 208552BB 9ED52907 7096966D
                670C354E 4ABC9804 F1746C08 CA18217C 32905E46 2E36CE3B
                E39E772C 180E8603 9B2783A2 EC07A28F B5C55DF0 6F4C52C9
                DE2BCBF6 95581718 3995497C EA956AE5 15D22618 98FA0510
                15728E5A 8AACAA68 FFFFFFFF FFFFFFFF";
            
            BigInteger p = BigInteger.Parse(Regex.Replace(primeHex, @"\s+", ""), NumberStyles.AllowHexSpecifier);
            BigInteger q = (p - 1) / 2;
            BigInteger g = 4;
            int groupElementSize = 2048 / 8;
            int exponentSize = 256 / 8;

            return new SecurityParameters(p, q, g, groupElementSize, exponentSize);
        }

        public static SecurityParameters CreateDefault768Bit()
        {
            // Recommendation from RFC 2409, 768-bit MODP group, id 1
            const string primeHex = @"0
                FFFFFFFF FFFFFFFF C90FDAA2 2168C234 C4C6628B 80DC1CD1
                29024E08 8A67CC74 020BBEA6 3B139B22 514A0879 8E3404DD
                EF9519B3 CD3A431B 302B0A6D F25F1437 4FE1356D 6D51C245
                E485B576 625E7EC6 F44C42E9 A63A3620 FFFFFFFF FFFFFFFF";
                
            BigInteger p = BigInteger.Parse(Regex.Replace(primeHex, @"\s+", ""), NumberStyles.AllowHexSpecifier);
            BigInteger q = (p - 1) / 2;
            BigInteger g = 4;
            int groupElementSize = 768 / 8;
            int exponentSize = 256 / 8;

            return new SecurityParameters(p, q, g, groupElementSize, exponentSize);
        }
    }
}
