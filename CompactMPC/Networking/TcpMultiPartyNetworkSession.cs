using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace CompactMPC.Networking
{
    public class TcpMultiPartyNetworkSession : IMultiPartyNetworkSession
    {
        private ITwoPartyNetworkSession[] _remotePartySessions;
        private Party _localParty;

        private TcpMultiPartyNetworkSession(Party localParty, ITwoPartyNetworkSession[] remotePartySessions)
        {
            _remotePartySessions = remotePartySessions;
            _localParty = localParty;
        }

        public static async Task<TcpMultiPartyNetworkSession> EstablishAsync(Party localParty, IPAddress address, int startPort, int numberOfParties)
        {
            TcpTwoPartyNetworkSession[] remotePartySessions = new TcpTwoPartyNetworkSession[numberOfParties - 1];

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
                if (i != localParty.Id && !remotePartySessions.Any(session => session.RemoteParty.Id == i))
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

        public Party LocalParty
        {
            get
            {
                return _localParty;
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
