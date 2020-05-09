using System;
using System.Net;
using CompactMPC.Networking;

namespace CompactMPC.UnitTests.Util
{
    public static class TestNetworkSession
    {
        private const int Port = 16741;

        public static IMultiPartyNetworkSession EstablishMultiParty(int localPartyId, int numberOfParties)
        {
            return TcpMultiPartyNetworkSession.EstablishAsync(new Party(localPartyId), IPAddress.Loopback, Port, numberOfParties).Result;
        }

        public static ITwoPartyNetworkSession EstablishTwoParty()
        {
            try
            {
                return TcpTwoPartyNetworkSession.AcceptAsync(new Party(1), Port).Result;
            }
            catch (Exception)
            {
                return TcpTwoPartyNetworkSession.ConnectAsync(new Party(0), IPAddress.Loopback, Port).Result;
            }
        }
    }
}
