using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public class TcpMultiPartyNetworkSession : IMultiPartyNetworkSession
    {
        private readonly IReadOnlyList<ITwoPartyNetworkSession> _remotePartySessions;

        public Party LocalParty { get; }

        private TcpMultiPartyNetworkSession(Party localParty, IReadOnlyList<ITwoPartyNetworkSession> remotePartySessions)
        {
            _remotePartySessions = remotePartySessions;
            LocalParty = localParty;
        }

        public static async Task<TcpMultiPartyNetworkSession> EstablishAsync(Party localParty, IPEndPoint localEndPoint, IPEndPoint[] remoteEndPoints)
        {
            TcpTwoPartyNetworkSession[] remotePartySessions = await TcpTwoPartyNetworkSession.EstablishAsync(localParty, localEndPoint, remoteEndPoints);
            return new TcpMultiPartyNetworkSession(localParty, remotePartySessions);
        }

        public void Dispose()
        {
            foreach (ITwoPartyNetworkSession remotePartySession in _remotePartySessions)
                remotePartySession.Dispose();
        }

        public IEnumerable<ITwoPartyNetworkSession> RemotePartySessions
        {
            get
            {
                return _remotePartySessions;
            }
        }

        public int NumberOfParties
        {
            get
            {
                return _remotePartySessions.Count + 1;
            }
        }
    }
}
