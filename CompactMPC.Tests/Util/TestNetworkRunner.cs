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

        public static Task RunMultiPartyNetwork(int numberOfParties, Action<IMultiPartyNetworkSession> perPartyAction)
        {
            IEnumerable<Task<TcpMultiPartyNetworkSession>> sessionTasks = Enumerable
                .Range(0, numberOfParties)
                .Select(partyId => TcpMultiPartyNetworkSession.EstablishLoopbackAsync(new Party(partyId), Port, numberOfParties));
            
            return RunNetwork(sessionTasks, perPartyAction);
        }
        
        public static Task RunTwoPartyNetwork(Action<ITwoPartyNetworkSession> perPartyAction)
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

        private static Task RunNetwork<T>(IEnumerable<Task<T>> sessionTasks, Action<T> perPartyAction)
            where T : IDisposable
        {
            return Task.WhenAll(sessionTasks.Select(sessionTask => RunPartyAsync(sessionTask, perPartyAction)));
        }

        private static Task RunPartyAsync<T>(Task<T> sessionTask, Action<T> perPartyAction)
            where T : IDisposable
        {
            return Task.Factory.StartNew(
                () =>
                {
                    using T session = sessionTask.Result;
                    perPartyAction(session);
                },
                TaskCreationOptions.LongRunning
            );
        }
    }
}
