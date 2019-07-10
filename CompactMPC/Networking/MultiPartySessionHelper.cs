using System;
using System.Collections.Generic;
using System.Text;

namespace CompactMPC.Networking
{
    public static class MultiPartySessionHelper
    {
        public static bool HasEvenNumberOfRemoteParties(this IMultiPartyNetworkSession session)
        {
            int numberOfRemoteParties = session.NumberOfParties - 1;
            return numberOfRemoteParties % 2 == 0;
        }

        public static bool IsFirstParty(this Party party)
        {
            return party.Id == 0;
        }
    }
}
