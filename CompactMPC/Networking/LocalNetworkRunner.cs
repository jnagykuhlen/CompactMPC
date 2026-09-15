using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using CompactMPC.Collections;
using CompactMPC.Protocol;

namespace CompactMPC.Networking;

public static class LocalNetworkRunner
{
    private const int StartPort = 16741;

    public static Task RunMultiPartyNetworkAsync(MultiPartySessionDescription sessionDescription, Func<SessionInfo, Task> eachPartyAction)
    {
        var endPoints = sessionDescription.PartySlots
            .Select((_, index) => new IPEndPoint(IPAddress.Loopback, StartPort + index))
            .ToArray();

        return Task.WhenAll(
            sessionDescription.PartySlots.Select((partySlot, index) =>
                EstablishMultiPartyAsync(partySlot.Role, index, endPoints)
                    .AndThenAsync(session => eachPartyAction(new SessionInfo(session, index)))
            )
        );
    }

    public static async Task<SessionInfo> RunMultiPartyNetworkSinglePartyAsync(int numberOfParties) =>
        await RunMultiPartyNetworkSinglePartyAsync(numberOfParties, Role.Default);

    public static async Task<SessionInfo> RunMultiPartyNetworkSinglePartyAsync(int numberOfParties, Role localPartyRole)
    {
        var endPoints = Enumerable.Range(0, numberOfParties)
            .Select(partyIndex => new IPEndPoint(IPAddress.Loopback, StartPort + partyIndex))
            .ToArray();

        for (var localPartyIndex = 0; localPartyIndex < numberOfParties; localPartyIndex++)
        {
            try
            {
                var session = await EstablishMultiPartyAsync(localPartyRole, localPartyIndex, endPoints);
                return new SessionInfo(session, localPartyIndex);
            }
            catch (SocketException socketException)
                when (socketException.SocketErrorCode == SocketError.AddressAlreadyInUse) { }
        }

        throw new SocketException((int)SocketError.AddressAlreadyInUse, "All designated ports are already in use.");
    }

    public static Task RunTwoPartyNetworkAsync(Func<ITwoPartyNetworkSession, Task> firstPartyAction, Func<ITwoPartyNetworkSession, Task> secondPartyAction)
    {
        var firstParty = new Party();
        var secondParty = new Party();

        var firstEndPoint = new IPEndPoint(IPAddress.Loopback, StartPort);
        var secondEndPoint = new IPEndPoint(IPAddress.Loopback, StartPort + 1);

        return Task.WhenAll(
            TcpTwoPartyNetworkSession.EstablishAsync(firstParty, firstEndPoint, secondEndPoint).AndThenAsync(firstPartyAction),
            TcpTwoPartyNetworkSession.EstablishAsync(secondParty, secondEndPoint, firstEndPoint).AndThenAsync(secondPartyAction)
        );
    }

    private static Task<TcpMultiPartyNetworkSession> EstablishMultiPartyAsync(Role role, int index, IPEndPoint[] endPoints) =>
        TcpMultiPartyNetworkSession.EstablishAsync(new Party(role), endPoints[index], endPoints.Without(endPoints[index]).ToArray());

    private static async Task AndThenAsync<T>(this Task<T> sessionTask, Func<T, Task> sessionAction)
        where T : IDisposable
    {
        using var session = await sessionTask;
        await sessionAction(session);
    }
}

public record SessionInfo(IMultiPartyNetworkSession Session, int LocalPartyIndex);
