using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

        public static async Task<TcpTwoPartyNetworkSession> EstablishAsync(Party localParty, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint)
        {
            return (await EstablishAsync(localParty, localEndPoint, new[] { remoteEndPoint })).First();
        }

        public static async Task<TcpTwoPartyNetworkSession[]> EstablishAsync(Party localParty, IPEndPoint localEndPoint, IPEndPoint[] remoteEndPoints)
        {
            TcpListener listener = new TcpListener(localEndPoint) { ExclusiveAddressUse = true };
            listener.Start();
            try
            {
                Task<TcpTwoPartyNetworkSession[]> listenSessionsTask = Task.WhenAll(remoteEndPoints.Select(_ => ListenAsync(listener, localParty)));
                Task<TcpTwoPartyNetworkSession[]> connectSessionsTask = Task.WhenAll(remoteEndPoints.Select(remoteEndPoint => ConnectAsync(remoteEndPoint, localParty)));

                TcpTwoPartyNetworkSession[] listenSessions = await listenSessionsTask;
                TcpTwoPartyNetworkSession[] connectSessions = await connectSessionsTask;

                TcpTwoPartyNetworkSession[] establishedSessions = listenSessions.Join(connectSessions, session => session.RemoteParty, (listenSession, connectSession) =>
                {
                    Party remoteParty = listenSession.RemoteParty;
                    if (localParty > remoteParty)
                    {
                        listenSession.Dispose();
                        return connectSession;
                    }
                    else
                    {
                        connectSession.Dispose();
                        return listenSession;
                    }
                }).ToArray();

                if (establishedSessions.Length != remoteEndPoints.Length)
                {
                    foreach (TcpTwoPartyNetworkSession session in listenSessions.Concat(connectSessions))
                        session.Dispose();

                    throw new NetworkConsistencyException("Inconsistent party info exchange.");
                }

                return establishedSessions;

            }
            finally
            {
                listener.Stop();
            }
        }

        private static async Task<TcpTwoPartyNetworkSession> ListenAsync(TcpListener listener, Party localParty)
        {
            return await CreateFromPartyInfoExchangeAsync(await listener.AcceptTcpClientAsync(), localParty);
        }

        private static async Task<TcpTwoPartyNetworkSession> ConnectAsync(IPEndPoint remoteEndPoint, Party localParty)
        {
            TcpClient client = new TcpClient();
            try
            {
                await client.ConnectAsync(remoteEndPoint);
                return await CreateFromPartyInfoExchangeAsync(client, localParty);
            }
            catch (Exception)
            {
                client.Dispose();
                throw;
            }
        }

        private static async Task<TcpTwoPartyNetworkSession> CreateFromPartyInfoExchangeAsync(TcpClient client, Party localParty)
        {
            Stream stream = client.GetStream();
            await WritePartyInfoAsync(stream, localParty);
            Party remoteParty = await ReadPartyInfoAsync(stream);
            return new TcpTwoPartyNetworkSession(client, localParty, remoteParty);
        }

        private static async Task WritePartyInfoAsync(Stream stream, Party party)
        {
            await stream.WriteInt32Async(party.Id);
            await stream.WriteStringAsync(party.Name);
            await stream.WriteGuidAsync(party.Guid);
        }

        private static async Task<Party> ReadPartyInfoAsync(Stream stream)
        {
            int id = await stream.ReadInt32Async();
            string name = await stream.ReadStringAsync();
            Guid guid = await stream.ReadGuidAsync();
            return new Party(id, name, guid);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public IMessageChannel Channel { get; }

        public Party LocalParty { get; }

        public Party RemoteParty { get; }
    }
}