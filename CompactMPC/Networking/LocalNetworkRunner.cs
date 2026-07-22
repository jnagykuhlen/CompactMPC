using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CompactMPC.Collections;

namespace CompactMPC.Networking;

public static class LocalNetworkRunner
{
    private const int StartPort = 16741;

    public static Task RunMultiPartyNetwork(int numberOfParties, Func<IMultiPartyNetworkSession, Task> eachPartyAction) =>
        RunMultiPartyNetwork(Enumerable.Repeat(eachPartyAction, numberOfParties).ToArray());

    public static Task RunMultiPartyNetwork(params Func<IMultiPartyNetworkSession, Task>[] partyActions)
    {
        var endPoints = partyActions
            .Select((_, index) => new IPEndPoint(IPAddress.Loopback, StartPort + index))
            .ToArray();

        var sessionTasks = partyActions.Select(
            (partyAction, index) => EstablishMultiPartyAsync(index, endPoints).AndThenAsync(partyAction)
        );

        return Task.WhenAll(sessionTasks);
    }

    private static Task<TcpMultiPartyNetworkSession> EstablishMultiPartyAsync(int index, IPEndPoint[] endPoints) =>
        TcpMultiPartyNetworkSession.EstablishAsync(new Party(index), endPoints[index], endPoints.Without(endPoints[index]).ToArray());

    public static Task RunTwoPartyNetwork(Func<ITwoPartyNetworkSession, Task> firstPartyAction, Func<ITwoPartyNetworkSession, Task> secondPartyAction)
    {
        var firstParty = new Party(0);
        var secondParty = new Party(1);

        var firstEndPoint = new IPEndPoint(IPAddress.Loopback, StartPort);
        var secondEndPoint = new IPEndPoint(IPAddress.Loopback, StartPort + 1);

        return Task.WhenAll(
            TcpTwoPartyNetworkSession.EstablishAsync(firstParty, firstEndPoint, secondEndPoint).AndThenAsync(firstPartyAction),
            TcpTwoPartyNetworkSession.EstablishAsync(secondParty, secondEndPoint, firstEndPoint).AndThenAsync(secondPartyAction)
        );
    }

    private static async Task AndThenAsync<T>(this Task<T> sessionTask, Func<T, Task> sessionAction)
        where T : IDisposable
    {
        using var session = await sessionTask;
        await sessionAction(session);
    }
}
