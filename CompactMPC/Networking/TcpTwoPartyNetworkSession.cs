using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CompactMPC.Collections;

namespace CompactMPC.Networking;

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

    public static async Task<TcpTwoPartyNetworkSession> EstablishAsync(Party localParty, IPEndPoint localEndPoint, IPEndPoint remoteEndPoint) =>
        (await EstablishAsync(localParty, localEndPoint, [remoteEndPoint])).First();

    public static async Task<TcpTwoPartyNetworkSession[]> EstablishAsync(Party localParty, IPEndPoint localEndPoint, IPEndPoint[] remoteEndPoints)
    {
        var listener = new TcpListener(localEndPoint) { ExclusiveAddressUse = true };
        listener.Start();
        try
        {
            var listenSessionsTask = Task.WhenAll(remoteEndPoints.Select(_ => ListenAsync(listener, localParty)));
            var connectSessionsTask = Task.WhenAll(remoteEndPoints.Select(remoteEndPoint => ConnectAsync(remoteEndPoint, localParty)));

            var listenSessions = await listenSessionsTask;
            var connectSessions = await connectSessionsTask;

            var establishedSessions = listenSessions.Join(connectSessions, session => session.RemoteParty, (listenSession, connectSession) =>
            {
                var remoteParty = listenSession.RemoteParty;
                var (sessionToDispose, establishedSession) = localParty > remoteParty ?
                    (listenSession, connectSession) :
                    (connectSession, listenSession);

                sessionToDispose.Dispose();
                return establishedSession;
            }).ToArray();

            if (establishedSessions.Length != remoteEndPoints.Length)
            {
                foreach (var session in listenSessions.Concat(connectSessions))
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
        var session = await CreateFromPartyInfoExchangeAsync(await listener.AcceptTcpClientAsync(), localParty);

#if DEBUG
        Console.WriteLine($"[{localParty.Name}] LISTEN successful to {session.RemoteParty.Name}.");
#endif

        return session;
    }

    private static Task<TcpTwoPartyNetworkSession> ConnectAsync(IPEndPoint remoteEndPoint, Party localParty)
    {
        var client = new TcpClient();
        try
        {
            return RetriedConnectAsync(client, remoteEndPoint, localParty);
        }
        catch (Exception)
        {
            client.Dispose();
            throw;
        }
    }

    private static async Task<TcpTwoPartyNetworkSession> RetriedConnectAsync(TcpClient client, IPEndPoint remoteEndPoint, Party localParty)
    {
        try
        {
            await client.ConnectAsync(remoteEndPoint);
            var session = await CreateFromPartyInfoExchangeAsync(client, localParty);

#if DEBUG
            Console.WriteLine($"[{localParty.Name}] CONNECT successful to {session.RemoteParty.Name}.");
#endif

            return session;
        }
        catch (SocketException exception) when (exception.SocketErrorCode == SocketError.ConnectionRefused)
        {
            return await RetriedConnectAsync(client, remoteEndPoint, localParty);
        }
    }

    private static async Task<TcpTwoPartyNetworkSession> CreateFromPartyInfoExchangeAsync(TcpClient client, Party localParty)
    {
        var stream = client.GetStream();
        await WritePartyInfoAsync(stream, localParty);
        var remoteParty = await ReadPartyInfoAsync(stream);
        return new TcpTwoPartyNetworkSession(client, localParty, remoteParty);
    }

    private static async Task WritePartyInfoAsync(Stream stream, Party party)
    {
        await stream.WriteGuidAsync(party.Guid);
        await stream.WriteStringAsync(party.Name);
    }

    private static async Task<Party> ReadPartyInfoAsync(Stream stream)
    {
        var guid = await stream.ReadGuidAsync();
        var name = await stream.ReadStringAsync();
        return new Party(guid, name);
    }

    public void Dispose() => _client.Dispose();

    public IMessageChannel Channel { get; }
    public Party LocalParty { get; }
    public Party RemoteParty { get; }
}
