using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CompactMPC.Networking
{
    public class TcpTwoPartyNetworkSession : ITwoPartyNetworkSession
    {
        private readonly TcpClient _client;

        private TcpTwoPartyNetworkSession(TcpClient client, Party localParty, Party remoteParty)
        {
            _client = client;
            Channel = new StreamMessageChannel(client.GetStream());
            LocalParty = localParty;
            RemoteParty = remoteParty;
        }

        public static Task<TcpTwoPartyNetworkSession> ConnectLoopbackAsync(Party localParty, int port)
        {
            return ConnectAsync(localParty, new IPEndPoint(IPAddress.Loopback, port));
        }

        public static async Task<TcpTwoPartyNetworkSession> EstablishAsync(Party localParty, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            Task<TcpClient> listenTask = ListenAsync(localEndPoint, cancellationTokenSource.Token);
            Task<TcpClient> connectTask = ConnectAsync(remoteEndPoint, cancellationTokenSource.Token);

            Task<TcpClient> selectedClientTask = await Task.WhenAny(
                listenTask,
                connectTask
            );

            cancellationTokenSource.Cancel();

            TcpClient selectedClient = selectedClientTask.Result;

            try
            {
                TcpClient[] clients = await Task.WhenAll(listenTask, connectTask);
                foreach (TcpClient client in clients)
                {
                    if (client != selectedClient)
                        client.Dispose();
                }
            }
            catch (OperationCanceledException)
            {
            }

            return CreateFromPartyInformationExchange(localParty, selectedClientTask.Result);
        }

        private static async Task<TcpClient> ListenAsync(IPEndPoint localEndPoint, CancellationToken cancellationToken)
        {
            TcpListener listener = new TcpListener(localEndPoint) { ExclusiveAddressUse = true };
            listener.Start();
            try
            {
                return await listener.AcceptTcpClientAsync(cancellationToken);
            }
            finally
            {
                listener.Stop();
            }
        }

        private static async Task<TcpClient> ConnectAsync(IPEndPoint remoteEndPoint, CancellationToken cancellationToken)
        {
            TcpClient client = new TcpClient();
            try
            {
                await client.ConnectAsync(remoteEndPoint, cancellationToken);
                return client;
            }
            catch (Exception)
            {
                client.Dispose();
                throw;
            }
        }

        public static async Task<TcpTwoPartyNetworkSession> ConnectAsync(Party localParty, IPEndPoint remoteEndPoint)
        {
            TcpClient client = new TcpClient();
            await client.ConnectAsync(remoteEndPoint);
            return CreateFromPartyInformationExchange(localParty, client);
        }

        public static ITwoPartyConnectionListener CreateListener(Party localParty, IPEndPoint localEndPoint)
        {
            return new ConnectionListener(localParty, localEndPoint);
        }

        public static ITwoPartyConnectionListener CreateListenerLoopback(Party localParty, int port)
        {
            return CreateListener(localParty, new IPEndPoint(IPAddress.Loopback, port));
        }

        private static TcpTwoPartyNetworkSession CreateFromPartyInformationExchange(Party localParty, TcpClient client)
        {
            WritePartyInformation(client, localParty);
            Party remoteParty = ReadPartyInformation(client);
            return new TcpTwoPartyNetworkSession(client, localParty, remoteParty);
        }

        private static void WritePartyInformation(TcpClient client, Party party)
        {
            using BinaryWriter writer = new BinaryWriter(client.GetStream(), Encoding.UTF8, true);
            writer.Write(party.Id);
            writer.Write(party.Name);
        }

        private static Party ReadPartyInformation(TcpClient client)
        {
            using BinaryReader reader = new BinaryReader(client.GetStream(), Encoding.UTF8, true);
            int id = reader.ReadInt32();
            string name = reader.ReadString();
            return new Party(id, name);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public IMessageChannel Channel { get; }

        public Party LocalParty { get; }

        public Party RemoteParty { get; }

        private class ConnectionListener : ITwoPartyConnectionListener
        {
            private readonly Party _localParty;
            private readonly TcpListener _listener;

            public ConnectionListener(Party localParty, IPEndPoint localEndPoint)
            {
                _localParty = localParty;
                _listener = new TcpListener(localEndPoint) { ExclusiveAddressUse = true };
                _listener.Start();
            }

            public async Task<TcpTwoPartyNetworkSession> AcceptAsync()
            {
                TcpClient client = await _listener.AcceptTcpClientAsync();
                return CreateFromPartyInformationExchange(_localParty, client);
            }

            public void Dispose()
            {
                _listener.Stop();
            }
        }
    }
}