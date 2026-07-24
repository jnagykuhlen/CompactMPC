using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.Protocol.Internal;

public class OrderedMultiPartyNetworkSession(IMultiPartyNetworkSession multiPartySession)
{
    private readonly List<OrderedParty> _orderedParties = multiPartySession.RemotePartySessions
        .Select(session => new OrderedParty(session.RemoteParty, new ReceiveHandler(session)))
        .Append(new OrderedParty(multiPartySession.LocalParty, new SendHandler(multiPartySession)))
        .OrderBy(orderedParty => orderedParty.Party)
        .ToList();

    public Task<T[]> ExchangeAsync<T>(SendAsync<T> sendAsync, ReceiveAsync<T> receiveAsync) =>
        Task.WhenAll(_orderedParties.Select(orderedParty => orderedParty.Handler.HandleAsync(sendAsync, receiveAsync)));

    public IEnumerable<Party> OrderedParties => _orderedParties.Select(orderedParty => orderedParty.Party);
    // TODO: Still required?
    public IEnumerable<ITwoPartyNetworkSession> RemotePartySessions => multiPartySession.RemotePartySessions;
    public Party LocalParty => multiPartySession.LocalParty;
    public int NumberOfParties => _orderedParties.Count;
    public bool IsLocalPartyLeading => _orderedParties[0].Party == LocalParty;

    private record OrderedParty(Party Party, ISendReceiveHandler Handler);

    private interface ISendReceiveHandler
    {
        Task<T> HandleAsync<T>(SendAsync<T> sendAsync, ReceiveAsync<T> receiveAsync);
    }

    private class SendHandler(IMultiPartyNetworkSession multiPartySession) : ISendReceiveHandler
    {
        public Task<T> HandleAsync<T>(SendAsync<T> sendAsync, ReceiveAsync<T> receiveAsync) =>
            sendAsync(multiPartySession);
    }

    private class ReceiveHandler(ITwoPartyNetworkSession twoPartySession) : ISendReceiveHandler
    {
        public Task<T> HandleAsync<T>(SendAsync<T> sendAsync, ReceiveAsync<T> receiveAsync) =>
            receiveAsync(twoPartySession);
    }
}

public delegate Task<T> SendAsync<T>(IMultiPartyNetworkSession multiPartySession);

public delegate Task<T> ReceiveAsync<T>(ITwoPartyNetworkSession twoPartySession);
