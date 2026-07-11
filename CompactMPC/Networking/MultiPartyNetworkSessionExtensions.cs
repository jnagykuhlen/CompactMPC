namespace CompactMPC.Networking;

public static class MultiPartyNetworkSessionExtensions
{
    public static bool HasEvenNumberOfRemoteParties(this IMultiPartyNetworkSession session)
    {
        var numberOfRemoteParties = session.NumberOfParties - 1;
        return numberOfRemoteParties % 2 == 0;
    }

    public static bool IsFirstParty(this Party party) => party.Id == 0;
}
