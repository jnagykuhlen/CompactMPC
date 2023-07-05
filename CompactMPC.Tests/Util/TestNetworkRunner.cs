using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.Util
{
    public static class TestNetworkRunner
    {
        private const int Port = 16741;

        public static Task RunMultiPartyNetwork(int numberOfParties, Func<IMultiPartyNetworkSession, Task> perPartyAction)
        {
            IEnumerable<Task<TcpMultiPartyNetworkSession>> sessionTasks = Enumerable
                .Range(0, numberOfParties)
                .Select(partyId => TcpMultiPartyNetworkSession.EstablishLoopbackAsync(new Party(partyId), Port, numberOfParties));
            
            return RunNetwork(sessionTasks, perPartyAction);
        }
        
        public static Task RunTwoPartyNetwork(Func<ITwoPartyNetworkSession, Task> perPartyAction)
        {
            Party firstParty = new Party(0);
            Party secondParty = new Party(1);
            
            using ITwoPartyConnectionListener listener = TcpTwoPartyNetworkSession.CreateListenerLoopback(secondParty, Port);
            Task<TcpTwoPartyNetworkSession>[] sessionTasks =
            {
                TcpTwoPartyNetworkSession.ConnectLoopbackAsync(firstParty, Port),
                listener.AcceptAsync()
            };

            return RunNetwork(sessionTasks, perPartyAction);
        }

        private static async Task RunNetwork<T>(IEnumerable<Task<T>> sessionTasks, Func<T, Task> perPartyAction)
            where T : IDisposable
        {
            T[] sessions = await Task.WhenAll(sessionTasks);
            await Task.WhenAll(sessions.Select(perPartyAction));
        }
    }
}
