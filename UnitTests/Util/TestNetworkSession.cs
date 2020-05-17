using System;
using CompactMPC.Networking;

namespace CompactMPC.UnitTests.Util
{
    public static class TestNetworkSession
    {
        private const int Port = 16741;

        public static IMultiPartyNetworkSession EstablishMultiParty(int localPartyId, int numberOfParties)
        {
            return TcpMultiPartyNetworkSession.EstablishLoopbackAsync(new Party(localPartyId), Port, numberOfParties).Result;
        }

        public static ITwoPartyNetworkSession EstablishTwoParty()
        {
            try
            {
                return TcpTwoPartyNetworkSession.AcceptLoopbackAsync(new Party(1), Port).Result;
            }
            catch (Exception)
            {
                return TcpTwoPartyNetworkSession.ConnectLoopbackAsync(new Party(0), Port).Result;
            }
        }
    }
}
