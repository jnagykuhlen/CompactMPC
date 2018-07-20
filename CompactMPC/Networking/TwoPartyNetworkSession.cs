using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace CompactMPC.Networking
{
    public class TwoPartyNetworkSession : INetworkSession
    {
        private TcpClient _client;
        private Party _localParty;
        private Party _remoteParty;

        public TwoPartyNetworkSession(int port)
        {
            try
            {
                Console.WriteLine("Starting TCP server...");
                _client = AcceptTcpClient(port);
                Console.WriteLine("TCP server started.");

                _localParty = new Party(0, "Alice");
                _remoteParty = new Party(1, "Bob");
            }
            catch(Exception)
            {
                Console.WriteLine("Starting TCP server failed, starting client...");
                _client = ConnectTcpClient(port);
                Console.WriteLine("TCP client started.");

                _remoteParty = new Party(0, "Alice");
                _localParty = new Party(1, "Bob");
            }
        }

        private TcpClient AcceptTcpClient(int port)
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
            if (remotePartyId != _remoteParty.Id)
                throw new ArgumentException("Invalid remote party.", nameof(remotePartyId));

            return _client.GetStream();
        }

        public void Dispose()
        {
            _client.Dispose();
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
                yield return _remoteParty;
            }
        }

        public int NumberOfParties
        {
            get
            {
                return 2;
            }
        }
    }
}
