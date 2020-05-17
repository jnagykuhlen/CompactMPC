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

        public static Task<TcpMultiPartyNetworkSession> EstablishLoopbackAsync(Party localParty, int startPort, int numberOfParties)
        {
            IPEndPoint[] endpoints = Enumerable
                .Range(startPort, numberOfParties)
                .Select(port => new IPEndPoint(IPAddress.Loopback, port))
                .ToArray();

            return EstablishAsync(localParty, endpoints);
        }

        public static async Task<TcpMultiPartyNetworkSession> EstablishAsync(Party localParty, IPEndPoint[] endPoints)
        {
            if (localParty.Id < 0 || localParty.Id >= endPoints.Length)
                throw new ArgumentException($"Local party id {localParty.Id} is out of range.");

            IPEndPoint localEndPoint = endPoints[localParty.Id];

            TcpTwoPartyNetworkSession[] outgoingSessions = await TcpTwoPartyNetworkSession.ConnectAsync(
                localParty,
                endPoints,
                localParty.Id
            );

            TcpTwoPartyNetworkSession[] incomingSessions = await TcpTwoPartyNetworkSession.AcceptAsync(
                localParty,
                localEndPoint,
                endPoints.Length - localParty.Id - 1
            );

            List<ITwoPartyNetworkSession> remotePartySessions = new List<ITwoPartyNetworkSession>(endPoints.Length - 1);
            remotePartySessions.AddRange(outgoingSessions);
            remotePartySessions.AddRange(incomingSessions);

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
