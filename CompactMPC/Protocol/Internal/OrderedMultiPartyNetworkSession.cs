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

    public Task<T[]> SendAndReceiveAsync<T>(SendToAllAsync<T> sendToAllAsync, ReceiveFromEachAsync<T> receiveFromEachAsync) =>
        Task.WhenAll(_orderedParties.Select(orderedParty => orderedParty.Handler.HandleAsync(sendToAllAsync, receiveFromEachAsync)));

    public Task<T[]> PairwiseSendOrReceiveAsync<T>(PairwiseSendAsync<T> sendAsync, PairwiseReceiveAsync<T> receiveAsync) =>
        Task.WhenAll(
            multiPartySession.RemotePartySessions
                .AsParallel()
                .Select(remotePartySession => remotePartySession.RemoteParty < remotePartySession.LocalParty ?
                    sendAsync(remotePartySession.Channel) :
                    receiveAsync(remotePartySession.Channel))
        );

    public IEnumerable<Party> OrderedParties => _orderedParties.Select(orderedParty => orderedParty.Party);
    public Party LocalParty => multiPartySession.LocalParty;
    public int NumberOfParties => _orderedParties.Count;
    public bool IsLocalPartyLeading => _orderedParties[0].Party == LocalParty;

    public MultiPartySessionDescription CreateSessionDescription() => new(NumberOfParties);

    private record OrderedParty(Party Party, ISendReceiveHandler Handler);

    private interface ISendReceiveHandler
    {
        Task<T> HandleAsync<T>(SendToAllAsync<T> sendToAll, ReceiveFromEachAsync<T> receiveFromEachAsync);
    }

    private class SendHandler(IMultiPartyNetworkSession multiPartySession) : ISendReceiveHandler
    {
        public Task<T> HandleAsync<T>(SendToAllAsync<T> sendToAll, ReceiveFromEachAsync<T> receiveFromEachAsync) =>
            sendToAll(multiPartySession);
    }

    private class ReceiveHandler(ITwoPartyNetworkSession twoPartySession) : ISendReceiveHandler
    {
        public Task<T> HandleAsync<T>(SendToAllAsync<T> sendToAll, ReceiveFromEachAsync<T> receiveFromEachAsync) =>
            receiveFromEachAsync(twoPartySession);
    }
}

public delegate Task<T> SendToAllAsync<T>(IMultiPartyNetworkSession multiPartySession);

public delegate Task<T> ReceiveFromEachAsync<T>(ITwoPartyNetworkSession twoPartySession);

public delegate Task<T> PairwiseSendAsync<T>(IMessageChannel channel);

public delegate Task<T> PairwiseReceiveAsync<T>(IMessageChannel channel);
