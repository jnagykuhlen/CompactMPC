using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public class TcpMultiPartyNetworkSession : IMultiPartyNetworkSession
    {
        private readonly ITwoPartyNetworkSession[] _remotePartySessions;
        
        public Party LocalParty { get; }

        private TcpMultiPartyNetworkSession(Party localParty, ITwoPartyNetworkSession[] remotePartySessions)
        {
            _remotePartySessions = remotePartySessions;
            LocalParty = localParty;
        }

        public static async Task<TcpMultiPartyNetworkSession> EstablishAsync(Party localParty, IPAddress address, int startPort, int numberOfParties)
        {
            ITwoPartyNetworkSession[] remotePartySessions = new ITwoPartyNetworkSession[numberOfParties - 1];

            for (int i = 0; i < localParty.Id; ++i)
            {
                remotePartySessions[i] = await TcpTwoPartyNetworkSession.ConnectAsync(localParty, address, startPort + i);
            }

            TcpListener listener = new TcpListener(IPAddress.Any, startPort + localParty.Id) { ExclusiveAddressUse = true };
            listener.Start();

            for (int j = localParty.Id + 1; j < numberOfParties; ++j)
            {
                remotePartySessions[j - 1] = await TcpTwoPartyNetworkSession.AcceptAsync(localParty, listener);
            }

            listener.Stop();

            for (int i = 0; i < numberOfParties; ++i)
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
                return _remotePartySessions.Length + 1;
            }
        }
    }
}
