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
    public class MultiPartyNetworkSession : INetworkSession
    {
        private TcpClient[] _connections;
        private Party _localParty;
        private List<Party> _remoteParties;
        private int _numberOfParties;

        public MultiPartyNetworkSession(int startPort, int numberOfParties, int localPartyId)
        {
            _connections = new TcpClient[numberOfParties];
            _localParty = CreateParty(localPartyId);
            _remoteParties = new List<Party>(numberOfParties - 1);
            _numberOfParties = numberOfParties;

            for (int i = 0; i < localPartyId; ++i)
            {
                _connections[i] = new TcpClient();
                _connections[i].ConnectAsync("127.0.0.1", startPort + i).Wait();
                _connections[i].GetStream().Write(new byte[] { (byte)localPartyId }, 0, 1);
                _remoteParties.Add(CreateParty(i));
            }

            TcpListener listener = new TcpListener(IPAddress.Any, startPort + localPartyId) { ExclusiveAddressUse = true };

            listener.Start();
            for (int j = localPartyId + 1; j < numberOfParties; ++j)
            {
                TcpClient connection = listener.AcceptTcpClientAsync().Result;
                int remotePartyId = connection.GetStream().Read(1)[0];

                if (remotePartyId < 0 || remotePartyId >= numberOfParties)
                    throw new InvalidDataException("Remote host sent invalid party identifier.");

                if (_connections[remotePartyId] != null)
                    throw new InvalidDataException("Remote host with this identifier is already connected.");

                _connections[remotePartyId] = connection;
                _remoteParties.Add(CreateParty(remotePartyId));
            }
            listener.Stop();
        }

        private static Party CreateParty(int id)
        {
            return new Party(id, "Party " + (id + 1));
        }

        private TcpClient CreateTcp(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port) { ExclusiveAddressUse = true };
            listener.Start();
            Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();
            acceptTask.Wait();
            listener.Stop();
            return acceptTask.Result;
        }

        private TcpClient ConnectTcpClient(int port)
        {
            TcpClient client = new TcpClient();
            client.ConnectAsync("127.0.0.1", port).Wait();
            return client;
        }

        public Stream GetConnection(int remotePartyId)
        {
            if (remotePartyId < 0 || remotePartyId >= _numberOfParties)
                throw new ArgumentOutOfRangeException(nameof(remotePartyId));

            if (remotePartyId == _localParty.Id)
                throw new ArgumentException("Cannot get connection to local party.", nameof(remotePartyId));

            return _connections[remotePartyId].GetStream();
        }

        public IMessageChannel GetChannel(int remotePartyId)
        {
            return new StreamMessageChannel(GetConnection(remotePartyId));
        }

        public void Dispose()
        {
            foreach(TcpClient connection in _connections)
            {
                if (connection != null)
                    connection.Dispose();
            }
        }

        public Party LocalParty
        {
            get
            {
                return _localParty;
            }
        }
        
        public IEnumerable<Party> RemoteParties
        {
            get
            {
                return _remoteParties;
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
