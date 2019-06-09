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
    public class TcpTwoPartyNetworkSession : ITwoPartyNetworkSession, IMultiPartyNetworkSession
    {
        private TcpClient _client;
        private IMessageChannel _channel;
        private Party _localParty;
        private Party _remoteParty;

        public TcpTwoPartyNetworkSession(TcpClient client, Party localParty)
        {
            Stream stream = client.GetStream();
            stream.Write(new byte[] { (byte)localParty.Id }, 0, 1);
            int remotePartyId = stream.Read(1)[0];

            _client = client;
            _channel = new StreamMessageChannel(stream);
            _localParty = localParty;
            _remoteParty = new Party(remotePartyId, "Party " + (remotePartyId + 1));
        }

        public static TcpTwoPartyNetworkSession FromPort(int port)
        {
            TcpClient client;
            Party localParty;

            try
            {
                Console.WriteLine("Starting TCP server...");
                client = AcceptTcpClient(port);
                Console.WriteLine("TCP server started.");

                localParty = new Party(0, "Party 0");
            }
            catch (Exception)
            {
                Console.WriteLine("Starting TCP server failed, starting client...");
                client = ConnectTcpClient(port);
                Console.WriteLine("TCP client started.");

                localParty = new Party(1, "Bob");
            }

            return new TcpTwoPartyNetworkSession(client, localParty);
        }

        private static TcpClient AcceptTcpClient(int port)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port) { ExclusiveAddressUse = true };
            listener.Start();
            Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync();
            acceptTask.Wait();
            listener.Stop();
            return acceptTask.Result;
        }

        private static TcpClient ConnectTcpClient(int port)
        {
            TcpClient client = new TcpClient();
            client.ConnectAsync("127.0.0.1", port).Wait();
            return client;
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public IMessageChannel Channel
        {
            get
            {
                return _channel;
            }
        }

        public Party LocalParty
        {
            get
            {
                return _localParty;
            }
        }
        
        public Party RemoteParty
        {
            get
            {
                return _remoteParty;
            }
        }

        public IEnumerable<ITwoPartyNetworkSession> RemotePartySessions
        {
            get
            {
                yield return this;
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
