using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CompactMPC.Networking;

namespace CompactMPC.Util
{
    public static class TestNetworkRunner
    {
        private const int StartPort = 16741;

        public static void RunMultiPartyNetwork(int numberOfParties, Action<IMultiPartyNetworkSession> perPartyAction)
        {
            var endPoints = Enumerable
                .Range(StartPort, numberOfParties)
                .Select(port => IPAddress.Loopback.BoundToPort(port))
                .ToArray();
            
            IEnumerable<Task<TcpMultiPartyNetworkSession>> sessionTasks = Enumerable
                .Range(0, numberOfParties)
                .Select(partyId => TcpMultiPartyNetworkSession.EstablishAsync(new Party(partyId), endPoints));
            
            RunNetwork(sessionTasks, perPartyAction);
        }
        
        public static void RunTwoPartyNetwork(Action<ITwoPartyNetworkSession> perPartyAction)
        {
            Party firstParty = new Party(0);
            Party secondParty = new Party(1);

            IPEndPoint endPoint = IPAddress.Loopback.BoundToPort(StartPort);
            
            using ITwoPartyConnectionListener listener = TcpTwoPartyNetworkSession.CreateListener(secondParty, endPoint);
            Task<TcpTwoPartyNetworkSession>[] sessionTasks =
            {
                TcpTwoPartyNetworkSession.ConnectAsync(firstParty, endPoint),
                listener.AcceptAsync()
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
