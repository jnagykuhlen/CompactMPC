using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.UnitTests.Util
{
    public static class TestNetworkRunner
    {
        private const int Port = 16741;

        public static void RunMultiPartyNetwork(int numberOfParties, Action<IMultiPartyNetworkSession> perPartyAction)
        {
            IEnumerable<Task<TcpMultiPartyNetworkSession>> sessionTasks = Enumerable
                .Range(0, numberOfParties)
                .Select(partyId => TcpMultiPartyNetworkSession.EstablishLoopbackAsync(new Party(partyId), Port, numberOfParties));
            
            RunNetwork(sessionTasks, perPartyAction);
        }
        
        public static void RunTwoPartyNetwork(Action<ITwoPartyNetworkSession> perPartyAction)
        {
            Task<TcpTwoPartyNetworkSession>[] sessionTasks =
            {
                TcpTwoPartyNetworkSession.ConnectLoopbackAsync(new Party(0), Port),
                TcpTwoPartyNetworkSession.AcceptLoopbackAsync(new Party(1), Port)
            };

            RunNetwork(sessionTasks, perPartyAction);
        }

        private static void RunNetwork<T>(IEnumerable<Task<T>> sessionTasks, Action<T> perPartyAction)
            where T : IDisposable
        {
            Task.WhenAll(sessionTasks.Select(sessionTask => RunPartyAsync(sessionTask, perPartyAction))).Wait();
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
