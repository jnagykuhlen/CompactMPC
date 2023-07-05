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

        public static Task RunMultiPartyNetwork(int numberOfParties, Func<IMultiPartyNetworkSession, Task> perPartyAction)
        {
            var endPoints = Enumerable
                .Range(StartPort, numberOfParties)
                .Select(port => IPAddress.Loopback.BoundToPort(port))
                .ToArray();
            
            IEnumerable<Task<TcpMultiPartyNetworkSession>> sessionTasks = Enumerable
                .Range(0, numberOfParties)
                .Select(partyId => TcpMultiPartyNetworkSession.EstablishAsync(new Party(partyId), endPoints));
            
            return RunNetwork(sessionTasks, perPartyAction);
        }
        
        public static Task RunTwoPartyNetwork(Func<ITwoPartyNetworkSession, Task> perPartyAction)
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
