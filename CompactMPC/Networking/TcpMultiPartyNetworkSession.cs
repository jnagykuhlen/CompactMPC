using System;
using System.Collections.Generic;
using System.Linq;
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

        public static async Task<TcpMultiPartyNetworkSession> EstablishAsync(Party localParty, IPEndPoint[] endPoints)
        {
            if (localParty.Id < 0 || localParty.Id >= endPoints.Length)
                throw new ArgumentException($"Local party id {localParty.Id} is out of range.");

            IPEndPoint localEndPoint = endPoints[localParty.Id];

            using ITwoPartyConnectionListener listener = TcpTwoPartyNetworkSession.CreateListener(localParty, localEndPoint);
            
            int numberOfConnects = localParty.Id;
            TcpTwoPartyNetworkSession[] remotePartySessions = await Task.WhenAll(Enumerable.Concat(
                endPoints
                    .Take(numberOfConnects)
                    .Select(endPoint => TcpTwoPartyNetworkSession.ConnectAsync(localParty, endPoint)),
                endPoints
                    .Skip(numberOfConnects + 1)
                    .Select(_ => listener.AcceptAsync())
            ));

            for (int i = 0; i < endPoints.Length; ++i)
            {
                if (i != localParty.Id && remotePartySessions.All(session => session.RemoteParty.Id != i))
                    throw new NetworkConsistencyException("Inconsistent TCP connection graph.");
            }

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
