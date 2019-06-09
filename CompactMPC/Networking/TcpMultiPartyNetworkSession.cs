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
        private List<ITwoPartyNetworkSession> _remotePartySessions;
        private Party _localParty;
        private int _numberOfParties;

        public TcpMultiPartyNetworkSession(int startPort, int numberOfParties, int localPartyId)
        {
            _remotePartySessions = new List<ITwoPartyNetworkSession>(numberOfParties - 1);
            _localParty = new Party(localPartyId, "Party " + (localPartyId + 1));
            _numberOfParties = numberOfParties;

            for (int i = 0; i < localPartyId; ++i)
            {
                TcpClient client = new TcpClient();
                client.ConnectAsync("127.0.0.1", startPort + i).Wait();
                _remotePartySessions.Add(new TcpTwoPartyNetworkSession(client, _localParty));
            }

            TcpListener listener = new TcpListener(IPAddress.Any, startPort + localPartyId) { ExclusiveAddressUse = true };
            listener.Start();

            for (int j = localPartyId + 1; j < numberOfParties; ++j)
            {
                TcpClient client = listener.AcceptTcpClientAsync().Result;
                _remotePartySessions.Add(new TcpTwoPartyNetworkSession(client, _localParty));
            }

            listener.Stop();

            for (int i = 0; i < numberOfParties; ++i)
            {
                if (i != localPartyId && !_remotePartySessions.Any(session => session.RemoteParty.Id == i))
                    throw new InvalidDataException("Establishing connections was unsuccessful.");
            }
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
                return _numberOfParties;
            }
        }
    }
}
